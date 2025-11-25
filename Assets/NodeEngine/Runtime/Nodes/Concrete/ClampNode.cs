using UnityEngine;

[NodePath("Math/Clamp")]
public class ClampNode : NodeBase
{
    private ConnectorValueFloat _value, _min, _max, _result;

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
        float val = _value.GetValue();
        float minVal = _min.GetValue();
        float maxVal = _max.GetValue();

        // Ensure min <= max
        if (minVal > maxVal)
        {
            float temp = minVal;
            minVal = maxVal;
            maxVal = temp;
        }

        _result.SetValue(Mathf.Clamp(val, minVal, maxVal));
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueFloat>(true).SetHandler(Value).SetDefaultValue(new ConnectorValueFloat(0)),
            new NodeField<ConnectorValueFloat>(true).SetHandler(Min).SetDefaultValue(new ConnectorValueFloat(0)),
            new NodeField<ConnectorValueFloat>(true).SetHandler(Max).SetDefaultValue(new ConnectorValueFloat(1))
        };
        outputFields = new()
        {
            new NodeField<ConnectorValueFloat>(false).SetHandler(Result).SetDefaultValue(new ConnectorValueFloat(0))
        };
    }
}