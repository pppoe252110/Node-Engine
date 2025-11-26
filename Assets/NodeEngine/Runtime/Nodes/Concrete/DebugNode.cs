using UnityEngine;

[NodePath("Debug/Log")]
public class DebugNode : ExecutableNodeBase
{
    private ConnectorValueString _logText;

    [NodeValue("LogString", typeof(string))]
    public void LogString(ConnectorValueString value)
    {
        _logText = value;
    }

    public override void Execute()
    {
        string text = _logText?.GetValue() ?? "null";

        if (ConsoleUI.Instance != null)
        {
            ConsoleUI.Instance.LogMessage($"[Debug] {text}");
        }
        else
        {
            Debug.Log($"[Debug] {text}");
        }

        base.Execute();
    }

    public override void Setup()
    {
        inputFields = new()
        {
            new NodeField<ConnectorValueString>(true).SetHandler(LogString).SetDefaultValue(new ConnectorValueString(""))
        };
        
        base.Setup();
    }
}