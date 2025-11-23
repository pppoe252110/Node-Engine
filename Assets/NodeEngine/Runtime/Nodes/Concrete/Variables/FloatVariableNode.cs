using System.Drawing;

public class FloatVariableNode : VariableNodeBase<ConnectorValueFloat>
{
    public override VariableType VariableType => VariableType.Float;

    [NodeValue("Value", typeof(float), KnownColor.Green)]
    public override void Output(ConnectorValueFloat value) { }
}