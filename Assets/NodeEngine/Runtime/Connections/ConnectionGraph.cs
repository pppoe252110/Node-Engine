using System.Collections.Generic;
using System.Linq;

public class ConnectionGraph
{
    private readonly List<DataConnection> _dataConnections = new();
    private readonly List<FlowConnection> _flowConnections = new();

    // Fast lookup by node/port
    private readonly Dictionary<BaseNode, Dictionary<string, List<DataConnection>>> _dataBySource = new();
    private readonly Dictionary<BaseNode, Dictionary<string, List<DataConnection>>> _dataByTarget = new();
    private readonly Dictionary<BaseNode, Dictionary<string, List<FlowConnection>>> _flowBySource = new();
    private readonly Dictionary<BaseNode, Dictionary<string, List<FlowConnection>>> _flowByTarget = new();

    public IReadOnlyList<DataConnection> DataConnections => _dataConnections;
    public IReadOnlyList<FlowConnection> FlowConnections => _flowConnections;

    public void AddDataConnection(DataConnection conn)
    {
        _dataConnections.Add(conn);
        AddToIndex(_dataBySource, conn.SourceNode, conn.OutputPortName, conn);
        AddToIndex(_dataByTarget, conn.TargetNode, conn.InputPortName, conn);
    }

    public void AddFlowConnection(FlowConnection conn)
    {
        _flowConnections.Add(conn);
        AddToIndex(_flowBySource, conn.SourceNode, conn.SourcePortName, conn);
        AddToIndex(_flowByTarget, conn.TargetNode, conn.TargetPortName, conn);
    }

    public bool RemoveDataConnection(DataConnection conn)
    {
        if (!_dataConnections.Remove(conn)) return false;
        RemoveFromIndex(_dataBySource, conn.SourceNode, conn.OutputPortName, conn);
        RemoveFromIndex(_dataByTarget, conn.TargetNode, conn.InputPortName, conn);
        return true;
    }

    public bool RemoveFlowConnection(FlowConnection conn)
    {
        if (!_flowConnections.Remove(conn)) return false;
        RemoveFromIndex(_flowBySource, conn.SourceNode, conn.SourcePortName, conn);
        RemoveFromIndex(_flowByTarget, conn.TargetNode, conn.TargetPortName, conn);
        return true;
    }

    public IEnumerable<DataConnection> GetDataConnectionsFrom(BaseNode node, string portName = null)
    {
        if (_dataBySource.TryGetValue(node, out var portDict))
        {
            if (portName != null)
                return portDict.TryGetValue(portName, out var list) ? list : Enumerable.Empty<DataConnection>();
            return portDict.Values.SelectMany(l => l);
        }
        return Enumerable.Empty<DataConnection>();
    }

    public IEnumerable<DataConnection> GetDataConnectionsTo(BaseNode node, string portName = null)
    {
        if (_dataByTarget.TryGetValue(node, out var portDict))
        {
            if (portName != null)
                return portDict.TryGetValue(portName, out var list) ? list : Enumerable.Empty<DataConnection>();
            return portDict.Values.SelectMany(l => l);
        }
        return Enumerable.Empty<DataConnection>();
    }

    public IEnumerable<FlowConnection> GetFlowConnectionsFrom(BaseNode node, string portName = null)
    {
        if (_flowBySource.TryGetValue(node, out var portDict))
        {
            if (portName != null)
                return portDict.TryGetValue(portName, out var list) ? list : Enumerable.Empty<FlowConnection>();
            return portDict.Values.SelectMany(l => l);
        }
        return Enumerable.Empty<FlowConnection>();
    }

    public IEnumerable<FlowConnection> GetFlowConnectionsTo(BaseNode node, string portName = null)
    {
        if (_flowByTarget.TryGetValue(node, out var portDict))
        {
            if (portName != null)
                return portDict.TryGetValue(portName, out var list) ? list : Enumerable.Empty<FlowConnection>();
            return portDict.Values.SelectMany(l => l);
        }
        return Enumerable.Empty<FlowConnection>();
    }

    public void Clear()
    {
        _dataConnections.Clear();
        _flowConnections.Clear();
        _dataBySource.Clear();
        _dataByTarget.Clear();
        _flowBySource.Clear();
        _flowByTarget.Clear();
    }

    private static void AddToIndex<T>(
        Dictionary<BaseNode, Dictionary<string, List<T>>> index,
        BaseNode node, string portName, T item)
    {
        if (!index.TryGetValue(node, out var portDict))
        {
            portDict = new Dictionary<string, List<T>>();
            index[node] = portDict;
        }
        if (!portDict.TryGetValue(portName, out var list))
        {
            list = new List<T>();
            portDict[portName] = list;
        }
        list.Add(item);
    }

    private static void RemoveFromIndex<T>(
        Dictionary<BaseNode, Dictionary<string, List<T>>> index,
        BaseNode node, string portName, T item)
    {
        if (index.TryGetValue(node, out var portDict) &&
            portDict.TryGetValue(portName, out var list))
        {
            list.Remove(item);
            if (list.Count == 0)
            {
                portDict.Remove(portName);
                if (portDict.Count == 0)
                    index.Remove(node);
            }
        }
    }
}