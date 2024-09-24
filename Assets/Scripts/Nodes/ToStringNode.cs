using System.Drawing;
using UnityEngine;

public class ToStringNode : NodeBase
{
    private ConnectorValueString _output;
    private ConnectorValueObject _input;

    [NodeValue("Input", typeof(object), KnownColor.DarkSlateBlue)]
    public void Input(ConnectorValueObject valueFloat)
    {
        _input.SetValue(valueFloat.GetValue());
    }

    [NodeValue("Output", typeof(string), KnownColor.PaleVioletRed)]
    public void Output(ConnectorValueString valueFloat)
    {
        _output.SetValue(_input.GetValue().ToString());
    }
    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueObject>().SetFunc(Input).ProvideDefaultValue(_input),
        };
        outputFields = new()
        {
            new NodeField<ConnectorValueString>().SetFunc(Output).ProvideDefaultValue(_output),
        };
    }
}
