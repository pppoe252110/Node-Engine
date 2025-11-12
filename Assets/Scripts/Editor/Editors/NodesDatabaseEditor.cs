using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NodesDatabase))]
public class NodesDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default Inspector (for fields like _serializableNodes)
        DrawDefaultInspector();

        // Add some spacing
        EditorGUILayout.Space();

        // Get the target object (the NodesDatabase instance)
        NodesDatabase database = (NodesDatabase)target;

        // Find all methods in the NodesDatabase class that have the ButtonAttribute
        var methods = database.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0)
            .ToArray();

        // For each method with the attribute, draw a button
        foreach (var method in methods)
        {
            var buttonAttr = (ButtonAttribute)method.GetCustomAttribute(typeof(ButtonAttribute), true);
            string buttonName = buttonAttr.Name;

            if (GUILayout.Button(buttonName))
            {
                // Invoke the method on the target object
                method.Invoke(database, null);
            }
        }
    }
}