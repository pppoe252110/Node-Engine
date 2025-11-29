using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var buttonAttribute = (ButtonAttribute)attribute;

        if (GUI.Button(position, buttonAttribute.Name))
        {
            var targetObject = property.serializedObject.targetObject;
            var methodName = string.IsNullOrEmpty(buttonAttribute.MethodName) ? property.name : buttonAttribute.MethodName;

            var method = targetObject.GetType().GetMethod(methodName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.FlattenHierarchy);

            if (method != null)
            {
                method.Invoke(targetObject, null);
            }
            else
            {
                Debug.LogWarning($"Method '{methodName}' not found in {targetObject.GetType()}");
            }
        }
    }
}
