using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderEndEdit : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public UnityAction<float> OnSliderEditEnd;

    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (!TryGetComponent<EventTrigger>(out _))
        {
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            SetupEventTriggers(eventTrigger);
        }
    }

    private void SetupEventTriggers(EventTrigger eventTrigger)
    {
        var pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => { OnSliderPointerUp(); });
        eventTrigger.triggers.Add(pointerUpEntry);
    }

    public void OnSliderPointerUp()
    {
        OnSliderEditEnd?.Invoke(slider.value);
    }
}