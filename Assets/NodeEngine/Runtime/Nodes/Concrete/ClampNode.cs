using UnityEngine;

[NodePath("Math/Clamp")]
public class ClampNode : ExecutableNodeBase
{
    private ConnectorValueFloat _value, _min, _max, _result;
    private NodeFieldTyped<ConnectorValueFloat> _valueField, _minField, _maxField, _resultField;

    [NodeValue("Value", typeof(float))]
    public void Value(ConnectorValueFloat value) => _value = value;

    [NodeValue("Min", typeof(float))]
    public void Min(ConnectorValueFloat min) => _min = min;

    [NodeValue("Max", typeof(float))]
    public void Max(ConnectorValueFloat max) => _max = max;

    [NodeValue("Result", typeof(float))]
    public void Result(ConnectorValueFloat result)
    {
        _result = result;
    }

    public override void Execute()
    {
        // Process input values
        _valueField?.ProceedValue();
        _minField?.ProceedValue();
        _maxField?.ProceedValue();

        // Calculate result
        if (_value != null && _min != null && _max != null && _result != null)
        {
            float val = _value.GetInnerValue();
            float minVal = _min.GetInnerValue();
            float maxVal = _max.GetInnerValue();

            // Ensure min <= max
            if (minVal > maxVal)
            {
                float temp = minVal;
                minVal = maxVal;
                maxVal = temp;
            }

            float clampedValue = Mathf.Clamp(val, minVal, maxVal);
            _result.SetInnerValue(clampedValue);

            // Trigger output field
            _resultField?.ProceedValue();
        }
    }

    public override void Setup()
    {
        base.Setup();

        _value = new ConnectorValueFloat(0);
        _min = new ConnectorValueFloat(0);
        _max = new ConnectorValueFloat(1);
        _result = new ConnectorValueFloat(0);

        _valueField = new NodeFieldTyped<ConnectorValueFloat>().SetHandler(Value).SetDefaultValue(_value);
        _minField = new NodeFieldTyped<ConnectorValueFloat>().SetHandler(Min).SetDefaultValue(_min);
        _maxField = new NodeFieldTyped<ConnectorValueFloat>().SetHandler(Max).SetDefaultValue(_max);
        _resultField = new NodeFieldTyped<ConnectorValueFloat>().SetHandler(Result).SetDefaultValue(_result);

        inputFields.Add(_valueField);
        inputFields.Add(_minField);
        inputFields.Add(_maxField);
        outputFields.Add(_resultField);
    }
}