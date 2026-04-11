using System;
using TMPro;
using UnityEngine;

public class DropdownUIElement : VariableUIElement
{
    [SerializeField] protected TMP_Dropdown _dropdown;

    public override bool CanBind(Type valueType) => valueType.IsEnum;

    public override void Bind(IVariableNode node)
    {
        PopulateDropdown(node);
        base.Bind(node);
        _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    protected virtual void PopulateDropdown(IVariableNode node)
    {
        _dropdown.ClearOptions();
        _dropdown.AddOptions(new System.Collections.Generic.List<string>(Enum.GetNames(node.ValueType)));
    }

    protected virtual void OnDropdownValueChanged(int index)
    {
        if (TargetNode == null) return;
        var enumValues = Enum.GetValues(TargetNode.ValueType);
        TargetNode.SetUntypedValue(enumValues.GetValue(index));
    }

    protected override void OnNodeValueChanged(object newValue)
    {
        if (newValue == null) return;
        int index = Array.IndexOf(Enum.GetValues(TargetNode.ValueType), newValue);
        if (index >= 0)
        {
            _dropdown.SetValueWithoutNotify(index);
        }
    }

    protected override void OnDestroy()
    {
        if (_dropdown != null)
        {
            _dropdown.onValueChanged.RemoveAllListeners();
        }
        base.OnDestroy();
    }
}