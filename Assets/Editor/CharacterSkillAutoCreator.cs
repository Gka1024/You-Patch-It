using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

[InitializeOnLoad]
public static class CharacterSkillAutoCreator
{
    private const string PendingCharacterPathKey = "CharacterSkillAutoCreator.PendingCharacterPath";
    private const string PendingSkillClassNameKey = "CharacterSkillAutoCreator.PendingSkillClassName";
    private const string PendingSkillAssetPathKey = "CharacterSkillAutoCreator.PendingSkillAssetPath";

    private static bool isProcessing;

    static CharacterSkillAutoCreator()
    {
        EditorApplication.update -= ProcessPendingSkillAsset;
        EditorApplication.update += ProcessPendingSkillAsset;
    }

    public static void CreateSkillClassAndConnect(Character character, string characterAssetPath, string skillName, string skillFolderPath)
    {
        if (character == null)
        {
            Debug.LogError("[SkillAutoCreator] 캐릭터가 지정되지 않았습니다.");
            return;
        }

        if (string.IsNullOrWhiteSpace(characterAssetPath) || string.IsNullOrWhiteSpace(skillFolderPath))
        {
            Debug.LogError("[SkillAutoCreator] 캐릭터 에셋 경로 또는 스킬 폴더 경로가 비어 있습니다.");
            return;
        }

        string safeName = SanitizeName(skillName);

        if (string.IsNullOrEmpty(safeName))
        {
            Debug.LogError("[SkillAutoCreator] 유효한 스킬 이름을 만들 수 없습니다.");
            return;
        }

        string className = $"CharacterSkill_{character.id:D3}_{safeName}";
        string scriptPath = $"{CharacterCreatorWindow.SkillScriptFolder}/{className}.cs";
        string skillAssetPath = $"{skillFolderPath}/{className}.asset";

        if (File.Exists(scriptPath))
        {
            Debug.LogError($"[SkillAutoCreator] 스킬 클래스 파일이 이미 존재합니다: {scriptPath}");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<CharacterSkill>(skillAssetPath) != null)
        {
            Debug.LogError($"[SkillAutoCreator] 스킬 SO가 이미 존재합니다: {skillAssetPath}");
            return;
        }

        if (!AssetDatabase.IsValidFolder(skillFolderPath))
        {
            Debug.LogError($"[SkillAutoCreator] 스킬 SO 폴더가 없습니다: {skillFolderPath}");
            return;
        }

        Directory.CreateDirectory(CharacterCreatorWindow.SkillScriptFolder);

        string scriptContent = $@"using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ""ScriptableObject/Character/Skill/{safeName}"")]
public class {className} : CharacterSkill
{{
    public override void Execute(BattleCharacter self, List<BattleCharacter> allies, List<BattleCharacter> enemies, float coefficient, System.Random random)
    {{
        throw new System.NotImplementedException();
    }}
}}
";

        try
        {
            File.WriteAllText(scriptPath, scriptContent);

            SessionState.SetString(PendingCharacterPathKey, characterAssetPath);
            SessionState.SetString(PendingSkillClassNameKey, className);
            SessionState.SetString(PendingSkillAssetPathKey, skillAssetPath);

            Debug.Log($"[SkillAutoCreator] 스킬 스크립트 생성: {scriptPath}");
            Debug.Log($"[SkillAutoCreator] 생성 예정 SO 경로: {skillAssetPath}");

            AssetDatabase.Refresh();
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SkillAutoCreator] 스킬 스크립트 생성 중 오류가 발생했습니다.\n{exception}");
        }
    }

    private static void ProcessPendingSkillAsset()
    {
        if (isProcessing || EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        string characterPath = SessionState.GetString(PendingCharacterPathKey, "");
        string className = SessionState.GetString(PendingSkillClassNameKey, "");
        string skillAssetPath = SessionState.GetString(PendingSkillAssetPathKey, "");

        if (string.IsNullOrEmpty(characterPath) ||
            string.IsNullOrEmpty(className) ||
            string.IsNullOrEmpty(skillAssetPath))
        {
            return;
        }

        isProcessing = true;

        try
        {
            Type skillType = TypeCache.GetTypesDerivedFrom<CharacterSkill>()
                .FirstOrDefault(type => type.Name == className && !type.IsAbstract);

            if (skillType == null)
            {
                // 새 스크립트의 타입이 아직 등록되지 않았다면 다음 update에서 다시 확인한다.
                return;
            }

            Character character = AssetDatabase.LoadAssetAtPath<Character>(characterPath);

            if (character == null)
            {
                Debug.LogError($"[SkillAutoCreator] 캐릭터 에셋을 찾을 수 없습니다: {characterPath}");
                ClearPendingData();
                return;
            }

            string skillAssetDirectory = Path.GetDirectoryName(skillAssetPath);

            if (string.IsNullOrEmpty(skillAssetDirectory) || !AssetDatabase.IsValidFolder(skillAssetDirectory))
            {
                Debug.LogError($"[SkillAutoCreator] SO 저장 폴더가 없습니다: {skillAssetDirectory}");
                ClearPendingData();
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<CharacterSkill>(skillAssetPath) != null)
            {
                Debug.LogError($"[SkillAutoCreator] 스킬 SO가 이미 존재합니다: {skillAssetPath}");
                ClearPendingData();
                return;
            }

            CharacterSkill skill = ScriptableObject.CreateInstance(skillType) as CharacterSkill;

            if (skill == null)
            {
                Debug.LogError($"[SkillAutoCreator] 스킬 인스턴스 생성 실패: {className}");
                ClearPendingData();
                return;
            }

            skill.skillOwner = character.id;
            skill.skillName = className;

            AssetDatabase.CreateAsset(skill, skillAssetPath);

            CharacterSkill createdSkill = AssetDatabase.LoadAssetAtPath<CharacterSkill>(skillAssetPath);

            if (createdSkill == null)
            {
                Debug.LogError($"[SkillAutoCreator] SO 에셋 저장에 실패했습니다: {skillAssetPath}");
                UnityEngine.Object.DestroyImmediate(skill);
                ClearPendingData();
                return;
            }

            character.skill = createdSkill;

            EditorUtility.SetDirty(character);
            EditorUtility.SetDirty(createdSkill);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SkillAutoCreator] 스킬 SO 생성 및 캐릭터 연결 완료: {skillAssetPath}");

            ClearPendingData();
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SkillAutoCreator] 스킬 SO 생성 중 예외가 발생했습니다.\n{exception}");
        }
        finally
        {
            isProcessing = false;
        }
    }

    private static void ClearPendingData()
    {
        SessionState.EraseString(PendingCharacterPathKey);
        SessionState.EraseString(PendingSkillClassNameKey);
        SessionState.EraseString(PendingSkillAssetPathKey);
    }

    private static string SanitizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        string result = new string(value.Trim().Where(character => char.IsLetterOrDigit(character) || character == '_').ToArray());

        if (string.IsNullOrEmpty(result))
        {
            return "";
        }

        if (char.IsDigit(result[0]))
        {
            result = $"_{result}";
        }

        return result;
    }
}