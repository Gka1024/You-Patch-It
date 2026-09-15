using System;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

[InitializeOnLoad]
public static class CharacterSkillAutoCreator
{
    private const string PendingCharacterPathKey = "PendingCharacterPath";
    private const string PendingSkillClassNameKey = "PendingSkillClassName";
    private const string PendingSkillAssetPathKey = "PendingSkillAssetPath";

    static CharacterSkillAutoCreator()
    {
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
        string scriptPath = $"{skillFolderPath}/{className}.cs";
        string skillAssetPath = $"{skillFolderPath}/{className}.asset";

        if (File.Exists(scriptPath) || AssetDatabase.LoadAssetAtPath<CharacterSkill>(skillAssetPath) != null)
        {
            Debug.LogError($"스킬 파일 또는 에셋이 이미 존재합니다: {className}");
            return;
        }

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

            Type skillType = AppDomain.CurrentDomain
         .GetAssemblies()
         .SelectMany(GetLoadableTypes)
         .FirstOrDefault(type =>
             type.Name == className &&
             !type.IsAbstract &&
             typeof(CharacterSkill).IsAssignableFrom(type));

            if (skillType == null)
            {
                Debug.LogError($"스킬 클래스를 찾을 수 없습니다: {className}");
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
        string result = new string(value.Where(char.IsLetterOrDigit).ToArray());

        if (string.IsNullOrEmpty(result))
        {
            return "NewSkill";
        }

        if (char.IsDigit(result[0]))
        {
            result = $"_{result}";
        }

        return result;
    }

    private static Type[] GetLoadableTypes(System.Reflection.Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (System.Reflection.ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type != null).ToArray();
        }
    }
}