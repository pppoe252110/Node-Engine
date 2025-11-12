using TMPro;
using UnityEngine;
using System.Globalization;

public class InputFieldVariableUI : VariableUIElement
{
    [SerializeField] private TMP_InputField _inputField;

    private VariableType _type;
    private object _value;
    private CultureInfo _culture;

    public override void Initialize(VariableDatabase database, VariableType type)
    {
        _type = type;
        _culture = CultureInfo.InvariantCulture; // Use invariant culture for consistent parsing
        SetupInputFieldForType();
        SetDefaultValue();
        _inputField.onValueChanged.AddListener(UpdateValue);
    }

    private void SetupInputFieldForType()
    {
        switch (_type)
        {
            case VariableType.Int:
                _inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                _inputField.characterLimit = 10;
                break;
            case VariableType.Single:
                _inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
                _inputField.characterLimit = 15;
                break;
            case VariableType.String:
                _inputField.contentType = TMP_InputField.ContentType.Standard;
                _inputField.characterLimit = 0;
                break;
            case VariableType.Vector3:
                _inputField.contentType = TMP_InputField.ContentType.Standard; // Allow any input for Vector3
                _inputField.characterLimit = 50;
                if (_inputField.placeholder != null)
                    _inputField.placeholder.GetComponent<TMP_Text>().text = "x; y; z  (use semicolons)";
                break;
        }
    }

    private void UpdateValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            SetDefaultValue();
            return;
        }

        try
        {
            switch (_type)
            {
                case VariableType.Int:
                    if (int.TryParse(value, NumberStyles.Integer, _culture, out int i))
                        _value = i;
                    else
                        FormatIntValue();
                    break;

                case VariableType.Single:
                    if (float.TryParse(value, NumberStyles.Float, _culture, out float f))
                        _value = f;
                    else
                        FormatFloatValue();
                    break;

                case VariableType.String:
                    _value = value;
                    break;

                case VariableType.Vector3:
                    ParseVector3Value(value);
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to parse value '{value}' for type {_type}: {e.Message}");
            FormatCurrentValue();
        }
    }

    private void ParseVector3Value(string value)
    {
        // Use semicolons as separators to avoid conflict with decimal commas
        string[] separators = new string[] { ";", "|", " " }; // Try multiple separators
        string[] parts = value.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 3)
        {
            // Take first 3 parts
            string xStr = parts[0].Trim();
            string yStr = parts[1].Trim();
            string zStr = parts[2].Trim();

            // Replace comma decimal separator with period for parsing
            xStr = xStr.Replace(",", ".");
            yStr = yStr.Replace(",", ".");
            zStr = zStr.Replace(",", ".");

            if (float.TryParse(xStr, NumberStyles.Float, _culture, out float x) &&
                float.TryParse(yStr, NumberStyles.Float, _culture, out float y) &&
                float.TryParse(zStr, NumberStyles.Float, _culture, out float z))
            {
                _value = new Vector3(x, y, z);
                FormatVector3Value();
                return;
            }
        }
        else if (parts.Length == 1)
        {
            // Single value - use for all components
            string singleValue = parts[0].Trim().Replace(",", ".");
            if (float.TryParse(singleValue, NumberStyles.Float, _culture, out float uniformValue))
            {
                _value = new Vector3(uniformValue, uniformValue, uniformValue);
                FormatVector3Value();
                return;
            }
        }

        // If parsing failed, set to default
        SetDefaultValue();
    }

    private void SetDefaultValue()
    {
        switch (_type)
        {
            case VariableType.Int:
                _value = 0;
                _inputField.text = "0";
                break;
            case VariableType.Single:
                _value = 0f;
                _inputField.text = "0.0";
                break;
            case VariableType.String:
                _value = "";
                _inputField.text = "";
                break;
            case VariableType.Vector3:
                _value = Vector3.zero;
                _inputField.text = "0.0; 0.0; 0.0";
                break;
        }
    }

    private void FormatCurrentValue()
    {
        switch (_type)
        {
            case VariableType.Int:
                FormatIntValue();
                break;
            case VariableType.Single:
                FormatFloatValue();
                break;
            case VariableType.Vector3:
                FormatVector3Value();
                break;
        }
    }

    private void FormatIntValue()
    {
        if (_value is int intValue)
        {
            _inputField.SetTextWithoutNotify(intValue.ToString(_culture));
        }
    }

    private void FormatFloatValue()
    {
        if (_value is float floatValue)
        {
            // Format with period as decimal separator, but allow user to input commas
            _inputField.SetTextWithoutNotify(floatValue.ToString("0.00", _culture));
        }
    }

    private void FormatVector3Value()
    {
        if (_value is Vector3 vectorValue)
        {
            // Use semicolons as separators, periods for decimals
            string formatted = $"{vectorValue.x:0.00}; {vectorValue.y:0.00}; {vectorValue.z:0.00}";
            _inputField.SetTextWithoutNotify(formatted);
        }
    }

    public void SetValue(object newValue)
    {
        if (newValue != null)
        {
            if ((_type == VariableType.Int && newValue is int) ||
                (_type == VariableType.Single && newValue is float) ||
                (_type == VariableType.String && newValue is string) ||
                (_type == VariableType.Vector3 && newValue is Vector3))
            {
                _value = newValue;
                FormatCurrentValue();
            }
            else
            {
                Debug.LogWarning($"Type mismatch: Cannot assign {newValue.GetType()} to {_type}");
            }
        }
    }

    public override object GetValue() => _value;

    // Handle locale-specific decimal separators
    private string ConvertToInvariantFormat(string input)
    {
        // Replace comma decimal separator with period for parsing
        return input.Replace(",", ".");
    }

    private void Start()
    {
        _inputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnEndEdit(string value)
    {
        // Re-format when user finishes editing
        FormatCurrentValue();
    }

    private void OnDestroy()
    {
        if (_inputField != null)
        {
            _inputField.onValueChanged.RemoveListener(UpdateValue);
            _inputField.onEndEdit.RemoveListener(OnEndEdit);
        }
    }
}