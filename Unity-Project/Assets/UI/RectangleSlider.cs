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
        var rect = value * SliderArea.rect.size;
        ValueArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.x);
        ValueArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.y);
        OnValueChanged?.Invoke(value);
    }

    public Vector2 GetValue()
    {
        var pos = (Vector2)Knob.localPosition + SliderArea.rect.size / 2;
        return pos / SliderArea.rect.size;
    }

    public void SetValue(float width, float height)
    {
        var knobPos = Knob.localPosition;
        knobPos.x = SliderArea.rect.width * (width - 0.5f);
        knobPos.y = SliderArea.rect.height * (height - 0.5f);
        Knob.localPosition = knobPos;
        AdjustValueArea();
    }

    public enum Axis { Horizontal, Vertical };
}