using System;
using System.Linq;
using UnityEngine;

public static class TypeChangeLogic
{
    public static void CheckAndDisconnectIncompatible(
        Connector connector,
        Type newType,
        ConnectionService connectionService)
    {
        if (connector == null || connectionService == null) return;

        var connectionsToCheck = connector.Connections.ToList();

        foreach (var connectedConnector in connectionsToCheck)
        {
            if (connectedConnector == null) continue;
            bool isCompatible = IsCompatibleType(newType, connectedConnector.ValueType);
            if (!isCompatible)
            {
                Debug.LogWarning($"Disconnecting incompatible connection: {connector.Node.NodeName} - {connector.PortName} ({newType.Name}) -> {connectedConnector.Node.NodeName} - {connectedConnector.PortName} ({connectedConnector.ValueType.Name})");
                connectionService.Disconnect(connector, connectedConnector);
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