using NUnit.Framework;
using Radishmouse;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

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
    }


    private void OnDisable()
    {
        _nodeDrag.OnBeginDragCallback.RemoveListener(OnNodeDrag);
        _nodeDrag.OnStopDragCallback.RemoveListener(OnNodeStopDrag);
    }

    private void Update()
    {
        if (_dragLineRenderer)
        {
            _dragLineRenderer.points = BezierFromTwoPoints.GetPoints(
                _dragLineRenderer.rectTransform.InverseTransformPoint(_dragConnector.DragPoint),
                _dragLineRenderer.rectTransform.InverseTransformPoint(Input.mousePosition), 0.5f, 10);
            _dragLineRenderer.SetAllDirty();
        }
    }

    private void OnNodeDrag(PointerEventData eventData, GameObject go)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        if(go.transform.parent.TryGetComponent(out _dragConnector))
        {
            _dragLineRenderer = Instantiate(_lineRendererPrefab, transform);
            _dragLineRenderer.material.SetColor("_Color1", _dragConnector.Color);
            _dragLineRenderer.material.SetColor("_Color2", _dragConnector.Color);
            _dragLineRenderer.points = new Vector2[] {
                _dragLineRenderer.rectTransform.InverseTransformPoint(_dragConnector.DragPoint),
                _dragLineRenderer.rectTransform.InverseTransformPoint(Input.mousePosition) };
        }
    }
    
    private void OnNodeStopDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            foreach (var item in eventData.hovered)
            {
                if(item.TryGetComponent(out Connector connector))
                {
                    if (_dragConnector.Node != connector.Node)
                    {
                        if(_dragConnector.ValueType == connector.ValueType || connector.ValueType == typeof(object))
                        {
                            LineRenderersController.Add(_dragConnector, connector, _dragLineRenderer);
                            _dragLineRenderer = null;
                            _dragConnector.SetConnectorFilled(true);
                            connector.SetConnectorFilled(true);
                            return;
                        }
                    }
                }
            }
        }

        if(_dragLineRenderer)
            Destroy(_dragLineRenderer.gameObject);
    }
}
