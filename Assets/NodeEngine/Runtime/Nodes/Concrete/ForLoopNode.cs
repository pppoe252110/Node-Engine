using System.Collections.Generic;
using UnityEngine;

[NodePath("Control Flow/For Loop")]
public class ForLoopNode : ExecutableNodeBase
{
    
    private ConnectorValueInt _count;
    private ConnectorValueInt _index;

    private NodeField _bodyField;
    private NodeField<ConnectorValueInt> _indexField;

    [NodeValue("Count", typeof(int))]
    public void Count(ConnectorValueInt count)
    {
        _count = count;
    }

    [NodeValue("Body", typeof(void))]
    public void Body(IConnectorValue body)
    {
        
    }

    [NodeValue("Index", typeof(int))]
    public void Index(ConnectorValueInt index)
    {
        
    }

    public override void Execute()
    {
        
        int loopCount = _count?.GetInnerValue() ?? 0;

        for (int i = 0; i < loopCount; i++)
        {
            if (_indexField != null)
            {
                _index.SetInnerValue(i);
                _indexField.ProceedValue();
            }

            if (_bodyField != null)
            {
                _bodyField.ProceedValue();
            }
        }

        base.Execute();
    }

    public override void Setup()
    {
        
        var countField = new NodeField<ConnectorValueInt>()
            .SetHandler(Count)
            .SetDefaultValue(new ConnectorValueInt(5)); 

        _index = new ConnectorValueInt(0);

        _indexField = new NodeField<ConnectorValueInt>()
            .SetHandler(Index)
            .SetDefaultValue(_index);

        _bodyField = new NodeField().SetHandler(Body);

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
