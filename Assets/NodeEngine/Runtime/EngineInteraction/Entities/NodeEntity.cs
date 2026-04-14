using System;
using UnityEngine;

[DisallowMultipleComponent]
public class NodeEntity : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private string _id;

    public string Id => _id;

    private void Awake()
    {
        if (string.IsNullOrEmpty(_id))
        {
            _id = Guid.NewGuid().ToString();
        }
    }

    private void OnEnable()
    {
        EntityRegistry.Register(this);
    }

    private void OnDisable()
    {
        EntityRegistry.Unregister(this);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_id))
        {
            _id = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}