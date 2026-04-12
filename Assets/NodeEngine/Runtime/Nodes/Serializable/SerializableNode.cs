using UnityEngine;

[System.Serializable]
public class SerializableNode
{
    public string nodeName;
    public string nodeType;
    public Sprite nodeIcon;

    public void Initialize(System.Type nodeType, string name)
    {
        this.nodeType = nodeType.AssemblyQualifiedName;
        this.nodeName = name;
    }
}
