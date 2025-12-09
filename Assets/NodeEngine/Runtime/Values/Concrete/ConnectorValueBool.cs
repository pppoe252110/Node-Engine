using System;

public class ConnectorValueBool : ConnectorValueBase<bool>
{
    public ConnectorValueBool(bool value = false) : base(value) { }
    public override Type InnerType => typeof(bool);
}