public interface IConnectorValue
{
    object GetInnerValue();
    event System.Action<object> ValueUpdated;
}