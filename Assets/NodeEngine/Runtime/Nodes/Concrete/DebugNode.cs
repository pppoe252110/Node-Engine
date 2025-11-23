using System.Drawing;
using UnityEngine;

[NodePath("Debug/Log")]
public class DebugNode : ExecutableNodeBase
{
    private ConnectorValueString _logText;

    [NodeValue("LogString", typeof(string), KnownColor.PaleVioletRed)]
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
        // Setup execution flow first
        base.Setup();

        // Add other input fields
        inputFields.Add(
            new NodeField<ConnectorValueString>(true)
                .SetHandler(LogString)
                .SetDefaultValue(new ConnectorValueString(""))
        );
    }
}