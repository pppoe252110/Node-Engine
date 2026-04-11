using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VariableUIRegistry))]
public class VariableUIRegistryEditor : Editor
{
    private SerializedProperty _mappingsProp;

    private void OnEnable()
    {
        _mappingsProp = serializedObject.FindProperty("_mappings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw the list of mappings with the custom drawer
        EditorGUILayout.PropertyField(_mappingsProp, new GUIContent("Variable UI Mappings"), true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);

        VariableUIRegistry registry = (VariableUIRegistry)target;

        if (GUILayout.Button("Auto Fill Missing Types", GUILayout.Height(30)))
        {
            Undo.RecordObject(registry, "Auto Fill Variable Node Types");
            registry.AutoFillVariableNodeTypes();
            EditorUtility.SetDirty(registry);

            // Refresh the cache in the property drawer
            System.Reflection.FieldInfo cacheField = typeof(VariableUIMappingDrawer).GetField(
                "_cachedTypes",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (cacheField != null) cacheField.SetValue(null, null);
        }

        serializedObject.ApplyModifiedProperties();
    }
}