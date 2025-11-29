using System.Collections.Generic;
using UnityEngine;

[NodePath("Control Flow/For Loop")]
public class ForLoopNode : ExecutableNodeBase
{
    // Data Fields
    private ConnectorValueInt _count;
    private ConnectorValueInt _index;

    // Execution Flow Fields
    private NodeField _bodyField;
    private NodeField<ConnectorValueInt> _indexField;

    // --- Input Handlers ---
    [NodeValue("Count", typeof(int))]
    public void Count(ConnectorValueInt count)
    {
        _count = count;
    }

    // --- Output Handlers ---
    [NodeValue("Body", typeof(void))]
    public void Body(IConnectorValue body)
    {
        // This handler doesn't need to do anything
    }

    [NodeValue("Index", typeof(int))]
    public void Index(ConnectorValueInt index)
    {
        // This handler doesn't need to do anything
    }

    public override void Execute()
    {
        // Get the loop count
        int loopCount = _count?.GetInnerValue() ?? 0;

        for (int i = 0; i < loopCount; i++)
        {
            if (_indexField != null)
            {
                _index.SetInnerValue(i);
                _indexField.ProceedValue();
            }

            // Execute the body
            if (_bodyField != null)
            {
                _bodyField.ProceedValue();
            }
        }

        // Trigger the main output execution
        base.Execute();
    }

    public override void Setup()
    {
        // IMPORTANT: Always call the base Setup() first

        // --- Setup Data Fields ---
        var countField = new NodeField<ConnectorValueInt>(true)
            .SetHandler(Count)
            .SetDefaultValue(new ConnectorValueInt(5)); // Default to 5 iterations

        _index = new ConnectorValueInt(0);

        // Create index field with default value
        _indexField = new NodeField<ConnectorValueInt>(false)
            .SetHandler(Index)
            .SetDefaultValue(_index);

        // Store reference to the index value

        // --- Setup Execution Flow Fields ---
        _bodyField = new NodeField().SetHandler(Body);

        // --- Add Fields to Node ---
        inputFields = new() 
        {
            countField 
        };

        outputFields = new()
        {
            _bodyField,
            _indexField
        };

        base.Setup();
    }
}