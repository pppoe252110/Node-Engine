using System;

[Serializable]
public struct EntityRef
{
    public string Id;

    public EntityRef(string id)
    {
        Id = id;
    }

    public bool IsValid => !string.IsNullOrEmpty(Id);

    public NodeEntity Resolve() => EntityRegistry.Resolve(Id);
}