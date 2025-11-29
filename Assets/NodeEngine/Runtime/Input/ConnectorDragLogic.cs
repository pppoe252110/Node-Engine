using Radishmouse;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ConnectorDragLogic : MonoBehaviour
{
    [SerializeField] private NodeBase _node;
    [SerializeField] private NodeDrag _nodeDrag;

    private UILineRenderer _dragLineRenderer;
    private Connector _dragConnector;
    private bool _isDragging = false;

    private void OnEnable()
    {
        _nodeDrag.OnBeginDragCallback.AddListener(OnNodeDrag);
        _nodeDrag.OnStopDragCallback.AddListener(OnNodeStopDrag);
        _nodeDrag.OnClickCallback.AddListener(OnNodeClick);
    }

    private void OnDisable()
    {
        _nodeDrag.OnBeginDragCallback.RemoveListener(OnNodeDrag);
        _nodeDrag.OnStopDragCallback.RemoveListener(OnNodeStopDrag);
        _nodeDrag.OnClickCallback.RemoveListener(OnNodeClick);
    }

    private void Update()
    {
        if (_isDragging && _dragLineRenderer != null)
        {
            UpdateDragLine();
        }
    }

    private void UpdateDragLine()
    {
        if (LineRenderersController.Instance == null)
        {
            Debug.LogError("LineRenderersController instance not found!");
            return;
        }

        var startPoint = _dragLineRenderer.rectTransform.InverseTransformPoint(_dragConnector.DragPoint);
        var endPoint = _dragLineRenderer.rectTransform.InverseTransformPoint(Mouse.current.position.value);

        float pixelDistance = Vector2.Distance(startPoint, endPoint);

        int pointsCount = LineRenderersController.Instance.CalculateDynamicPointsCount(pixelDistance);
        float dynamicCurveIntensity = LineRenderersController.Instance.CalculateDynamicCurveIntensity(pixelDistance);

        _dragLineRenderer.points = BezierFromTwoPoints.GetPoints(startPoint, endPoint, dynamicCurveIntensity, pointsCount);
        _dragLineRenderer.SetAllDirty();
    }
    private void OnNodeClick(PointerEventData eventData, GameObject clickedObject)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            HandleRightClick(clickedObject);
        }
    }

    private void HandleRightClick(GameObject clickedObject)
    {
        if (clickedObject.transform.parent.TryGetComponent(out Connector connector))
        {
            ClearConnectorConnections(connector);
        }
    }

    private void ClearConnectorConnections(Connector connector)
    {
        if (connector.ConnectionsCount > 0)
        {
            foreach (var connectedConnector in connector.Connections)
            {
                LineRenderersController.Remove(connector, connectedConnector);
                connectedConnector.Connections.Remove(connector);
                connectedConnector.UpdateFilled();
            }
            connector.Connections.Clear();
            connector.UpdateFilled();
        }
    }

    private void OnNodeDrag(PointerEventData eventData, GameObject draggedObject)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (draggedObject.transform.parent.TryGetComponent(out _dragConnector))
        {
            StartDragConnection();
        }
    }

    private void StartDragConnection()
    {
        _dragLineRenderer = Instantiate(LineRenderersController.Instance.LineRendererPrefab, transform);
        _dragLineRenderer.material = CreateLineMaterial(_dragConnector.Color);
        _isDragging = true;

        UpdateDragLine();
    }

    private Material CreateLineMaterial(Color color)
    {
        var material = new Material(_dragLineRenderer.material);
        material.SetColor("_Color1", color);
        material.SetColor("_Color2", color);
        return material;
    }

    private void OnNodeStopDrag(PointerEventData eventData)
    {
        if (_dragConnector == null || eventData.button != PointerEventData.InputButton.Left) return;

        TryCreateConnection(eventData);
        CleanupDrag();
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
               IsCompatibleType(_dragConnector.ValueType, targetConnector.ValueType);
    }


    private void CreateConnection(Connector targetConnector)
    {
        LineRenderersController.Add(_dragConnector, targetConnector, _dragLineRenderer);
        _dragLineRenderer = null;

        _dragConnector.AddConnection(targetConnector);
        targetConnector.AddConnection(_dragConnector);

        _dragConnector.UpdateFilled();
        targetConnector.UpdateFilled();


        if (ConnectionManager.Instance != null)
        {
            var fromAttr = _dragConnector.Field?.GetAttribute();
            var toAttr = targetConnector.Field?.GetAttribute();

            if (fromAttr != null && toAttr != null)
            {
                ConnectionManager.Instance.CreateConnection(
                    _dragConnector.Node.Guid,
                    targetConnector.Node.Guid,
                    fromAttr.attributeName,
                    toAttr.attributeName
                );
            }
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

    private bool IsCompatibleType(Type dragType, Type targetType)
    {
        return dragType == targetType || targetType == typeof(object);
    }
}