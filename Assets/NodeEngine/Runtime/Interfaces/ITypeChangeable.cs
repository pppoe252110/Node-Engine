using System;

public interface ITypeChangeable
{
    bool ChangeOutputType(Type newType);
    bool CanUpdateType(Type newType);
}