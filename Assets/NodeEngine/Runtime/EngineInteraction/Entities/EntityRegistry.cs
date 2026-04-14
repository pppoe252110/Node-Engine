using System.Collections.Generic;
using UnityEngine;

public static class EntityRegistry
{
    private static readonly Dictionary<string, NodeEntity> _entities = new();

    public static void Register(NodeEntity entity)
    {
        if (entity != null && !string.IsNullOrEmpty(entity.Id))
        {
            _entities[entity.Id] = entity;
        }
    }

    public static void Unregister(NodeEntity entity)
    {
        if (entity != null && !string.IsNullOrEmpty(entity.Id))
        {
            _entities.Remove(entity.Id);
        }
    }

    public static NodeEntity Resolve(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _entities.TryGetValue(id, out var go) ? go : null;
    }

    public static IEnumerable<KeyValuePair<string, NodeEntity>> GetAllEntities() => _entities;
}