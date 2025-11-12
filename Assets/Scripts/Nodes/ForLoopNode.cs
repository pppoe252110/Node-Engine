using System.Drawing;

public class ForLoopNode : ExecutableNode
{
    private ConnectorValueInt _count;  // Assume you add ConnectorValueInt
    private ConnectorValueVoid _body, _end;
    private ConnectorValueInt _index;

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
        for (int i = 0; i < _count.GetValue(); i++)
        {
            _index.SetValue(i);
            _body?.ValueUpdated?.Invoke(_body);  // Trigger body
        }
        _end?.ValueUpdated?.Invoke(_end);
    }

    public override void Setup()
    {
        inputFields = new()
        {
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