using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TypeSelectorUIElement : DropdownUIElement
{
    // Explicitly define which types should appear in the dropdown
    private static readonly List<Type> _allowedTypes = new()
    {
        typeof(int),
        typeof(float),
        typeof(bool),
        typeof(string),
        typeof(Vector2),
        typeof(Vector3),
        typeof(Color),
        typeof(Quaternion),
        typeof(GameObject),
        typeof(Type),
        typeof(ComparisonOperation)
    };

    private Type _currentType;

    public override bool CanBind(Type valueType) => valueType == typeof(Type);

    protected override void PopulateDropdown(IVariableNode node)
    {
        _dropdown.ClearOptions();
        _dropdown.AddOptions(_allowedTypes.Select(t => GetTypeDisplayName(t)).ToList());
    }

    private string GetTypeDisplayName(Type type)
    {
        if (type == typeof(int)) return "Integer";
        if (type == typeof(float)) return "Float";
        if (type == typeof(bool)) return "Boolean";
        if (type == typeof(string)) return "String";
        if (type == typeof(Vector2)) return "Vector2";
        if (type == typeof(Vector3)) return "Vector3";
        if (type == typeof(Color)) return "Color";
        if (type == typeof(Quaternion)) return "Quaternion";
        if (type == typeof(GameObject)) return "GameObject";
        if (type == typeof(Type)) return "Type";
        if (type == typeof(ComparisonOperation)) return "Comparison";
        return type.Name;
    }

    protected override void OnDropdownValueChanged(int index)
    {
        if (TargetNode == null || index >= _allowedTypes.Count) return;
        _currentType = _allowedTypes[index];
        TargetNode.SetUntypedValue(_currentType);
    }

    protected override void OnNodeValueChanged(object newValue)
    {
        _currentType = newValue as Type;
        if (_currentType != null)
        {
            int index = _allowedTypes.IndexOf(_currentType);
            if (index >= 0)
            {
                _dropdown.SetValueWithoutNotify(index);
            }
        }
    }
}