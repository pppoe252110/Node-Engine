using System.Drawing;

public class AddNode : NodeBase
{
    private ConnectorValueFloat _a, _b, _result;

    [NodeValue("A", typeof(float), KnownColor.LawnGreen)]
    public void A(ConnectorValueFloat a) => _a = a;

    [NodeValue("B", typeof(float), KnownColor.LawnGreen)]
    public void B(ConnectorValueFloat b) => _b = b;

    [NodeValue("Result", typeof(float), KnownColor.LawnGreen)]
    public void Result(ConnectorValueFloat result)
    {
        _result = result;
        _result.SetValue(_a.GetValue() + _b.GetValue());
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueFloat>(true).SetFunc(A).ProvideDefaultValue(new ConnectorValueFloat(0)),
            new NodeField<ConnectorValueFloat>(true).SetFunc(B).ProvideDefaultValue(new ConnectorValueFloat(0))
        };
        outputFields = new()
        {
            new NodeField<ConnectorValueFloat>(false).SetFunc(Result).ProvideDefaultValue(new ConnectorValueFloat(0))
        };
    }
}