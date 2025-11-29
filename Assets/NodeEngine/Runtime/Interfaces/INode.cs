using System.Collections.Generic;

public interface INode
{
    void Process(List<Connector> fromConnectors = null);
    void Setup();
}
