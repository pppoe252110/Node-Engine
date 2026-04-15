using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NodesDatabase))]
public class NodesDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NodesDatabase database = (NodesDatabase)target;
        
        // --- New button to open the editor window ---
        if (GUILayout.Button("Open Editor Window"))
        {
            NodesDatabaseWindow.Open(database);
        }

        DrawDefaultInspector();

        EditorGUILayout.Space();

        // --- Existing button logic for methods marked with [Button] ---
        var methods = database.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0)
            .ToArray();

        foreach (var method in methods)
        {
            var buttonAttr = (ButtonAttribute)method.GetCustomAttribute(typeof(ButtonAttribute), true);
            string buttonName = buttonAttr.Name;

            if (GUILayout.Button(buttonName))
            {
                method.Invoke(database, null);
            }
        }
    }
}