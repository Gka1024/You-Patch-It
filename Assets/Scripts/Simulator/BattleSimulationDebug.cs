using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BattleSimulationDebug : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private Character redCharacter;
    [SerializeField] private Character blueCharacter;

    [Header("Red Player")]
    [SerializeField] private PlayerProfile redProfile;
    [SerializeField] private TierSetting redTier;

    [Header("Blue Player")]
    [SerializeField] private PlayerProfile blueProfile;
    [SerializeField] private TierSetting blueTier;

    [Header("Seeds")]
    [SerializeField] private int redPlayerSeed = 100;
    [SerializeField] private int bluePlayerSeed = 200;
    [SerializeField] private int battleSeed = 300;

    [ContextMenu("Run Test")]
    public void RunTest()
    {
        if (!ValidateInputs())
            return;

        if (BattleSimulator.Instance == null)
        {
            Debug.LogError("[Battle Test] BattleSimulator.Instance가 없습니다. 씬에 BattleSimulator가 있는지 확인해 주세요.", this);
            return;
        }

        RuntimeCharacter runtimeRedCharacter = new RuntimeCharacter(redCharacter);
        RuntimeCharacter runtimeBlueCharacter = new RuntimeCharacter(blueCharacter);

        RuntimePlayer runtimeRedPlayer = new RuntimePlayer(redProfile, redTier, new System.Random(redPlayerSeed));
        RuntimePlayer runtimeBluePlayer = new RuntimePlayer(blueProfile, blueTier, new System.Random(bluePlayerSeed));

        Debug.Log(
            $"[Battle Test] 시작\n" +
            $"Red: {redCharacter.characterName} / Player Seed: {redPlayerSeed}\n" +
            $"Blue: {blueCharacter.characterName} / Player Seed: {bluePlayerSeed}\n" +
            $"Battle Seed: {battleSeed}",
            this);

        try
        {
            BattleResult result = BattleSimulator.Instance.Simulate(
                runtimeRedCharacter,
                runtimeRedPlayer,
                runtimeBlueCharacter,
                runtimeBluePlayer,
                battleSeed);

            LogResult(result);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[Battle Test] 시뮬레이션 중 예외 발생\n{exception}", this);
        }
    }

    private bool ValidateInputs()
    {
        if (redCharacter == null || blueCharacter == null)
        {
            Debug.LogError("[Battle Test] Red/Blue 캐릭터를 모두 지정해 주세요.", this);
            return false;
        }

        if (redProfile == null || redTier == null || blueProfile == null || blueTier == null)
        {
            Debug.LogError("[Battle Test] 양쪽 플레이어의 Profile과 TierSetting을 모두 지정해 주세요.", this);
            return false;
        }

        return true;
    }

    private void LogResult(BattleResult result)
    {
        if (result == null)
        {
            Debug.LogError("[Battle Test] BattleSimulator가 null 결과를 반환했습니다.", this);
            return;
        }

        string outcome;

        if (result.isDraw)
        {
            outcome = "무승부";
        }
        else
        {
            outcome = $"승리: {FormatCharacters(result.winner)}";
        }

        StringBuilder log = new StringBuilder();
        log.AppendLine("[Battle Test] 완료");
        log.AppendLine($"Red: {redCharacter.characterName}");
        log.AppendLine($"Blue: {blueCharacter.characterName}");
        log.AppendLine($"Battle Seed: {battleSeed}");
        log.AppendLine($"결과: {outcome}");
        log.AppendLine($"전투 시간: {result.battleTime:F2}초");

        if (result.statistics != null)
        {
            log.AppendLine($"통계 전투 시간: {result.statistics.battleDuration:F2}초");
            log.AppendLine($"Red 통계 캐릭터 수: {result.statistics.Red.Count}");
            log.AppendLine($"Blue 통계 캐릭터 수: {result.statistics.Blue.Count}");
        }

        Debug.Log(log.ToString(), this);
    } 

    private string FormatCharacters(List<RuntimeCharacter> characters)
    {
        if (characters == null || characters.Count == 0)
            return "(없음)";

        List<string> names = new List<string>();

        foreach (RuntimeCharacter character in characters)
        {
            names.Add(character?.OriginCharacter != null
                ? character.OriginCharacter.characterName
                : "(알 수 없는 캐릭터)");
        }

        return string.Join(", ", names);
    }
}