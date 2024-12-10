using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UIInteractiveTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField]
    Color normalColor = Color.white;

    [SerializeField]
    Color highlightedColor = Color.red;

    [SerializeField]
    Color pressedColor = Color.green;

    [SerializeField]
    Color selectedColor = Color.blue;

    private TextMeshProUGUI textComponent;
    private bool isSelected = false;

    void OnEnable()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
        textComponent.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
            textComponent.color = highlightedColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
            textComponent.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        textComponent.color = pressedColor;
    }

    public void OnSelect(BaseEventData eventData)
    {
        textComponent.color = selectedColor;
        isSelected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        textComponent.color = normalColor;
        isSelected = false;
    }
}
