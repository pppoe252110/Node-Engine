using System.Drawing;
using UnityEngine;

public class ToStringNode : NodeBase
{
    private ConnectorValueObject _input;
    private ConnectorValueString _output;

    [NodeValue("Input", typeof(object), KnownColor.DarkSlateBlue)]
    public void Input(ConnectorValueObject value)
    {
        _input = value;
    }

    [NodeValue("Output", typeof(string), KnownColor.PaleVioletRed)]
    public void Output(ConnectorValueString value)
    {
        var inputValue = _input?.GetValue() ?? "null";
        value.SetValue(inputValue.ToString());
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueObject>(true).SetFunc(Input).ProvideDefaultValue(new ConnectorValueObject(null)),
        };
        outputFields = new()
        {
            new NodeField<ConnectorValueString>(false).SetFunc(Output).ProvideDefaultValue(new ConnectorValueString("")),
        };
    }
}