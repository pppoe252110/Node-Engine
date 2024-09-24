using Radishmouse;
using UnityEngine;
using System.Collections.Generic;

public class LineRenderersController : MonoBehaviour
{
    private static LineRenderersController instance 
    {
        get
        {
            return _instance ?? (_instance = FindFirstObjectByType<LineRenderersController>());
        }
    }
    private static LineRenderersController _instance;

    [SerializeField] private RectTransform _inheritTransform;

    private List<(Connector, Connector, UILineRenderer)> connectors = new List<(Connector, Connector, UILineRenderer)>();

    private void Awake()
    {
        _instance = this;
    }

    private void LateUpdate()
    {
        UpdateLineRenderers();
    }

    public static void Add(Connector connectorA, Connector connectorB, UILineRenderer lineRenderer)
    {
        lineRenderer.transform.parent = instance.transform;
        lineRenderer.material = new Material(lineRenderer.material);
        lineRenderer.material.SetColor("_Color1", connectorA.Color);
        lineRenderer.material.SetColor("_Color2", connectorB.Color);
        instance.connectors.Add((connectorA, connectorB, lineRenderer));
    }

    private void UpdateLineRenderers()
    {
        transform.position = _inheritTransform.position;
        transform.localScale = _inheritTransform.localScale;

        foreach (var item in connectors)
        {
            if(item.Item1==null|| item.Item2 == null)
            {
                connectors.Remove(item);
                UpdateLineRenderers();
                return;
            }
            item.Item3.material.SetVector("_Point1", new Vector2(item.Item1.AnchoredPositionPoint.x / Screen.width, item.Item1.AnchoredPositionPoint.y / Screen.height));
            item.Item3.material.SetVector("_Point2", new Vector2(item.Item2.AnchoredPositionPoint.x / Screen.width, item.Item2.AnchoredPositionPoint.y / Screen.height));
            item.Item3.points = BezierFromTwoPoints.GetPoints(
                item.Item3.rectTransform.InverseTransformPoint(item.Item1.DragPoint),
                item.Item3.rectTransform.InverseTransformPoint(item.Item2.DragPoint), 0.5f, 10);
            item.Item3.SetAllDirty();
        }
    }
}
