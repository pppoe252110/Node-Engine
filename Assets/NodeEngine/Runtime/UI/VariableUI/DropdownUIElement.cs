using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownUIElement : VariableUIElement
{
    [SerializeField] private TMP_Dropdown _typeDropdown;

    private TypeVariableNode _typeNode;
    private Dictionary<int, Type> _typeMapping = new Dictionary<int, Type>();

    public void Initialize(TypeVariableNode typeNode)
    {
        _typeNode = typeNode;
        SetupUI();
        UpdateUI();
    }

    public override void Initialize(VariableNode node, VariableType type)
    {
        if (node is TypeVariableNode typeNode)
        {
            Initialize(typeNode);
        }
    }

    private void SetupUI()
    {
        if (_typeDropdown == null) return;

        // Clear and setup dropdown options
        _typeDropdown.ClearOptions();

        // Create type mapping
        var typeNames = new List<string>();
        _typeMapping.Clear();

        var commonTypes = new Dictionary<int, (string, Type)>
        {
            {0, ("Float", typeof(float))},
            {1, ("Integer", typeof(int))},
            {2, ("Boolean", typeof(bool))},
            {3, ("String", typeof(string))},
            {4, ("Vector3", typeof(Vector3))},
            {5, ("GameObject", typeof(GameObject))},
            {6, ("Object", typeof(object))}
        };

        foreach (var mapping in commonTypes)
        {
            typeNames.Add(mapping.Value.Item1);
            _typeMapping[mapping.Key] = mapping.Value.Item2;
        }

        _typeDropdown.AddOptions(typeNames);
        _typeDropdown.onValueChanged.AddListener(OnTypeSelected);
    }

    private void UpdateUI()
    {
        if (_typeNode == null || _typeDropdown == null) return;

        // Find current type in mapping
        int currentIndex = 6; // Default to "Object"
        foreach (var mapping in _typeMapping)
        {
            if (mapping.Value == _typeNode.SelectedType)
            {
                currentIndex = mapping.Key;
                break;
            }
        }

        _typeDropdown.SetValueWithoutNotify(currentIndex);
    }

    private void OnTypeSelected(int index)
    {
        if (_typeNode == null || !_typeMapping.ContainsKey(index)) return;

        Type selectedType = _typeMapping[index];
        Debug.LogError(selectedType);
        _typeNode.ChangeSelectedType(selectedType);
    }

    public override object GetValue()
    {
        return _typeNode?.SelectedType;
    }
}