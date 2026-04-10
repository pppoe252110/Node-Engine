using UniMediator.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

public class NodeDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    private RectTransform _rectTransform;
    private bool _canDrag = false;
    private NodeLogic _nodeLogic;
    private ConnectorDragLogic _connectorDragLogic; // <-- cache this

    private CanvasService _canvasService;
    private IMediator _mediator;

    [Inject]
    public void Construct(CanvasService canvasService, IMediator mediator)
    {
        _canvasService = canvasService;
        _mediator = mediator;
    }

    private void Awake()
    {
        _nodeLogic = GetComponent<NodeLogic>();
        _connectorDragLogic = GetComponent<ConnectorDragLogic>();
    }

    private void Start() => _rectTransform = transform as RectTransform;

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && _canDrag)
            Move(eventData.delta);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _canDrag = eventData.pointerPressRaycast.gameObject == gameObject;
        transform.SetAsLastSibling();

        if (TryGetConnectorFromEvent(eventData, out var connector))
        {
            _connectorDragLogic?.HandleDragStarted(eventData, connector);
            _canDrag = false;
            return;
        }
        if (_canDrag) Move(eventData.delta);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (TryGetConnectorFromEvent(eventData, out _))
        {
            _connectorDragLogic?.HandleDragEnded(eventData);
        }
        else
        {
            _mediator.Publish(new UpdateNodeLogicConnectionsNotification(_nodeLogic));
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (TryGetConnectorFromEvent(eventData, out var connector))
        {
            _connectorDragLogic?.HandleClicked(eventData, connector);
        }
        else
        {
            _mediator.Publish(new UpdateNodeLogicConnectionsNotification(_nodeLogic));
        }
    }

    private bool TryGetConnectorFromEvent(PointerEventData eventData, out Connector connector)
    {
        connector = null;
        if (eventData.pointerPressRaycast.gameObject == null) return false;
        connector = eventData.pointerPressRaycast.gameObject.GetComponentInParent<Connector>();
        return connector != null;
    }

    private void Move(Vector2 delta)
    {
        _rectTransform.anchoredPosition += delta / _canvasService.NodesCanvas.scaleFactor / _canvasService.CanvasSize;
        _mediator.Publish(new UpdateNodeLogicConnectionsNotification(_nodeLogic));
    }
}