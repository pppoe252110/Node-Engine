using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class NodesSetupWindow : EditorWindow
{
    private const string SETUP_COMPLETE_KEY = "NodeEngine_SetupComplete";
    // The final destination for the copied files
    private const string TARGET_RESOURCE_PATH = "Assets/Resources/NodeEngine";

    private bool copyResources = true;
    private Vector2 scrollPosition;
    private bool showPackageResources = false;
    private bool showAdvancedOptions = false;

    [MenuItem("Tools/Node Engine/Setup")]
    public static void ShowWindow()
    {
        NodesSetupWindow window = GetWindow<NodesSetupWindow>("Node Engine Setup");
        window.minSize = new Vector2(400, 400);
        window.Show();
    }

    [InitializeOnLoadMethod]
    private static void InitializeOnLoad()
    {
        if (!EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false))
        {
            EditorApplication.delayCall += () => {
                if (!EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false))
                {
                    ShowWindow();
                }
            };
        }
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space(20);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.largeLabel) { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.LabelField("Node Engine Setup", headerStyle);
        EditorGUILayout.Space(10);

        GUIStyle descStyle = new GUIStyle(EditorStyles.wordWrappedLabel) { fontSize = 12, richText = true };
        EditorGUILayout.LabelField("This setup will copy the necessary resources from the package to your project and fix all references.", descStyle);
        EditorGUILayout.Space(20);

        // REMOVED: Folder structure option
        EditorGUILayout.LabelField("Setup Options", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        copyResources = EditorGUILayout.ToggleLeft(" Copy Resources and Fix References", copyResources);
        EditorGUILayout.HelpBox("Copies files from the package to 'Assets/Resources/NodeEngine' and updates all internal GUID references.", MessageType.Info);

        EditorGUILayout.Space(30);

        GUI.enabled = copyResources;
        if (GUILayout.Button("Run Setup", GUILayout.Height(40)))
        {
            RunSetup();
        }
        GUI.enabled = true;

        EditorGUILayout.Space(20);

        EditorGUILayout.LabelField("Setup Status", EditorStyles.boldLabel);
        bool isSetupComplete = EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false);
        string statusText = isSetupComplete ? "✅ Setup Complete" : "❌ Setup Required";
        Color statusColor = isSetupComplete ? Color.green : Color.yellow;

        GUIStyle statusStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = statusColor }, fontStyle = FontStyle.Bold };
        EditorGUILayout.LabelField(statusText, statusStyle);

        if (isSetupComplete)
        {
            EditorGUILayout.HelpBox("Node Engine is ready to use! You can access nodes through the Space key context menu.", MessageType.Info);
            if (GUILayout.Button("Open NodeEngine Folder"))
            {
                OpenNodeEngineFolder();
            }
        }

        EditorGUILayout.Space(20);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        showPackageResources = EditorGUILayout.Foldout(showPackageResources, "Package Resources", true);
        if (showPackageResources)
        {
            EditorGUILayout.Space(10);
            ShowPackageResourcesPreview();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(20);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options", true);
        if (showAdvancedOptions)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Use these options to reset or manually manage the setup.", MessageType.Warning);

            if (GUILayout.Button("Force Re-Setup"))
            {
                ResetSetupStatus();
                RunSetup();
            }

            if (GUILayout.Button("Show NodeEngine in Explorer")) ShowInExplorer();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Manual Path Configuration", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"Package Path: {FindPackageResourcesPath()}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Target Path: {TARGET_RESOURCE_PATH}", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }

    private void RunSetup()
    {
        try
        {

            EditorUtility.DisplayProgressBar("Node Engine Setup", "Starting setup...", 0f);

            if (copyResources)
            {
                EditorUtility.DisplayProgressBar("Node Engine Setup", "Copying resources and fixing references...", 0.5f);
                CopyResourcesAndFixReferences();
            }
            EditorPrefs.SetBool(SETUP_COMPLETE_KEY, true);
            EditorUtility.DisplayProgressBar("Node Engine Setup", "Finalizing...", 1f);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();

            EditorUtility.DisplayDialog("Setup Complete", "Node Engine has been successfully set up!\n\nResources copied and references fixed.\n\nYou can now press Space to open the nodes menu!", "OK");
            HighlightCreatedFolder();
            this.Repaint();
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Setup Failed", $"Setup encountered an error: {e.Message}\n\nCheck the console for more details.", "OK");
            Debug.LogError($"Node Engine Setup Failed: {e}\n{e.StackTrace}");
        }
    }

    private void CopyResourcesAndFixReferences()
    {
        string sourcePath = FindPackageResourcesPath();
        if (string.IsNullOrEmpty(sourcePath) || !Directory.Exists(sourcePath))
        {
            throw new System.Exception($"Package resources not found at: {sourcePath}");
        }

        string targetRelativePath = TARGET_RESOURCE_PATH;
        string targetFullPath = Path.Combine(Application.dataPath, targetRelativePath.Substring("Assets/".Length));

        // Ensure the target directory exists
        Directory.CreateDirectory(targetFullPath);

        // --- Step 1: Get a map of original file paths to their original GUIDs from the package ---
        Dictionary<string, string> originalGuids = new Dictionary<string, string>();
        string[] sourceFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).Where(f => !f.EndsWith(".meta")).ToArray();

        foreach (string sourceFile in sourceFiles)
        {
            string relativePath = sourceFile.Substring(sourcePath.Length + 1).Replace('\\', '/');
            string metaFile = sourceFile + ".meta";
            if (File.Exists(metaFile))
            {
                string guid = ExtractGuidFromMeta(metaFile);
                if (!string.IsNullOrEmpty(guid))
                {
                    originalGuids[relativePath] = guid;
                }
            }
        }

        // --- Step 2: Copy files (excluding .meta) ---
        foreach (string sourceFile in sourceFiles)
        {
            string relativePath = sourceFile.Substring(sourcePath.Length + 1);
            string targetFile = Path.Combine(targetFullPath, relativePath);
            string targetDir = Path.GetDirectoryName(targetFile);
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
            File.Copy(sourceFile, targetFile, true);
        }

        // --- Step 3: Let Unity import the new files to generate new GUIDs ---
        AssetDatabase.Refresh();

        // --- Step 4: Get a map of file paths to their NEW GUIDs ---
        Dictionary<string, string> newGuids = new Dictionary<string, string>();
        foreach (var entry in originalGuids.Keys)
        {
            string newAssetPath = Path.Combine(targetRelativePath, entry).Replace('\\', '/');
            string newGuid = AssetDatabase.AssetPathToGUID(newAssetPath);
            if (!string.IsNullOrEmpty(newGuid))
            {
                newGuids[entry] = newGuid;
            }
        }

        // --- Step 5: Create the final mapping from old GUID to new GUID ---
        Dictionary<string, string> guidReplacementMap = new Dictionary<string, string>();
        foreach (var entry in originalGuids)
        {
            string relativePath = entry.Key;
            string oldGuid = entry.Value;
            if (newGuids.TryGetValue(relativePath, out string newGuid))
            {
                if (oldGuid != newGuid)
                {
                    guidReplacementMap[oldGuid] = newGuid;
                    Debug.Log($"Mapping GUID: {oldGuid} -> {newGuid} for {relativePath}");
                }
            }
        }

        // --- Step 6: Find all prefabs in the target folder and replace the GUIDs ---
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { targetRelativePath });
        foreach (string prefabGuid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            string fullPath = Path.Combine(Application.dataPath, prefabPath.Substring("Assets/".Length));

            string content = File.ReadAllText(fullPath);
            bool modified = false;

            foreach (var mapping in guidReplacementMap)
            {
                if (content.Contains(mapping.Key))
                {
                    content = content.Replace(mapping.Key, mapping.Value);
                    modified = true;
                }
            }

            if (modified)
            {
                File.WriteAllText(fullPath, content);
                Debug.Log($"Updated references in {prefabPath}");
            }
        }

        // Final refresh to ensure all changes are picked up
        AssetDatabase.Refresh();
        Debug.Log("Resource copy and reference fixing complete.");
    }

    private string FindPackageResourcesPath()
    {
        string packageCacheRoot = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Library", "PackageCache");
        if (!Directory.Exists(packageCacheRoot)) return null;

        string packageSearchPattern = "com.parity.nodeengine*";
        string[] packageFolders = Directory.GetDirectories(packageCacheRoot, packageSearchPattern);

        if (packageFolders.Length > 0)
        {
            // The first one is usually fine
            string resourcesPath = Path.Combine(packageFolders[0], "Resources");
            if (Directory.Exists(resourcesPath)) return resourcesPath;
        }

        return null;
    }

    private string ExtractGuidFromMeta(string metaFilePath)
    {
        string[] lines = File.ReadAllLines(metaFilePath);
        foreach (string line in lines)
        {
            if (line.Trim().StartsWith("guid:"))
            {
                return line.Split(':')[1].Trim();
            }
        }
        return null;
    }

    private void ShowPackageResourcesPreview()
    {
        string packagePath = FindPackageResourcesPath();
        bool packageExists = !string.IsNullOrEmpty(packagePath) && Directory.Exists(packagePath);

        if (!packageExists)
        {
            EditorGUILayout.HelpBox($"Package not found.\n\nPlease check:\n• Package installation", MessageType.Error);
            return;
        }

        string[] packageFiles = Directory.GetFiles(packagePath, "*", SearchOption.AllDirectories).Where(f => !f.EndsWith(".meta")).ToArray();
        if (packageFiles.Length == 0)
        {
            EditorGUILayout.HelpBox("No resources found in package.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("Package Resources Found:", EditorStyles.boldLabel);
        foreach (string file in packageFiles.Take(15)) EditorGUILayout.LabelField($"• {Path.GetFileName(file)}", EditorStyles.miniLabel);
        if (packageFiles.Length > 15) EditorGUILayout.LabelField($"... and {packageFiles.Length - 15} more files", EditorStyles.miniLabel);
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox($"Total files in package: {packageFiles.Length}", MessageType.Info);
    }

    private void ResetSetupStatus()
    {
        EditorPrefs.DeleteKey(SETUP_COMPLETE_KEY);
        Debug.Log("Setup status reset.");
        this.Repaint();
    }

    private void OpenNodeEngineFolder()
    {
        string folderPath = TARGET_RESOURCE_PATH;
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
            Selection.activeObject = folder;
            EditorGUIUtility.PingObject(folder);
        }
        else
        {
            EditorUtility.DisplayDialog("Not Found", "NodeEngine folder not found. Please run the setup first.", "OK");
        }
    }

    private void HighlightCreatedFolder()
    {
        OpenNodeEngineFolder();
    }

    private void ShowInExplorer()
    {
        string path = Path.Combine(Application.dataPath, TARGET_RESOURCE_PATH.Substring("Assets/".Length));
        if (Directory.Exists(path))
        {
            EditorUtility.RevealInFinder(path);
        }
        else
        {
            EditorUtility.DisplayDialog("Not Found", "NodeEngine folder not found. Please run the setup first.", "OK");
        }
    }
}