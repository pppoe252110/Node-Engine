using UnityEngine;

[NodePath("Conversion/ToString")]
public class ToStringNode : NodeBase
{
    private ConnectorValueObject _input;
    private ConnectorValueString _output;

    [NodeValue("Input", typeof(object))]
    public void Input(ConnectorValueObject input)
    {
        _input = input;
        
        if (outputFields.Count > 0)
        {
            outputFields[0].ProceedValue();  
        }
    }

    [NodeValue("Output", typeof(string))]
    public void Output(ConnectorValueString output)
    {
        _output = output;
        if (_output != null && _input != null)
        {
            _output.SetInnerValue(_input.GetInnerValue()?.ToString() ?? "");
        }
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueObject>().SetHandler(Input).SetDefaultValue(new ConnectorValueObject(null))
        };
        outputFields = new()
        {
            new NodeField<ConnectorValueString>().SetHandler(Output).SetDefaultValue(new ConnectorValueString(""))
        };
    }
}
