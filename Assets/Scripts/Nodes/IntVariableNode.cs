using System.Drawing;

public class IntVariableNode : VariableNodeBase<ConnectorValueInt>
{
    public override VariableType VariableType => VariableType.Int;

    [NodeValue("Value", typeof(int), KnownColor.Red)]
    public override void Output(ConnectorValueInt value)
    {
        // Ensure the value is properly set from the UI
        if (UIElement != null)
        {
            var uiValue = UIElement.GetValue();
            if (uiValue is int intVal)
            {
                value.SetValue(intVal);
            }
        }
    }

    // Add a method to force value update
    public void UpdateValue(int newValue)
    {
        // Update the UI element if exists
        if (UIElement != null && UIElement is InputFieldVariableUI inputField)
        {
            inputField.SetValue(newValue);
        }

        // Update the output field
        if (outputFields.Count > 0 && outputFields[0] is NodeField<ConnectorValueInt> outputField)
        {
            outputField.ProceedValue();
        }
    }
}