public interface IExecutableConnector : IConnectorValue
{
    /// <summary>
    /// Executes the action associated with this connector.
    /// </summary>
    void Execute();
}