[NodePath("Math/Divide")]
public class DivideNode : NodeBase
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
        float denominator = _b.GetValue();

        
        if (denominator == 0)
        {
            _result.SetValue(0);
        }
        else
        {
            _result.SetValue(_a.GetValue() / denominator);
        }
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueFloat>(true).SetHandler(A).SetDefaultValue(new ConnectorValueFloat(0)),
            new NodeField<ConnectorValueFloat>(true).SetHandler(B).SetDefaultValue(new ConnectorValueFloat(1))
        };
        outputFields = new()
        {
            new NodeField<ConnectorValueFloat>(false).SetHandler(Result).SetDefaultValue(new ConnectorValueFloat(0))
        };
    }
}
