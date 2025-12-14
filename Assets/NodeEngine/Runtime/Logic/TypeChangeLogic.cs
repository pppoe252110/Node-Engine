using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TypeChangeLogic
{
    // In TypeChangeLogic.cs, update CheckAndDisconnectIncompatible:
    public static void CheckAndDisconnectIncompatible(Connector connector, Type newType)
    {
        if (connector == null) return;

        // Create a copy of the connections list to avoid modification during iteration
        var connectionsToCheck = connector.Connections.ToList();

        foreach (var connectedConnector in connectionsToCheck)
        {
            if (connectedConnector == null) continue;

            // Check if types are compatible
            bool isCompatible = IsCompatibleType(newType, connectedConnector.ValueType);

            if (!isCompatible)
            {
                Debug.LogWarning($"Disconnecting incompatible connection: {newType.Name} -> {connectedConnector.ValueType.Name}");

                // Remove connection from both connectors
                connector.RemoveConnection(connectedConnector);
                connectedConnector.RemoveConnection(connector);

                // Remove visual line
                if (LineRenderersController.Instance != null)
                {
                    LineRenderersController.Remove(connector, connectedConnector);
                }

                // Notify the nodes about the disconnection
                NotifyDisconnection(connector, connectedConnector);
            }
        }
    }

    private static void NotifyDisconnection(Connector fromConnector, Connector toConnector)
    {
        // Notify connection listeners
        if (fromConnector.Node is IConnectionListener fromListener)
        {
            fromListener.OnDisconnected(fromConnector, toConnector);
        }

        if (toConnector.Node is IConnectionListener toListener)
        {
            toListener.OnDisconnected(toConnector, fromConnector);
        }
    }

    public static bool IsCompatibleType(Type sourceType, Type targetType)
    {
        if (sourceType == targetType) return true;
        if (targetType == typeof(object)) return true;  // Object accepts any type

        // Add type compatibility rules
        if (sourceType == typeof(int) && targetType == typeof(float)) return true;
        if (sourceType == typeof(float) && targetType == typeof(int)) return true;

        // Allow numeric to object conversions
        if ((sourceType == typeof(int) || sourceType == typeof(float)) && targetType == typeof(object)) return true;

        return false;
    }
}