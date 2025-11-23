using System;

public class ExecutableConnectorBridge : IConnectorValueBridge
{
    private readonly IExecutableConnector _executableConnector;

    public ExecutableConnectorBridge(IExecutableConnector executableConnector)
    {
        _executableConnector = executableConnector ?? throw new ArgumentNullException(nameof(executableConnector));
    }

    public IConnectorValue WrappedValue => _executableConnector;
    public Type ValueType => typeof(void);

    /// <summary>
    /// "Setting a value" on an executable connector means triggering its action.
    /// The provided value is ignored.
    /// </summary>
    public void SetValueFast(object value) { }

    /// <summary>
    /// An executable connector has no value to get.
    /// </summary>
    public object GetValueFast()
    {
        return null;
    }
}