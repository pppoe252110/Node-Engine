using System;
using UnityEngine;
using VContainer;

public class TypeChangeService
{
    private readonly ConnectionManager _connectionManager;
    private readonly NodeRunner _nodeRunner;
    private readonly LineRenderersController _lineRenderersController;
    private bool _isInUpdate = false;

    [Inject]
    public TypeChangeService(
        ConnectionManager connectionManager,
        NodeRunner nodeRunner,
        LineRenderersController lineRenderersController)
    {
        _connectionManager = connectionManager;
        _nodeRunner = nodeRunner;
        _lineRenderersController = lineRenderersController;
    }

    public bool TryChangeConnectorType(Connector connector, Type newType)
    {
        if (_isInUpdate) return false;
        if (connector == null || connector.ValueType == newType) return false;

        _isInUpdate = true;
        try
        {
            // 1. Disconnect incompatible connections
            TypeChangeLogic.CheckAndDisconnectIncompatible(connector, newType, _connectionManager);

            // 2. Update the visual connector
            connector.SetValueType(newType);

            // 3. Update the Node's Port definition
            var portInfo = connector.Node.Ports.Find(p => p.Name == connector.PortName && p.IsInput == connector.IsInput);
            if (portInfo != null) portInfo.ValueType = newType;

            // 4. Update line colors using the injected controller
            _lineRenderersController.UpdateConnectionColors(connector);

            // 5. Mark graph dirty
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
}