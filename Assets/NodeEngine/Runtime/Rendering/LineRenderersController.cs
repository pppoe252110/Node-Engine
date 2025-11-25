using Radishmouse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineRenderersController : MonoBehaviour
{
    public static LineRenderersController Instance => _instance ??= FindFirstObjectByType<LineRenderersController>();
    private static LineRenderersController _instance;

    [SerializeField] private RectTransform _inheritTransform;
    [SerializeField] private Material _lineRendererMaterial; 

    private List<ConnectionData> _connections = new List<ConnectionData>();

    private void Awake() => _instance = this;

    private void LateUpdate() => UpdateLineRenderers();

    public static void Add(Connector connectorA, Connector connectorB, UILineRenderer lineRenderer)
    {
        if (Instance == null || connectorA == null || connectorB == null || lineRenderer == null)
        {
            Debug.LogError("Cannot add connection: null parameters");
            return;
        }

        lineRenderer.transform.SetParent(Instance.transform);
        lineRenderer.material = CreateConnectionMaterial(connectorA.Color, connectorB.Color);
        Instance._connections.Add(new ConnectionData(connectorA, connectorB, lineRenderer));
    }

    public static bool Remove(Connector connectorA, Connector connectorB)
    {
        if (Instance == null) return false;

        var connection = Instance._connections.FirstOrDefault(c =>
            (c.ConnectorA == connectorA && c.ConnectorB == connectorB) ||
            (c.ConnectorA == connectorB && c.ConnectorB == connectorA));

        if (connection.IsValid)
        {
            Destroy(connection.LineRenderer.gameObject);
            return Instance._connections.Remove(connection);
        }
        return false;
    }

    private static Material CreateConnectionMaterial(Color colorA, Color colorB)
    {
        if (Instance._lineRendererMaterial == null)
        {
            Debug.LogError("LineRenderer material is not assigned in LineRenderersController");
            
            var fallbackMaterial = new Material(Shader.Find("UI/Default"));
            fallbackMaterial.SetColor("_Color1", colorA);
            fallbackMaterial.SetColor("_Color2", colorB);
            return fallbackMaterial;
        }

        var material = new Material(Instance._lineRendererMaterial);
        material.SetColor("_Color1", colorA);
        material.SetColor("_Color2", colorB);
        return material;
    }

    private void UpdateLineRenderers()
    {
        if (_connections.Count == 0) return;

        transform.position = _inheritTransform.position;
        transform.localScale = Vector3.one;

        for (int i = _connections.Count - 1; i >= 0; i--)
        {
            var connection = _connections[i];

            if (!connection.IsValid)
            {
                _connections.RemoveAt(i);
                continue;
            }

            UpdateConnectionVisuals(connection);
        }
    }

    private void UpdateConnectionVisuals(ConnectionData connection)
    {
        var lineRenderer = connection.LineRenderer;

        
        lineRenderer.material.SetVector("_Point1",
            new Vector2(connection.ConnectorA.AnchoredPositionPoint.x / Screen.width,
                       connection.ConnectorA.AnchoredPositionPoint.y / Screen.height));
        lineRenderer.material.SetVector("_Point2",
            new Vector2(connection.ConnectorB.AnchoredPositionPoint.x / Screen.width,
                       connection.ConnectorB.AnchoredPositionPoint.y / Screen.height));

        
        lineRenderer.points = BezierFromTwoPoints.GetPoints(
            lineRenderer.rectTransform.InverseTransformPoint(connection.ConnectorA.DragPoint),
            lineRenderer.rectTransform.InverseTransformPoint(connection.ConnectorB.DragPoint),
            0.5f, 10);

        lineRenderer.SetAllDirty();
    }

    private readonly struct ConnectionData
    {
        public readonly Connector ConnectorA;
        public readonly Connector ConnectorB;
        public readonly UILineRenderer LineRenderer;

        public ConnectionData(Connector a, Connector b, UILineRenderer renderer)
        {
            ConnectorA = a;
            ConnectorB = b;
            LineRenderer = renderer;
        }

        public bool IsValid => ConnectorA != null && ConnectorB != null && LineRenderer != null;
    }

    
    public static void ClearAllConnections()
    {
        if (Instance == null) return;

        foreach (var connection in Instance._connections)
        {
            if (connection.LineRenderer != null)
                Destroy(connection.LineRenderer.gameObject);
        }
        Instance._connections.Clear();
    }
}