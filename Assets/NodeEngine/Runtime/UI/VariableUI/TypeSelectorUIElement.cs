using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TypeSelectorUIElement : DropdownUIElement
{
    // Single static list containing only types that have a registered IValuePersister
    private static readonly List<Type> _persistableTypes;
    private Type _currentType;

    // Static constructor runs once when the class is first accessed
    static TypeSelectorUIElement()
    {
        _persistableTypes = BuildPersistableTypeList();
    }

    /// <summary>
    /// Builds the list of all public non-abstract types that can be persisted by at least one IValuePersister.
    /// </summary>
    private static List<Type> BuildPersistableTypeList()
    {
        // Find all IValuePersister implementations
        var persisterTypes = typeof(IValuePersister).Assembly.GetTypes()
            .Where(t => typeof(IValuePersister).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        // Instantiate each persister (they are simple, parameterless classes)
        var persisters = persisterTypes
            .Select(t => (IValuePersister)Activator.CreateInstance(t))
            .ToList();

        // Get all public, non-abstract types from all loaded assemblies
        var allTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return Type.EmptyTypes; }
            })
            .Where(t => t.IsPublic && !t.IsAbstract);

        // Filter to types that can be persisted
        var supported = new HashSet<Type>();
        foreach (var type in allTypes)
        {
            foreach (var persister in persisters)
            {
                if (persister.CanPersist(type))
                {
                    supported.Add(type);
                    break;
                }
            }
        }

        // Return sorted list for predictable dropdown order
        return supported.OrderBy(t => t.FullName).ToList();
    }

    public override bool CanBind(Type valueType) => valueType == typeof(Type);

    protected override void PopulateDropdown(IVariableNode node)
    {
        _dropdown.ClearOptions();
        _dropdown.AddOptions(_persistableTypes.Select(t => t.Name).ToList());
    }

    protected override void OnDropdownValueChanged(int index)
    {
        if (TargetNode == null) return;
        _currentType = _persistableTypes[index];
        TargetNode.SetUntypedValue(_currentType);
    }

    protected override void OnNodeValueChanged(object newValue)
    {
        _currentType = newValue as Type;
        if (_currentType != null)
        {
            int index = _persistableTypes.IndexOf(_currentType);
            if (index >= 0)
            {
                _dropdown.SetValueWithoutNotify(index);
            }
        }
    }
}