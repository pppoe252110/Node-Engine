using System.Collections.Generic;
using System.Drawing;

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
        string stringValue = inputValue.ToString();

        value.SetValue(stringValue);
        _output = value;
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueObject>(true).SetHandler(Input).SetDefaultValue(new ConnectorValueObject(null)),
        };
        outputFields = new()
        {
            new NodeField<ConnectorValueString>(false).SetHandler(Output).SetDefaultValue(new ConnectorValueString("")),
        };
    }

    public override void Process(List<Connector> fromConnectors = null)
    {
        base.Process(fromConnectors);

        // Force output update
        if (_output != null && _input != null)
        {
            var inputValue = _input.GetValue();
            string stringValue = inputValue?.ToString() ?? "null";

            _output.SetValue(stringValue);
        }
    }
}