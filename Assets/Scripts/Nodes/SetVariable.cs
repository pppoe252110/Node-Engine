using System.Collections.Generic;
using System.Drawing;

public class SetVariableNode : ExecutableNode
{
    private static Dictionary<string, object> _variables = new();  // Simple global storage

    private ConnectorValueObject _value;
    private ConnectorValueString _name;

    [NodeValue("Value", typeof(object), KnownColor.Gray)]
    public void Value(ConnectorValueObject value) => _value = value;

    [NodeValue("Name", typeof(string), KnownColor.Yellow)]
    public void Name(ConnectorValueString name) => _name = name;

    public override void Execute()
    {
        if (!string.IsNullOrEmpty(_name.GetValue()))
            _variables[_name.GetValue()] = _value.GetValue();
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueObject>(true).SetFunc(Value).ProvideDefaultValue(new ConnectorValueObject(null)),
            new NodeField<ConnectorValueString>(true).SetFunc(Name).ProvideDefaultValue(new ConnectorValueString(""))
        };
    }
}