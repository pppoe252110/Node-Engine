public abstract class VariableNodeBase<T> : NodeBase where T : class, IConnectorValue, new()
{
    public abstract VariableType VariableType { get; }
    public VariableUIElement UIElement { get; set; }

    private NodeField<T> _outputField;

    public override void Setup()
    {
        _outputField = new NodeField<T>(isInput: false)
            .SetFunc(Output)
            .ProvideDefaultValue(new T());

        outputFields = new() { _outputField };
    }

    public abstract void Output(T value);

    public void UpdateOutputValue()
    {
        if (UIElement != null && _outputField != null)
        {
            var val = UIElement.GetValue();
            var currentValue = _outputField.GetObjectValue() as T;

            if (currentValue != null)
            {
                SetTypedValue(currentValue, val);
                _outputField.ProceedValue();
            }
        }
    }

    private void SetTypedValue(T connector, object value)
    {
        switch (connector)
        {
            case ConnectorValueInt intConnector when value is int intVal:
                intConnector.SetValue(intVal);
                break;
            case ConnectorValueFloat floatConnector when value is float floatVal:
                floatConnector.SetValue(floatVal);
                break;
            case ConnectorValueBool boolConnector when value is bool boolVal:
                boolConnector.SetValue(boolVal);
                break;
            case ConnectorValueString stringConnector when value is string stringVal:
                stringConnector.SetValue(stringVal);
                break;
        }
    }

    public object GetCurrentValue()
    {
        return _outputField?.GetObjectValue();
    }
}