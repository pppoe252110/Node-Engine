using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class AssemblyData
{
    private Assembly _assembly;

    public AssemblyData(Assembly assembly)
    {
        _assembly = assembly;
    }

    public List<Type> GetClasses()
    {
        return _assembly.GetTypes().Where(s=>s.IsClass).ToList();
    }

    public List<MethodInfo> GetMethods(Type type)
    {
        return type.GetMethods().Where(m => m.DeclaringType != typeof(object)).ToList();
    }

    public List<PropertyInfo> GetProperties(Type type)
    {
        return type.GetProperties().ToList();
    }
}
