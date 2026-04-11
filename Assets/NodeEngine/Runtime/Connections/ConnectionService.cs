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

    public bool CreateConnection(Connector from, Connector to)
    {
        if (from == null || to == null || from.Node == null || to.Node == null)
            return false;
        if (from.Node == to.Node)
            return false;
        if (AreAlreadyConnected(from, to))
            return false;

        if (from.IsFlow)
        {
            var flowConn = new FlowConnection
            {
                SourceNode = from.Node,
                SourcePortName = from.PortName,
                TargetNode = to.Node,
                TargetPortName = to.PortName
            };
            _graph.AddFlowConnection(flowConn);
        }
        else
        {
            var dataConn = new DataConnection
            {
                SourceNode = from.Node,
                OutputPortName = from.PortName,
                TargetNode = to.Node,
                InputPortName = to.PortName
            };
            _graph.AddDataConnection(dataConn);
        }

        _mediator.Publish(new ConnectionChangedNotification(from, to, wasAdded: true));
        _mediator.Publish(new MarkGraphDirtyNotification());
        return true;
    }

    public void Disconnect(Connector from, Connector to)
    {
        if (from == null || to == null) return;

        bool removed = false;
        if (from.IsFlow)
        {
            var conn = _graph.GetFlowConnectionsFrom(from.Node, from.PortName)
                .FirstOrDefault(c => c.TargetNode == to.Node && c.TargetPortName == to.PortName);
            if (conn != null)
                removed = _graph.RemoveFlowConnection(conn);
        }
        else
        {
            var conn = _graph.GetDataConnectionsFrom(from.Node, from.PortName)
                .FirstOrDefault(c => c.TargetNode == to.Node && c.InputPortName == to.PortName);
            if (conn != null)
                removed = _graph.RemoveDataConnection(conn);
        }

        if (removed)
        {
            _mediator.Publish(new ConnectionChangedNotification(from, to, wasAdded: false));
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