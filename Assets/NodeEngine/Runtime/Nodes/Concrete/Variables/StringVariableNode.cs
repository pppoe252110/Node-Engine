using System.Drawing;

public class StringVariableNode : VariableNodeBase<ConnectorValueString>
{
    public override VariableType VariableType => VariableType.String;

    [NodeValue("Value", typeof(string), KnownColor.Yellow)]
    public override void Output(ConnectorValueString value) { }
}