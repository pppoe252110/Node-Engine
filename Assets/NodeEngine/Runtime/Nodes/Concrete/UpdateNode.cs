using System.Collections.Generic;

[NodePath("Events/Update")]
public class UpdateNode : NodeBase
{
    [NodeValue("Update", typeof(void))]
    public void UpdateVoid(ConnectorValueVoid value) { }

    public override void Setup()
    {
        outputFields = new()
        {
            new NodeFieldTyped<ConnectorValueVoid>().SetHandler(UpdateVoid).SetDefaultValue(new ConnectorValueVoid())
        };
    }
}
