using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleUIElement : VariableUIElement
{
    [SerializeField] private Toggle _toggle;

    public override bool CanBind(Type valueType) => valueType == typeof(bool);

    public override void Bind(IVariableNode node)
    {
        base.Bind(node);
        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool value)
    {
        TargetNode?.SetUntypedValue(value);
    }

    protected override void OnNodeValueChanged(object newValue)
    {
        if (newValue is bool b)
            _toggle.SetIsOnWithoutNotify(b);
    }

    protected override void OnDestroy()
    {
        if (_toggle != null)
            _toggle.onValueChanged.RemoveListener(OnToggleChanged);
        base.OnDestroy();
    }
}