using UnityEngine;

public class FieldConverter<T>
{
    public virtual T GetValue(object value)
    {
        return default;
    } 

    public virtual void SetValue(T value)
    {
    
    }
}
