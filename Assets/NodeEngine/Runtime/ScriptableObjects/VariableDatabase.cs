using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VariableUIOptions
{
    public VariableType variableType;
    public bool useDropdown;
    public List<string> dropdownOptions;
    public List<string> dropdownValues;
}

[CreateAssetMenu(fileName = "VariableDatabase", menuName = "Node Engine/VariableDatabase", order = 2)]
public class VariableDatabase : ScriptableObject
{
    [SerializeField] private VariableUIEntry[] _entries;
    [SerializeField] private DropdownUIElement _converterUIPrefab;
    [SerializeField] private List<VariableUIOptions> _uiOptions;

    public DropdownUIElement ConverterUIPrefab => _converterUIPrefab;
    public VariableUIEntry[] Entries => _entries;

    public VariableUIElement GetPrefabForType(VariableType type)
    {
        foreach (var entry in _entries)
        {
            if (entry.Type == type) return entry.Prefab;
        }
        return null;
    }

    public VariableUIOptions GetUIOptions(VariableType type) =>
    _uiOptions?.Find(o => o.variableType == type);
}
