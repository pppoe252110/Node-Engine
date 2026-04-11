using Radishmouse;
using System.Collections.Generic;
using UniMediator.Runtime;
using UnityEngine;

public class LineRenderersController : MonoBehaviour, INotificationHandler<ConnectionChangedNotification>, INotificationHandler<UpdateNodeLogicConnectionsNotification>, INotificationHandler<UpdateLinesNotification>
{
    [SerializeField] private UILineRenderer _lineRendererPrefab;
    public UILineRenderer LineRendererPrefab => _lineRendererPrefab;

    private Dictionary<(Connector, Connector), UILineRenderer> _activeLines = new();
    private Dictionary<Connector, List<UILineRenderer>> _connectorToLines = new();

    #region Event Handlers

    public void Handle(ConnectionChangedNotification notification)
    {
        if (notification.WasAdded)
            CreateLine(notification.Source, notification.Target);
        else
            RemoveLine(notification.Source, notification.Target);
    }

    #endregion

    #region Node Drag Subscription

    public void Handle(UpdateNodeLogicConnectionsNotification notification)
    {
        UpdateLinesForNode(notification.NodeLogic);
    }

    public void Handle(UpdateLinesNotification notification)
    {
        RefreshAllLineColors();
    }

    private void UpdateLinesForNode(NodeLogic nodeLogic)
    {
        // Update all lines connected to any connector of this node
        foreach (var connector in nodeLogic.InputConnectors)
            UpdateLinesForConnector(connector);
        foreach (var connector in nodeLogic.OutputConnectors)
            UpdateLinesForConnector(connector);
    }

    #endregion

    #region Public API

    public void CreateLine(Connector from, Connector to)
    {
        if (from == null || to == null) return;

        var ordered = OrderByDirection(from, to);
        var key = GetOrderedKey(ordered.output, ordered.input);
        if (_activeLines.ContainsKey(key)) return;

        var lineInstance = Instantiate(_lineRendererPrefab, transform);
        lineInstance.material = CreateLineMaterial(ordered.output.Color, ordered.input.Color);
        _activeLines[key] = lineInstance;

        AddConnectorLineMapping(ordered.output, lineInstance);
        AddConnectorLineMapping(ordered.input, lineInstance);

        UpdateLinePoints(lineInstance, ordered.output, ordered.input);
    }

    public void UpdateConnectionColors(Connector connector)
    {
        if (_connectorToLines.TryGetValue(connector, out var lines))
        {
            foreach (var line in lines)
            {
                // Find the other connector for this line
                foreach (var kvp in _activeLines)
                {
                    if (kvp.Value == line)
                    {
                        var other = kvp.Key.Item1 == connector ? kvp.Key.Item2 : kvp.Key.Item1;
                        // Update both colors based on current connector colors
                        line.material.SetColor("_Color1", kvp.Key.Item1.Color);
                        line.material.SetColor("_Color2", kvp.Key.Item2.Color);
                        break;
                    }
                }
            }
        }
    }

    private Material CreateLineMaterial(Color color1, Color color2)
    {
        var material = new Material(_lineRendererPrefab.material);
        material.SetColor("_Color1", color1);
        material.SetColor("_Color2", color2);
        return material;
    }

    public float CalculateDynamicCurveIntensity(float pixelDistance)
    {
        float minIntensity = 0f;
        float maxIntensity = 0.5f;
        float threshold = 400f;

        float t = Mathf.Clamp01(pixelDistance / threshold);

        // Smoothstep for natural easing
        t = t * t * (3f - 2f * t);
        return Mathf.Lerp(minIntensity, maxIntensity, t);
    }

    public int CalculateDynamicPointsCount(float pixelDistance)
    {
        int minPoints = 20;
        int maxPoints = 50;
        float threshold = 500f;
        float t = Mathf.Clamp01(pixelDistance / threshold);
        return Mathf.RoundToInt(Mathf.Lerp(minPoints, maxPoints, t));
    }

    #endregion

    #region Private Helpers

    private (Connector output, Connector input) OrderByDirection(Connector a, Connector b)
    {
        if (!a.IsInput && b.IsInput) return (a, b);
        if (a.IsInput && !b.IsInput) return (b, a);
        // Fallback for flow connections (both IsFlow true) – preserve original order
        return (a, b);
    }

    private void AddConnectorLineMapping(Connector connector, UILineRenderer line)
    {
        if (!_connectorToLines.ContainsKey(connector))
            _connectorToLines[connector] = new List<UILineRenderer>();
        if (!_connectorToLines[connector].Contains(line))
            _connectorToLines[connector].Add(line);
    }

    private void RemoveConnectorLineMapping(Connector connector, UILineRenderer line)
    {
        if (_connectorToLines.TryGetValue(connector, out var lines))
        {
            lines.Remove(line);
            if (lines.Count == 0)
                _connectorToLines.Remove(connector);
        }
    }

    private void UpdateLinesForConnector(Connector connector)
    {
        if (_connectorToLines.TryGetValue(connector, out var lines))
        {
            foreach (var line in lines)
            {
                foreach (var kvp in _activeLines)
                {
                    if (kvp.Value == line)
                    {
                        var (output, input) = OrderByDirection(kvp.Key.Item1, kvp.Key.Item2);
                        UpdateLinePoints(line, output, input);
                        break;
                    }
                }
            }
        }
    }

    private void RemoveLine(Connector from, Connector to)
    {
        var ordered = OrderByDirection(from, to);
        var key = GetOrderedKey(ordered.output, ordered.input);
        if (_activeLines.TryGetValue(key, out var line))
        {
            RemoveConnectorLineMapping(ordered.output, line);
            RemoveConnectorLineMapping(ordered.input, line);
            Destroy(line.gameObject);
            _activeLines.Remove(key);
        }
    }

    public void RefreshAllLineColors()
    {
        foreach (var kvp in _activeLines)
        {
            var (connectorA, connectorB) = kvp.Key;
            var line = kvp.Value;

            // Update material colors to current connector colors
            line.material.SetColor("_Color1", connectorA.Color);
            line.material.SetColor("_Color2", connectorB.Color);

            // Also ensure shader position vectors are up-to-date
            Vector2 screenPointA = connectorA.DragPoint;
            Vector2 screenPointB = connectorB.DragPoint;
            line.material.SetVector("_Point1", new Vector2(screenPointA.x / Screen.width, screenPointA.y / Screen.height));
            line.material.SetVector("_Point2", new Vector2(screenPointB.x / Screen.width, screenPointB.y / Screen.height));

            line.SetAllDirty();
        }
    }

    private (Connector, Connector) GetOrderedKey(Connector a, Connector b)
    {
        return (a, b);
    }

    private void UpdateLinePoints(UILineRenderer line, Connector from, Connector to)
    {
        var startPoint = line.rectTransform.InverseTransformPoint(from.DragPoint);
        var endPoint = line.rectTransform.InverseTransformPoint(to.DragPoint);

        float pixelDistance = Vector2.Distance(startPoint, endPoint);
        float curveIntensity = CalculateDynamicCurveIntensity(pixelDistance);
        int pointsCount = CalculateDynamicPointsCount(pixelDistance);

        line.points = BezierFromTwoPoints.GetPoints(startPoint, endPoint, curveIntensity, pointsCount);

        // Update shader colors to current connector colors (in case of dynamic type changes)
        line.material.SetColor("_Color1", from.Color);
        line.material.SetColor("_Color2", to.Color);

        Vector2 screenPoint1 = from.DragPoint;
        Vector2 screenPoint2 = to.DragPoint;
        line.material.SetVector("_Point1", new Vector2(screenPoint1.x / Screen.width, screenPoint1.y / Screen.height));
        line.material.SetVector("_Point2", new Vector2(screenPoint2.x / Screen.width, screenPoint2.y / Screen.height));

        line.SetAllDirty();
    }

    #endregion
}