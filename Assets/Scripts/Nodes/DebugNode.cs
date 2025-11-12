using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class DebugNode : ExecutableNode
{
    private ConnectorValueVoid _log;
    private ConnectorValueString _logText;

    public override void Execute()
    {
        Debug.LogError(_logText.GetValue());
    }

    [NodeValue("Event", typeof(void), KnownColor.BlueViolet)]
    public void Log(ConnectorValueVoid value)
    {
        _log = value;
    }

    [NodeValue("LogString", typeof(string), KnownColor.PaleVioletRed)]
    public void LogString(ConnectorValueString value)
    {
        _logText = value;
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueVoid>(true).SetFunc(Log).ProvideDefaultValue(new ConnectorValueVoid(null)),
            new NodeField<ConnectorValueString>(true).SetFunc(LogString).ProvideDefaultValue(new ConnectorValueString(""))
        };
    }
}