using System.Linq;
using UniMediator.Runtime;
using VContainer;

public class ConnectionService
{
    private readonly ConnectionGraph _graph = new();
    private readonly IMediator _mediator;

    public ConnectionGraph Graph => _graph;

    [Inject]
    public ConnectionService(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Update these two methods in ConnectionService.cs

    public bool CreateConnection(Connector from, Connector to)
    {
        if (from == null || to == null || from.Node == null || to.Node == null)
            return false;
        if (from.Node == to.Node)
            return false;

        Connector source = from.IsInput ? to : from;
        Connector target = from.IsInput ? from : to;

        if (source.IsInput || !target.IsInput)
            return false;

        if (AreAlreadyConnected(source, target))
            return false;

        if (source.IsFlow)
        {
            var flowConn = new FlowConnection
            {
                SourceNode = source.Node,
                SourcePortName = source.PortName,
                TargetNode = target.Node,
                TargetPortName = target.PortName
            };
            _graph.AddFlowConnection(flowConn);
        }
        else
        {
            var dataConn = new DataConnection
            {
                SourceNode = source.Node,
                OutputPortName = source.PortName,
                TargetNode = target.Node,
                InputPortName = target.PortName
            };
            _graph.AddDataConnection(dataConn);
        }

        _mediator.Publish(new ConnectionChangedNotification(source, target, wasAdded: true));
        _mediator.Publish(new MarkGraphDirtyNotification());
        return true;
    }

    public void Disconnect(Connector from, Connector to)
    {
        if (from == null || to == null) return;

        Connector source = from.IsInput ? to : from;
        Connector target = from.IsInput ? from : to;

        bool removed = false;
        if (source.IsFlow)
        {
            var conn = _graph.GetFlowConnectionsFrom(source.Node, source.PortName)
                .FirstOrDefault(c => c.TargetNode == target.Node && c.TargetPortName == target.PortName);
            if (conn != null)
                removed = _graph.RemoveFlowConnection(conn);
        }
        else
        {
            var conn = _graph.GetDataConnectionsFrom(source.Node, source.PortName)
                .FirstOrDefault(c => c.TargetNode == target.Node && c.InputPortName == target.PortName);
            if (conn != null)
                removed = _graph.RemoveDataConnection(conn);
        }

        if (removed)
        {
            _mediator.Publish(new ConnectionChangedNotification(source, target, wasAdded: false));
            _mediator.Publish(new MarkGraphDirtyNotification());
        }
    }

    public void ClearAllConnections()
    {
        // Notify disconnection for each existing connection
        foreach (var conn in _graph.DataConnections.ToList())
        {
            var sourceConn = conn.SourceNode.GetConnector(conn.OutputPortName);
            var targetConn = conn.TargetNode.GetConnector(conn.InputPortName);
            _mediator.Publish(new ConnectionChangedNotification(sourceConn, targetConn, wasAdded: false));
        }
        foreach (var conn in _graph.FlowConnections.ToList())
        {
            var sourceConn = conn.SourceNode.GetConnector(conn.SourcePortName);
            var targetConn = conn.TargetNode.GetConnector(conn.TargetPortName);
            _mediator.Publish(new ConnectionChangedNotification(sourceConn, targetConn, wasAdded: false));
        }

        _graph.Clear();
        _mediator.Publish(new MarkGraphDirtyNotification());
    }

    private bool AreAlreadyConnected(Connector from, Connector to)
    {
        if (from.IsFlow)
        {
            return _graph.GetFlowConnectionsFrom(from.Node, from.PortName)
                .Any(c => c.TargetNode == to.Node && c.TargetPortName == to.PortName);
        }
        else
        {
            return _graph.GetDataConnectionsFrom(from.Node, from.PortName)
                .Any(c => c.TargetNode == to.Node && c.InputPortName == to.PortName);
        }
    }
}