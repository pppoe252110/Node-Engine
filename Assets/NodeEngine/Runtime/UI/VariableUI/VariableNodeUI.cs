using TMPro;
using UnityEngine;

public class VariableNodeUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _singleInputFieldPrefab; // Prefab for single box (int, float, string)
    [SerializeField] private TMP_InputField _vectorInputFieldPrefab; // Prefab for Vector3 boxes (can be the same as above)

    private VariableType _type;
    private object _value; // Runtime value storage (not serialized)
    private TMP_InputField[] _inputFields; // Array of spawned fields

    public void Initialize(VariableType type)
    {
        _type = type;
        SpawnUI();
        SetDefaultValue();
    }

    private void SpawnUI()
    {
        var parent = transform.Find("LeftConnectorsParent"); // Assuming _leftConnectorsParent is a child named this; adjust if needed
        if (parent == null) return;

        switch (_type)
        {
            case VariableType.Int:
            case VariableType.Float:
            case VariableType.String:
                // Spawn 1 field
                _inputFields = new TMP_InputField[1];
                _inputFields[0] = Instantiate(_singleInputFieldPrefab, parent);
                _inputFields[0].onValueChanged.AddListener(UpdateValue);
                break;
            case VariableType.Vector3:
                // Spawn 3 fields (x, y, z)
                _inputFields = new TMP_InputField[3];
                for (int i = 0; i < 3; i++)
                {
                    _inputFields[i] = Instantiate(_vectorInputFieldPrefab, parent);
                    int index = i; // Capture for lambda
                    _inputFields[i].onValueChanged.AddListener(value => UpdateVectorValue(index, value));
                    // Optionally set placeholder text: _inputFields[i].placeholder.GetComponent<TextMeshProUGUI>().text = (i == 0 ? "X" : i == 1 ? "Y" : "Z");
                }
                break;
                // Add cases for other types (e.g., Bool could use a Toggle)
        }
    }

    private void SetDefaultValue()
    {
        switch (_type)
        {
            case VariableType.Int: _value = 0; _inputFields[0].text = "0"; break;
            case VariableType.Float: _value = 0f; _inputFields[0].text = "0"; break;
            case VariableType.String: _value = ""; _inputFields[0].text = ""; break;
            case VariableType.Vector3: _value = Vector3.zero; UpdateVectorUI(); break;
        }
    }

    private void UpdateValue(string value)
    {
        switch (_type)
        {
            case VariableType.Int: if (int.TryParse(value, out int i)) _value = i; break;
            case VariableType.Float: if (float.TryParse(value, out float f)) _value = f; break;
            case VariableType.String: _value = value; break;
        }
    }

    private void UpdateVectorValue(int index, string value)
    {
        if (_value is Vector3 vec && float.TryParse(value, out float f))
        {
            vec[index] = f; // index 0=x, 1=y, 2=z
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