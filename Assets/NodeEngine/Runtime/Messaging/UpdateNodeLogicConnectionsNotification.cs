using UniMediator.Runtime;

public class UpdateNodeLogicConnectionsNotification : INotification
{
    public NodeLogic NodeLogic { get; }

    public UpdateNodeLogicConnectionsNotification(NodeLogic nodeLogic)
    {
        NodeLogic = nodeLogic;
    }
}