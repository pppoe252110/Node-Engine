public interface IValuePersister
{
    bool CanPersist(VariableType variableType);
    string PersistValue(object value);
    object RestoreValue(string serializedValue, VariableType variableType);
}