using System;
using TMPro;
using UnityEngine;

public class InputFieldUIElement : VariableUIElement
{
    [SerializeField] private TMP_InputField _inputField;

    // This UI element can bind to strings, ints, and floats!
    public override bool CanBind(Type valueType)
    {
        return valueType == typeof(string) ||
               valueType == typeof(int) ||
               valueType == typeof(float);
    }

    public override void Bind(IVariableNode node)
    {
        base.Bind(node);
        _inputField.onValueChanged.AddListener(OnUIInputValueChanged);
    }

    private void OnUIInputValueChanged(string input)
    {
        if (TargetNode == null) return;

        try
        {
            // Dynamically convert the string to the node's required type
            object parsedValue = Convert.ChangeType(input, TargetNode.ValueType, System.Globalization.CultureInfo.InvariantCulture);
            TargetNode.SetUntypedValue(parsedValue);
        }
        catch
        {
            // Invalid input (e.g., typing "abc" into an int field). Ignore or show warning.
        }
    }

    protected override void OnNodeValueChanged(object newValue)
    {
        if (newValue != null)
        {
            // Set text without triggering the event to avoid infinite loops
            _inputField.SetTextWithoutNotify(newValue.ToString());
        }
    }
}