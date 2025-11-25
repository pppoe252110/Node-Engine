public class ConnectorValueVoid : IExecutableConnector
{
    public ConnectorValueVoid()
    {

    }
    
    public void Execute() { }

    
    public object GetInnerValue() => null; 
}