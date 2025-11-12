using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class UpdateNode : NodeBase
{
    [NodeValue("Update", typeof(void), KnownColor.BlueViolet)]
    public void UpdateVoid(ConnectorVoid value)
    {
        // Trigger only; no value
    }

    public override void Setup()
    {
        outputFields = new()
        {
            new NodeField<ConnectorVoid>(false).SetFunc(UpdateVoid).ProvideDefaultValue(new ConnectorVoid(default))
        };
    }
}