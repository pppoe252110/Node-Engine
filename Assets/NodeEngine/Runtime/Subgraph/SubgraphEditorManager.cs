using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class SubgraphEditorManager : MonoBehaviour
{
    [Inject] private NodeSpawnerService _spawner;
    [Inject] private ConnectionService _connectionService;
    [Inject] private GraphSaveLoadCoordinator _graphCoordinator;
    [Inject] private SubgraphLibraryService _library;
    [Inject] private IObjectResolver _resolver;
    [Inject] private PersistenceService _persistence;

    private SubgraphDefinition _currentDefinition;
    private bool _isEditing;
    private string _mainGraphTempSavePath;

    public bool IsEditing => _isEditing;
    public SubgraphDefinition CurrentDefinition => _currentDefinition;

    public event Action OnEditStarted;
    public event Action OnEditEnded;

    public void OpenSubgraphEditor(SubgraphDefinition definition)
    {
        if (_isEditing) CloseEditor(saveChanges: false);

        _mainGraphTempSavePath = "_TempMainGraph";
        _graphCoordinator.SaveGraph(_mainGraphTempSavePath);

        _currentDefinition = definition;
        _isEditing = true;
        NodeEngine.Core.NodeEngine.SetIsClearingGraph(true);
        try
        {
            _graphCoordinator.ClearCurrentGraph();
        }
        finally
        {
            NodeEngine.Core.NodeEngine.SetIsClearingGraph(false);
        }

        if (definition.nodes.Count > 0)
        {
            RestoreSubgraphNodes(definition);
        }
        else
        {
            CreateDefaultIONodes(definition);
        }

        OnEditStarted?.Invoke();
    }

    public void SaveAndClose()
    {
        if (_currentDefinition == null) return;

        SaveCurrentGraphToDefinition();
        _library.SaveDefinition(_currentDefinition);
        CloseEditor(saveChanges: true);
    }

    public void CloseEditor(bool saveChanges)
    {
        if (!_isEditing) return;

        _graphCoordinator.ClearCurrentGraph();
        _currentDefinition = null;
        _isEditing = false;

        if (!string.IsNullOrEmpty(_mainGraphTempSavePath) && _graphCoordinator.GetSaveFiles().Contains(_mainGraphTempSavePath))
        {
            _graphCoordinator.LoadGraph(_mainGraphTempSavePath);
            _graphCoordinator.DeleteSaveFile(_mainGraphTempSavePath);
        }

        OnEditEnded?.Invoke();
    }

    private void SaveCurrentGraphToDefinition()
    {
        var nodeLogics = _spawner.GetAllNodes().ToList();

        _currentDefinition.nodes.Clear();
        foreach (var logic in nodeLogics)
        {
            var node = logic.Node;
            string serialized = null;
            if (node is IVariableNode vn)
                serialized = _persistence.Serialize(vn);

            _currentDefinition.nodes.Add(new SubgraphDefinition.SerializedNode
            {
                nodeId = node.NodeId,
                nodeType = node.GetType().AssemblyQualifiedName,
                position = logic.transform.localPosition,
                serializedValue = serialized
            });
        }

        _currentDefinition.connections.Clear();
        foreach (var dc in _connectionService.ActiveDataConnections)
        {
            _currentDefinition.connections.Add(new SubgraphDefinition.SerializedConnection
            {
                sourceNodeId = dc.SourceNode.NodeId,
                sourcePortName = dc.OutputPortName,
                targetNodeId = dc.TargetNode.NodeId,
                targetPortName = dc.InputPortName,
                isFlow = false
            });
        }
        foreach (var fc in _connectionService.ActiveFlowConnections)
        {
            _currentDefinition.connections.Add(new SubgraphDefinition.SerializedConnection
            {
                sourceNodeId = fc.SourceNode.NodeId,
                sourcePortName = fc.SourcePortName,
                targetNodeId = fc.TargetNode.NodeId,
                targetPortName = fc.TargetPortName,
                isFlow = true
            });
        }

        _currentDefinition.MarkDirty();
        SyncPortDefinitionsFromNodes();
    }

    private void SyncPortDefinitionsFromNodes()
    {
        var inputNodes = new Dictionary<string, SubgraphInputNodeBase>();
        var outputNodes = new Dictionary<string, SubgraphOutputNodeBase>();

        foreach (var logic in _spawner.GetAllNodes())
        {
            if (logic.Node is SubgraphInputNodeBase input)
                inputNodes[logic.Node.NodeId] = input;
            else if (logic.Node is SubgraphOutputNodeBase output)
                outputNodes[logic.Node.NodeId] = output;
        }

        foreach (var port in _currentDefinition.inputPorts)
            port.boundNodeId = null;
        foreach (var port in _currentDefinition.outputPorts)
            port.boundNodeId = null;

        foreach (var kvp in inputNodes)
        {
            var node = kvp.Value;
            var portDef = _currentDefinition.inputPorts.FirstOrDefault(p => p.name == node.Ports[0].Name);
            if (portDef != null)
            {
                portDef.boundNodeId = kvp.Key;
                portDef.boundPortName = node.Ports[0].Name;
                portDef.typeName = node.GetValueType().AssemblyQualifiedName;
            }
        }

        foreach (var kvp in outputNodes)
        {
            var node = kvp.Value;
            var portDef = _currentDefinition.outputPorts.FirstOrDefault(p => p.name == node.Ports[0].Name);
            if (portDef != null)
            {
                portDef.boundNodeId = kvp.Key;
                portDef.boundPortName = node.Ports[0].Name;
                portDef.typeName = node.GetValueType().AssemblyQualifiedName;
            }
        }

        _currentDefinition.MarkDirty();
    }

    private void RestoreSubgraphNodes(SubgraphDefinition definition)
    {
        var nodeLookup = new Dictionary<string, BaseNode>();

        foreach (var nodeData in definition.nodes)
        {
            Type nodeType = Type.GetType(nodeData.nodeType);
            if (nodeType == null) continue;

            BaseNode instance = (BaseNode)_resolver.Resolve(nodeType);
            if (instance is IVariableNode varNode && !string.IsNullOrEmpty(nodeData.serializedValue))
                _persistence.Deserialize(varNode, nodeData.serializedValue);

            var logic = _spawner.SpawnNode(instance, nodeData.position, nodeData.nodeId);
            if (logic != null) nodeLookup[nodeData.nodeId] = instance;
        }

        foreach (var connData in definition.connections)
        {
            if (!nodeLookup.TryGetValue(connData.sourceNodeId, out var source) ||
                !nodeLookup.TryGetValue(connData.targetNodeId, out var target))
                continue;

            Connector sourceConn = FindConnector(source, connData.sourcePortName, isOutput: true);
            Connector targetConn = FindConnector(target, connData.targetPortName, isOutput: false);

            if (sourceConn != null && targetConn != null)
                _connectionService.CreateConnection(sourceConn, targetConn);
        }
    }

    private Connector FindConnector(BaseNode node, string portName, bool isOutput)
    {
        var logic = node.LogicView;
        if (logic == null) return null;
        return isOutput
            ? logic.OutputConnectors.FirstOrDefault(c => c.PortName == portName)
            : logic.InputConnectors.FirstOrDefault(c => c.PortName == portName);
    }

    private void CreateDefaultIONodes(SubgraphDefinition definition)
    {
        foreach (var portDef in definition.inputPorts)
        {
            BaseNode node;
            if (portDef.isFlow)
            {
                node = new SubgraphFlowInputNode();
            }
            else
            {
                Type valueType = Type.GetType(portDef.typeName) ?? typeof(object);
                Type inputNodeType = typeof(SubgraphInputNode<>).MakeGenericType(valueType);
                node = (SubgraphInputNodeBase)Activator.CreateInstance(inputNodeType);
            }
            var logic = _spawner.SpawnNode(node, new Vector2(-300, definition.inputPorts.IndexOf(portDef) * 100));
            portDef.boundNodeId = node.NodeId;
            portDef.boundPortName = node.Ports[0].Name;
        }

        foreach (var portDef in definition.outputPorts)
        {
            BaseNode node;
            if (portDef.isFlow)
            {
                node = new SubgraphFlowOutputNode();
            }
            else
            {
                Type valueType = Type.GetType(portDef.typeName) ?? typeof(object);
                Type outputNodeType = typeof(SubgraphOutputNode<>).MakeGenericType(valueType);
                node = (SubgraphOutputNodeBase)Activator.CreateInstance(outputNodeType);
            }
            var logic = _spawner.SpawnNode(node, new Vector2(300, definition.outputPorts.IndexOf(portDef) * 100));
            portDef.boundNodeId = node.NodeId;
            portDef.boundPortName = node.Ports[0].Name;
        }
    }

    public void AddInputPort(string name, Type type, bool isFlow)
    {
        if (_currentDefinition == null) return;
        var portDef = _currentDefinition.AddPort(name, type, isInput: true, isFlow);

        BaseNode node;
        if (isFlow)
        {
            node = new SubgraphFlowInputNode();
        }
        else
        {
            Type nodeType = typeof(SubgraphInputNode<>).MakeGenericType(type);
            node = (SubgraphInputNodeBase)Activator.CreateInstance(nodeType);
        }

        var logic = _spawner.SpawnNode(node, new Vector2(-300, _currentDefinition.inputPorts.Count * 100));
        portDef.boundNodeId = node.NodeId;
        portDef.boundPortName = node.Ports[0].Name;
        _library.SaveDefinition(_currentDefinition);
    }

    public void AddOutputPort(string name, Type type, bool isFlow)
    {
        if (_currentDefinition == null) return;
        var portDef = _currentDefinition.AddPort(name, type, isInput: false, isFlow);

        BaseNode node;
        if (isFlow)
        {
            node = new SubgraphFlowOutputNode();
        }
        else
        {
            Type nodeType = typeof(SubgraphOutputNode<>).MakeGenericType(type);
            node = (SubgraphOutputNodeBase)Activator.CreateInstance(nodeType);
        }

        var logic = _spawner.SpawnNode(node, new Vector2(300, _currentDefinition.outputPorts.Count * 100));
        portDef.boundNodeId = node.NodeId;
        portDef.boundPortName = node.Ports[0].Name;
        _library.SaveDefinition(_currentDefinition);
    }

    public void RemovePort(string portId)
    {
        if (_currentDefinition == null) return;
        var port = _currentDefinition.inputPorts.Concat(_currentDefinition.outputPorts).FirstOrDefault(p => p.id == portId);
        if (port == null) return;

        if (!string.IsNullOrEmpty(port.boundNodeId))
        {
            var nodeLogic = _spawner.GetAllNodes().FirstOrDefault(l => l.Node.NodeId == port.boundNodeId);
            if (nodeLogic != null) _spawner.DeleteNode(nodeLogic);
        }

        _currentDefinition.RemovePort(portId);
        _library.SaveDefinition(_currentDefinition);
    }
}