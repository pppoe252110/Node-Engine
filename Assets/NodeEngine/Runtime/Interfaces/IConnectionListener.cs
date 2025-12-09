// Add to your existing Interfaces folder
public interface IConnectionListener
{
    void OnConnected(Connector myConnector, Connector otherConnector);
    void OnDisconnected(Connector myConnector, Connector otherConnector);
}