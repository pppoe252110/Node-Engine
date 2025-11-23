using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class UpdateNode : NodeBase
{
    [NodeValue("Update", typeof(void), KnownColor.BlueViolet)]
    public void UpdateVoid(ConnectorValueVoid value)
    {
        // Trigger only; no value
    }

    public override void Setup()
    {
        outputFields = new()
        {
            new NodeField<ConnectorValueVoid>(false).SetHandler(UpdateVoid).SetDefaultValue(new ConnectorValueVoid())
        };
    }
}