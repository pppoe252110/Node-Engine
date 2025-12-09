using System;

public class ConnectorValueVoid : IConnectorValue, IExecutableConnector
{
    public static readonly ConnectorValueVoid Instance = new ConnectorValueVoid();
    public object GetInnerValue() => null;
    public void SetInnerValue(object value) { }
    public void Execute() { }
    public Type InnerType => typeof(void);
}