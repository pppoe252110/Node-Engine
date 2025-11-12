using System.Drawing;

public class AddNode : NodeBase
{
    private ConnectorValueSingle _a, _b, _result;

    [NodeValue("A", typeof(float), KnownColor.LawnGreen)]
    public void A(ConnectorValueSingle a) => _a = a;

    [NodeValue("B", typeof(float), KnownColor.LawnGreen)]
    public void B(ConnectorValueSingle b) => _b = b;

    [NodeValue("Result", typeof(float), KnownColor.LawnGreen)]
    public void Result(ConnectorValueSingle result)
    {
        _result = result;
        _result.SetValue(_a.GetValue() + _b.GetValue());
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueSingle>(true).SetFunc(A).ProvideDefaultValue(new ConnectorValueSingle(0)),
            new NodeField<ConnectorValueSingle>(true).SetFunc(B).ProvideDefaultValue(new ConnectorValueSingle(0))
        };
        outputFields = new()
        {
            new NodeField<ConnectorValueSingle>(false).SetFunc(Result).ProvideDefaultValue(new ConnectorValueSingle(0))
        };
    }
}