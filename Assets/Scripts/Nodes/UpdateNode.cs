using System.Drawing;
using UnityEngine;

public class UpdateNode : NodeBase
{
    private ConnectorVoid _update = new(default);

    [NodeValue("Update", typeof(void), KnownColor.BlueViolet)]
    public void UpdateVoid(ConnectorVoid valueVoid)
    {
        Debug.Log("Update Method Called");
    }

    private void Update()
    {
        _update.SetValue(default);    
    }

    public override void Setup()
    {
        outputFields = new()
        {
            new NodeField<ConnectorVoid>().SetFunc(UpdateVoid).ProvideDefaultValue(_update)
        };
    }
}
