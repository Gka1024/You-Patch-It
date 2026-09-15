using System.IO;
using UnityEditor;
using UnityEngine;

public class CharacterCreatorWindow : EditorWindow
{
    private const string CharacterRootFolder = "Assets/Data/Characters";

    private int characterId;
    private string characterName = "NewCharacter";
    private CharacterRole characterRole;

    private CharacterDatabase characterDatabase;
    private CharacterAIDatabase characterAIDatabase;

    [MenuItem("Tools/Character Creator")]
    private static void OpenWindow()
    {
        GetWindow<CharacterCreatorWindow>("Character Creator");
    }

    private void OnEnable()
    {
        FindDatabases();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Character", EditorStyles.boldLabel);

        characterId = EditorGUILayout.IntField("ID", characterId);
        characterName = EditorGUILayout.TextField("Name", characterName);
        characterRole = (CharacterRole)EditorGUILayout.EnumPopup("Role", characterRole);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Databases", EditorStyles.boldLabel);
        characterDatabase = (CharacterDatabase)EditorGUILayout.ObjectField("Character Database", characterDatabase, typeof(CharacterDatabase), false);
        characterAIDatabase = (CharacterAIDatabase)EditorGUILayout.ObjectField("Character AI Database", characterAIDatabase, typeof(CharacterAIDatabase), false);

        if (GUILayout.Button("Find Databases"))
        {
            FindDatabases();
        }

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(!CanCreateCharacter()))
        {
            if (GUILayout.Button("Create Character", GUILayout.Height(32)))
            {
                CreateCharacter();
            }
        }

        if (string.IsNullOrWhiteSpace(characterName))
        {
            EditorGUILayout.HelpBox("캐릭터 이름을 입력해줘.", MessageType.Warning);
        }
    }

    private bool CanCreateCharacter()
    {
        return characterId >= 0
            && !string.IsNullOrWhiteSpace(characterName)
            && characterDatabase != null
            && characterAIDatabase != null;
    }

    private void FindDatabases()
    {
        characterDatabase = FindSingleAsset<CharacterDatabase>();
        characterAIDatabase = FindSingleAsset<CharacterAIDatabase>();
    }

    private static T FindSingleAsset<T>() where T : Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

        if (guids.Length != 1)
        {
            Debug.LogWarning($"{typeof(T).Name} 에셋은 정확히 하나 있어야 합니다. 현재 개수: {guids.Length}");
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private void CreateCharacter()
    {
        string safeName = SanitizeName(characterName);

        if (string.IsNullOrEmpty(safeName))
        {
            Debug.LogError("캐릭터 이름에서 유효한 파일명을 만들 수 없습니다.");
            return;
        }

        if (characterDatabase.GetCharacter(characterId) != null)
        {
            Debug.LogError($"이미 사용 중인 캐릭터 ID입니다: {characterId}");
            return;
        }

        BattleAI selectedAI = characterAIDatabase.GetAI(characterRole);

        if (selectedAI == null)
        {
            Debug.LogError($"해당 역할의 BattleAI를 찾을 수 없습니다: {characterRole}");
            return;
        }

        if (!AssetDatabase.IsValidFolder(CharacterRootFolder))
        {
            Directory.CreateDirectory(CharacterRootFolder);
            AssetDatabase.Refresh();
        }

        string characterFolderName = $"{characterId:D4}_{safeName}";
        string characterFolderPath = $"{CharacterRootFolder}/{characterFolderName}";

        if (AssetDatabase.IsValidFolder(characterFolderPath))
        {
            Debug.LogError($"캐릭터 폴더가 이미 존재합니다: {characterFolderPath}");
            return;
        }

        AssetDatabase.CreateFolder(CharacterRootFolder, characterFolderName);

        string characterAssetPath = $"{characterFolderPath}/{safeName}.asset";

        Character character = CreateInstance<Character>();
        character.id = characterId;
        character.characterName = characterName.Trim();
        character.role = characterRole;
        character.battleAI = selectedAI;

        AssetDatabase.CreateAsset(character, characterAssetPath);

        if (!characterDatabase.EditorAddCharacter(character))
        {
            AssetDatabase.DeleteAsset(characterFolderPath);
            Debug.LogError($"캐릭터 데이터베이스 등록에 실패했습니다. ID: {characterId}");
            return;
        }

        CharacterSkillAutoCreator.CreateSkillClassAndConnect(character, characterAssetPath, character.characterName, characterFolderPath);

        EditorUtility.SetDirty(characterDatabase);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = character;
        EditorGUIUtility.PingObject(character);

        Debug.Log($"캐릭터 에셋과 스킬 클래스 생성 요청 완료: {character.characterName}");
    }

    private static string SanitizeName(string value)
    {
        string result = "";

        foreach (char character in value.Trim())
        {
            if (char.IsLetterOrDigit(character) || character == '_')
            {
                result += character;
            }
        }

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