using UniMediator.Runtime;
using UnityEngine.EventSystems;

public class ConnectorDragStartedNotification : INotification
{
    public Connector Connector { get; }
    public PointerEventData EventData { get; }
    public ConnectorDragStartedNotification(Connector connector, PointerEventData eventData)
    {
        Connector = connector;
        EventData = eventData;
    }
}

public class ConnectorDragEndedNotification : INotification
{
    public PointerEventData EventData { get; }
    public ConnectorDragEndedNotification(PointerEventData eventData) => EventData = eventData;
}

public class ConnectorClickedNotification : INotification
{
    public Connector Connector { get; }
    public PointerEventData EventData { get; }
    public ConnectorClickedNotification(Connector connector, PointerEventData eventData)
    {
        Connector = connector;
        EventData = eventData;
    }
}