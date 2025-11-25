using System;
using UnityEngine;

[Serializable]
public class ConnectorValueVector3 : ConnectorValueBase, IFastConnectorValue<Vector3>, IConnectorValueBridge
{
    [SerializeField] private Vector3 _value;

    public ConnectorValueVector3() => _value = Vector3.zero;
    public ConnectorValueVector3(Vector3 value) => _value = value;

    
    public void SetValue(Vector3 value) => _value = value;
    public Vector3 GetValue() => _value;
    public override object GetInnerValue() => _value;

    
    public IConnectorValue WrappedValue => this;
    public Type ValueType => typeof(Vector3);
    public void SetValueFast(object value) => _value = (Vector3)value;
    public object GetValueFast() => _value;

    
    public ConnectorValueVector3 Add(Vector3 other) => new ConnectorValueVector3(_value + other);
    public ConnectorValueVector3 Subtract(Vector3 other) => new ConnectorValueVector3(_value - other);
    public ConnectorValueVector3 Multiply(float scalar) => new ConnectorValueVector3(_value * scalar);
    public ConnectorValueVector3 Divide(float scalar) => new ConnectorValueVector3(_value / scalar);

    
    public float X => _value.x;
    public float Y => _value.y;
    public float Z => _value.z;

    public void SetX(float x) => _value.x = x;
    public void SetY(float y) => _value.y = y;
    public void SetZ(float z) => _value.z = z;

    
    public float Magnitude() => _value.magnitude;
    public float SqrMagnitude() => _value.sqrMagnitude;
    public ConnectorValueVector3 Normalized() => new ConnectorValueVector3(_value.normalized);
    public float Dot(Vector3 other) => Vector3.Dot(_value, other);
    public ConnectorValueVector3 Cross(Vector3 other) => new ConnectorValueVector3(Vector3.Cross(_value, other));

    
    public float DistanceTo(Vector3 other) => Vector3.Distance(_value, other);
    public ConnectorValueVector3 DirectionTo(Vector3 other) => new ConnectorValueVector3((other - _value).normalized);

    
    public ConnectorValueVector3 Lerp(Vector3 target, float t) => new ConnectorValueVector3(Vector3.Lerp(_value, target, t));
    public ConnectorValueVector3 Slerp(Vector3 target, float t) => new ConnectorValueVector3(Vector3.Slerp(_value, target, t));

    
    public ConnectorValueVector3 Scale(Vector3 scale) => new ConnectorValueVector3(Vector3.Scale(_value, scale));
    public ConnectorValueVector3 Reflect(Vector3 normal) => new ConnectorValueVector3(Vector3.Reflect(_value, normal.normalized));

    
    public ConnectorValueVector3 ProjectOnPlane(Vector3 planeNormal) => new ConnectorValueVector3(Vector3.ProjectOnPlane(_value, planeNormal.normalized));
    public ConnectorValueVector3 Project(Vector3 onNormal) => new ConnectorValueVector3(Vector3.Project(_value, onNormal.normalized));

    
    public ConnectorValueString ToStringValue() => new ConnectorValueString(_value.ToString());
    public ConnectorValueFloat ToMagnitude() => new ConnectorValueFloat(_value.magnitude);

    
    public static ConnectorValueVector3 Zero => new ConnectorValueVector3(Vector3.zero);
    public static ConnectorValueVector3 One => new ConnectorValueVector3(Vector3.one);
    public static ConnectorValueVector3 Forward => new ConnectorValueVector3(Vector3.forward);
    public static ConnectorValueVector3 Back => new ConnectorValueVector3(Vector3.back);
    public static ConnectorValueVector3 Up => new ConnectorValueVector3(Vector3.up);
    public static ConnectorValueVector3 Down => new ConnectorValueVector3(Vector3.down);
    public static ConnectorValueVector3 Left => new ConnectorValueVector3(Vector3.left);
    public static ConnectorValueVector3 Right => new ConnectorValueVector3(Vector3.right);

    
    public bool Approximately(Vector3 other, float tolerance = 0.0001f) => Vector3.Distance(_value, other) < tolerance;
    public bool Equals(Vector3 other) => _value == other;

    
    public static implicit operator Vector3(ConnectorValueVector3 connector) => connector._value;
    public static implicit operator ConnectorValueVector3(Vector3 vector) => new ConnectorValueVector3(vector);

    
    protected override IConnectorValueBridge CreateFastBridge()
    {
        return new FastConnectorBridge<Vector3>(this);
    }

    public override string ToString() => _value.ToString();
    public override bool Equals(object obj) => obj is ConnectorValueVector3 other && _value == other._value;
    public override int GetHashCode() => _value.GetHashCode();
}