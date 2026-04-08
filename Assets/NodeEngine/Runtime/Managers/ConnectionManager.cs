using UnityEngine;
using System.Collections.Generic;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance { get; private set; }

    // Global trackers for the Compiler
    public List<DataConnection> ActiveDataConnections { get; private set; } = new List<DataConnection>();
    public List<FlowConnection> ActiveFlowConnections { get; private set; } = new List<FlowConnection>();

    private void Awake() => Instance = this;

    public bool CreateConnectionWithConnectors(Connector from, Connector to)
    {
        if (from == null || to == null || from.Node == to.Node) return false;

        BindLogic(from, to);
        from.AddVisualConnection(to);
        to.AddVisualConnection(from);

        NotifyConnection(from, to);

        NodeRunner.Instance?.MarkDirty();
        return true;
    }

    public void Disconnect(Connector from, Connector to)
    {
        UnbindLogic(from, to);
        from.RemoveVisualConnection(to);
        to.RemoveVisualConnection(from);

        LineRenderersController.Remove(from, to);

        NotifyDisconnection(from, to);

        NodeRunner.Instance?.MarkDirty();
    }

    private void NotifyConnection(Connector fromConnector, Connector toConnector)
    {
        if (fromConnector.Node is IConnectionListener fromListener)
            fromListener.OnConnected(fromConnector, toConnector);

        if (toConnector.Node is IConnectionListener toListener)
            toListener.OnConnected(toConnector, fromConnector);
    }

    private void NotifyDisconnection(Connector fromConnector, Connector toConnector)
    {
        if (fromConnector.Node is IConnectionListener fromListener)
            fromListener.OnDisconnected(fromConnector, toConnector);

        if (toConnector.Node is IConnectionListener toListener)
            toListener.OnDisconnected(toConnector, fromConnector);
    }

    private void BindLogic(Connector from, Connector to)
    {
        if (from.IsFlow)
        {
            ActiveFlowConnections.Add(new FlowConnection
            {
                SourceNode = from.Node,
                SourcePortName = from.PortName,
                TargetNode = to.Node,
                TargetPortName = to.PortName
            });
        }
        else
        {
            ActiveDataConnections.Add(new DataConnection
            {
                SourceNode = from.Node,
                OutputPortName = from.PortName,
                TargetNode = to.Node,
                InputPortName = to.PortName
            });
        }
    }

    private void UnbindLogic(Connector from, Connector to)
    {
        if (from.IsFlow)
        {
            ActiveFlowConnections.RemoveAll(c => c.SourceNode == from.Node && c.SourcePortName == from.PortName && c.TargetNode == to.Node);
        }
        else
        {
            ActiveDataConnections.RemoveAll(c =>
                c.SourceNode == from.Node && c.OutputPortName == from.PortName &&
                c.TargetNode == to.Node && c.InputPortName == to.PortName);
        }
    }
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