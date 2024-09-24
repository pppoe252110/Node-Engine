using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class NodeDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public UnityEvent<PointerEventData, GameObject> OnBeginDragCallback;
    public UnityEvent<PointerEventData> OnStopDragCallback;

    private RectTransform _rectTransform;
    private bool _canDrag = false;
    
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && _canDrag)
            Move(eventData.delta);
    }

    private void Move(Vector2 delta)
    {
        _rectTransform.anchoredPosition += delta / UIInstance.NodesCanvas.scaleFactor / UIInstance.NodesCanvasSize;
    }

    void Start()
    {
        _rectTransform = transform as RectTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _canDrag = eventData.pointerPressRaycast.gameObject == gameObject;

        transform.SetAsLastSibling();
        OnBeginDragCallback?.Invoke(eventData, eventData.pointerPressRaycast.gameObject);
        
        if (_canDrag)
        {
            Move(eventData.delta);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnStopDragCallback?.Invoke(eventData);
    }
}
