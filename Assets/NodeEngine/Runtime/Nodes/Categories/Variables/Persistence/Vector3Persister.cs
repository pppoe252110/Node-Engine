using UnityEngine;

public class Vector3Persister : JsonPersisterBase<Vector3>
{
    protected override Vector3 DefaultValue => Vector3.zero;
}