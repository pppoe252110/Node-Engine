using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;

public class NamespaceGenerator : EditorWindow
{
    private string rootNamespace = "YourCompany.YourGame";
    private bool includeFolderStructure = true;
    private bool backupScripts = true;
    private string selectedFolderPath = "Assets";

    private Vector2 scrollPosition;
    private Vector2 folderScrollPosition;
    private List<ScriptInfo> filteredScripts = new List<ScriptInfo>();
    private Dictionary<string, string> folderOverrides = new Dictionary<string, string>();
    private Dictionary<string, string> persistentFolderOverrides = new Dictionary<string, string>();
    private bool showSelectAllOptions = true;
    private bool isLoading = false;
    private bool showFolderOverrides = false;
    private FolderNode rootFolderNode;
    private bool needsFullRepaint = false;

    private const string FOLDER_OVERRIDES_PREF_KEY = "NamespaceGenerator_FolderOverrides";
    private const string LAST_FOLDER_PREF_KEY = "NamespaceGenerator_LastFolder";

    [MenuItem("Tools/Generate Namespaces")]
    public static void ShowWindow()
    {
        GetWindow<NamespaceGenerator>("Namespace Generator");
    }

    private void OnEnable()
    {
        LoadFolderOverrides();
        LoadLastFolder();

        folderOverrides.Clear();
        foreach (var kvp in persistentFolderOverrides)
        {
            folderOverrides[kvp.Key] = kvp.Value;
        }

        if (!string.IsNullOrEmpty(selectedFolderPath) && Directory.Exists(selectedFolderPath))
        {
            EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    ScanSelectedFolder();
                    needsFullRepaint = true;
                }
            };
        }
    }

    private void OnDisable()
    {
        SaveFolderOverrides();
        SaveLastFolder();
    }

    private void LoadLastFolder()
    {
        selectedFolderPath = EditorPrefs.GetString(LAST_FOLDER_PREF_KEY, "Assets");
    }

    private void SaveLastFolder()
    {
        EditorPrefs.SetString(LAST_FOLDER_PREF_KEY, selectedFolderPath);
    }

    private void LoadFolderOverrides()
    {
        persistentFolderOverrides.Clear();
        string savedData = EditorPrefs.GetString(FOLDER_OVERRIDES_PREF_KEY, "");

        if (!string.IsNullOrEmpty(savedData))
        {
            string[] entries = savedData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string entry in entries)
            {
                string[] parts = entry.Split(new[] { '|' }, 2);
                if (parts.Length == 2)
                {
                    persistentFolderOverrides[parts[0]] = parts[1];
                }
            }
        }
    }

    private void SaveFolderOverrides()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var kvp in persistentFolderOverrides)
        {
            // Only save non-null and non-empty overrides
            if (!string.IsNullOrEmpty(kvp.Value))
            {
                sb.AppendLine($"{kvp.Key}|{kvp.Value}");
            }
        }
        EditorPrefs.SetString(FOLDER_OVERRIDES_PREF_KEY, sb.ToString());
    }

    [System.Serializable]
    public class ScriptInfo
    {
        public string path;
        public string fileName;
        public string generatedNamespace;
        public string customNamespace;
        public bool includeInBatch = true;
        public bool hasExistingNamespace = false;
        public bool isEditorScript = false;
        public string folderPath;

        public string FinalNamespace => string.IsNullOrEmpty(customNamespace) ? generatedNamespace : customNamespace;
    }

    private class FolderNode
    {
        public string name;
        public string fullPath;
        public List<FolderNode> children = new List<FolderNode>();
        public bool isExpanded = true;
        public bool hasChildren => children.Count > 0;
        public bool hasScriptsOrScriptChildren = false;
    }

    private void OnGUI()
    {
        if (needsFullRepaint)
        {
            needsFullRepaint = false;
            ForceExpandAllFolders(rootFolderNode);
            Repaint();
        }

        GUILayout.Label("Namespace Generator", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
        rootNamespace = EditorGUILayout.TextField("Root Namespace:", rootNamespace);
        includeFolderStructure = EditorGUILayout.Toggle("Include Folder Structure", includeFolderStructure);
        backupScripts = EditorGUILayout.Toggle("Create Backups", backupScripts);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Target Folder", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        string newFolderPath = EditorGUILayout.TextField("Folder:", selectedFolderPath, GUILayout.ExpandWidth(true));
        if (newFolderPath != selectedFolderPath)
        {
            selectedFolderPath = newFolderPath;
            SaveLastFolder();
        }

        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string newFolder = EditorUtility.OpenFolderPanel("Select Folder to Scan for Scripts", selectedFolderPath, "");
            if (!string.IsNullOrEmpty(newFolder))
            {
                if (newFolder.StartsWith(Application.dataPath))
                {
                    string newRelativePath = "Assets" + newFolder.Substring(Application.dataPath.Length);
                    if (newRelativePath != selectedFolderPath)
                    {
                        selectedFolderPath = newRelativePath;
                        SaveLastFolder();
                        ScanSelectedFolder();
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Folder", "Please select a folder within your Unity project.", "OK");
                }
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox($"Scripts will be scanned from: {selectedFolderPath}", MessageType.Info);

        EditorGUILayout.Space();

        showFolderOverrides = EditorGUILayout.Foldout(showFolderOverrides, "Folder Namespace Overrides", true);
        if (showFolderOverrides)
        {
            DrawFolderOverrides();
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Scan Selected Folder"))
        {
            ScanSelectedFolder();
        }

        if (GUILayout.Button("Expand All Folders"))
        {
            ForceExpandAllFolders(rootFolderNode);
            Repaint();
        }

        if (GUILayout.Button("Collapse All Folders"))
        {
            ForceCollapseAllFolders(rootFolderNode);
            Repaint();
        }

        if (isLoading)
        {
            EditorGUILayout.LabelField("Scanning...", EditorStyles.miniLabel);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (filteredScripts.Count > 0)
        {
            DrawScriptsList();
        }
        else if (!isLoading)
        {
            EditorGUILayout.HelpBox("No scripts found in selected folder. Click 'Scan Selected Folder' to find scripts.", MessageType.Info);
        }
    }

    private void ForceExpandAllFolders(FolderNode node)
    {
        if (node == null) return;

        node.isExpanded = true;
        foreach (var child in node.children)
        {
            ForceExpandAllFolders(child);
        }
    }

    private void ForceCollapseAllFolders(FolderNode node)
    {
        if (node == null) return;

        node.isExpanded = false;
        foreach (var child in node.children)
        {
            ForceCollapseAllFolders(child);
        }
    }

    private void DrawFolderOverrides()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.HelpBox("Override namespace parts for specific folders in the hierarchy. Leave empty to skip folder in the namespace. Overrides cascade: parent overrides affect children unless overridden.", MessageType.Info);

        if (isLoading)
        {
            EditorGUILayout.LabelField("Loading folder structure...");
        }
        else if (filteredScripts.Count == 0)
        {
            EditorGUILayout.LabelField("No scripts found. Scan a folder first.");
        }
        else if (rootFolderNode == null || rootFolderNode.children.Count == 0)
        {
            EditorGUILayout.LabelField("No subfolders found with scripts.");
        }
        else
        {
            folderScrollPosition = EditorGUILayout.BeginScrollView(folderScrollPosition, GUILayout.Height(200));
            var relevantChildren = rootFolderNode.children.Where(c => c.hasScriptsOrScriptChildren).OrderBy(c => c.name);
            foreach (var child in relevantChildren)
            {
                DrawFolderNode(child, 0);
            }
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawFolderNode(FolderNode node, int depth)
    {
        if (!node.hasScriptsOrScriptChildren)
            return;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(depth * 15);

        if (node.hasChildren && node.children.Any(c => c.hasScriptsOrScriptChildren))
        {
            node.isExpanded = EditorGUILayout.Foldout(node.isExpanded, $"📁 {node.name}", true);
        }
        else
        {
            EditorGUILayout.LabelField($"📁 {node.name}", GUILayout.ExpandWidth(true));
        }

        if (!folderOverrides.ContainsKey(node.fullPath))
        {
            if (persistentFolderOverrides.ContainsKey(node.fullPath))
            {
                folderOverrides[node.fullPath] = persistentFolderOverrides[node.fullPath];
            }
            else
            {
                folderOverrides[node.fullPath] = CleanNamespacePart(node.name);
            }
        }

        string currentOverride = folderOverrides[node.fullPath];
        string newOverride = EditorGUILayout.TextField(currentOverride, GUILayout.Width(200));

        if (newOverride != currentOverride)
        {
            // Prevent saving empty overrides - use null instead
            folderOverrides[node.fullPath] = newOverride;
            persistentFolderOverrides[node.fullPath] = string.IsNullOrWhiteSpace(newOverride) ? null : newOverride;
            SaveFolderOverrides();
            UpdateAllGeneratedNamespaces();
        }

        if (GUILayout.Button("Reset", GUILayout.Width(60)))
        {
            string resetValue = CleanNamespacePart(node.name);
            folderOverrides[node.fullPath] = resetValue;
            persistentFolderOverrides[node.fullPath] = string.IsNullOrWhiteSpace(resetValue) ? null : resetValue;
            SaveFolderOverrides();
            UpdateAllGeneratedNamespaces();
        }

        EditorGUILayout.EndHorizontal();

        if (node.isExpanded && node.hasChildren)
        {
            var relevantChildren = node.children.Where(c => c.hasScriptsOrScriptChildren).OrderBy(c => c.name);
            foreach (var child in relevantChildren)
            {
                DrawFolderNode(child, depth + 1);
            }
        }
    }
    private void UpdateAllGeneratedNamespaces()
    {
        foreach (var script in filteredScripts)
        {
            script.generatedNamespace = GenerateNamespaceName(script.path);
            if (script.customNamespace == script.generatedNamespace)
            {
                script.customNamespace = "";
            }
        }
        Repaint();
    }

    private void ScanSelectedFolder()
    {
        if (!Directory.Exists(selectedFolderPath))
        {
            EditorUtility.DisplayDialog("Folder Not Found", "The selected folder does not exist.", "OK");
            return;
        }

        isLoading = true;
        Repaint();

        EditorApplication.delayCall += () =>
        {
            try
            {
                ScanFolderImmediate(selectedFolderPath);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        };
    }

    private void ScanFolderImmediate(string folderPath)
    {
        filteredScripts.Clear();

        string absoluteFolderPath = Path.GetFullPath(folderPath);

        rootFolderNode = new FolderNode
        {
            name = Path.GetFileName(folderPath),
            fullPath = folderPath
        };

        BuildCompleteFolderTree(folderPath, rootFolderNode);

        string[] allScriptFiles = Directory.GetFiles(absoluteFolderPath, "*.cs", SearchOption.AllDirectories);
        List<string> scriptFolders = new List<string>();

        foreach (string absoluteScriptPath in allScriptFiles)
        {
            string relativeScriptPath = absoluteScriptPath.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/');

            if (relativeScriptPath.StartsWith(dataPath))
            {
                relativeScriptPath = "Assets" + relativeScriptPath.Substring(dataPath.Length);
            }
            else
            {
                continue;
            }

            ScriptInfo scriptInfo = new ScriptInfo
            {
                path = relativeScriptPath,
                fileName = Path.GetFileName(relativeScriptPath),
                isEditorScript = IsEditorScript(relativeScriptPath),
                folderPath = Path.GetDirectoryName(relativeScriptPath).Replace('\\', '/')
            };

            try
            {
                string content = File.ReadAllText(absoluteScriptPath);
                scriptInfo.hasExistingNamespace = Regex.IsMatch(content, @"^\s*namespace\s+\S+", RegexOptions.Multiline);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not read {scriptInfo.fileName}: {e.Message}");
                scriptInfo.hasExistingNamespace = false;
            }

            scriptInfo.generatedNamespace = GenerateNamespaceName(relativeScriptPath);
            scriptInfo.includeInBatch = !scriptInfo.hasExistingNamespace;

            filteredScripts.Add(scriptInfo);
            scriptFolders.Add(scriptInfo.folderPath);
        }

        MarkFoldersWithScripts(rootFolderNode, scriptFolders);

        folderOverrides.Clear();
        foreach (var kvp in persistentFolderOverrides)
        {
            folderOverrides[kvp.Key] = kvp.Value;
        }

        filteredScripts = filteredScripts.OrderBy(s => s.path).ToList();

        // IMPORTANT: Update all generated namespaces after loading overrides
        UpdateAllGeneratedNamespaces();

        Debug.Log($"Found {filteredScripts.Count} scripts in '{folderPath}'");

        isLoading = false;
        needsFullRepaint = true;
        Repaint();
    }

    private void MarkFoldersWithScripts(FolderNode node, List<string> scriptFolders)
    {
        bool hasScripts = scriptFolders.Contains(node.fullPath);

        bool childHasScripts = false;
        foreach (var child in node.children)
        {
            MarkFoldersWithScripts(child, scriptFolders);
            if (child.hasScriptsOrScriptChildren)
            {
                childHasScripts = true;
            }
        }

        node.hasScriptsOrScriptChildren = hasScripts || childHasScripts;
    }

    private void BuildCompleteFolderTree(string rootPath, FolderNode rootNode)
    {
        string absoluteRootPath = Path.GetFullPath(rootPath);

        if (!Directory.Exists(absoluteRootPath))
            return;

        string[] subdirectories = Directory.GetDirectories(absoluteRootPath, "*", SearchOption.AllDirectories);

        foreach (string absoluteSubDir in subdirectories)
        {
            string relativeSubDir = absoluteSubDir.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/');

            if (relativeSubDir.StartsWith(dataPath))
            {
                relativeSubDir = "Assets" + relativeSubDir.Substring(dataPath.Length);
            }

            if (relativeSubDir.StartsWith(rootPath))
            {
                AddFolderToTree(rootNode, relativeSubDir);
            }
        }
    }

    private void AddFolderToTree(FolderNode root, string fullFolderPath)
    {
        if (fullFolderPath == root.fullPath)
            return;

        string relativePath = fullFolderPath.Substring(root.fullPath.Length).Trim('/');
        string[] pathParts = relativePath.Split('/');

        FolderNode currentNode = root;
        string currentPath = root.fullPath;

        foreach (string part in pathParts)
        {
            if (string.IsNullOrEmpty(part))
                continue;

            currentPath = currentPath + "/" + part;

            FolderNode existingNode = currentNode.children.FirstOrDefault(c => c.fullPath == currentPath);
            if (existingNode == null)
            {
                existingNode = new FolderNode
                {
                    name = part,
                    fullPath = currentPath
                };
                currentNode.children.Add(existingNode);
            }

            currentNode = existingNode;
        }
    }

    private string GenerateNamespaceName(string scriptPath)
    {
        if (!includeFolderStructure)
            return rootNamespace;

        string directory = Path.GetDirectoryName(scriptPath);
        directory = directory.Replace('\\', '/');

        if (directory.StartsWith(selectedFolderPath))
        {
            List<string> parts = new List<string>();
            string currentPath = selectedFolderPath;

            string relativePath = directory.Substring(selectedFolderPath.Length).Trim('/');

            if (string.IsNullOrEmpty(relativePath))
            {
                return rootNamespace;
            }

            string[] folders = relativePath.Split('/');

            foreach (string folder in folders)
            {
                if (string.IsNullOrEmpty(folder))
                    continue;

                currentPath = currentPath + "/" + folder;

                if (folderOverrides.ContainsKey(currentPath))
                {
                    string overrideValue = folderOverrides[currentPath];
                    if (!string.IsNullOrEmpty(overrideValue))
                    {
                        parts.Add(overrideValue);
                    }
                    // If override is empty, skip this folder entirely
                }
                else
                {
                    string clean = CleanNamespacePart(folder);
                    if (!string.IsNullOrEmpty(clean))
                    {
                        parts.Add(clean);
                    }
                    // If cleaned name is empty, skip this folder entirely
                }
            }

            if (parts.Count == 0)
                return rootNamespace;

            return rootNamespace + "." + string.Join(".", parts);
        }
        else
        {
            return rootNamespace;
        }
    }
    private void DrawScriptsList()
    {
        int selectedCount = filteredScripts.Count(s => s.includeInBatch);
        int totalCount = filteredScripts.Count;

        EditorGUILayout.LabelField($"Scripts in Folder: {totalCount} total, {selectedCount} selected", EditorStyles.boldLabel);

        if (filteredScripts.Count > 0)
        {
            DrawSelectionOptions();
        }

        EditorGUILayout.Space();

        var scriptsByFolder = filteredScripts.GroupBy(s => s.folderPath)
                                           .OrderBy(g => g.Key)
                                           .ToList();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

        foreach (var folderGroup in scriptsByFolder)
        {
            DrawFolderGroup(folderGroup.Key, folderGroup.ToList());
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (selectedCount > 0)
        {
            if (GUILayout.Button($"Generate Namespaces for {selectedCount} Selected Scripts", GUILayout.Height(30)))
            {
                GenerateNamespacesForSelectedScripts();
            }
        }
    }

    private void DrawFolderGroup(string folderPath, List<ScriptInfo> scripts)
    {
        string displayFolder = folderPath;
        if (displayFolder.StartsWith("Assets/"))
        {
            displayFolder = displayFolder.Substring(7);
        }

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        bool hasOverride = folderOverrides.ContainsKey(folderPath);
        string overrideInfo = hasOverride ? " (Override)" : "";

        EditorGUILayout.LabelField($"📁 {displayFolder}{overrideInfo}", EditorStyles.boldLabel);

        bool allSelected = scripts.All(s => s.includeInBatch);
        bool mixedSelection = scripts.Any(s => s.includeInBatch) && scripts.Any(s => !s.includeInBatch);

        EditorGUI.showMixedValue = mixedSelection;
        bool newSelection = EditorGUILayout.Toggle(allSelected, GUILayout.Width(20));
        EditorGUI.showMixedValue = false;

        if (newSelection != allSelected)
        {
            foreach (var script in scripts)
            {
                script.includeInBatch = newSelection;
            }
        }

        EditorGUILayout.EndHorizontal();

        foreach (var script in scripts.OrderBy(s => s.fileName))
        {
            DrawScriptItem(script, true);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawScriptItem(ScriptInfo script, bool indented = false)
    {
        if (indented)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
        }

        EditorGUILayout.BeginVertical("box");

        // First row: checkbox, filename, status
        EditorGUILayout.BeginHorizontal();
        script.includeInBatch = EditorGUILayout.Toggle(script.includeInBatch, GUILayout.Width(20));
        EditorGUILayout.LabelField(script.fileName, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));

        string status = script.isEditorScript ? "Editor" :
                       script.hasExistingNamespace ? "Has Namespace" : "No Namespace";
        EditorGUILayout.LabelField(status, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        // Second row: namespace label and field
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Namespace:", GUILayout.Width(80));

        // Always show the current namespace in a text field
        string currentNamespace = string.IsNullOrEmpty(script.customNamespace) ? script.generatedNamespace : script.customNamespace;
        string displayNamespace = currentNamespace;

        // If there's no custom namespace, show it as read-only but still in a text field
        if (string.IsNullOrEmpty(script.customNamespace))
        {
            GUI.enabled = false; // Disable the field
            EditorGUILayout.TextField(displayNamespace, GUILayout.ExpandWidth(true));
            GUI.enabled = true; // Re-enable
        }
        else
        {
            // Editable custom namespace
            script.customNamespace = EditorGUILayout.TextField(script.customNamespace, GUILayout.ExpandWidth(true));
        }

        if (string.IsNullOrEmpty(script.customNamespace))
        {
            if (GUILayout.Button("Customize", GUILayout.Width(80)))
            {
                script.customNamespace = script.generatedNamespace;
            }
        }
        else
        {
            if (GUILayout.Button("Reset", GUILayout.Width(80)))
            {
                script.customNamespace = "";
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        if (indented)
        {
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawSelectionOptions()
    {
        EditorGUILayout.BeginVertical("box");

        showSelectAllOptions = EditorGUILayout.Foldout(showSelectAllOptions, "Batch Selection Options", true);

        if (showSelectAllOptions)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Select All"))
            {
                foreach (var script in filteredScripts)
                {
                    script.includeInBatch = true;
                }
            }

            if (GUILayout.Button("Deselect All"))
            {
                foreach (var script in filteredScripts)
                {
                    script.includeInBatch = false;
                }
            }

            if (GUILayout.Button("Select Without Namespaces"))
            {
                foreach (var script in filteredScripts)
                {
                    script.includeInBatch = !script.hasExistingNamespace;
                }
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Update All Generated Namespaces"))
            {
                UpdateAllGeneratedNamespaces();
            }
        }

        EditorGUILayout.EndVertical();
    }

    private string CleanNamespacePart(string folderName)
    {
        if (string.IsNullOrEmpty(folderName))
            return "Default";

        // Remove common non-namespace folder names
        string clean = folderName.Trim('_', ' ', '-', '.');

        // Skip common folder names that shouldn't be in namespaces
        if (clean.Equals("Assets", StringComparison.OrdinalIgnoreCase) ||
            clean.Equals("Scripts", StringComparison.OrdinalIgnoreCase) ||
            clean.Equals("Editor", StringComparison.OrdinalIgnoreCase) ||
            clean.Equals("Resources", StringComparison.OrdinalIgnoreCase) ||
            clean.Equals("Plugins", StringComparison.OrdinalIgnoreCase))
        {
            return "";
        }

        clean = Regex.Replace(clean, @"[^a-zA-Z0-9_]", " ");
        clean = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(clean);
        clean = clean.Replace(" ", "");

        if (string.IsNullOrEmpty(clean))
        {
            // Final fallback - use alphanumeric characters only
            clean = Regex.Replace(folderName, @"[^a-zA-Z0-9]", "");
            if (string.IsNullOrEmpty(clean))
            {
                clean = "Folder";
            }
        }

        if (clean.Length > 0 && !char.IsLetter(clean[0]))
        {
            clean = "N" + clean;
        }

        return clean;
    }
    private void GenerateNamespacesForSelectedScripts()
    {
        var selectedScripts = filteredScripts.Where(s => s.includeInBatch).ToList();
        int processedCount = 0;
        int errorCount = 0;

        foreach (var script in selectedScripts)
        {
            try
            {
                if (ProcessScript(script))
                {
                    processedCount++;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to process {script.path}: {e.Message}");
                errorCount++;
            }
        }

        AssetDatabase.Refresh();
        ScanFolderImmediate(selectedFolderPath);

        EditorUtility.DisplayDialog("Namespace Generation Complete",
            $"Processed: {processedCount} scripts\nErrors: {errorCount}", "OK");
    }

    private bool ProcessScript(ScriptInfo script)
    {
        string content = File.ReadAllText(script.path);

        if (script.hasExistingNamespace)
        {
            Debug.LogWarning($"Skipped {script.fileName} - already has namespace");
            return false;
        }

        if (backupScripts)
        {
            string backupPath = script.path + ".backup";
            File.WriteAllText(backupPath, content);
        }

        string newContent = AddNamespaceToScript(content, script.FinalNamespace);
        File.WriteAllText(script.path, newContent);

        Debug.Log($"Added namespace '{script.FinalNamespace}' to {script.fileName}");
        return true;
    }

    private string AddNamespaceToScript(string content, string namespaceName)
    {
        string[] lines = content.Split('\n');
        List<string> usingStatements = new List<string>();
        List<string> otherLines = new List<string>();
        bool inUsingBlock = true;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            if (inUsingBlock && trimmed.StartsWith("using ") && trimmed.EndsWith(";"))
            {
                usingStatements.Add(line);
            }
            else
            {
                inUsingBlock = false;
                if (!string.IsNullOrEmpty(trimmed) || otherLines.Count > 0)
                {
                    otherLines.Add(line);
                }
            }
        }

        System.Text.StringBuilder newContent = new System.Text.StringBuilder();

        if (usingStatements.Count > 0)
        {
            newContent.AppendLine(string.Join("\n", usingStatements));
            newContent.AppendLine();
        }

        newContent.AppendLine($"namespace {namespaceName}");
        newContent.AppendLine("{");

        foreach (string line in otherLines)
        {
            newContent.AppendLine("\t" + line);
        }

        newContent.AppendLine("}");

        return newContent.ToString();
    }

    private bool IsEditorScript(string scriptPath)
    {
        return scriptPath.Contains("/Editor/") || Path.GetFileName(scriptPath).EndsWith("Editor.cs");
    }
}