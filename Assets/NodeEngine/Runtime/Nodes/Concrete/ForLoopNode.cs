[NodePath("Control Flow/For Loop")]
public class ForLoopNode : ExecutableNodeBase
{
    private ConnectorValueVoid _execute;
    private ConnectorValueInt _count;
    private ConnectorValueVoid _body, _end;
    private ConnectorValueInt _index;

    private NodeField<ConnectorValueVoid> _bodyField, _endField;
    private NodeField<ConnectorValueInt> _indexField;

    [NodeValue("Execute", typeof(void))]
    public void ExecuteInput(ConnectorValueVoid execute) => _execute = execute;

    [NodeValue("Count", typeof(int))]
    public void Count(ConnectorValueInt count) => _count = count;

    [NodeValue("Body", typeof(void))]
    public void Body(ConnectorValueVoid body) => _body = body;

    [NodeValue("Index", typeof(int))]
    public void Index(ConnectorValueInt index) => _index = index;

    [NodeValue("End", typeof(void))]
    public void End(ConnectorValueVoid end) => _end = end;

    public override void Execute()
    {
        Process();

        int loopCount = _count?.GetValue() ?? 0;

        for (int i = 0; i < loopCount; i++)
        {
            _index?.SetValue(i);
            _bodyField.ProceedValue();
        }

        _endField.ProceedValue();
    }
    public override void Setup()
    {
        var bodyVoid = new ConnectorValueVoid();
        var endVoid = new ConnectorValueVoid();

        _bodyField = new NodeField<ConnectorValueVoid>(false).SetHandler(Body).SetDefaultValue(bodyVoid);
        _endField = new NodeField<ConnectorValueVoid>(false).SetHandler(End).SetDefaultValue(endVoid);
        _indexField = new NodeField<ConnectorValueInt>(false).SetHandler(Index).SetDefaultValue(new ConnectorValueInt(0));  

        inputFields = new()
        {
            new NodeField<ConnectorValueVoid>(true).SetHandler(ExecuteInput).SetDefaultValue(new ConnectorValueVoid()),
            new NodeField<ConnectorValueInt>(true).SetHandler(Count).SetDefaultValue(new ConnectorValueInt(0))
        };
        outputFields = new()
        {
            _bodyField,
            _indexField,
            _endField
        };

        _body = bodyVoid;
        _end = endVoid;
        _index = new ConnectorValueInt(0);
    }
}
