using NUnit.Framework;
using Radishmouse;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ConnectorDragLogic : MonoBehaviour
{
    [SerializeField] private NodeBase _node;
    [SerializeField] private NodeDrag _nodeDrag;
    [SerializeField] private UILineRenderer _lineRendererPrefab;

    private UILineRenderer _dragLineRenderer;
    private Connector _dragConnector;

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
        if (_dragLineRenderer)
        {
            _dragLineRenderer.points = BezierFromTwoPoints.GetPoints(
                _dragLineRenderer.rectTransform.InverseTransformPoint(_dragConnector.DragPoint),
                _dragLineRenderer.rectTransform.InverseTransformPoint(Mouse.current.position.value), 0.5f, 10);
            _dragLineRenderer.SetAllDirty();
        }
    }

    private void OnNodeClick(PointerEventData eventData, GameObject go)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (go.transform.parent.TryGetComponent(out Connector clickConnector))
            {
                if (clickConnector.ConnectionsCount > 0)
                    foreach (var dragConnections in clickConnector.Connections)
                    {
                        LineRenderersController.Remove(clickConnector, dragConnections);
                        dragConnections.Connections.Remove(clickConnector);
                        dragConnections.UpdateFilled();
                    }
                clickConnector.Connections.Clear();
                clickConnector.UpdateFilled();
            }
        }
    }

    private void OnNodeDrag(PointerEventData eventData, GameObject go)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        if (go.transform.parent.TryGetComponent(out _dragConnector))
        {
            _dragLineRenderer = Instantiate(_lineRendererPrefab, transform);
            _dragLineRenderer.material.SetColor("_Color1", _dragConnector.Color);
            _dragLineRenderer.material.SetColor("_Color2", _dragConnector.Color);
            _dragLineRenderer.points = new Vector2[] {
                _dragLineRenderer.rectTransform.InverseTransformPoint(_dragConnector.DragPoint),
                _dragLineRenderer.rectTransform.InverseTransformPoint(Mouse.current.position.value) };
        }
    }

    private void OnNodeStopDrag(PointerEventData eventData)
    {
        if (_dragConnector == null || eventData.button != PointerEventData.InputButton.Left) return;
        foreach (var item in eventData.hovered)
        {
            if (item.TryGetComponent(out Connector connector))
            {
                if (_dragConnector.Node == connector.Node) continue; // Prevent self-connection
                if (connector.ConnectionsCount > 0) continue; // Only allow one connection per input
                                                              // Strict type check
                if (!IsCompatibleType(_dragConnector.ValueType, connector.ValueType))
                {
                    Debug.LogWarning($"Incompatible types: {_dragConnector.ValueType} to {connector.ValueType}");
                    continue;
                }
                // Create connection
                LineRenderersController.Add(_dragConnector, connector, _dragLineRenderer);
                _dragLineRenderer = null;
                _dragConnector.AddConnection(connector);
                _dragConnector.UpdateFilled();
                connector.AddConnection(_dragConnector);
                connector.UpdateFilled();
                return;
            }
        }
        if (_dragLineRenderer) Destroy(_dragLineRenderer.gameObject);
    }
    private bool IsCompatibleType(Type dragType, Type targetType)
    {
        return dragType == targetType || targetType == typeof(object);
    }
}
