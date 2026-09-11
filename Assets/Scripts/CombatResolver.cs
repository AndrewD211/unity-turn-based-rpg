using UnityEngine;

public enum HitQuality
{
    Miss,
    Graze,
    Normal,
    Strong,
    Perfect
}

public readonly struct AttackOutcome
{
    public bool Hit { get; }
    public HitQuality Quality { get; }
    public int Damage { get; }
    public bool IsPerfect { get; }
    public int MeterGain { get; }

    public AttackOutcome(bool hit, HitQuality quality, int damage, bool isPerfect, int meterGain)
    {
        Hit = hit;
        Quality = quality;
        Damage = damage;
        IsPerfect = isPerfect;
        MeterGain = meterGain;
    }
}

public class CombatResolver : MonoBehaviour
{
    [Header("Damage Tuning")]
    public int attackVariance = 2;
    public float grazeMultiplier = 0.75f;
    public float normalMultiplier = 1.00f;
    public float strongMultiplier = 1.25f;
    public float perfectMultiplier = 1.50f;

    [Header("Special Meter Gain")]
    public int grazeMeterGain = 5;
    public int normalMeterGain = 10;
    public int strongMeterGain = 20;
    public int perfectMeterGain = 30;

    public AttackOutcome ResolveNormalAttack(
        BattleCharacter attacker,
        BattleCharacter defender,
        bool awardMeter)
    {
        if (attacker == null || defender == null || attacker.IsDead() || defender.IsDead())
            return Miss();

        float netHitChance = Mathf.Clamp01(attacker.baseHitChance - defender.evasion);
        if (Random.value > netHitChance)
            return Miss();

        float qualityRoll = Random.value;
        HitQuality quality;
        float damageMultiplier;
        int meterGain;

        if (qualityRoll < 0.20f)
        {
            quality = HitQuality.Graze;
            damageMultiplier = grazeMultiplier;
            meterGain = grazeMeterGain;
        }
        else if (qualityRoll < 0.70f)
        {
            quality = HitQuality.Normal;
            damageMultiplier = normalMultiplier;
            meterGain = normalMeterGain;
        }
        else if (qualityRoll < 0.95f)
        {
            quality = HitQuality.Strong;
            damageMultiplier = strongMultiplier;
            meterGain = strongMeterGain;
        }
        else
        {
            quality = HitQuality.Perfect;
            damageMultiplier = perfectMultiplier;
            meterGain = perfectMeterGain;
        }

        int variance = Random.Range(-attackVariance, attackVariance + 1);
        int baseDamage = Mathf.Max(1, attacker.baseAttackDamage + variance);
        int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * damageMultiplier));

        return new AttackOutcome(
            true,
            quality,
            damage,
            quality == HitQuality.Perfect,
            awardMeter ? meterGain : 0);
    }

    private static AttackOutcome Miss()
    {
        return new AttackOutcome(false, HitQuality.Miss, 0, false, 0);
    }
}
