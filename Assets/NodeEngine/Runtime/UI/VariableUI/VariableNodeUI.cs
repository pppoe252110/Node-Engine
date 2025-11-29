using TMPro;
using UnityEngine;

public class VariableNodeUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _singleInputFieldPrefab; 
    [SerializeField] private TMP_InputField _vectorInputFieldPrefab; 

    private VariableType _type;
    private object _value; 
    private TMP_InputField[] _inputFields; 

    public void Initialize(VariableType type)
    {
        _type = type;
        SpawnUI();
        SetDefaultValue();
    }

    private void SpawnUI()
    {
        var parent = transform.Find("LeftConnectorsParent"); 
        if (parent == null) return;

        switch (_type)
        {
            case VariableType.Int:
            case VariableType.Single:
            case VariableType.String:
                
                _inputFields = new TMP_InputField[1];
                _inputFields[0] = Instantiate(_singleInputFieldPrefab, parent);
                _inputFields[0].onValueChanged.AddListener(UpdateValue);
                break;
            case VariableType.Vector3:
                
                _inputFields = new TMP_InputField[3];
                for (int i = 0; i < 3; i++)
                {
                    _inputFields[i] = Instantiate(_vectorInputFieldPrefab, parent);
                    int index = i; 
                    _inputFields[i].onValueChanged.AddListener(value => UpdateVectorValue(index, value));
                    
                }
                break;
                
        }
    }

    private void SetDefaultValue()
    {
        switch (_type)
        {
            case VariableType.Int: _value = 0; _inputFields[0].text = "0"; break;
            case VariableType.Single: _value = 0f; _inputFields[0].text = "0"; break;
            case VariableType.String: _value = ""; _inputFields[0].text = ""; break;
            case VariableType.Vector3: _value = Vector3.zero; UpdateVectorUI(); break;
        }
    }

    private void UpdateValue(string value)
    {
        switch (_type)
        {
            case VariableType.Int: if (int.TryParse(value, out int i)) _value = i; break;
            case VariableType.Single: if (float.TryParse(value, out float f)) _value = f; break;
            case VariableType.String: _value = value; break;
        }
    }

    private void UpdateVectorValue(int index, string value)
    {
        if (_value is Vector3 vec && float.TryParse(value, out float f))
        {
            vec[index] = f; 
            _value = vec;
        }
    }

    private void UpdateVectorUI()
    {
        if (_value is Vector3 vec)
        {
            for (int i = 0; i < 3; i++)
            {
                _inputFields[i].text = vec[i].ToString();
            }
        }
    }

    public object GetValue() => _value;
}
