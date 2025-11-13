using UnityEngine;
using UnityEngine.UI;

public class DragDropHandler : MonoBehaviour
{
    public static DragDropHandler Instance;

    public Image dragIcon;
    public ItemData DraggedItem { get; private set; }
    public bool IsDragging => DraggedItem != null;

    private ItemSlot originSlot;

    void Awake()
    {
        Instance = this;
        dragIcon.gameObject.SetActive(false);
    }

    public void StartDrag(ItemSlot fromSlot, ItemData item, Sprite icon)
    {
        originSlot = fromSlot;
        DraggedItem = item;
        dragIcon.sprite = icon;
        dragIcon.gameObject.SetActive(true);
    }

    public void UpdateDragPosition(UnityEngine.EventSystems.PointerEventData eventData)
    {
        dragIcon.transform.position = eventData.position;
    }

    public void EndDrag()
    {
        // 드롭 실패 → 원래 슬롯으로 복귀
        if (DraggedItem != null && originSlot != null)
            originSlot.SetItem(DraggedItem);

        ClearDrag();
    }

    public void ClearDrag()
    {
        DraggedItem = null;
        originSlot = null;
        dragIcon.gameObject.SetActive(false);
    }
}
