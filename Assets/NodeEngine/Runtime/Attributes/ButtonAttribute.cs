using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ButtonAttribute : PropertyAttribute
{
    public string Name { get; private set; }
    public string MethodName { get; private set; }

    public ButtonAttribute(string name = "", string methodName = "")
    {
        Name = string.IsNullOrEmpty(name) ? "Button" : name;
        MethodName = string.IsNullOrEmpty(methodName) ? name : methodName;
    }
}
