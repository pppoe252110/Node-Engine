using System;
using System.Reflection;

public static class AssemblyManager
{
    public static AssemblyData LoadAssembly(string path)
    {
        return new AssemblyData(Assembly.LoadFile(path));
    }

    public static AssemblyData LoadAssembly(Type assembly)
    {
        return new AssemblyData(Assembly.GetAssembly(assembly));
    }
}
