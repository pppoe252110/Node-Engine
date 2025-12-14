using UnityEngine;
using UnityEngine.UI;

[NodePath("Conversion/ToString")]
public class ToStringNode : ExecutableNodeBase
{
    private ConnectorValueObject _input;
    private ConnectorValueString _output;
    private NodeFieldTyped<ConnectorValueObject> _inputField;
    private NodeFieldTyped<ConnectorValueString> _outputField;

    [NodeValue("InputValue", typeof(object))]
    public void Input(ConnectorValueObject input)
    {
        _input = input;
    }

    [NodeValue("OutputValue", typeof(string))]
    public void Output(ConnectorValueString output)
    {
        _output = output;
    }

    public override void Execute()
    {
        // First, make sure we have the latest input value
        if (_inputField != null)
        {
            _inputField.ProceedValue(); // This pulls the latest value from connected nodes
        }

        // Now convert the value
        if (_output != null && _input != null)
        {
            var innerValue = _input.GetInnerValue();
            string result = innerValue?.ToString() ?? "null";

            _output.SetInnerValue(result);

            // Push the output to connected nodes
            if (_outputField != null)
            {
                _outputField.ProceedValue();
            }
        }

        base.Execute();
    }

    public override void Setup()
    {
        base.Setup();

        // Initialize with default values
        _input = new ConnectorValueObject(null);
        _output = new ConnectorValueString("");

        // Create fields
        _inputField = new NodeFieldTyped<ConnectorValueObject>().SetHandler(Input).SetDefaultValue(_input);
        _outputField = new NodeFieldTyped<ConnectorValueString>().SetHandler(Output).SetDefaultValue(_output);

        inputFields.Add(_inputField);
        outputFields.Add(_outputField);
    }
}