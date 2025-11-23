public class ConnectorValueVoid : IExecutableConnector
{
    public ConnectorValueVoid()
    {

    }
    // Implementation of the new interface
    public void Execute() { }

    // Implementation of the base IConnectorValue interface
    public object GetInnerValue() => null; // An action has no inner value
}