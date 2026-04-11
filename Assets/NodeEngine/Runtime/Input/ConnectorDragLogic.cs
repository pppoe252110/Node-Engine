using Radishmouse;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using VContainer;

public class ConnectorDragLogic : MonoBehaviour
{
    private UILineRenderer _dragLineRenderer;
    private Connector _dragConnector;
    private bool _isDragging = false;

    private ConnectionManager _connectionManager;
    private LineRenderersController _lineRenderersController;

    [Inject]
    public void Construct(ConnectionManager connectionManager, LineRenderersController lineRenderersController)
    {
        _connectionManager = connectionManager;
        _lineRenderersController = lineRenderersController;
    }

    private void Update()
    {
        if (_isDragging && _dragLineRenderer != null)
            UpdateDragLine();
    }

    public void HandleDragStarted(PointerEventData eventData, Connector connector)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (connector.IsFlow && connector.ConnectionsCount > 0)
        {
            // Pop connector with tween
            return;
        }

        _dragConnector = connector;
        StartDragConnection();
    }

    // Called by NodeDrag
    public void HandleDragEnded(PointerEventData eventData)
    {
        if (_dragConnector == null || eventData.button != PointerEventData.InputButton.Left) return;
        TryCreateConnection(eventData);
        CleanupDrag();
    }

    // Called by NodeDrag
    public void HandleClicked(PointerEventData eventData, Connector connector)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            ClearConnectorConnections(connector);
    }

    private void StartDragConnection()
    {
        if (_lineRenderersController?.LineRendererPrefab == null)
        {
            Debug.LogError("[ConnectorDragLogic] LineRendererPrefab is not assigned in LineRenderersController!");
            return;
        }

        _dragLineRenderer = Instantiate(_lineRenderersController.LineRendererPrefab, transform);
        _dragLineRenderer.material = CreateLineMaterial(_dragConnector.Color);
        _isDragging = true;
        UpdateDragLine();
    }

    private void UpdateDragLine()
    {
        var startPoint = _dragLineRenderer.rectTransform.InverseTransformPoint(_dragConnector.DragPoint);
        var endPoint = _dragLineRenderer.rectTransform.InverseTransformPoint(Mouse.current.position.value);

        float pixelDistance = Vector2.Distance(startPoint, endPoint);
        int pointsCount = _lineRenderersController.CalculateDynamicPointsCount(pixelDistance);
        float dynamicCurveIntensity = _lineRenderersController.CalculateDynamicCurveIntensity(pixelDistance);

        _dragLineRenderer.points = BezierFromTwoPoints.GetPoints(startPoint, endPoint, dynamicCurveIntensity, pointsCount);
        _dragLineRenderer.SetAllDirty();
    }

    private void TryCreateConnection(PointerEventData eventData)
    {
        foreach (var hoveredObject in eventData.hovered)
        {
            if (hoveredObject.TryGetComponent(out Connector targetConnector))
            {
                if (IsValidConnection(targetConnector))
                {
                    CreateConnection(targetConnector);
                    return;
                }
            }
        }
    }

    private bool IsValidConnection(Connector targetConnector)
    {
        return _dragConnector.Node != targetConnector.Node &&
               targetConnector.ConnectionsCount == 0 &&
               TypeChangeLogic.IsCompatibleType(_dragConnector.ValueType, targetConnector.ValueType);
    }

    private void CreateConnection(Connector targetConnector)
    {
        if (_connectionManager != null)
        {
            _connectionManager.CreateConnectionWithConnectors(_dragConnector, targetConnector);
        }
    }

    private void CleanupDrag()
    {
        if (_dragLineRenderer != null)
        {
            Destroy(_dragLineRenderer.gameObject);
        }
        _dragLineRenderer = null;
        _dragConnector = null;
        _isDragging = false;
    }

    private void ClearConnectorConnections(Connector connector)
    {
        if (connector.ConnectionsCount > 0)
        {
            var connectionsToRemove = connector.Connections.ToArray();
            foreach (var connectedConnector in connectionsToRemove)
            {
                var source = connector.IsInput ? connectedConnector : connector;
                var target = connector.IsInput ? connector : connectedConnector;
                _connectionManager.Disconnect(source, target);
            }
        }
    }

    private Material CreateLineMaterial(Color color)
    {
        var material = new Material(_dragLineRenderer.material);
        material.SetColor("_Color1", color);
        material.SetColor("_Color2", color);
        return material;
    }
}