/// <summary>
/// Non-generic interface for holding a value. Used by the non-generic Connector MonoBehaviour
/// and for reference types like string or execution flow (void).
/// </summary>
public interface IConnectorValue
{
    object GetInnerValue();
    void SetInnerValue(object value);
}

/// <summary>
/// Generic interface for strongly-typed access to the value. Used by NodeField<T>
/// and for value types like int, float, bool.
/// </summary>
public interface IConnectorValue<out T> : IConnectorValue
{
    new T GetInnerValue();
}