using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IDropHandler
{
    public ItemType slotType;
    public Image iconImage;
    public ItemData equippedItem;

    public void SetItem(ItemData item)
    {
        equippedItem = item;
        iconImage.sprite = item != null ? item.icon : null;
        iconImage.enabled = item != null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!DragDropHandler.Instance.IsDragging) return;

        ItemData item = DragDropHandler.Instance.DraggedItem;

        if (item.itemType == slotType)
        {
            SetItem(item);
            DragDropHandler.Instance.ClearDrag();
        }
        else
        {
            Debug.Log($" {item.itemType}은(는) {slotType} 칸에 장착할 수 없습니다.");
        }
    }
}
