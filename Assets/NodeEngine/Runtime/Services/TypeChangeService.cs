using System;
using System.Linq;
using UnityEngine;
using VContainer;

public class TypeChangeService
{
    private readonly ConnectionService _connectionService;
    private readonly NodeRunner _nodeRunner;
    private readonly LineRenderersController _lineRenderersController;
    private bool _isInUpdate = false;

    [Inject]
    public TypeChangeService(ConnectionService connectionService, NodeRunner nodeRunner, LineRenderersController lineRenderersController)
    {
        _connectionService = connectionService;
        _nodeRunner = nodeRunner;
        _lineRenderersController = lineRenderersController;
    }

    public bool TryChangeConnectorType(Connector connector, Type newType)
    {
        if (_isInUpdate || connector == null || connector.ValueType == newType) return false;

        _isInUpdate = true;
        try
        {
            // Use the static helper instead of duplicated code
            TypeChangeLogic.CheckAndDisconnectIncompatible(connector, newType, _connectionService);

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
}