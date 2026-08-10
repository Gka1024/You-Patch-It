public static class DisplayNameHelper
{
    public static string GetStatName(CharacterStatType stat)
    {
        return stat switch
        {
            CharacterStatType.Attack => "공격력",
            CharacterStatType.Health => "체력",
            CharacterStatType.Defence => "방어력",
            CharacterStatType.AttackSpeed => "공격 속도",
            CharacterStatType.MoveSpeed => "이동 속도",
            CharacterStatType.AttackRange => "사거리",
            CharacterStatType.HealthRegen => "10초당 체력 재생",
            CharacterStatType.GainMana => "마나 회복",
            CharacterStatType.MaxMana => "최대 마나",
            CharacterStatType.ManaCost => "사용 마나",
            CharacterStatType.SkillCoefficient => "스킬 계수",
            _ => stat.ToString()
        };
    }

    public static string GetReasonName(PatchReason reason)
    {
        return reason switch
        {
            PatchReason.HighWinrate => "승률이 너무 높음",
            PatchReason.LowWinrate => "승률이 너무 낮음",
            PatchReason.HighPickrate => "픽률이 너무 높음",
            PatchReason.LowPickrate => "픽률이 너무 낮음",
            PatchReason.HighBanrate => "밴률이 너무 높음",
            PatchReason.LowBanrate => "밴률이 너무 낮음",
            PatchReason.MetaDiversity => "메타 다양성 확보",
            PatchReason.UserFeedBack => "유저 피드백",
            PatchReason.InternalTest => "내부 테스트",
            PatchReason.Other => "기타",
            _ => "없음"
        };
    }
}