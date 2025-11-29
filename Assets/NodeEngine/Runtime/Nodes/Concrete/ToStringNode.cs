using UnityEngine;

[NodePath("Conversion/ToString")]
public class ToStringNode : NodeBase
{
    private ConnectorValueObject _input;
    private ConnectorValueString _output;

    [NodeValue("Input", typeof(object))]
    public void Input(ConnectorValueObject input)
    {
        Debug.LogError("In");
        _input = input;
        // NEW: Trigger output computation and propagation when input changes
        if (outputFields.Count > 0)
        {
            outputFields[0].ProceedValue();  // Proceed the "Output" field
        }
    }

    [NodeValue("Output", typeof(string))]
    public void Output(ConnectorValueString output)
    {
        Debug.LogError("Out");
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
            new NodeField<ConnectorValueObject>(true).SetHandler(Input).SetDefaultValue(new ConnectorValueObject(null))
        };
        outputFields = new()
        {
            new NodeField<ConnectorValueString>(false).SetHandler(Output).SetDefaultValue(new ConnectorValueString(""))
        };
    }
}