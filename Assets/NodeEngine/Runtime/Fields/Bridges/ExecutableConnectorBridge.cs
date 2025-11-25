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

        public void SetValueFast(object value) { }

        public object GetValueFast()
    {
        return null;
    }
}