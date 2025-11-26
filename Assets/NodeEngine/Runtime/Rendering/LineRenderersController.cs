using Radishmouse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineRenderersController : MonoBehaviour
{
    public static LineRenderersController Instance => _instance ??= FindFirstObjectByType<LineRenderersController>();
    private static LineRenderersController _instance;

    public UILineRenderer LineRendererPrefab => _lineRendererPrefab;

    [SerializeField] private RectTransform _inheritTransform;
    [SerializeField] private Material _lineRendererMaterial;
    [SerializeField] private UILineRenderer _lineRendererPrefab;

    [Header("Line Quality Settings")]
    [SerializeField] private float _pointsPerPixel = 0.1f;
    [SerializeField] private int _minPoints = 5;
    [SerializeField] private int _maxPoints = 50;
    [SerializeField] private float _curveIntensity = 0.5f;

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
        lineRenderer.transform.localScale = Vector3.one;

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


        Vector2 startPoint = lineRenderer.rectTransform.InverseTransformPoint(connection.ConnectorA.DragPoint);
        Vector2 endPoint = lineRenderer.rectTransform.InverseTransformPoint(connection.ConnectorB.DragPoint);

        float pixelDistance = Vector2.Distance(startPoint, endPoint);
        int pointsCount = CalculateDynamicPointsCount(pixelDistance);


        float dynamicCurveIntensity = CalculateDynamicCurveIntensity(pixelDistance);

        lineRenderer.points = BezierFromTwoPoints.GetPoints(
            startPoint,
            endPoint,
            dynamicCurveIntensity,
            pointsCount);

        lineRenderer.SetAllDirty();
    }

    public int CalculateDynamicPointsCount(float pixelDistance)
    {

        int calculatedPoints = Mathf.RoundToInt(pixelDistance * _pointsPerPixel);


        return Mathf.Clamp(calculatedPoints, _minPoints, _maxPoints);
    }

    public float CalculateDynamicCurveIntensity(float pixelDistance)
    {

        float baseIntensity = _curveIntensity;
        float distanceFactor = Mathf.Clamp(pixelDistance / 1000f, 0f, 1f);
        float additionalCurve = distanceFactor * 0.3f;

        return baseIntensity + additionalCurve;
    }

    public void SetLineQuality(float pointsPerPixel, int minPoints = 5, int maxPoints = 50, float curveIntensity = 0.5f)
    {
        _pointsPerPixel = Mathf.Max(0.01f, pointsPerPixel);
        _minPoints = Mathf.Max(2, minPoints);
        _maxPoints = Mathf.Max(_minPoints, maxPoints);
        _curveIntensity = Mathf.Clamp01(curveIntensity);
    }


    public readonly struct ConnectionData
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


    [ContextMenu("Debug Line Quality")]
    public void DebugLineQuality()
    {
        if (_connections.Count == 0)
        {
            Debug.Log("No active connections to debug");
            return;
        }

        foreach (var connection in _connections)
        {
            Vector2 startPoint = connection.LineRenderer.rectTransform.InverseTransformPoint(connection.ConnectorA.DragPoint);
            Vector2 endPoint = connection.LineRenderer.rectTransform.InverseTransformPoint(connection.ConnectorB.DragPoint);
            float pixelDistance = Vector2.Distance(startPoint, endPoint);
            int pointsCount = CalculateDynamicPointsCount(pixelDistance);

            Debug.Log($"Connection: {pointsCount} points for {pixelDistance:F0}px distance " +
                     $"(from {connection.ConnectorA.Node.GetType().Name} to {connection.ConnectorB.Node.GetType().Name})");
        }
    }
}