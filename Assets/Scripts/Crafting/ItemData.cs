using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Crafting/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int    maxStack = 99;
}
