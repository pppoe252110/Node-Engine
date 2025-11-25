using UnityEngine;

[CreateAssetMenu(fileName = "VariableDatabase", menuName = "Node Engine/VariableDatabase", order = 2)]
public class VariableDatabase : ScriptableObject
{
    [SerializeField] private VariableUIEntry[] _entries;

    public VariableUIEntry[] Entries => _entries;

    
    public VariableUIElement GetPrefabForType(VariableType type)
    {
        foreach (var entry in _entries)
        {
            if (entry.Type == type) return entry.Prefab;
        }
        return null;
    }
}