using System;
using System.Collections.Generic;
using System.Linq;

public class PersistenceService
{
    private readonly List<IValuePersister> _persisters;

    public PersistenceService(IEnumerable<IValuePersister> persisters)
    {
        _persisters = persisters.ToList();
    }

    public bool CanPersist(Type type)
    {
        return _persisters.Any(p => p.CanPersist(type));
    }

    public string Serialize(IVariableNode node)
    {
        var value = node.GetUntypedValue();
        var persister = _persisters.FirstOrDefault(p => p.CanPersist(node.ValueType));
        return persister?.Serialize(value) ?? string.Empty;
    }

    public void Deserialize(IVariableNode node, string data)
    {
        var persister = _persisters.FirstOrDefault(p => p.CanPersist(node.ValueType));
        var value = persister?.Deserialize(data, node.ValueType);
        node.SetUntypedValue(value);
    }
}