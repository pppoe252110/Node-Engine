using System;

public class ConnectorValueFloat : ConnectorValueBase<float>
{
    public ConnectorValueFloat(float value = 0f) : base(value) { }
    public override Type InnerType => typeof(float);
}