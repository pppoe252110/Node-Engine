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
            TypeChangeLogic.CheckAndDisconnectIncompatible(connector, newType, _connectionService);

            connector.SetValueType(newType);

            var portInfo = connector.Node.Ports.Find(p => p.Name == connector.PortName && p.IsInput == connector.IsInput);
            if (portInfo != null) portInfo.ValueType = newType;

            _lineRenderersController.UpdateConnectionColors(connector);

            PropagateDynamicTypeChange(connector);

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

    private void PropagateDynamicTypeChange(Connector changedConnector)
    {
        if (changedConnector.IsInput) return;

        foreach (var targetConnector in changedConnector.Connections)
        {
            BaseNode targetNode = targetConnector.Node;
            if (targetNode.TryGetDynamicBindingTargets(targetConnector.PortName, out var boundOutputs))
            {
                foreach (string outputPortName in boundOutputs)
                {
                    targetNode.UpdatePortType(outputPortName, changedConnector.ValueType);
                }
            }
        }
    }
}