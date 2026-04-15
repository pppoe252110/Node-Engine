using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

// -----------------------------------------------------------------------------
//  Editor Window that replaces the custom inspector
// -----------------------------------------------------------------------------
public class NodesDatabaseWindow : EditorWindow
{
    private NodesDatabase _targetDatabase;
    private SerializedObject _serializedObject;
    private SerializedProperty _nodesProperty;

    private NodesTreeView _treeView;
    private TreeViewState<int> _treeViewState;
    private SearchField _searchField;

    private static Type[] _cachedNodeTypes;

    private string _notificationMessage;
    private double _notificationEndTime;

    // -------------------------------------------------------------------------
    //  Open the window for a specific database (called by double‑click handler)
    // -------------------------------------------------------------------------
    public static void Open(NodesDatabase database)
    {
        if (database == null) return;

        var window = GetWindow<NodesDatabaseWindow>(false, database.name, true);
        window.SetTarget(database);
        window.Show();
    }

    // -------------------------------------------------------------------------
    //  Set / change the database to edit
    // -------------------------------------------------------------------------
    public void SetTarget(NodesDatabase database)
    {
        _targetDatabase = database;
        _serializedObject = new SerializedObject(_targetDatabase);
        _nodesProperty = _serializedObject.FindProperty("_serializableNodes");

        // Recreate tree view with new data
        _treeViewState = new TreeViewState<int>();
        _treeView = new NodesTreeView(_treeViewState, _nodesProperty);
        _searchField = new SearchField();
        _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;

        _treeView.Reload();
        Repaint();
    }

    private void OnEnable()
    {
        // If window is reopened and we already had a target, re‑bind
        if (_targetDatabase != null)
            SetTarget(_targetDatabase);
    }

    private void OnGUI()
    {
        if (_targetDatabase == null)
        {
            EditorGUILayout.HelpBox("No NodesDatabase selected. Double‑click a NodesDatabase asset to open it.", MessageType.Info);
            if (GUILayout.Button("Select Database"))
                PickDatabase();
            return;
        }

        // Sync changes from the asset
        _serializedObject.Update();

        DrawMainToolbar();
        DrawTreeView();
        DrawTreeControlBar();
        DrawSelectedNodeProperties();

        _serializedObject.ApplyModifiedProperties();
    }

    private void DrawMainToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        // Show which database is currently open + allow picking another
        if (GUILayout.Button(new GUIContent(_targetDatabase.name, "Switch to another database"), EditorStyles.toolbarDropDown, GUILayout.MinWidth(120)))
            PickDatabase();

        GUILayout.Space(10);

        if (GUILayout.Button(new GUIContent("Auto Fill"), EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            _targetDatabase.AutoFill();
            EditorUtility.SetDirty(_targetDatabase);
            _serializedObject.Update();
            _treeView.Reload();
        }

        if (GUILayout.Button(new GUIContent("Select Missing"), EditorStyles.toolbarButton, GUILayout.Width(90)))
            SelectMissingNodes();

        DrawNotification();

        GUILayout.FlexibleSpace();

        // Search field
        Rect searchRect = GUILayoutUtility.GetRect(150, 250, 16, 16, EditorStyles.toolbarSearchField);
        _treeView.searchString = _searchField.OnToolbarGUI(searchRect, _treeView.searchString);

        GUILayout.EndHorizontal();
    }

    private void DrawTreeView()
    {
        Rect treeRect = EditorGUILayout.GetControlRect(false, position.height-160);
        _treeView.OnGUI(treeRect);
    }

    private void DrawTreeControlBar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button(new GUIContent("+", "Add new node"), EditorStyles.toolbarButton, GUILayout.Width(25)))
        {
            _nodesProperty.arraySize++;
            var newProp = _nodesProperty.GetArrayElementAtIndex(_nodesProperty.arraySize - 1);
            newProp.FindPropertyRelative("nodeName").stringValue = "New Node";
            newProp.FindPropertyRelative("nodeType").stringValue = "";
            newProp.FindPropertyRelative("nodeIcon").objectReferenceValue = null;
            _treeView.Reload();
            _treeView.SetSelection(new List<int> { _nodesProperty.arraySize - 1 }, TreeViewSelectionOptions.RevealAndFrame);
        }

        GUI.enabled = _treeView.HasSelection();
        if (GUILayout.Button(new GUIContent("-", "Remove selected"), EditorStyles.toolbarButton, GUILayout.Width(25)))
        {
            var selection = _treeView.GetSelection().OrderByDescending(x => x).ToList();
            foreach (var id in selection)
            {
                if (id >= 0 && id < _nodesProperty.arraySize)
                    _nodesProperty.DeleteArrayElementAtIndex(id);
            }
            _treeView.SetSelection(new List<int>());
            _treeView.Reload();
        }
        GUI.enabled = true;

        GUILayout.Space(10);
        if (GUILayout.Button("Expand All", EditorStyles.toolbarButton)) _treeView.ExpandAll();
        if (GUILayout.Button("Collapse All", EditorStyles.toolbarButton)) _treeView.CollapseAll();

        GUILayout.FlexibleSpace();

        GUILayout.Label("Sort:", EditorStyles.miniLabel);
        EditorGUI.BeginChangeCheck();
        _treeView.CurrentSortMode = (NodesTreeView.SortMode)EditorGUILayout.EnumPopup(
            _treeView.CurrentSortMode,
            EditorStyles.toolbarDropDown,
            GUILayout.Width(100)
        );
        if (EditorGUI.EndChangeCheck())
            _treeView.Reload();

        GUILayout.EndHorizontal();
    }

    private void DrawSelectedNodeProperties()
    {
        var selection = _treeView.GetSelection();
        if (selection.Count != 1) return;

        int index = selection.First();
        if (index < 0 || index >= _nodesProperty.arraySize) return;

        var selectedProp = _nodesProperty.GetArrayElementAtIndex(index);
        var nameProp = selectedProp.FindPropertyRelative("nodeName");
        var typeProp = selectedProp.FindPropertyRelative("nodeType");
        var iconProp = selectedProp.FindPropertyRelative("nodeIcon");

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Selected Node", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();

        // Icon selector (64x64 square)
        EditorGUILayout.BeginVertical(GUILayout.Width(68));
        EditorGUI.BeginChangeCheck();
        UnityEngine.Object newIcon = EditorGUILayout.ObjectField(
            GUIContent.none,
            iconProp.objectReferenceValue,
            typeof(Sprite),
            false,
            GUILayout.Width(64),
            GUILayout.Height(64)
        );
        if (EditorGUI.EndChangeCheck())
        {
            iconProp.objectReferenceValue = newIcon;
            _serializedObject.ApplyModifiedProperties();
            _treeView.Reload();
        }
        EditorGUILayout.EndVertical();

        // Name and Type
        EditorGUILayout.BeginVertical();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(nameProp, new GUIContent("Name"));
        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            _treeView.Reload();
        }

        DrawTypeDropdown(typeProp, index);
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawTypeDropdown(SerializedProperty typeProp, int nodeIndex)
    {
        CacheNodeTypes();

        string currentTypeName = typeProp.stringValue;
        Type currentType = string.IsNullOrEmpty(currentTypeName) ? null : Type.GetType(currentTypeName);
        string displayText = currentType != null ? currentType.Name : "None";

        Rect rect = EditorGUILayout.GetControlRect();
        Rect labelRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);
        Rect buttonRect = new Rect(rect.x + EditorGUIUtility.labelWidth, rect.y, rect.width - EditorGUIUtility.labelWidth, rect.height);

        EditorGUI.LabelField(labelRect, "Type");

        if (GUI.Button(buttonRect, new GUIContent(displayText), EditorStyles.popup))
        {
            var dropdown = new NodeTypeDropdown(new AdvancedDropdownState(), _cachedNodeTypes, selectedType =>
            {
                _serializedObject.Update();
                var nodesArr = _serializedObject.FindProperty("_serializableNodes");
                if (nodeIndex >= 0 && nodeIndex < nodesArr.arraySize)
                {
                    var targetNode = nodesArr.GetArrayElementAtIndex(nodeIndex);
                    var tProp = targetNode.FindPropertyRelative("nodeType");
                    tProp.stringValue = selectedType?.AssemblyQualifiedName ?? "";
                    _serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_targetDatabase);
                    _treeView.Reload();
                    Repaint();
                }
            });
            dropdown.Show(buttonRect);
        }
    }

    private void SelectMissingNodes()
    {
        List<int> missingIndices = new List<int>();

        for (int i = 0; i < _nodesProperty.arraySize; i++)
        {
            var element = _nodesProperty.GetArrayElementAtIndex(i);
            string typeName = element.FindPropertyRelative("nodeType").stringValue;
            string nodeName = element.FindPropertyRelative("nodeName").stringValue;
            var iconProp = element.FindPropertyRelative("nodeIcon");

            bool typeMissing = string.IsNullOrEmpty(typeName) || Type.GetType(typeName) == null;
            bool nameMissing = string.IsNullOrWhiteSpace(nodeName);
            bool iconMissing = (iconProp.objectReferenceValue == null);

            if (typeMissing || nameMissing || iconMissing)
            {
                missingIndices.Add(i);
            }
        }

        if (missingIndices.Count > 0)
        {
            _treeView.SetSelection(missingIndices, TreeViewSelectionOptions.RevealAndFrame);
        }
        else
        {
            ShowNotification("All nodes have valid type, name, and icon.");
        }
    }

    private void ShowNotification(string message, float duration = 2f)
    {
        _notificationMessage = message;
        _notificationEndTime = EditorApplication.timeSinceStartup + duration;
        Repaint();
    }

    private void DrawNotification()
    {
        if (string.IsNullOrEmpty(_notificationMessage)) return;

        if (EditorApplication.timeSinceStartup >= _notificationEndTime)
        {
            _notificationMessage = null;
            return;
        }

        // Calculate alpha for fade out (last 0.5 seconds)
        float timeLeft = (float)(_notificationEndTime - EditorApplication.timeSinceStartup);
        float alpha = Mathf.Clamp01(timeLeft / 0.5f);

        // Save original colors
        Color originalColor = GUI.color;
        Color originalContentColor = GUI.contentColor;

        // Apply alpha to the label
        GUI.color = new Color(1f, 1f, 1f, alpha);
        GUI.contentColor = new Color(1f, 1f, 1f, alpha);

        // Draw a simple label that fits in the toolbar
        GUILayout.Label(_notificationMessage, EditorStyles.label);

        // Restore colors
        GUI.color = originalColor;
        GUI.contentColor = originalContentColor;

        // Force repaint while fading
        Repaint();
    }

    private void PickDatabase()
    {
        var selected = EditorUtility.OpenFilePanel("Select NodesDatabase", "Assets", "asset");
        if (string.IsNullOrEmpty(selected)) return;

        // Convert absolute path to asset path relative to project
        if (selected.StartsWith(Application.dataPath))
        {
            string assetPath = "Assets" + selected.Substring(Application.dataPath.Length);
            var database = AssetDatabase.LoadAssetAtPath<NodesDatabase>(assetPath);
            if (database != null)
                SetTarget(database);
        }
    }

    private static void CacheNodeTypes()
    {
        if (_cachedNodeTypes != null) return;

        _cachedNodeTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return Type.EmptyTypes; }
            })
            .Where(t => t.IsSubclassOf(typeof(BaseNode)) && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToArray();
    }

    // -------------------------------------------------------------------------
    //  Double‑click handler: open this window instead of the default inspector
    // -------------------------------------------------------------------------
    [OnOpenAsset]
    public static bool OnOpenAsset(EntityId entityId, int line)
    {
        var asset = EditorUtility.EntityIdToObject(entityId) as NodesDatabase;
        if (asset != null)
        {
            Open(asset);
            return true; // prevent default inspector
        }
        return false;
    }
}