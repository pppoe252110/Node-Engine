using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;

public class TimeNode : NodeBase
{
    private ConnectorValueFloat _deltaTime = new(0);
    private ConnectorValueFloat _scaledTime = new(0);

    [NodeValue("DeltaTime", typeof(float), KnownColor.LawnGreen)]
    public void DeltaTime(ConnectorValueFloat valueFloat)
    {
        valueFloat.SetValue(Time.deltaTime);
    }

    [NodeValue("ScaledTime", typeof(float), KnownColor.LawnGreen)]
    public void ScaledTime(ConnectorValueFloat valueFloat)
    {
        valueFloat.SetValue(Time.deltaTime);
    }

    public override void Setup()
    {
        outputFields = new()
        {
            new NodeField<ConnectorValueFloat>().SetFunc(DeltaTime).ProvideDefaultValue(_deltaTime),
            new NodeField<ConnectorValueFloat>().SetFunc(ScaledTime).ProvideDefaultValue(_scaledTime)
        };
    }
}
