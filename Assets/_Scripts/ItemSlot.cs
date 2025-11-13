using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image iconImage;
    public ItemData currentItem;

    public void SetItem(ItemData item)
    {
        currentItem = item;
        iconImage.sprite = item != null ? item.icon : null;
        iconImage.enabled = item != null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        DragDropHandler.Instance.StartDrag(this, currentItem, iconImage.sprite);
        SetItem(null);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (DragDropHandler.Instance.IsDragging)
            DragDropHandler.Instance.UpdateDragPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DragDropHandler.Instance.IsDragging)
            DragDropHandler.Instance.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (DragDropHandler.Instance.IsDragging)
        {
            // 다른 슬롯으로부터 아이템이 떨어졌을 때
            SetItem(DragDropHandler.Instance.DraggedItem);
            DragDropHandler.Instance.ClearDrag();
        }
    }
}
