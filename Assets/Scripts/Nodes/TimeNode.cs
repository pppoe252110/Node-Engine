using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;

public class TimeNode : NodeBase
{
    [NodeValue("DeltaTime", typeof(float), KnownColor.LawnGreen)]
    public void DeltaTime(ConnectorValueFloat value)
    {
        value.SetValue(UnityEngine.Time.deltaTime);
    }

    [NodeValue("Time", typeof(float), KnownColor.LawnGreen)]
    public void Time(ConnectorValueFloat value)
    {
        value.SetValue(UnityEngine.Time.time);
    }

    public override void Setup()
    {
        outputFields = new()
        {
            new NodeField<ConnectorValueFloat>(false).SetFunc(DeltaTime).ProvideDefaultValue(new ConnectorValueFloat(0)),
            new NodeField<ConnectorValueFloat>(false).SetFunc(Time).ProvideDefaultValue(new ConnectorValueFloat(0))
        };
    }

    public override void Process(List<Connector> fromConnectors = null)
    {
        base.Process(fromConnectors);
    }
}