using UnityEngine;

[NodePath("Control Flow/For Loop")]
public class ForLoopNode : ExecutableNodeBase
{
    private ConnectorValueInt _count;
    private NodeFieldTyped<ConnectorValueInt> _countField;
    private ConnectorValueInt _index;

    private NodeField _bodyField;
    private NodeFieldTyped<ConnectorValueInt> _indexField;

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
        _index = index;
    }

    public override void Execute()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        _countField.ProceedValue();

        var indexField = _indexField;
        var bodyField = _bodyField;
        var indexValue = _index;

        int loopCount = _count?.GetInnerValue() ?? 0;

        for (int i = 0; i < loopCount; i++)
        {
            if (indexField != null)
            {
                indexValue.SetInnerValue(i);
            }

            if (bodyField != null)
            {
                bodyField.ProceedValue();
            }
        }

        sw.Stop();
        Debug.LogError(sw.ElapsedMilliseconds + "ms");
        base.Execute();
    }

    public override void Setup()
    {
        _countField = new NodeFieldTyped<ConnectorValueInt>()
            .SetHandler(Count)
            .SetDefaultValue(new ConnectorValueInt(5));

        _index = new ConnectorValueInt(0);

        _indexField = new NodeFieldTyped<ConnectorValueInt>()
            .SetHandler(Index)
            .SetDefaultValue(_index);

        _bodyField = new NodeField().SetHandler(Body);

        inputFields = new()
        {
            _countField
        };

        outputFields = new()
        {
            _bodyField,
            _indexField
        };

        base.Setup();
    }
}
