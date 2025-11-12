using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class ForLoopNode : ExecutableNode
{
    private ConnectorValueVoid _execute;
    private ConnectorValueInt _count;
    private ConnectorValueVoid _body, _end;
    private ConnectorValueInt _index;

    [NodeValue("Execute", typeof(void), KnownColor.BlueViolet)]
    public void ExecuteInput(ConnectorValueVoid execute) => _execute = execute;

    [NodeValue("Count", typeof(int), KnownColor.Purple)]
    public void Count(ConnectorValueInt count) => _count = count;

    [NodeValue("Body", typeof(void), KnownColor.BlueViolet)]
    public void Body(ConnectorValueVoid body) => _body = body;

    [NodeValue("Index", typeof(int), KnownColor.Purple)]
    public void Index(ConnectorValueInt index) => _index = index;

    [NodeValue("End", typeof(void), KnownColor.BlueViolet)]
    public void End(ConnectorValueVoid end) => _end = end;

    public override void Execute()
    {
        int loopCount = _count?.GetValue() ?? 0;
        Debug.Log($"ForLoopNode: Starting loop with count {loopCount}");

        // Process input values first to ensure we have the latest count
        if (_count != null)
        {
            _count.ProceedValue();
        }

        for (int i = 0; i < loopCount; i++)
        {
            Debug.Log($"ForLoopNode: Iteration {i}");

            // Set the current index value and trigger updates
            _index?.SetValue(i);

            // Process the index output to ensure downstream nodes get the value
            if (_index != null)
            {
                _index.ProceedValue();
            }

            // Trigger the body execution and process connected nodes
            _body?.ValueUpdated?.Invoke(_body);

            // If body is connected to other nodes, process them
            if (_body != null && this is NodeBase nodeBase)
            {
                // This will process the entire execution chain for this iteration
                var connectors = new List<Connector>();
                foreach (var connector in nodeBase.outputConnectors)
                {
                    if (connector.Field.GetAttribute().attributeName == "Body")
                    {
                        connector.Process(connectors);
                        break;
                    }
                }
            }
        }

        Debug.Log($"ForLoopNode: Loop completed");

        // Trigger the end execution after loop completes
        _end?.ValueUpdated?.Invoke(_end);
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueVoid>(true).SetFunc(ExecuteInput).ProvideDefaultValue(new ConnectorValueVoid(null)),
            new NodeField<ConnectorValueInt>(true).SetFunc(Count).ProvideDefaultValue(new ConnectorValueInt(0))
        };
        outputFields = new()
        {
            new NodeField<ConnectorValueVoid>(false).SetFunc(Body).ProvideDefaultValue(new ConnectorValueVoid(null)),
            new NodeField<ConnectorValueInt>(false).SetFunc(Index).ProvideDefaultValue(new ConnectorValueInt(0)),
            new NodeField<ConnectorValueVoid>(false).SetFunc(End).ProvideDefaultValue(new ConnectorValueVoid(null))
        };
    }
}