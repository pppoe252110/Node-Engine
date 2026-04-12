using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(ConnectorColorDatabase))]
public class ConnectorColorDatabaseEditor : Editor
{
    private ConnectorColorDatabase database;
    private Vector2 scrollPosition;
    private string searchFilter = "";
    private List<string> availableTypes = new List<string>();
    private int selectedAvailableTypeIndex = 0;

    private Dictionary<string, int> typeUsageCounts = new Dictionary<string, int>();
    private List<Type> allUsedTypes = new List<Type>();
    private bool hasScanned = false; 

    private const string CacheKeyUsageCounts = "ConnectorColorDatabase_UsageCounts";
    private const string CacheKeyAllUsedTypes = "ConnectorColorDatabase_AllUsedTypes";
    private const string CacheKeyHasScanned = "ConnectorColorDatabase_HasScanned";

    private void OnEnable()
    {
        database = (ConnectorColorDatabase)target;
        LoadCachedData(); 
        RefreshAvailableTypes(); 
    }

    private void OnDisable()
    {
        SaveCachedData(); 
    }

    private void LoadCachedData()
    {
        hasScanned = EditorPrefs.GetBool(CacheKeyHasScanned, false);
        if (hasScanned)
        {
            
            string usageJson = EditorPrefs.GetString(CacheKeyUsageCounts, "");
            if (!string.IsNullOrEmpty(usageJson))
            {
                typeUsageCounts = JsonUtility.FromJson<SerializableDictionary<string, int>>(usageJson).ToDictionary();
            }

            string typesJson = EditorPrefs.GetString(CacheKeyAllUsedTypes, "");
            if (!string.IsNullOrEmpty(typesJson))
            {
                var typeNames = JsonUtility.FromJson<SerializableList<string>>(typesJson).list;
                allUsedTypes = typeNames.Select(name => Type.GetType(name)).Where(t => t != null).ToList();
            }
        }
    }

    private void SaveCachedData()
    {
        EditorPrefs.SetBool(CacheKeyHasScanned, hasScanned);
        if (hasScanned)
        {
            
            var serializableUsage = new SerializableDictionary<string, int>(typeUsageCounts);
            EditorPrefs.SetString(CacheKeyUsageCounts, JsonUtility.ToJson(serializableUsage));

            var typeNames = allUsedTypes.Select(t => t.AssemblyQualifiedName).ToList();
            var serializableTypes = new SerializableList<string>(typeNames);
            EditorPrefs.SetString(CacheKeyAllUsedTypes, JsonUtility.ToJson(serializableTypes));
        }
    }

    public override void OnInspectorGUI()
    {
        
        DrawHeader();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            DrawAutoFillSection();
            DrawFallbackColorSection();
            DrawTypeListSection();
        }
        EditorGUILayout.EndVertical();

        EditorUtility.SetDirty(database);
    }

    private new void DrawHeader()
    {
        EditorGUILayout.LabelField("CONNECTOR COLOR DATABASE", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Mapped Types: {database.ColorMappings.Count}", EditorStyles.miniLabel);
        EditorGUILayout.Space();
    }

    private void DrawAutoFillSection()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.LabelField("Auto Discovery", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("🔍 Find All Types", GUILayout.Height(25)))
                {
                    AutoFillTypes();
                }

                if (GUILayout.Button("🗑️ Clear All", GUILayout.Width(80), GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog("Clear All Mappings",
                        "Are you sure you want to clear all type color mappings?", "Yes", "No"))
                    {
                        database.ColorMappings.Clear();
                        RefreshAvailableTypes();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("Automatically discovers types used in NodeValueAttributes throughout your project.", MessageType.Info);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
    }

    private void DrawFallbackColorSection()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.LabelField("Fallback Color", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            {
                database.FallbackColor = EditorGUILayout.ColorField("Color", database.FallbackColor);
                EditorGUILayout.LabelField("Used for unmapped types", EditorStyles.miniLabel, GUILayout.Width(150));
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
    }

    private void DrawTypeListSection()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.LabelField("Type Color Mappings", EditorStyles.boldLabel);

            DrawAddTypeSection();

            EditorGUILayout.Space();

            if (database.ColorMappings.Count == 0)
            {
                EditorGUILayout.HelpBox("No type mappings. Click 'Find All Types' or add types manually above.", MessageType.Info);
            }
            else
            {
                DrawTypeMappingsList();
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawAddTypeSection()
    {
        EditorGUILayout.BeginVertical("HelpBox");
        {
            EditorGUILayout.LabelField("Add New Type", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            {
                
                string newSearch = EditorGUILayout.TextField("Search:", searchFilter, GUILayout.ExpandWidth(true));
                if (newSearch != searchFilter)
                {
                    searchFilter = newSearch;
                    RefreshAvailableTypes();
                }

                if (!string.IsNullOrEmpty(searchFilter) && GUILayout.Button("✕", GUILayout.Width(25)))
                {
                    searchFilter = "";
                    RefreshAvailableTypes();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                
                string[] availableTypeNames = availableTypes.ToArray();
                selectedAvailableTypeIndex = EditorGUILayout.Popup("Type:", selectedAvailableTypeIndex, availableTypeNames, GUILayout.ExpandWidth(true));

                GUI.enabled = availableTypes.Count > 0;
                if (GUILayout.Button("➕ Add", GUILayout.Width(60)))
                {
                    AddSelectedType();
                }
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

            if (availableTypes.Count == 0)
            {
                EditorGUILayout.HelpBox(hasScanned ? "No available types found. All types might already be in the list." : "No available types found. Click 'Find All Types' to scan for types.", MessageType.Info);
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawTypeMappingsList()
    {
        EditorGUILayout.LabelField($"Mapped Types ({database.ColorMappings.Count}):", EditorStyles.miniBoldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(Mathf.Min(database.ColorMappings.Count * 55 + 20, 300)));

        for (int i = 0; i < database.ColorMappings.Count; i++)
        {
            DrawTypeMappingItem(i);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawTypeMappingItem(int index)
    {
        var mapping = database.ColorMappings[index];

        EditorGUILayout.BeginVertical("Box");
        {
            
            EditorGUILayout.BeginHorizontal();
            {
                
                EditorGUILayout.BeginVertical(GUILayout.Width(60));
                {
                    mapping.color = EditorGUILayout.ColorField(GUIContent.none, mapping.color, false, true, false, GUILayout.Height(30), GUILayout.Width(50));

                    Rect colorRect = GUILayoutUtility.GetRect(50, 3);
                    EditorGUI.DrawRect(colorRect, mapping.color);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField(mapping.typeName, EditorStyles.boldLabel);

                    int usageCount = typeUsageCounts.TryGetValue(mapping.typeName, out int count) ? count : 0;
                    string usageText = usageCount == 0 ? "Usage: Unknown (scan to update)" : (usageCount == 1 ? "1 usage" : $"{usageCount} usages");
                    EditorGUILayout.LabelField(usageText, EditorStyles.miniLabel);
                }
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginVertical();
                {
                    GUI.enabled = index > 0;
                    if (GUILayout.Button("▲", GUILayout.Width(20), GUILayout.Height(14)))
                    {
                        MoveItem(index, -1);
                        return;
                    }
                    GUI.enabled = index < database.ColorMappings.Count - 1;
                    if (GUILayout.Button("▼", GUILayout.Width(20), GUILayout.Height(14)))
                    {
                        MoveItem(index, 1);
                        return;
                    }
                    GUI.enabled = true;
                }
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("🗑️", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    database.ColorMappings.RemoveAt(index);
                    RefreshAvailableTypes();
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
    }

    private void MoveItem(int currentIndex, int direction)
    {
        int newIndex = currentIndex + direction;
        if (newIndex >= 0 && newIndex < database.ColorMappings.Count)
        {
            var temp = database.ColorMappings[currentIndex];
            database.ColorMappings[currentIndex] = database.ColorMappings[newIndex];
            database.ColorMappings[newIndex] = temp;
        }
    }

    private void RefreshAvailableTypes()
    {
        var mappedTypes = new HashSet<string>(database.ColorMappings.Select(m => m.typeName));

        availableTypes = allUsedTypes
            .Where(t => !mappedTypes.Contains(t.Name))
            .Where(t => string.IsNullOrEmpty(searchFilter) || t.Name.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
            .Select(t => t.Name)
            .OrderBy(name => name)
            .ToList();

        selectedAvailableTypeIndex = Mathf.Clamp(selectedAvailableTypeIndex, 0, Mathf.Max(0, availableTypes.Count - 1));
    }

    private void AddSelectedType()
    {
        if (availableTypes.Count == 0 || selectedAvailableTypeIndex < 0 || selectedAvailableTypeIndex >= availableTypes.Count)
            return;

        string typeName = availableTypes[selectedAvailableTypeIndex];
        var type = allUsedTypes.FirstOrDefault(t => t.Name == typeName);

        if (type != null)
        {
            var newMapping = new ConnectorColorDatabase.TypeColorMapping
            {
                typeName = typeName,
                color = GetDefaultColorForType(type)
            };

            database.ColorMappings.Add(newMapping);
            RefreshAvailableTypes();
        }
    }

    private void AutoFillTypes()
    {
        
        EditorUtility.DisplayProgressBar("Scanning for Types", "Finding all used types...", 0f);
        allUsedTypes = FindAllUsedTypes();
        typeUsageCounts = ComputeUsageCounts(allUsedTypes);
        hasScanned = true;
        EditorUtility.ClearProgressBar();

        var mappings = database.ColorMappings;
        var existingTypes = new HashSet<string>(mappings.Select(m => m.typeName));

        int addedCount = 0;

        foreach (var type in allUsedTypes)
        {
            if (!existingTypes.Contains(type.Name))
            {
                var newMapping = new ConnectorColorDatabase.TypeColorMapping
                {
                    typeName = type.Name,
                    color = GetDefaultColorForType(type)
                };

                mappings.Add(newMapping);
                addedCount++;
            }
        }

        RefreshAvailableTypes();

        if (addedCount > 0)
        {
            EditorGUILayout.HelpBox($"✅ Added {addedCount} new types!", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("ℹ️ All types are already mapped!", MessageType.Info);
        }
    }

    private List<Type> FindAllUsedTypes()
    {
        var foundTypes = new HashSet<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    // Look at fields with NodePortAttribute
                    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (field.GetCustomAttribute<NodePortAttribute>() != null)
                        {
                            var fieldType = field.FieldType;
                            // Skip generic parameters (like T) and open generic definitions (like List<>)
                            if (!fieldType.IsGenericParameter && !fieldType.IsGenericTypeDefinition)
                                foundTypes.Add(fieldType);
                        }
                    }

                    // Check methods with NodePortAttribute (for flow ports)
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (method.GetCustomAttribute<NodePortAttribute>() != null)
                        {
                            var parameters = method.GetParameters();
                            if (parameters.Length > 0)
                            {
                                var paramType = parameters[0].ParameterType;
                                if (!paramType.IsGenericParameter && !paramType.IsGenericTypeDefinition)
                                    foundTypes.Add(paramType);
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException) { continue; }
        }

        return foundTypes.OrderBy(t => t.Name).ToList();
    }

    private Dictionary<string, int> ComputeUsageCounts(List<Type> types)
    {
        var counts = new Dictionary<string, int>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            try
            {
                var assemblyTypes = assembly.GetTypes();
                foreach (var type in assemblyTypes)
                {
                    // Fields with NodePortAttribute
                    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (field.GetCustomAttribute<NodePortAttribute>() != null)
                        {
                            string typeName = field.FieldType.Name;
                            counts[typeName] = counts.TryGetValue(typeName, out int c) ? c + 1 : 1;
                        }
                    }
                    // Methods with NodePortAttribute
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (method.GetCustomAttribute<NodePortAttribute>() != null)
                        {
                            // 1. Check Return Type (This picks up 'void' or specific return flows)
                            string returnTypeName = method.ReturnType.Name;
                            counts[returnTypeName] = counts.TryGetValue(returnTypeName, out int c1) ? c1 + 1 : 1;

                            // 2. Check Parameters (Keep this if you use methods for input ports too)
                            var parameters = method.GetParameters();
                            foreach (var param in parameters)
                            {
                                string paramTypeName = param.ParameterType.Name;
                                counts[paramTypeName] = counts.TryGetValue(paramTypeName, out int c2) ? c2 + 1 : 1;
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException) { continue; }
        }

        return counts;
    }

    private Color GetDefaultColorForType(Type type)
    {
        return type.Name switch
        {
            "Int32" => new Color(1f, 0.2f, 0.2f),      // Red
            "Single" => new Color(0.2f, 0.9f, 0.2f),   // Green
            "Boolean" => new Color(0.1f, 0.5f, 1f),    // Blue
            "String" => new Color(1f, 0.8f, 0.1f),     // Yellow
            "Void" => new Color(0.8f, 0.2f, 0.8f),     // Purple
            "Vector3" => new Color(1f, 0.5f, 0f),      // Orange
            "GameObject" => new Color(0f, 0.8f, 1f),   // Cyan
            "Object" => new Color(0.9f, 0.1f, 0.5f),   // Pink
            "Type" => new Color(0.8f, 0.4f, 0.6f),     // Magenta
            "ComparisonOperation" => new Color(0.2f, 0.8f, 0.8f), // Teal
            "Color" => new Color(1f, 1f, 1f),           // White
            "Key" => new Color(0.6f, 0.6f, 0.6f),        // Gray
            "MouseButton" => new Color(0.4f, 0.3f, 0.2f), // Brown
            "Quaternion" => new Color(0.5f, 0f, 1f),     // Deep Violet
            "Vector2" => new Color(0.7f, 1f, 0f),        // Lime Green
            _ => database.FallbackColor
        };
    }


    [Serializable]
    private class SerializableDictionary<TKey, TValue>
    {
        public List<TKey> keys = new List<TKey>();
        public List<TValue> values = new List<TValue>();

        public SerializableDictionary(Dictionary<TKey, TValue> dict)
        {
            keys = new List<TKey>(dict.Keys);
            values = new List<TValue>(dict.Values);
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dict = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++)
            {
                dict[keys[i]] = values[i];
            }
            return dict;
        }
    }

    [Serializable]
    private class SerializableList<T>
    {
        public List<T> list = new List<T>();

        public SerializableList(List<T> inputList)
        {
            list = inputList;
        }
    }
}
