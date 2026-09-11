using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    private readonly Dictionary<string, int> items = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddItem(string itemName, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemName) || amount <= 0)
            return;

        if (!items.ContainsKey(itemName))
            items[itemName] = 0;

        items[itemName] += amount;
    }

    public bool UseItem(string itemName, int amount = 1)
    {
        if (amount <= 0 || !items.TryGetValue(itemName, out int count) || count < amount)
            return false;

        items[itemName] -= amount;
        return true;
    }

    public int GetAmount(string itemName)
    {
        return items.TryGetValue(itemName, out int count) ? count : 0;
    }

    public IReadOnlyDictionary<string, int> GetAllItems()
    {
        return items;
    }
}

public static class ItemEffects
{
    public const string HealingItem = "Health Pack";

    public static bool Apply(string itemName, BattleCharacter target)
    {
        if (target == null || target.IsDead())
            return false;

        switch (itemName)
        {
            case HealingItem:
                target.Heal(25);
                return true;

            default:
                return false;
        }
    }
}
