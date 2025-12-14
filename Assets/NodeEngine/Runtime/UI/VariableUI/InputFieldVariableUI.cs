using System.Globalization;
using TMPro;
using UnityEngine;

public class InputFieldVariableUI : VariableUIElement
{
    [SerializeField] private TMP_InputField _inputField;

    private object _value;
    private CultureInfo _culture;

    public override void Initialize(VariableNode node, VariableType type)
    {
        base.Initialize(node, type);

        _type = type;

        _culture = CultureInfo.InvariantCulture;

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
                _inputField.contentType = TMP_InputField.ContentType.Standard; 
                _inputField.characterLimit = 50;
                if (_inputField.placeholder != null)
                    _inputField.placeholder.GetComponent<TMP_Text>().text = "x; y; z  (use semicolons)";
                break;
        }
    }

    public void UpdateValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            SetDefaultValue();
            return;
        }

        bool isValid = false;
        object newValue = null;

        try
        {
            switch (_type)
            {
                case VariableType.Int:
                    if (int.TryParse(value, NumberStyles.Integer, _culture, out int i))
                    {
                        newValue = i;
                        isValid = true;
                        
                        _inputField.SetTextWithoutNotify(i.ToString(_culture));
                    }
                    break;
                case VariableType.Single:
                    if (float.TryParse(value, NumberStyles.Float, _culture, out float f))
                    {
                        newValue = f;
                        isValid = true;
                        _inputField.SetTextWithoutNotify(f.ToString("0.00", _culture));  
                    }
                    break;
                case VariableType.String:
                    newValue = value;
                    isValid = true;
                    
                    break;
                case VariableType.Vector3:
                    
                    ParseVector3Value(value);
                    if (_value is Vector3)  
                    {
                        isValid = true;
                        FormatVector3Value();
                    }
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to parse value '{value}' for type {_type}: {e.Message}");
        }

        if (isValid && newValue != null)
        {
            _value = newValue;
        }
        else
        {
            
            FormatCurrentValue();
        }

        _node?.UpdateOutputValue();
    }

    private void ParseVector3Value(string value)
    {
        
        string[] separators = new string[] { ";", "|", " " }; 
        string[] parts = value.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 3)
        {
            
            string xStr = parts[0].Trim();
            string yStr = parts[1].Trim();
            string zStr = parts[2].Trim();

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
            
            string singleValue = parts[0].Trim().Replace(",", ".");
            if (float.TryParse(singleValue, NumberStyles.Float, _culture, out float uniformValue))
            {
                _value = new Vector3(uniformValue, uniformValue, uniformValue);
                FormatVector3Value();
                return;
            }
        }

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
            
            _inputField.SetTextWithoutNotify(floatValue.ToString("0.00", _culture));
        }
    }

    private void FormatVector3Value()
    {
        if (_value is Vector3 vectorValue)
        {
            
            string formatted = $"{vectorValue.x:0.00}; {vectorValue.y:0.00}; {vectorValue.z:0.00}";
            _inputField.SetTextWithoutNotify(formatted);
        }
    }

    public override void SetValue(object value)
    {
        if (_inputField == null) return;

        if (value == null)
        {
            _inputField.text = "";
            return;
        }

        // Format based on type
        switch (_type)
        {
            case VariableType.Single:
                // For floats, use invariant culture to avoid comma issues
                if (value is float floatValue)
                {
                    _inputField.text = floatValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    _inputField.text = value.ToString();
                }
                break;
            case VariableType.Int:
            case VariableType.Bool:
            case VariableType.String:
            default:
                _inputField.text = value.ToString();
                break;
        }

        _value = value;
    }

    public override object GetValue() => _value;

    private void Start()
    {
        _inputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnEndEdit(string value)
    {
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
