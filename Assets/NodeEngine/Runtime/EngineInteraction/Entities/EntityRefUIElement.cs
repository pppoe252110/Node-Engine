using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EntityRefDropdownUIElement : VariableUIElement
{
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private bool _refreshOnOpen = true;

    private List<EntityRef> _entityRefs = new();
    private EntityRef _currentRef;

    public override bool CanBind(Type valueType) => valueType == typeof(EntityRef);

    public override void Bind(IVariableNode node)
    {
        PopulateDropdown();
        base.Bind(node);
        _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void PopulateDropdown()
    {
        _dropdown.ClearOptions();
        _entityRefs.Clear();

        // Gather all currently registered entities
        var entities = EntityRegistry.GetAllEntities()
            .Select(kvp => kvp.Value)
            .Where(e => e != null)
            .OrderBy(e => e.name)
            .ToList();

        var options = new List<string> { "None" };
        _entityRefs.Add(new EntityRef());

        foreach (var entity in entities)
        {
            // Use GameObject name as display (or you could show ID)
            options.Add($"{entity.name} ({entity.Id.Substring(0, 8)}...)");
            _entityRefs.Add(new EntityRef(entity.Id));
        }

        _dropdown.AddOptions(options);
    }

    private void OnDropdownValueChanged(int index)
    {
        if (TargetNode == null || index < 0 || index >= _entityRefs.Count) return;
        _currentRef = _entityRefs[index];
        TargetNode.SetUntypedValue(_currentRef);
    }

    protected override void OnNodeValueChanged(object newValue)
    {
        if (newValue is EntityRef entityRef)
        {
            _currentRef = entityRef;
            // Find the index in our list (by ID match)
            int index = _entityRefs.FindIndex(r => r.Id == entityRef.Id);
            if (index >= 0)
            {
                _dropdown.SetValueWithoutNotify(index);
            }
            else
            {
                // If the entity isn't in the dropdown (e.g., just spawned), default to None
                _dropdown.SetValueWithoutNotify(0);
            }
        }
    }

    public void OnDropdownClicked()
    {
        if (_refreshOnOpen)
        {
            // Cache current value before refresh
            EntityRef current = _currentRef;
            PopulateDropdown();
            // Restore selection if possible
            int idx = _entityRefs.FindIndex(r => r.Id == current.Id);
            _dropdown.SetValueWithoutNotify(idx >= 0 ? idx : 0);
        }
    }

    protected override void OnDestroy()
    {
        if (_dropdown != null)
            _dropdown.onValueChanged.RemoveAllListeners();
        base.OnDestroy();
    }
}