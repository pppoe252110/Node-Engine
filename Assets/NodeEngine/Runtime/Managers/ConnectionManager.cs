using System;
using System.Collections.Generic;
using UniMediator.Runtime;
using UnityEngine;
using VContainer;

public class ConnectionManager : MonoBehaviour
{
    public List<DataConnection> ActiveDataConnections { get; private set; } = new();
    public List<FlowConnection> ActiveFlowConnections { get; private set; } = new();

    private IMediator _mediator;

    [Inject]
    public void Construct(IMediator mediator)
    {
        _mediator = mediator;
    }

    public bool CreateConnectionWithConnectors(Connector from, Connector to)
    {
        if (from == null || to == null)
        {
            Debug.LogError("[CM] Source or target connector is null.");
            return false;
        }
        if (from.Node == null || to.Node == null)
        {
            Debug.LogError($"[CM] Connector '{from.PortName}' or '{to.PortName}' has null Node reference.");
            return false;
        }
        if (from.Node == to.Node)
        {
            Debug.LogWarning("[CM] Cannot connect a node to itself.");
            return false;
        }
        if (from.Connections.Contains(to))
        {
            Debug.LogWarning("[CM] Connection already exists.");
            return false;
        }

        BindLogic(from, to);
        from.AddVisualConnection(to);
        to.AddVisualConnection(from);
        NotifyConnection(from, to);

        // Publish unified notification
        _mediator.Publish(new ConnectionChangedNotification(from, to, wasAdded: true));
        _mediator.Publish(new MarkGraphDirtyNotification());

        return true;
    }

    public void Disconnect(Connector from, Connector to)
    {
        if (from == null || to == null) return;

        UnbindLogic(from, to);
        from.RemoveVisualConnection(to);
        to.RemoveVisualConnection(from);
        NotifyDisconnection(from, to);

        // Publish unified notification
        _mediator.Publish(new ConnectionChangedNotification(from, to, wasAdded: false));
        _mediator.Publish(new MarkGraphDirtyNotification());
    }

    public void ClearAllConnections()
    {
        var dataCopy = new List<DataConnection>(ActiveDataConnections);
        var flowCopy = new List<FlowConnection>(ActiveFlowConnections);

        foreach (var conn in dataCopy)
        {
            var sourceConn = conn.SourceNode.GetConnector(conn.OutputPortName);
            var targetConn = conn.TargetNode.GetConnector(conn.InputPortName);
            Disconnect(sourceConn, targetConn);
        }

        foreach (var conn in flowCopy)
        {
            var sourceConn = conn.SourceNode.GetConnector(conn.SourcePortName);
            var targetConn = conn.TargetNode.GetConnector(conn.TargetPortName);
            Disconnect(sourceConn, targetConn);
        }

        ActiveDataConnections.Clear();
        ActiveFlowConnections.Clear();

        _mediator.Publish(new MarkGraphDirtyNotification());
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
            ActiveFlowConnections.RemoveAll(c =>
                c.SourceNode == from.Node &&
                c.SourcePortName == from.PortName &&
                c.TargetNode == to.Node);
        }
        else
        {
            ActiveDataConnections.RemoveAll(c =>
                c.SourceNode == from.Node &&
                c.OutputPortName == from.PortName &&
                c.TargetNode == to.Node &&
                c.InputPortName == to.PortName);
        }
    }

    private void NotifyConnection(Connector fromConnector, Connector toConnector)
    {
        try { fromConnector.Node.OnConnected(fromConnector, toConnector); }
        catch (Exception ex) { Debug.LogError($"[CM] OnConnected failed for {fromConnector.Node?.GetType().Name}: {ex.Message}"); }

        try { toConnector.Node.OnConnected(toConnector, fromConnector); }
        catch (Exception ex) { Debug.LogError($"[CM] OnConnected failed for {toConnector.Node?.GetType().Name}: {ex.Message}"); }
    }

    private void NotifyDisconnection(Connector fromConnector, Connector toConnector)
    {
        fromConnector.Node.OnDisconnected(fromConnector, toConnector);
        toConnector.Node.OnDisconnected(toConnector, fromConnector);
    }
}

// Data structures and extension method remain unchanged
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