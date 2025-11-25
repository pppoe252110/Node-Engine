using UnityEngine;

[System.Serializable]
public class SerializableNode
{
    public string nodeName;
    public string nodeType;
    public VariableType variableType;
    public Sprite nodeIcon;

    public NodeBase CreateInstance()
    {
        var type = System.Type.GetType(nodeType);
        if (type != null)
        {
            var instance = System.Activator.CreateInstance(type) as NodeBase;

            if (instance is VariableNode varNode)
            {
                
                var field = typeof(VariableNode).GetField("_variableType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(varNode, variableType);
            }
            return instance;
        }
        return null;
    }

    public void Initialize(System.Type nodeType, string name)
    {
        this.nodeType = nodeType.AssemblyQualifiedName;
        this.nodeName = name;
    }
}