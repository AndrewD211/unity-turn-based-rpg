using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Participants")]
    public BattleCharacter[] heroes;
    public BattleCharacter[] enemies;

    [Header("Systems")]
    public CombatResolver combatResolver;

    [Header("Special Meter")]
    public int meterMax = 100;
    [Range(0f, 1f)] public float specialMinimumFraction = 0.15f;
    public float specialMinimumMultiplier = 1.20f;
    public float specialMaximumMultiplier = 3.00f;

    [Header("Enemy Phase")]
    public float enemyActionDelay = 0.75f;

    private bool battleEnded;
    private bool playerPhase;
    private bool[] heroHasActed;
    private int[] heroMeter;
    private int currentHeroIndex = -1;
    private int selectedEnemyIndex = -1;

    public bool IsPlayerPhase => playerPhase && !battleEnded;
    public bool BattleEnded => battleEnded;
    public int CurrentHeroIndex => currentHeroIndex;
    public int SelectedEnemyIndex => selectedEnemyIndex;

    private void Start()
    {
        heroHasActed = new bool[heroes.Length];
        heroMeter = new int[heroes.Length];

        foreach (BattleCharacter hero in heroes)
        {
            if (hero != null)
                hero.OnDeath += HandleCharacterDeath;
        }

        foreach (BattleCharacter enemy in enemies)
        {
            if (enemy != null)
                enemy.OnDeath += HandleCharacterDeath;
        }

        BeginPlayerRound();
    }

    public bool SelectEnemy(int index)
    {
        if (!IsPlayerPhase || index < 0 || index >= enemies.Length)
            return false;

        BattleCharacter enemy = enemies[index];
        if (enemy == null || enemy.IsDead())
            return false;

        selectedEnemyIndex = index;
        return true;
    }

    public AttackOutcome AttackSelectedEnemy()
    {
        if (!IsPlayerPhase)
            return default;

        BattleCharacter hero = GetCurrentHero();
        BattleCharacter enemy = GetSelectedEnemy();
        if (hero == null || enemy == null)
            return default;

        AttackOutcome outcome = combatResolver.ResolveNormalAttack(hero, enemy, awardMeter: true);

        if (outcome.Hit)
        {
            enemy.TakeDamage(outcome.Damage);
            GainMeter(currentHeroIndex, outcome.MeterGain);
        }

        FinishPlayerAction();
        return outcome;
    }

    public bool UseHealingItem(int targetHeroIndex)
    {
        if (!IsPlayerPhase || Inventory.Instance == null)
            return false;

        if (targetHeroIndex < 0 || targetHeroIndex >= heroes.Length)
            return false;

        BattleCharacter target = heroes[targetHeroIndex];
        if (target == null || target.IsDead())
            return false;

        if (!Inventory.Instance.UseItem(ItemEffects.HealingItem))
            return false;

        if (!ItemEffects.Apply(ItemEffects.HealingItem, target))
            return false;

        FinishPlayerAction();
        return true;
    }

    public bool UseSpecial()
    {
        if (!IsPlayerPhase || !CanUseSpecial(currentHeroIndex))
            return false;

        BattleCharacter hero = GetCurrentHero();
        BattleCharacter enemy = GetSelectedEnemy();
        if (hero == null || enemy == null)
            return false;

        float meterFraction = (float)heroMeter[currentHeroIndex] / meterMax;
        float t = Mathf.InverseLerp(specialMinimumFraction, 1f, meterFraction);
        float multiplier = Mathf.Lerp(specialMinimumMultiplier, specialMaximumMultiplier, t);
        int damage = Mathf.Max(1, Mathf.RoundToInt(hero.baseAttackDamage * multiplier));

        enemy.TakeDamage(damage);
        heroMeter[currentHeroIndex] = 0;

        FinishPlayerAction();
        return true;
    }

    public bool CanUseSpecial(int heroIndex)
    {
        if (heroIndex < 0 || heroIndex >= heroMeter.Length)
            return false;

        int required = Mathf.CeilToInt(meterMax * specialMinimumFraction);
        return heroMeter[heroIndex] >= required;
    }

    public int GetMeter(int heroIndex)
    {
        if (heroIndex < 0 || heroIndex >= heroMeter.Length)
            return 0;

        return heroMeter[heroIndex];
    }

    private void BeginPlayerRound()
    {
        if (CheckBattleEnd())
            return;

        playerPhase = true;

        for (int i = 0; i < heroHasActed.Length; i++)
            heroHasActed[i] = false;

        currentHeroIndex = GetNextLivingHero(-1);
        selectedEnemyIndex = GetFirstLivingEnemy();
    }

    private void FinishPlayerAction()
    {
        if (CheckBattleEnd())
            return;

        heroHasActed[currentHeroIndex] = true;

        int nextHero = GetNextLivingHero(currentHeroIndex);
        if (nextHero >= 0)
        {
            currentHeroIndex = nextHero;
            selectedEnemyIndex = GetFirstLivingEnemy();
            return;
        }

        StartCoroutine(RunEnemyPhase());
    }

    private IEnumerator RunEnemyPhase()
    {
        playerPhase = false;
        currentHeroIndex = -1;

        foreach (BattleCharacter enemy in enemies)
        {
            if (enemy == null || enemy.IsDead())
                continue;

            yield return new WaitForSeconds(enemyActionDelay);

            BattleCharacter target = GetRandomLivingHero();
            if (target == null)
                break;

            AttackOutcome outcome = combatResolver.ResolveNormalAttack(enemy, target, awardMeter: false);
            if (outcome.Hit)
                target.TakeDamage(outcome.Damage);

            if (CheckBattleEnd())
                yield break;
        }

        BeginPlayerRound();
    }

    private void GainMeter(int heroIndex, int amount)
    {
        if (heroIndex < 0 || heroIndex >= heroMeter.Length)
            return;

        heroMeter[heroIndex] = Mathf.Clamp(heroMeter[heroIndex] + amount, 0, meterMax);
    }

    private int GetNextLivingHero(int afterIndex)
    {
        for (int i = afterIndex + 1; i < heroes.Length; i++)
        {
            if (heroes[i] != null && !heroes[i].IsDead() && !heroHasActed[i])
                return i;
        }

        return -1;
    }

    private int GetFirstLivingEnemy()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && !enemies[i].IsDead())
                return i;
        }

        return -1;
    }

    private BattleCharacter GetCurrentHero()
    {
        if (currentHeroIndex < 0 || currentHeroIndex >= heroes.Length)
            return null;

        BattleCharacter hero = heroes[currentHeroIndex];
        return hero != null && !hero.IsDead() ? hero : null;
    }

    private BattleCharacter GetSelectedEnemy()
    {
        if (selectedEnemyIndex < 0 || selectedEnemyIndex >= enemies.Length)
            return null;

        BattleCharacter enemy = enemies[selectedEnemyIndex];
        return enemy != null && !enemy.IsDead() ? enemy : null;
    }

    private BattleCharacter GetRandomLivingHero()
    {
        List<BattleCharacter> livingHeroes = new List<BattleCharacter>();

        foreach (BattleCharacter hero in heroes)
        {
            if (hero != null && !hero.IsDead())
                livingHeroes.Add(hero);
        }

        if (livingHeroes.Count == 0)
            return null;

        return livingHeroes[Random.Range(0, livingHeroes.Count)];
    }

    private void HandleCharacterDeath(BattleCharacter character)
    {
        CheckBattleEnd();

        if (!battleEnded && character != null && selectedEnemyIndex >= 0 &&
            selectedEnemyIndex < enemies.Length && enemies[selectedEnemyIndex] == character)
        {
            selectedEnemyIndex = GetFirstLivingEnemy();
        }
    }

    private bool CheckBattleEnd()
    {
        bool heroesAlive = AnyLiving(heroes);
        bool enemiesAlive = AnyLiving(enemies);

        if (!heroesAlive || !enemiesAlive)
        {
            battleEnded = true;
            playerPhase = false;
            return true;
        }

        return false;
    }

    private static bool AnyLiving(BattleCharacter[] characters)
    {
        foreach (BattleCharacter character in characters)
        {
            if (character != null && !character.IsDead())
                return true;
        }

        return false;
    }
}
