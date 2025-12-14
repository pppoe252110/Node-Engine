[NodePath("Math/Multiply")]
public class MultiplyNode : ExecutableNodeBase
{
    private ConnectorValueFloat _a, _b, _result;
    private NodeFieldTyped<ConnectorValueFloat> _aField, _bField, _resultField;

    [NodeValue("A", typeof(float))]
    public void A(ConnectorValueFloat a) => _a = a;

    [NodeValue("B", typeof(float))]
    public void B(ConnectorValueFloat b) => _b = b;

    [NodeValue("Result", typeof(float))]
    public void Result(ConnectorValueFloat result)
    {
        _result = result;
    }

    public override void Execute()
    {
        // Process input values
        _aField?.ProceedValue();
        _bField?.ProceedValue();

        // Calculate result
        if (_a != null && _b != null && _result != null)
        {
            float resultValue = _a.GetInnerValue() * _b.GetInnerValue();
            _result.SetInnerValue(resultValue);

            // Trigger output field
            _resultField?.ProceedValue();
        }
    }

    public override void Setup()
    {
        base.Setup();

        _a = new ConnectorValueFloat(0);
        _b = new ConnectorValueFloat(0);
        _result = new ConnectorValueFloat(0);

        _aField = new NodeFieldTyped<ConnectorValueFloat>().SetHandler(A).SetDefaultValue(_a);
        _bField = new NodeFieldTyped<ConnectorValueFloat>().SetHandler(B).SetDefaultValue(_b);
        _resultField = new NodeFieldTyped<ConnectorValueFloat>().SetHandler(Result).SetDefaultValue(_result);

        inputFields.Add(_aField);
        inputFields.Add(_bField);
        outputFields.Add(_resultField);
    }
}