using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TypeChangeLogic
{
    public static void CheckAndDisconnectIncompatible(Connector connector, Type newType)
    {
        if (connector == null) return;

        var connectionsToCheck = connector.Connections.ToList();

        foreach (var connectedConnector in connectionsToCheck)
        {
            if (connectedConnector == null) continue;

            bool isCompatible = IsCompatibleType(newType, connectedConnector.ValueType);

            if (!isCompatible)
            {
                Debug.LogWarning($"Disconnecting incompatible connection: {newType.Name} -> {connectedConnector.ValueType.Name}");

                if (ConnectionManager.Instance != null)
                {
                    // REFACTORED: This single call now handles Logic, Visuals, and the OnDisconnected event
                    ConnectionManager.Instance.Disconnect(connector, connectedConnector);
                }
            }
        }
    }

    public static bool IsCompatibleType(Type source, Type target)
    {
        if (source == null || target == null) return false;

        if (target.IsAssignableFrom(source)) return true;

        if (IsNumericType(source) && IsNumericType(target))
        {
            return GetNumericPrecedence(source) <= GetNumericPrecedence(target);
        }

        // Custom converter exists? (You can extend this)
        // if (TypeConverterRegistry.CanConvert(source, target)) return true;

        return false;
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(byte) || type == typeof(sbyte) ||
               type == typeof(short) || type == typeof(ushort) ||
               type == typeof(int) || type == typeof(uint) ||
               type == typeof(long) || type == typeof(ulong) ||
               type == typeof(float) || type == typeof(double) || type == typeof(decimal);
    }

    private static int GetNumericPrecedence(Type type)
    {
        if (type == typeof(byte) || type == typeof(sbyte)) return 1;
        if (type == typeof(short) || type == typeof(ushort)) return 2;
        if (type == typeof(int) || type == typeof(uint)) return 3;
        if (type == typeof(long) || type == typeof(ulong)) return 4;
        if (type == typeof(float)) return 5;
        if (type == typeof(double)) return 6;
        if (type == typeof(decimal)) return 7;
        return 0;
    }
}