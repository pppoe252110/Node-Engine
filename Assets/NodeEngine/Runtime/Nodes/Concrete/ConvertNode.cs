using System;
using UnityEngine;

[NodePath("Conversion/Convert")]
public class ConvertNode : ExecutableNodeBase, IConnectionListener
{
    private ConnectorValueObject _inputValue;
    private ConnectorValueType _targetType;
    private ConnectorValueObject _outputValue;
    private NodeFieldTyped<ConnectorValueObject> _inputValueField;
    private NodeFieldTyped<ConnectorValueType> _targetTypeField;
    private NodeFieldTyped<ConnectorValueObject> _outputValueField;

    private Type _currentOutputType = typeof(object);

    [NodeValue("InputValue", typeof(object))]
    public void InputValue(ConnectorValueObject value) => _inputValue = value;

    [NodeValue("TargetType", typeof(Type))]
    public void TargetType(ConnectorValueType type)
    {
        _targetType = type;

        if (!NodeEngine.IsExecuting)
            UpdateOutputType();
    }

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

        _inputValueField = new NodeFieldTyped<ConnectorValueObject>().SetHandler(InputValue).SetDefaultValue(_inputValue);
        _targetTypeField = new NodeFieldTyped<ConnectorValueType>().SetHandler(TargetType).SetDefaultValue(_targetType);
        _outputValueField = new NodeFieldTyped<ConnectorValueObject>().SetHandler(Output).SetDefaultValue(_outputValue);

        inputFields = new()
        {
            _inputValueField,
            _targetTypeField
        };

        base.Setup();

        outputFields.Add(_outputValueField);
    }

    private void UpdateOutputType()
    {
        if (_targetType == null || _outputValueField == null)
            return;

        Type newType = _targetType.GetInnerValue();
        if (newType == null)
            return;

        // Skip if type hasn't changed
        if (newType == _currentOutputType)
            return;

        // Use the centralized TypeChangeService (value update now happens here)
        bool success = _outputValueField.TryUpdateOutputType(newType);

        if (success)
        {
            _currentOutputType = newType;
        }
        else
        {
            Debug.LogWarning($"ConvertNode: Failed to update output type to {newType.Name}");
        }
    }

    public override void Execute()
    {
        _targetTypeField?.ProceedValue();
        _inputValueField?.ProceedValue();

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

        if (_outputValueField != null)
        {
            _outputValueField.ProceedValue();
        }

        base.Execute();
    }

    // IConnectionListener implementation
    public void OnConnected(Connector myConnector, Connector otherConnector)
    {
        // When target type field is connected, update the output type
        if (myConnector?.Field == _targetTypeField)
        {
            UpdateOutputType();
        }
    }

    public void OnDisconnected(Connector myConnector, Connector otherConnector)
    {
        // Handle disconnection if needed
        if (myConnector?.Field == _targetTypeField)
        {
            // Reset to default type when disconnected
            _targetType?.SetInnerValue(typeof(object));
            UpdateOutputType();
        }
    }

    protected override void Initialized()
    {
        base.Initialized();

        // Register this node as a connection listener for its connectors
        if (_nodeLogic != null)
        {
            foreach (var connector in inputConnectors)
            {
                connector?.AddConnectionListener(this);
            }

            // Also listen to output connectors to detect when they're connected
            foreach (var connector in outputConnectors)
            {
                connector?.AddConnectionListener(this);
            }
        }
    }
}