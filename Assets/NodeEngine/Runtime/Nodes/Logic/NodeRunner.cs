using System.Linq;
using UnityEngine;

public class NodeRunner : MonoBehaviour
{
    public static NodeRunner Instance;
    private CompiledGraph _currentGraph;
    private bool _isDirty = true;
    private bool _hasStarted = false;

    private void Awake() => Instance = this;
    public void MarkDirty() => _isDirty = true;

    private void Update()
    {
        if (_isDirty)
        {
            Recompile();
            _isDirty = false;
            _hasStarted = false; // Reset Start logic when graph changes
        }

        if (_currentGraph == null) return;

        // Execute "On Play" nodes only once per compile
        if (!_hasStarted)
        {
            ExecuteNodesOfType<StartNode>();
            _hasStarted = true;
        }

        // Execute continuous tick nodes
        ExecuteNodesOfType<UpdateNode>();
    }

    public void ExecuteTest()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        ExecuteNodesOfType<TestNode>();

        stopwatch.Stop();
        Debug.LogError(stopwatch.ElapsedMilliseconds);
    }

    private void ExecuteNodesOfType<T>() where T : BaseNode
    {
        var nodes = NodeSpawnerService.Instance.GetAllNodes()
            .Select(n => n.Node)
            .OfType<T>();

        foreach (var node in nodes)
        {
            _currentGraph.ExecuteNode(node);
        }
    }

    private void Recompile()
    {
        if (NodeSpawnerService.Instance == null || ConnectionManager.Instance == null) return;
        var nodes = NodeSpawnerService.Instance.GetAllNodes().Select(n => n.Node).ToList();
        var dataConns = ConnectionManager.Instance.ActiveDataConnections;
        var flowConns = ConnectionManager.Instance.ActiveFlowConnections;

        _currentGraph = NodeCompiler.Compile(nodes, dataConns, flowConns);
    }
}