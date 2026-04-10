using System;
using VContainer;

public class NodeFactory : INodeFactory
{
    private readonly IObjectResolver _resolver;

    [Inject]
    public NodeFactory(IObjectResolver resolver)
    {
        _resolver = resolver;
    }

    public BaseNode CreateNode(Type nodeType)
    {
        return (BaseNode)_resolver.Resolve(nodeType);
    }
}