using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class RectangleSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] RectTransform Knob;
    [SerializeField] RectTransform SliderArea;
    [SerializeField] RectTransform ValueArea;
    event UnityAction<Vector2> OnValueChanged;
    public void AddOnValueChangedListener(UnityAction<Vector2> listener) => OnValueChanged += listener;

    void Start()
    {
        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        pointerDownEntry.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
        trigger.triggers.Add(pointerDownEntry);

        EventTrigger.Entry dragEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Drag
        };
        dragEntry.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
        trigger.triggers.Add(dragEntry);

        AdjustValueArea();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ClampPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ClampPosition(eventData);
    }

    void ClampPosition(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(SliderArea, eventData.position, Camera.main, out Vector2 pos))
        {
            Knob.localPosition = new Vector2(
            Mathf.Clamp(pos.x, -SliderArea.rect.width / 2f, SliderArea.rect.width / 2f),
            Mathf.Clamp(pos.y, -SliderArea.rect.height / 2f, SliderArea.rect.height / 2f));

            AdjustValueArea();
        }
    }

    void AdjustValueArea()
    {
        var value = GetValue();
        OnValueChanged?.Invoke(value);
        var rect = value * SliderArea.rect.size;
        ValueArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.x);
        ValueArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.y);
    }

    public Vector2 GetValue()
    {
        var pos = (Vector2)Knob.localPosition + SliderArea.rect.size / 2;
        return pos / SliderArea.rect.size;
    }

    // ToDo: Implement
    public void SetValue(float value, Axis axis)
    {
        switch(axis)
        {
            case Axis.Horizontal:
                break;

            case Axis.Vertical:
                break;
        }
    }

    public enum Axis { Horizontal, Vertical };
}