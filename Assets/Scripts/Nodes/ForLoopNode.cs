using System.Drawing;
using UnityEngine;

public class ForLoopNode : ExecutableNode
{
    private ConnectorValueVoid _execute;
    private ConnectorValueInt _count;
    private ConnectorValueVoid _body, _end;
    private ConnectorValueInt _index;

    // Store references to the actual NodeField objects
    private NodeField<ConnectorValueVoid> _bodyField, _endField;

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

        for (int i = 0; i < loopCount; i++)
        {
            Debug.Log($"ForLoopNode: Iteration {i}");

            // Set the current index value
            _index?.SetValue(i);

            // Trigger body execution - propagate through the body field
            if (_bodyField != null)
            {
                Debug.Log($"ForLoopNode: Triggering body for iteration {i}");
                _bodyField.ProceedValue();
            }
        }

        Debug.Log($"ForLoopNode: Loop completed");

        // Trigger end execution
        if (_endField != null)
        {
            Debug.Log($"ForLoopNode: Triggering end");
            _endField.ProceedValue();
        }
    }

    public override void Setup()
    {
        // Create the fields and store references
        _bodyField = new NodeField<ConnectorValueVoid>(false).SetFunc(Body).ProvideDefaultValue(new ConnectorValueVoid(null));
        _endField = new NodeField<ConnectorValueVoid>(false).SetFunc(End).ProvideDefaultValue(new ConnectorValueVoid(null));

        inputFields = new()
        {
            new NodeField<ConnectorValueVoid>(true).SetFunc(ExecuteInput).ProvideDefaultValue(new ConnectorValueVoid(null)),
            new NodeField<ConnectorValueInt>(true).SetFunc(Count).ProvideDefaultValue(new ConnectorValueInt(0))
        };
        outputFields = new()
        {
            _bodyField,
            new NodeField<ConnectorValueInt>(false).SetFunc(Index).ProvideDefaultValue(new ConnectorValueInt(0)),
            _endField
        };
    }
}