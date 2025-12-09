using System;
using UnityEngine;

[Serializable]
public class ConnectorValueVector3 : ConnectorValueBase<Vector3>
{
    public ConnectorValueVector3(Vector3 value) => _value = value;

    public override Type InnerType => typeof(Vector3);
}
