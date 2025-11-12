using UnityEngine;

public abstract class VariableUIElement : MonoBehaviour
{
    // Abstract method to get the value (implement in subclasses)
    public abstract object GetValue();

    // Optional: Initialize with VariableDatabase entry (e.g., set defaults)
    public virtual void Initialize(VariableDatabase database, VariableType type) { }
}