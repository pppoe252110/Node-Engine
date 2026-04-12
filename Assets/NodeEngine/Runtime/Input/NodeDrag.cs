using System.ComponentModel.Design;
using UniMediator.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using VContainer;

public class NodeDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    private bool _canDrag = false;
    private NodeLogic _nodeLogic;
    private ConnectorDragLogic _connectorDragLogic;

    private CanvasService _canvasService;
    private IMediator _mediator;
    private SelectionService _selectionService;

    [Inject]
    public void Construct(CanvasService canvasService, IMediator mediator, SelectionService selectionService)
    {
        _canvasService = canvasService;
        _mediator = mediator;
        _selectionService = selectionService;
    }

    private void Awake()
    {
        _nodeLogic = GetComponent<NodeLogic>();
        _connectorDragLogic = GetComponent<ConnectorDragLogic>();
    }

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

        if (_canDrag)
        {
            if (!_selectionService.IsSelected(_nodeLogic))
            {
                bool isAdditive = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.leftCtrlKey.isPressed;
                _selectionService.Select(_nodeLogic, isAdditive);
            }

            Move(eventData.delta);
        }
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
        else if (!eventData.dragging)
        {
            bool isAdditive = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.leftCtrlKey.isPressed;
            _selectionService.Select(_nodeLogic, isAdditive);

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
        Vector2 adjustedDelta = delta / _canvasService.NodesCanvas.scaleFactor / _canvasService.CanvasSize;

        foreach (var selectedNode in _selectionService.SelectedNodes)
        {
            var rt = selectedNode.transform as RectTransform;
            rt.anchoredPosition += adjustedDelta;
            _mediator.Publish(new UpdateNodeLogicConnectionsNotification(selectedNode));
        }
    }
}