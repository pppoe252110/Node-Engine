using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownUIElement : VariableUIElement
{
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private VariableDatabase _variableDatabase;

    private VariableNode _node;
    private VariableType _type;
    private List<object> _optionValues = new();
    private bool _isUpdating;

    public override void Initialize(VariableNode node, VariableType type)
    {
        base.Initialize(node, type);
        _node = node;
        _type = type;

        BuildOptions();
        _dropdown.onValueChanged.AddListener(OnDropdownChanged);
        UpdateUIFromNode();

        if (_node != null)
            _node.OnValueChanged += OnNodeValueChanged;
    }

    private void BuildOptions()
    {
        _dropdown.ClearOptions();
        _optionValues.Clear();

        var uiOptions = _variableDatabase?.GetUIOptions(_type);
        if (uiOptions != null && uiOptions.dropdownOptions != null && uiOptions.dropdownOptions.Count > 0)
        {
            var displayNames = uiOptions.dropdownOptions;
            var serializedValues = uiOptions.dropdownValues;
            _dropdown.AddOptions(displayNames);
            for (int i = 0; i < displayNames.Count; i++)
            {
                string valStr = (serializedValues != null && i < serializedValues.Count) ? serializedValues[i] : displayNames[i];
                _optionValues.Add(ParseValueFromString(valStr));
            }
            return;
        }

        // 2. Auto-generate for enums
        Type systemType = VariableNode.GetSystemType(_type);
        if (systemType.IsEnum)
        {
            var enumValues = Enum.GetValues(systemType);
            var displayNames = new List<string>();
            foreach (var val in enumValues)
            {
                displayNames.Add(val.ToString());
                _optionValues.Add(val);
            }
            _dropdown.AddOptions(displayNames);
            return;
        }

        // 3. Default for Type
        if (_type == VariableType.Type)
        {
            var commonTypes = new (string Name, Type Type)[]
            {
                ("Float", typeof(float)),
                ("Integer", typeof(int)),
                ("Boolean", typeof(bool)),
                ("String", typeof(string)),
                ("Vector3", typeof(Vector3)),
                ("GameObject", typeof(GameObject)),
                ("Object", typeof(object))
            };
            var names = new List<string>();
            foreach (var t in commonTypes)
            {
                names.Add(t.Name);
                _optionValues.Add(t.Type);
            }
            _dropdown.AddOptions(names);
            return;
        }

        Debug.LogError($"[DropdownUIElement] No options defined for VariableType {_type}");
    }

    private object ParseValueFromString(string str)
    {
        Type sysType = VariableNode.GetSystemType(_type);
        if (sysType.IsEnum)
            return Enum.Parse(sysType, str);
        if (_type == VariableType.Type)
            return Type.GetType(str) ?? typeof(object);
        // Extend for other custom types
        return str;
    }

    private void OnDropdownChanged(int index)
    {
        if (_isUpdating || _node == null || index < 0 || index >= _optionValues.Count)
            return;

        _isUpdating = true;
        object selectedValue = _optionValues[index];
        _node.SetValue(selectedValue);
        _isUpdating = false;
    }

    private void UpdateUIFromNode()
    {
        if (_node == null) return;
        object currentValue = _node.GetValue();
        int index = _optionValues.IndexOf(currentValue);
        if (index >= 0)
            _dropdown.SetValueWithoutNotify(index);
        else
            Debug.LogWarning($"[DropdownUIElement] Current value '{currentValue}' not found in dropdown options for {_type}");
    }

    private void OnNodeValueChanged(object newValue)
    {
        UpdateUIFromNode();
    }

    public override object GetValue() => _node?.GetValue();

    public override void SetValue(object value)
    {
        int index = _optionValues.IndexOf(value);
        if (index >= 0)
        {
            _dropdown.SetValueWithoutNotify(index);
            if (_node != null)
                _node.SetValue(value);
        }
        else
        {
            Debug.LogWarning($"[DropdownUIElement] Cannot set value '{value}' - not in options.");
        }
    }

    private void OnDestroy()
    {
        if (_dropdown != null)
            _dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        if (_node != null)
            _node.OnValueChanged -= OnNodeValueChanged;
    }
}