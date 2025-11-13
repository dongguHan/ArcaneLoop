using UnityEngine;

public enum ItemType
{
    Weapon,
    Armor,
    Accessory
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
}
