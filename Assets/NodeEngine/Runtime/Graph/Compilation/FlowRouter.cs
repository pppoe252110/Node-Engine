using System.Collections.Generic;

public class FlowRouter
{
    public Dictionary<BaseNode, Dictionary<string, int>> RouteFlows(
        List<BaseNode> nodes,
        List<FlowConnection> flowConnections,
        Dictionary<BaseNode, int> nodeToIndex)
    {
        var flowTargets = new Dictionary<BaseNode, Dictionary<string, int>>();

        // Initialize empty dictionary for each node
        foreach (var node in nodes)
            flowTargets[node] = new Dictionary<string, int>();

        // Fill targets from connections
        foreach (var conn in flowConnections)
        {
            if (nodeToIndex.TryGetValue(conn.TargetNode, out int targetIdx))
                flowTargets[conn.SourceNode][conn.SourcePortName] = targetIdx;
        }

        return flowTargets;
    }
}