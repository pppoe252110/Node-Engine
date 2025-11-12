using UnityEngine;

[System.Serializable]
public class SerializableNode
{
    public string nodeName;
    public string nodeType;
    public Sprite nodeIcon;

    public NodeBase CreateInstance()
    {
        var type = System.Type.GetType(nodeType);
        if (type != null)
        {
            return System.Activator.CreateInstance(type) as NodeBase;
        }
        return null;
    }

    public void Initialize(System.Type nodeType, string name)
    {
        this.nodeType = nodeType.AssemblyQualifiedName;
        this.nodeName = name;
    }
}