public class ConnectorValueVoid : IConnectorValue, IExecutableConnector
{
    public static readonly ConnectorValueVoid Instance = new ConnectorValueVoid();

    public object GetInnerValue() => null;
    public void SetInnerValue(object value) { /* Do nothing for void */ }

    public void Execute()
    {

    }
}