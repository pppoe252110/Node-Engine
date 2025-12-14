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
    private bool _isUpdatingFromUI = false;

    public void Initialize(TypeVariableNode typeNode)
    {
        _typeNode = typeNode;
        SetupUI();
        UpdateUIFromNode(); // Use a different method name
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
        _typeMapping.Clear();

        var typeNames = new List<string>();

        // Common types with their display names
        var commonTypes = new List<(string, Type)>
        {
            ("Float", typeof(float)),
            ("Integer", typeof(int)),
            ("Boolean", typeof(bool)),
            ("String", typeof(string)),
            ("Vector3", typeof(Vector3)),
            ("GameObject", typeof(GameObject)),
            ("Object", typeof(object))
        };

        for (int i = 0; i < commonTypes.Count; i++)
        {
            typeNames.Add(commonTypes[i].Item1);
            _typeMapping[i] = commonTypes[i].Item2;
        }

        _typeDropdown.AddOptions(typeNames);
        _typeDropdown.onValueChanged.AddListener(OnTypeSelected);
    }

    private void UpdateUIFromNode()
    {
        if (_typeNode == null || _typeDropdown == null) return;

        // Find current type in mapping
        int currentIndex = 6; // Default to "Object"
        Type selectedType = _typeNode.SelectedType;

        foreach (var mapping in _typeMapping)
        {
            if (mapping.Value == selectedType)
            {
                currentIndex = mapping.Key;
                break;
            }
        }

        _typeDropdown.SetValueWithoutNotify(currentIndex);
    }

    private void OnTypeSelected(int index)
    {
        if (_typeNode == null || !_typeMapping.ContainsKey(index) || _isUpdatingFromUI)
            return;

        Type selectedType = _typeMapping[index];

        // Prevent recursion
        _isUpdatingFromUI = true;
        try
        {
            _typeNode.ChangeSelectedType(selectedType);
        }
        finally
        {
            _isUpdatingFromUI = false;
        }
    }

    public override object GetValue()
    {
        return _typeNode?.SelectedType;
    }

    // PUBLIC METHODS FOR EXTERNAL ACCESS
    public override void SetValue(object value)
    {
        if (value is Type typeValue)
        {
            SetSelectedType(typeValue, false); // Pass false to indicate external call
        }
    }

    public void SetSelectedType(Type type, bool updateNode = true)
    {
        if (_typeDropdown == null || _typeMapping == null || _isUpdatingFromUI)
        {
            Debug.LogWarning("Dropdown not ready or already updating");
            return;
        }

        // Find the index of this type in our mapping
        int targetIndex = -1;
        foreach (var mapping in _typeMapping)
        {
            if (mapping.Value == type)
            {
                targetIndex = mapping.Key;
                break;
            }
        }

        if (targetIndex >= 0 && targetIndex < _typeDropdown.options.Count)
        {
            _typeDropdown.SetValueWithoutNotify(targetIndex);

            // Only update the node if requested (prevents recursion)
            if (updateNode && _typeNode != null)
            {
                _isUpdatingFromUI = true;
                try
                {
                    _typeNode.ChangeSelectedType(type);
                }
                finally
                {
                    _isUpdatingFromUI = false;
                }
            }
        }
        else
        {
            Debug.LogWarning($"Type {type?.Name ?? "null"} not found in dropdown options.");
        }
    }
}