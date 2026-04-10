using UniMediator.Runtime;

public class NodeClickNotification : INotification
{
    public Connector Source { get; }
    public Connector Target { get; }

    public NodeClickNotification(Connector source, Connector target)
    {
        Source = source;
        Target = target;
    }
}
