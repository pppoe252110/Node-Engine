using UnityEngine;

public class QuaternionPersister : JsonPersisterBase<Quaternion>
{
    protected override Quaternion DefaultValue => Quaternion.identity;
}