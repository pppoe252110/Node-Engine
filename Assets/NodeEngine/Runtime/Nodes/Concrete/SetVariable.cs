using System.Collections.Generic;

[NodePath("Variables/Set")]
public class SetVariableNode : ExecutableNodeBase
{
    private static Dictionary<string, object> _variables = new();  

    private ConnectorValueObject _value;
    private ConnectorValueString _name;

    [NodeValue("Value", typeof(object))]
    public void Value(ConnectorValueObject value) => _value = value;

    [NodeValue("Name", typeof(string))]
    public void Name(ConnectorValueString name) => _name = name;

    public override void Execute()
    {
        if (!string.IsNullOrEmpty(_name.GetInnerValue()))
            _variables[_name.GetInnerValue()] = _value.GetValue();

        base.Execute();
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueObject>().SetHandler(Value).SetDefaultValue(new ConnectorValueObject(null)),
            new NodeField<ConnectorValueString>().SetHandler(Name).SetDefaultValue(new ConnectorValueString(""))
        };

        base.Setup();
    }
}
