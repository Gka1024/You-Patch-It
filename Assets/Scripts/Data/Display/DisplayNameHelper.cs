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
            CharacterStatType.HealthRegen => "체력 재생",
            CharacterStatType.GainMana => "마나 회복",
            CharacterStatType.MaxMana => "최대 마나",
            CharacterStatType.ManaCost => "사용 마나",
            CharacterStatType.SkillCoefficient => "스킬 계수",
            _ => stat.ToString()
        };
    }

}