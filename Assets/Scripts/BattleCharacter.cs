using System;
using UnityEngine;

public class BattleCharacter : MonoBehaviour
{
    [Header("Identity")]
    public string characterName = "Character";

    [Header("Health")]
    public int maxHP = 100;
    public int currentHP = 100;

    [Header("Combat")]
    public int baseAttackDamage = 10;
    [Range(0f, 1f)] public float baseHitChance = 0.90f;
    [Range(0f, 1f)] public float evasion = 0.05f;

    public event Action<BattleCharacter> OnDeath;

    public bool IsDead()
    {
        return currentHP <= 0 || !gameObject.activeInHierarchy;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead()) return;

        currentHP = Mathf.Max(0, currentHP - Mathf.Max(0, amount));

        if (currentHP == 0)
            OnDeath?.Invoke(this);
    }

    public void Heal(int amount)
    {
        if (IsDead()) return;
        currentHP = Mathf.Min(maxHP, currentHP + Mathf.Max(0, amount));
    }
}
