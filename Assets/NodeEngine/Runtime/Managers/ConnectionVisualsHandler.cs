using UniMediator.Runtime;
using UnityEngine;

public class ConnectionVisualsHandler : MonoBehaviour, INotificationHandler<ConnectionChangedNotification>
{
    public void Handle(ConnectionChangedNotification notification)
    {
        if (notification.WasAdded)
        {
            notification.Source.AddVisualConnection(notification.Target);
            notification.Target.AddVisualConnection(notification.Source);

            // Call node callbacks here, isolating exception risk from the ConnectionManager
            NotifyNodesSafely(notification.Source, notification.Target, isConnecting: true);
        }
        else
        {
            notification.Source.RemoveVisualConnection(notification.Target);
            notification.Target.RemoveVisualConnection(notification.Source);

            NotifyNodesSafely(notification.Source, notification.Target, isConnecting: false);
        }
    }

    private void NotifyNodesSafely(Connector source, Connector target, bool isConnecting)
    {
        try
        {
            if (isConnecting) source.Node.OnConnected(source, target);
            else source.Node.OnDisconnected(source, target);
        }
        catch (System.Exception ex) { Debug.LogError($"Node callback failed: {ex.Message}"); }

        try
        {
            if (isConnecting) target.Node.OnConnected(target, source);
            else target.Node.OnDisconnected(target, source);
        }
        catch (System.Exception ex) { Debug.LogError($"Node callback failed: {ex.Message}"); }
    }
}