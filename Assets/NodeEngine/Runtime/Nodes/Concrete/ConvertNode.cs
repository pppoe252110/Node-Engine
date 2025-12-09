using System;
using UnityEngine;

[NodePath("Conversion/Convert")]
public class ConvertNode : ExecutableNodeBase
{
    private ConnectorValueObject _inputValue;
    private ConnectorValueType _targetType;
    private ConnectorValueObject _outputValue;
    private NodeField<ConnectorValueObject> _inputValueField;
    private NodeField<ConnectorValueType> _targetTypeField;
    private NodeField<ConnectorValueObject> _outputValueField; // Store this reference!

    [NodeValue("InputValue", typeof(object))]
    public void InputValue(ConnectorValueObject value) => _inputValue = value;

    [NodeValue("TargetType", typeof(Type))]
    public void TargetType(ConnectorValueType type) => _targetType = type;

    [NodeValue("OutputValue", typeof(object))]
    public void Output(ConnectorValueObject output)
    {
        _outputValue = output;
    }

    public override void Setup()
    {
        _inputValue = new ConnectorValueObject(null);
        _targetType = new ConnectorValueType(typeof(object));
        _outputValue = new ConnectorValueObject(null);

        _inputValueField = new NodeField<ConnectorValueObject>().SetHandler(InputValue).SetDefaultValue(_inputValue);
        _targetTypeField = new NodeField<ConnectorValueType>().SetHandler(TargetType).SetDefaultValue(_targetType);

        // Store the output field reference!
        _outputValueField = new NodeField<ConnectorValueObject>().SetHandler(Output).SetDefaultValue(_outputValue);

        inputFields = new()
        {
            _inputValueField,
            _targetTypeField
        };

        base.Setup();

        outputFields.Add(_outputValueField);
    }

    public override void Execute()
    {
        _inputValueField?.ProceedValue();
        _targetTypeField?.ProceedValue();

        object input = _inputValue?.GetInnerValue();
        Type targetType = _targetType?.GetInnerValue();

        if (input == null || targetType == null)
        {
            Debug.LogWarning("Input or target type is null");
            _outputValue?.SetValue(null);
        }
        else
        {
            try
            {
                // Convert the value
                object converted = Convert.ChangeType(input, targetType);
                _outputValue?.SetInnerValue(converted);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Conversion failed: {e.Message}. Input: {input} ({input.GetType()}) to {targetType}");
                _outputValue?.SetValue(null);
            }
        }

        // Push the output value to connected nodes
        if (_outputValueField != null)
        {
            _outputValueField.ProceedValue();
        }

        base.Execute();
    }
}