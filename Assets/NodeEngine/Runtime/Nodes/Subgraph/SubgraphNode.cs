using NodeEngine.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

[HideInNodeList]
[NodePath("Subgraph/Subgraph Instance")]
public class SubgraphNode : BaseNode, IGraphSerializable
{
    [SerializeField] private string _subgraphId;

    private SubgraphDefinition _definition;
    private SubgraphLibraryService _library;
    private IObjectResolver _resolver;
    private PersistenceService _persistence;

    public SubgraphDefinition Definition
    {
        get => _definition;
        set
        {
            if (_definition == value) return;
            _definition = value;
            _subgraphId = value?.subgraphId;
            RebuildPorts();
            UpdateDisplayName();
        }
    }

    [Inject]
    public void Construct(SubgraphLibraryService library, IObjectResolver resolver, PersistenceService persistence)
    {
        _library = library;
        _resolver = resolver;
        _persistence = persistence;
    }

    public void ResolveDefinition()
    {
        if (_definition == null && !string.IsNullOrEmpty(_subgraphId))
        {
            _definition = _library?.GetDefinition(_subgraphId);
            if (_definition != null)
            {
                RebuildPorts();
            }
        }
        UpdateDisplayName();
        ApplyMissingVisuals();
    }

    public override void Initialize(NodeLogic logic, string guid)
    {
        base.Initialize(logic, guid);
        
        ResolveDefinition();

        if (_definition == null && !string.IsNullOrEmpty(_subgraphId))
        {
            _definition = _library?.GetDefinition(_subgraphId);
            if (_definition != null)
            {
                RebuildPorts();
            }
            else
            {
                Debug.LogWarning($"[SubgraphNode] Subgraph with ID '{_subgraphId}' not found.");
            }
        }

        UpdateDisplayName();
        ApplyMissingVisuals();
    }

    private void UpdateDisplayName()
    {
        if (_definition != null)
        {
            NodeName = _definition.subgraphName;
            if (LogicView != null && LogicView.NodeNameText != null)
                LogicView.NodeNameText.text = _definition.subgraphName;
        }
        else
        {
            NodeName = "⚠ Missing Subgraph";
            if (LogicView != null && LogicView.NodeNameText != null)
                LogicView.NodeNameText.text = NodeName;
        }
    }

    private void ApplyMissingVisuals()
    {
        if (LogicView == null) return;

        // Optionally tint the node red or add an icon
        if (_definition == null)
        {
            LogicView.BackgroundImage.color = new Color(1f, 0.3f, 0.3f, 1f);
        }
    }

    private void RebuildPorts()
    {
        Ports.Clear();
        if (_definition == null) return;

        foreach (var portDef in _definition.InputPorts)
        {
            Ports.Add(new NodePortInfo
            {
                Name = portDef.name,
                IsInput = true,
                IsFlow = portDef.isFlow,
                ValueType = Type.GetType(portDef.typeName) ?? typeof(object),
                Order = 0
            });
        }

        foreach (var portDef in _definition.OutputPorts)
        {
            Ports.Add(new NodePortInfo
            {
                Name = portDef.name,
                IsInput = false,
                IsFlow = portDef.isFlow,
                ValueType = Type.GetType(portDef.typeName) ?? typeof(object),
                Order = 0
            });
        }
    }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        if (_definition == null)
        {
            Debug.LogError($"[SubgraphNode] Cannot compile – subgraph ID '{_subgraphId}' not found.");
            return _ => ExecutionResult.Halt();
        }

        if (_definition == null) return _ => ExecutionResult.Stop();

        if (_definition.CompiledGraph == null)
            _definition.InstantiateNodes(_resolver, _persistence);

        var compiledSubgraph = _definition.CompiledGraph;
        var inputMappings = BuildPortMappings(context, isInput: true);
        var outputMappings = BuildPortMappings(context, isInput: false);
        var inputNodeMap = _definition.GetInputNodeMap();
        var outputNodeMap = _definition.GetOutputNodeMap();

        // Find the flow entry node (SubgraphFlowInputNode) and exit flow target
        int entryFlowIndex = -1;
        int exitFlowIndex = GetFlowTargetIndex(context, isInput: false);

        // Locate the flow input node in the compiled graph
        foreach (var kvp in compiledSubgraph.NodeToIndex)
        {
            if (kvp.Key is SubgraphFlowInputNode)
            {
                entryFlowIndex = kvp.Value;
                break;
            }
        }

        return parentCtx =>
        {
            // Pass data inputs
            foreach (var kvp in inputMappings)
            {
                string portId = kvp.Key;
                int parentMemId = kvp.Value;
                if (inputNodeMap.TryGetValue(portId, out var inputNode))
                {
                    object val = parentCtx.Read<object>(parentMemId);
                    inputNode.SetValueFromParentUntyped(val);
                }
            }

            // Execute all data-only nodes (pull model)
            foreach (var node in compiledSubgraph.NodeToIndex.Keys)
            {
                if (!node.Ports.Any(p => p.IsFlow))
                    compiledSubgraph.ExecuteNode(node);
            }

            // Execute flow starting from entry node
            if (entryFlowIndex >= 0)
            {
                compiledSubgraph.Context.ExecuteFlow(entryFlowIndex);
            }

            // Retrieve outputs
            foreach (var kvp in outputMappings)
            {
                string portId = kvp.Key;
                int parentMemId = kvp.Value;
                if (outputNodeMap.TryGetValue(portId, out var outputNode))
                {
                    object val = outputNode.GetUntypedValue();
                    parentCtx.Write(parentMemId, val);
                }
            }

            return ExecutionResult.Continue(exitFlowIndex);
        };
    }

    private Dictionary<string, int> BuildPortMappings(NodeCompilationContext context, bool isInput)
    {
        var dict = new Dictionary<string, int>();
        var portDefs = isInput ? _definition.InputPorts : _definition.OutputPorts;
        foreach (var portDef in portDefs)
        {
            if (portDef.isFlow) continue; // flow ports don't use memory
            int id = isInput ? context.GetInputId(portDef.name) : context.GetOutputId(portDef.name);
            if (id >= 0) dict[portDef.id] = id;
        }
        return dict;
    }

    private int GetFlowTargetIndex(NodeCompilationContext context, bool isInput)
    {
        var portDefs = isInput ? _definition.InputPorts : _definition.OutputPorts;
        var flowPort = portDefs.FirstOrDefault(p => p.isFlow);
        if (flowPort == null) return -1;
        return isInput ? context.GetFlowId(flowPort.name) : context.GetFlowId(flowPort.name);
    }

    public string SerializeCustomData()
    {
        var data = new SubgraphNodeData { subgraphId = _subgraphId };
        return JsonUtility.ToJson(data);
    }

    public void DeserializeCustomData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;
        try
        {
            var loaded = JsonUtility.FromJson<SubgraphNodeData>(data);
            _subgraphId = loaded.subgraphId;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SubgraphNode] Failed to deserialize custom data: {e.Message}");
        }
    }

    [Serializable]
    private class SubgraphNodeData
    {
        public string subgraphId;
    }
}