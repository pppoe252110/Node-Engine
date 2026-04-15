using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VContainer;

public class SubgraphLibraryService
{
    private readonly string _libraryPath;
    private readonly Dictionary<string, SubgraphDefinition> _loadedDefinitions = new();

    public IReadOnlyDictionary<string, SubgraphDefinition> LoadedDefinitions => _loadedDefinitions;

    [Inject]
    public SubgraphLibraryService()
    {
        _libraryPath = Path.Combine(Application.persistentDataPath, "Subgraphs");
        if (!Directory.Exists(_libraryPath))
            Directory.CreateDirectory(_libraryPath);
        RefreshLibrary();
    }

    public struct SubgraphNodeInfo
    {
        public string SubgraphId;
        public string DisplayName;
        public string CategoryPath; // e.g., "Subgraphs"
    }

    public IEnumerable<SubgraphNodeInfo> GetAllSubgraphInfos()
    {
        foreach (var def in _loadedDefinitions.Values)
        {
            yield return new SubgraphNodeInfo
            {
                SubgraphId = def.subgraphId,
                DisplayName = def.subgraphName,
                CategoryPath = "Subgraphs" // or could use a custom path if stored in definition
            };
        }
    }

    public SubgraphDefinition GetDefinition(string id) => _loadedDefinitions.TryGetValue(id, out var def) ? def : null;

    public void RefreshLibrary()
    {
        _loadedDefinitions.Clear();
        var files = Directory.GetFiles(_libraryPath, "*.subgraph");
        foreach (var file in files)
        {
            try
            {
                string json = File.ReadAllText(file);
                var def = JsonUtility.FromJson<SubgraphDefinition>(json);
                if (def != null && !string.IsNullOrEmpty(def.subgraphId))
                {
                    _loadedDefinitions[def.subgraphId] = def;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load subgraph {file}: {e.Message}");
            }
        }
    }

    public SubgraphDefinition CreateNewSubgraph(string name)
    {
        var def = new SubgraphDefinition
        {
            subgraphId = System.Guid.NewGuid().ToString(),
            subgraphName = name
        };
        // Add default flow ports
        def.inputPorts.Add(new SubgraphDefinition.PortDefinition
        {
            id = "FlowIn",
            name = "In",
            typeName = typeof(void).AssemblyQualifiedName,
            isFlow = true
        });
        def.outputPorts.Add(new SubgraphDefinition.PortDefinition
        {
            id = "FlowOut",
            name = "Out",
            typeName = typeof(void).AssemblyQualifiedName,
            isFlow = true
        });
        SaveDefinition(def);
        _loadedDefinitions[def.subgraphId] = def;
        return def;
    }

    public void SaveDefinition(SubgraphDefinition definition)
    {
        if (definition == null) return;

        string json = JsonUtility.ToJson(definition, true);
        string filePath = Path.Combine(_libraryPath, $"{definition.subgraphId}.subgraph");
        File.WriteAllText(filePath, json);

        _loadedDefinitions[definition.subgraphId] = definition;
    }

    public void DeleteDefinition(string subgraphId)
    {
        string filePath = Path.Combine(_libraryPath, $"{subgraphId}.subgraph");
        if (File.Exists(filePath))
            File.Delete(filePath);
        _loadedDefinitions.Remove(subgraphId);
    }
}