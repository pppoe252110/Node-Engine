[NodePath("Math/Subtract")]
public class SubtractNode : NodeBase
{
    private ConnectorValueFloat _a, _b, _result;

    [NodeValue("A", typeof(float))]
    public void A(ConnectorValueFloat a) => _a = a;

    [NodeValue("B", typeof(float))]
    public void B(ConnectorValueFloat b) => _b = b;

    [NodeValue("Result", typeof(float))]
    public void Result(ConnectorValueFloat result)
    {
        _result = result;
        _result.SetInnerValue(_a.GetInnerValue() - _b.GetInnerValue());
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueFloat>().SetHandler(A).SetDefaultValue(new ConnectorValueFloat(0)),
            new NodeField<ConnectorValueFloat>().SetHandler(B).SetDefaultValue(new ConnectorValueFloat(0))
        };
        outputFields = new()
        {
            new NodeField<ConnectorValueFloat>().SetHandler(Result).SetDefaultValue(new ConnectorValueFloat(0))
        };
    }
}
