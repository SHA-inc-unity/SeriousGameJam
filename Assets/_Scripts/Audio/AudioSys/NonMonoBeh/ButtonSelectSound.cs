using UnityEngine;
using UnityEngine.EventSystems;


public class ButtonSelectSound : MonoBehaviour, IPointerEnterHandler
{
    private System.Action onSelect;

    public void Init(System.Action callback)
    {
        onSelect = callback;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onSelect != null) onSelect();
    }
}
