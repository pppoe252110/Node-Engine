using UnityEngine;

public class Vector2Persister : JsonPersisterBase<Vector2>
{
    protected override Vector2 DefaultValue => Vector2.zero;
}