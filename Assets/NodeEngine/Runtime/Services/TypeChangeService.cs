using System;
using System.Collections.Generic;
using UnityEngine;

public static class TypeChangeService
{
    private static bool _isInUpdate = false;

    public static bool TryChangeConnectorType(Connector connector, Type newType)
    {
        if (_isInUpdate) return false;
        if (connector == null || connector.ValueType == newType) return false;

        _isInUpdate = true;
        try
        {
            // 1. Disconnect anything that is no longer compatible
            TypeChangeLogic.CheckAndDisconnectIncompatible(connector, newType);

            // 2. Update the visual connector
            connector.SetValueType(newType);

            // 3. Update the underlying Node's Port definition so the Compiler knows
            var portInfo = connector.Node.Ports.Find(p => p.Name == connector.PortName && p.IsInput == connector.IsInput);
            if (portInfo != null) portInfo.ValueType = newType;

            // 4. Update existing valid line colors
            UpdateConnectionLineColors(connector, connector.Color);

            // 5. Mark for recompile
            NodeRunner.Instance?.MarkDirty();

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error changing type: {e.Message}");
            return false;
        }
        finally
        {
            _isInUpdate = false;
        }
    }

    private static void UpdateConnectionLineColors(Connector connector, Color newColor)
    {
        if (connector == null || LineRenderersController.Instance == null) return;
        try
        {
            var controllerType = typeof(LineRenderersController);
            var connectionsField = controllerType.GetField("_connections", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (connectionsField == null) return;

            var connections = connectionsField.GetValue(LineRenderersController.Instance) as List<LineRenderersController.ConnectionData>;
            if (connections == null) return;

            foreach (var connection in connections)
            {
                if (!connection.IsValid) continue;
                bool isOurConnection = connection.ConnectorA == connector || connection.ConnectorB == connector;

                if (isOurConnection)
                {
                    var lineRenderer = connection.LineRenderer;
                    if (lineRenderer != null && lineRenderer.material != null)
                    {
                        Color otherColor = connection.ConnectorA == connector ? connection.ConnectorB.Color : connection.ConnectorA.Color;
                        lineRenderer.material.SetColor("_Color1", connection.ConnectorA == connector ? newColor : otherColor);
                        lineRenderer.material.SetColor("_Color2", connection.ConnectorB == connector ? newColor : otherColor);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to update line colors: {e.Message}");
        }
    }
}