using System.Drawing;
using UnityEngine;

public class DebugNode : NodeBase
{
    private ConnectorVoid _log;
    private ConnectorValueFloat _logText;

    [NodeValue("Event", typeof(void), KnownColor.BlueViolet)]
    public void Log(ConnectorVoid valueVoid)
    {
        Debug.Log(_logText.GetValue());
    }
    
    [NodeValue("LogString", typeof(string), KnownColor.PaleVioletRed)]
    public void LogString(ConnectorValueFloat valueVoid)
    {
        Debug.Log(valueVoid);
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorVoid>().SetFunc(Log).ProvideDefaultValue(_log),
            new NodeField<ConnectorValueFloat>().SetFunc(LogString).ProvideDefaultValue(_logText)
        };
    }

}
