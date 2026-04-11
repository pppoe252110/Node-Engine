using System;
using System.Linq;
using UnityEngine;
using VContainer;

public class TypeChangeService
{
    private readonly ConnectionManager _connectionManager;
    private readonly NodeRunner _nodeRunner;
    private readonly LineRenderersController _lineRenderersController;
    private bool _isInUpdate = false;

    [Inject]
    public TypeChangeService(ConnectionManager connectionManager, NodeRunner nodeRunner, LineRenderersController lineRenderersController)
    {
        _connectionManager = connectionManager;
        _nodeRunner = nodeRunner;
        _lineRenderersController = lineRenderersController;
    }

    public bool TryChangeConnectorType(Connector connector, Type newType)
    {
        if (_isInUpdate || connector == null || connector.ValueType == newType) return false;

        _isInUpdate = true;
        try
        {
            CheckAndDisconnectIncompatible(connector, newType);

            connector.SetValueType(newType);

            var portInfo = connector.Node.Ports.Find(p => p.Name == connector.PortName && p.IsInput == connector.IsInput);
            if (portInfo != null) portInfo.ValueType = newType;

            _lineRenderersController.UpdateConnectionColors(connector);
            _nodeRunner?.MarkDirty();

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[TypeChangeService] Error changing type: {e.Message}");
            return false;
        }
        finally
        {
            _isInUpdate = false;
        }
    }

    private void CheckAndDisconnectIncompatible(Connector connector, Type newType)
    {
        var connectionsToCheck = connector.Connections.ToList();

        foreach (var connectedConnector in connectionsToCheck)
        {
            if (connectedConnector == null) continue;

            bool isCompatible = TypeChangeLogic.IsCompatibleType(newType, connectedConnector.ValueType);
            if (!isCompatible)
            {
                Debug.LogWarning($"Disconnecting incompatible connection: {newType.Name} -> {connectedConnector.ValueType.Name}");
                _connectionManager.Disconnect(connector, connectedConnector);
            }
        }
    }
}