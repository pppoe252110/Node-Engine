using System.Collections.Generic;
using System.Linq;
using UniMediator.Runtime;
using UnityEngine;
using VContainer;

public class NodeRunner : MonoBehaviour, INotificationHandler<MarkGraphDirtyNotification>
{
    [Header("Settings")]
    [SerializeField] private bool _logExecutionTime = false;

    private NodeCompiler.CompiledGraph _currentGraph;
    private bool _isDirty = false;
    private bool _hasStarted = false;

    private readonly List<StartNode> _startNodes = new();
    private readonly List<UpdateNode> _updateNodes = new();
    private readonly List<TestNode> _testNodes = new();

    private NodeSpawnerService _nodeSpawner;
    private ConnectionManager _connectionManager;

    [Inject]
    public void Construct(NodeSpawnerService nodeSpawner, ConnectionManager connectionManager)
    {
        _nodeSpawner = nodeSpawner;
        _connectionManager = connectionManager;
    }

    public void Handle(MarkGraphDirtyNotification notification)
    {
        MarkDirty();
    }

    public void MarkDirty()
    {
        _isDirty = true;
        _hasStarted = false;
    }

    private void Update()
    {
        if (_isDirty)
        {
            Recompile();
            _isDirty = false;
            _hasStarted = false;
        }

        if (_currentGraph == null) return;

        if (!_hasStarted)
        {
            for (int i = 0; i < _startNodes.Count; i++)
                _currentGraph.ExecuteNode(_startNodes[i]);
            _hasStarted = true;
        }

        for (int i = 0; i < _updateNodes.Count; i++)
            _currentGraph.ExecuteNode(_updateNodes[i]);
    }

    public void ExecuteTest()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < _testNodes.Count; i++)
            _currentGraph.ExecuteNode(_testNodes[i]);
        sw.Stop();
        if (_logExecutionTime) Debug.Log($"[NodeRunner] Graph test execution time: {sw.ElapsedMilliseconds} ms");
    }

    public void ExecuteNodeWithData(BaseNode node, Dictionary<string, object> externalData = null)
    {
        if (_currentGraph == null || node == null) return;

        if (externalData != null)
            _currentGraph.SetNodeInputData(node, externalData);

        _currentGraph.ExecuteNode(node);
    }

    private void Recompile()
    {
        if (_nodeSpawner == null || _connectionManager == null) return;

        _startNodes.Clear();
        _updateNodes.Clear();
        _testNodes.Clear();

        var allLogics = _nodeSpawner.GetAllNodes();
        var nodes = new List<BaseNode>(allLogics.Count());

        foreach (var logic in allLogics)
        {
            var node = logic.Node;
            if (node == null) continue;
            nodes.Add(node);

            if (node is StartNode sn) _startNodes.Add(sn);
            else if (node is UpdateNode un) _updateNodes.Add(un);
            else if (node is TestNode tn) _testNodes.Add(tn);
        }

        var dataConns = _connectionManager.ActiveDataConnections;
        var flowConns = _connectionManager.ActiveFlowConnections;

        _currentGraph = NodeCompiler.Compile(nodes, dataConns, flowConns);
        Debug.Log($"[NodeRunner] Graph compiled with {nodes.Count} nodes.");
    }
}