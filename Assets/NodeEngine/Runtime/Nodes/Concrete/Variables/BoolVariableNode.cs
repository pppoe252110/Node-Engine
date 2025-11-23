using System.Drawing;

public class BoolVariableNode : VariableNodeBase<ConnectorValueBool>
{
    public override VariableType VariableType => VariableType.Bool;

    [NodeValue("Value", typeof(bool), KnownColor.Blue)]
    public override void Output(ConnectorValueBool value) { }
}