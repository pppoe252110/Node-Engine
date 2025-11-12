using TMPro;
using UnityEngine;

public class InputFieldVariableUI : VariableUIElement  // Inherit from base class
{
    [SerializeField] private TMP_InputField _inputField;

    private VariableType _type;
    private object _value;

    public override void Initialize(VariableDatabase database, VariableType type)
    {
        _type = type;
        SetDefaultValue();
        _inputField.onValueChanged.AddListener(UpdateValue);
    }

    private void UpdateValue(string value)
    {
        // Your existing parsing logic from VariableInputField
        switch (_type)
        {
            case VariableType.Int: if (int.TryParse(value, out int i)) _value = i; break;
            case VariableType.Float: if (float.TryParse(value, out float f)) _value = f; break;
            case VariableType.String: _value = value; break;
            case VariableType.Vector3:
                var parts = value.Split(',');
                if (parts.Length == 3 &&
                    float.TryParse(parts[0], out float x) &&
                    float.TryParse(parts[1], out float y) &&
                    float.TryParse(parts[2], out float z))
                {
                    _value = new Vector3(x, y, z);
                }
                break;
        }
    }

    private void SetDefaultValue()
    {
        // Your existing default logic
        switch (_type)
        {
            case VariableType.Int: _value = 0; _inputField.text = "0"; break;
            case VariableType.Float: _value = 0f; _inputField.text = "0"; break;
            case VariableType.String: _value = ""; _inputField.text = ""; break;
            case VariableType.Vector3: _value = Vector3.zero; _inputField.text = "0,0,0"; break;
        }
    }

    public override object GetValue() => _value;  // Required by base class
}