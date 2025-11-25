using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NodesDatabase))]
public class NodesDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        
        DrawDefaultInspector();

        
        EditorGUILayout.Space();

        
        NodesDatabase database = (NodesDatabase)target;

        
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