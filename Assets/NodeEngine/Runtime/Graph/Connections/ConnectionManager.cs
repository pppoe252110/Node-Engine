using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class ConnectionManager : MonoBehaviour
{
    private ConnectionService _connectionService;

    // Expose connection lists for backward compatibility
    public List<DataConnection> ActiveDataConnections => new(_connectionService.Graph.DataConnections);
    public List<FlowConnection> ActiveFlowConnections => new(_connectionService.Graph.FlowConnections);

    [Inject]
    public void Construct(ConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public bool CreateConnectionWithConnectors(Connector from, Connector to)
        => _connectionService.CreateConnection(from, to);

    public void Disconnect(Connector from, Connector to)
        => _connectionService.Disconnect(from, to);

    public void ClearAllConnections()
        => _connectionService.ClearAllConnections();
}

public class DataConnection
{
    public BaseNode SourceNode;
    public string OutputPortName;
    public BaseNode TargetNode;
    public string InputPortName;
}

public class FlowConnection
{
    public BaseNode SourceNode;
    public string SourcePortName;
    public BaseNode TargetNode;
    public string TargetPortName;
}

public static class BaseNodeExtensions
{
    public static Connector GetConnector(this BaseNode node, string portName)
    {
        if (node.LogicView == null) return null;
        return node.LogicView.InputConnectors.Find(c => c.PortName == portName)
            ?? node.LogicView.OutputConnectors.Find(c => c.PortName == portName);
    }
}