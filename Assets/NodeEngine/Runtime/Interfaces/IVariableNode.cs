using System;

public interface IVariableNode
{
    object GetUntypedValue();
    void SetUntypedValue(object value);
    Type ValueType { get; }
    event Action<object> OnUntypedValueChanged;
}