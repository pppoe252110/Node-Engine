using UniMediator.Runtime;

/// <summary>
/// Published whenever a connection is added or removed.
/// Subscribers should treat this as a signal that the graph topology has changed.
/// </summary>
public class ConnectionChangedNotification : INotification
{
    // Optional: include details about what changed if needed
    public Connector Source { get; }
    public Connector Target { get; }
    public bool WasAdded { get; }

    public ConnectionChangedNotification(Connector source, Connector target, bool wasAdded)
    {
        Source = source;
        Target = target;
        WasAdded = wasAdded;
    }
}