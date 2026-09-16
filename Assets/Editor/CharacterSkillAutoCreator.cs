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

    static CharacterSkillAutoCreator()
    {
        CompilationPipeline.compilationFinished -= OnCompilationFinished;
        CompilationPipeline.compilationFinished += OnCompilationFinished;
    }

    public static void CreateSkillClassAndConnect(Character character, string characterAssetPath, string skillName, string skillFolderPath)
    {
        if (character == null)
        {
            Debug.LogError("캐릭터가 지정되지 않았습니다.");
            return;
        }

        string safeName = SanitizeName(skillName);

        if (string.IsNullOrEmpty(safeName))
        {
            Debug.LogError("유효한 스킬 이름을 만들 수 없습니다.");
            return;
        }

        string className = $"CharacterSkill_{safeName}";
        string scriptPath = $"{CharacterCreatorWindow.SkillScriptFolder}/{className}.cs";
        string skillAssetPath = $"{skillFolderPath}/{className}.asset";

        if (File.Exists(scriptPath))
        {
            Debug.LogError($"이미 스킬 클래스 파일이 존재합니다: {scriptPath}");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<CharacterSkill>(skillAssetPath) != null)
        {
            Debug.LogError($"스킬 에셋이 이미 존재합니다: {skillAssetPath}");
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

        File.WriteAllText(scriptPath, scriptContent);

        SessionState.SetString(PendingCharacterPathKey, characterAssetPath);
        SessionState.SetString(PendingSkillClassNameKey, className);
        SessionState.SetString(PendingSkillAssetPathKey, skillAssetPath);

        AssetDatabase.Refresh();
    }

    private static void OnCompilationFinished(object context)
    {
        string characterPath = SessionState.GetString(PendingCharacterPathKey, "");
        string className = SessionState.GetString(PendingSkillClassNameKey, "");
        string skillAssetPath = SessionState.GetString(PendingSkillAssetPathKey, "");

        if (string.IsNullOrEmpty(characterPath) ||
            string.IsNullOrEmpty(className) ||
            string.IsNullOrEmpty(skillAssetPath))
        {
            return;
        }

        SessionState.EraseString(PendingCharacterPathKey);
        SessionState.EraseString(PendingSkillClassNameKey);
        SessionState.EraseString(PendingSkillAssetPathKey);

        EditorApplication.delayCall += () =>
        {
            Character character = AssetDatabase.LoadAssetAtPath<Character>(characterPath);

            if (character == null)
            {
                Debug.LogError($"캐릭터 에셋을 찾을 수 없습니다: {characterPath}");
                return;
            }

            Type skillType = TypeCache.GetTypesDerivedFrom<CharacterSkill>().FirstOrDefault(type => type.Name == className && !type.IsAbstract);

            if (skillType == null)
            {
                Debug.LogError($"스킬 클래스를 찾을 수 없습니다: {className}. 스크립트 컴파일 오류를 확인해줘.");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<CharacterSkill>(skillAssetPath) != null)
            {
                Debug.LogError($"스킬 에셋이 이미 존재합니다: {skillAssetPath}");
                return;
            }

            CharacterSkill skill = ScriptableObject.CreateInstance(skillType) as CharacterSkill;

            if (skill == null)
            {
                Debug.LogError($"CharacterSkill 생성에 실패했습니다: {className}");
                return;
            }

            skill.skillOwner = character.id;
            skill.skillName = className;

            AssetDatabase.CreateAsset(skill, skillAssetPath);

            character.skill = skill;

            EditorUtility.SetDirty(character);
            EditorUtility.SetDirty(skill);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"스킬 생성 및 캐릭터 연결 완료: {className}");
        };
    }

    private static string SanitizeName(string value)
    {
        string result = new string(value.Where(character => char.IsLetterOrDigit(character) || character == '_').ToArray());

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