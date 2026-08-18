using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton-инвентарь для крафт-ресурсов.
/// Данные хранятся в Dictionary&lt;string,int&gt;.
/// Подписываться на OnInventoryChanged, чтобы обновлять UI.
/// </summary>
public class L0Inventory : MonoBehaviour
{
    public static L0Inventory Instance { get; private set; }

    public System.Action OnInventoryChanged;

    readonly Dictionary<string, int> _items = new Dictionary<string, int>();

    // ── Ensure ──────────────────────────────────────────────────────────────
    public static L0Inventory Ensure()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("L0Inventory");
        return go.AddComponent<L0Inventory>();
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── API ─────────────────────────────────────────────────────────────────
    public bool HasItem(string name, int amount = 1)
    {
        return _items.TryGetValue(name, out int c) && c >= amount;
    }

    public void AddItem(string name, int amount = 1)
    {
        _items.TryGetValue(name, out int c);
        _items[name] = c + amount;
        OnInventoryChanged?.Invoke();
    }

    public bool RemoveItem(string name, int amount = 1)
    {
        if (!HasItem(name, amount)) return false;
        _items[name] -= amount;
        if (_items[name] <= 0) _items.Remove(name);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetCount(string name)
    {
        return _items.TryGetValue(name, out int c) ? c : 0;
    }

    public Dictionary<string, int> GetAll() => _items;
}
