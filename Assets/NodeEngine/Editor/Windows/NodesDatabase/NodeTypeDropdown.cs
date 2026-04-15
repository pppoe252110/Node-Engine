using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;

public class NodeTypeDropdown : AdvancedDropdown
{
    private readonly Type[] _types;
    private readonly Action<Type> _onTypeSelected;

    // Key is now the FullName of the type
    private readonly Dictionary<string, Type> _typeMap = new Dictionary<string, Type>();

    public NodeTypeDropdown(AdvancedDropdownState state, Type[] types, Action<Type> onTypeSelected)
        : base(state)
    {
        _types = types;
        _onTypeSelected = onTypeSelected;
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        var root = new AdvancedDropdownItem("Node Types");
        _typeMap.Clear();

        root.AddChild(new AdvancedDropdownItem("None (Clear)"));
        root.AddSeparator();

        var grouped = _types.GroupBy(GetGroupName).OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            var groupItem = new AdvancedDropdownItem(group.Key);
            foreach (var type in group.OrderBy(t => t.Name))
            {
                // We use the FullName as the 'name' of the item for internal tracking
                var item = new AdvancedDropdownItem(type.Name);

                // Store using the display name as the key
                _typeMap[type.Name] = type;

                groupItem.AddChild(item);
            }
            root.AddChild(groupItem);
        }

        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        if (item.name == "None (Clear)")
        {
            _onTypeSelected?.Invoke(null);
            return;
        }

        if (_typeMap.TryGetValue(item.name, out var selectedType))
        {
            _onTypeSelected?.Invoke(selectedType);
        }
    }

    private string GetGroupName(Type type) =>
        string.IsNullOrEmpty(type.Namespace) ? "Global" : type.Namespace.Split('.').Last();
}