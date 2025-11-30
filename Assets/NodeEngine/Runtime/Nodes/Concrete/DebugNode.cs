using UnityEngine;

[NodePath("Debug/Log")]
public class DebugNode : ExecutableNodeBase
{
    private ConnectorValueString _logText;
    private NodeField<ConnectorValueString> _inputField;

    [NodeValue("LogString", typeof(string))]
    public void LogString(ConnectorValueString value)
    {
        _logText = value;
    }

    public override void Execute()
    {
        _inputField.ProceedValue();
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
        _logText = new ConnectorValueString();
        _inputField = new NodeField<ConnectorValueString>().SetHandler(LogString).SetDefaultValue(_logText);

        inputFields = new()
        {
            _inputField
        };
        
        base.Setup();
    }
}