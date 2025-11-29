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
        string text = _logText?.GetInnerValue() ?? "null";
        if (ConsoleUI.Instance != null)
        {
            //ConsoleUI.Instance.LogMessage($"[Debug] {text}");
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
            new NodeField<ConnectorValueString>().SetHandler(LogString).SetDefaultValue(new ConnectorValueString(""))
        };
        
        base.Setup();
    }
}