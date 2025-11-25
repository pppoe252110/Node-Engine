using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class NodesSetupWindow : EditorWindow
{
    private const string SETUP_COMPLETE_KEY = "NodeEngine_SetupComplete";
    private const string TARGET_RESOURCE_PATH = "Assets/Resources/NodeEngine";

    private const string PACKAGE_VERSION_KEY = "NodeEngine_PackageVersion";
    private const string GITHUB_PACKAGE_URL = "https://raw.githubusercontent.com/pppoe252110/Node-Engine/main/Assets/NodeEngine/package.json";

    private bool copyResources = true;
    private bool copyGraphExamples = true;
    private Vector2 scrollPosition;
    private bool showPackageResources = false;
    private bool showAdvancedOptions = false;
    private bool isCheckingForUpdates = false;
    private bool updateAvailable = false;
    private string localVersion = "";
    private string remoteVersion = "";

    private Color accentColor = new Color(0.2f, 0.6f, 1f, 1f);
    private Color successColor = new Color(0.2f, 0.8f, 0.4f, 1f);
    private Color warningColor = new Color(1f, 0.7f, 0.2f, 1f);
    private Color errorColor = new Color(0.9f, 0.3f, 0.3f, 1f);

    [MenuItem("Tools/Node Engine/Setup")]
    public static void ShowWindow()
    {
        NodesSetupWindow window = GetWindow<NodesSetupWindow>("Node Engine Setup");
        window.minSize = new Vector2(500, 500);
        window.Show();
    }

    [InitializeOnLoadMethod]
    private static void InitializeOnLoad()
    {
        if (IsOutputFolderNotEmpty())
        {
            EditorPrefs.SetBool(SETUP_COMPLETE_KEY, true);
        }

        if (!EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false))
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false))
                {
                    ShowWindow();
                }
            };
        }
    }

    private static bool IsOutputFolderNotEmpty()
    {
        string targetFullPath = Path.Combine(Application.dataPath, TARGET_RESOURCE_PATH.Substring("Assets/".Length));

        if (!Directory.Exists(targetFullPath))
        {
            return false;
        }

        string[] files = Directory.GetFiles(targetFullPath, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".meta")).ToArray();

        return files.Length > 0;
    }

    private void OnEnable()
    {
        
        GetLocalVersion();

        
        CheckForUpdatesAsync().Forget();
    }

    private void GetLocalVersion()
    {
        
        string packagePath = FindPackageJsonPath();
        if (!string.IsNullOrEmpty(packagePath) && File.Exists(packagePath))
        {
            try
            {
                string jsonContent = File.ReadAllText(packagePath);
                var packageJson = JsonUtility.FromJson<PackageInfo>(jsonContent);
                localVersion = packageJson.version;
                EditorPrefs.SetString(PACKAGE_VERSION_KEY, localVersion);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read local package version: {e.Message}");
                localVersion = "Unknown";
            }
        }
        else
        {
            
            localVersion = EditorPrefs.GetString(PACKAGE_VERSION_KEY, "Unknown");
        }
    }

    private string FindPackageJsonPath()
    {
        
        string[] guids = AssetDatabase.FindAssets("package.json");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("NodeEngine") || path.Contains("Node Engine"))
            {
                return path;
            }
        }

        
        string packageCacheRoot = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Library", "PackageCache");
        if (Directory.Exists(packageCacheRoot))
        {
            string[] packageFolders = Directory.GetDirectories(packageCacheRoot, "com.parity.nodeengine*");
            if (packageFolders.Length > 0)
            {
                return Path.Combine(packageFolders[0], "package.json");
            }
        }

        return null;
    }

    private async UniTaskVoid CheckForUpdatesAsync()
    {
        if (isCheckingForUpdates) return;

        isCheckingForUpdates = true;
        updateAvailable = false;

        using (var request = UnityWebRequest.Get(GITHUB_PACKAGE_URL))
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonContent = request.downloadHandler.text;
                    var packageJson = JsonUtility.FromJson<PackageInfo>(jsonContent);
                    remoteVersion = packageJson.version;

                    
                    if (!string.IsNullOrEmpty(localVersion) && !string.IsNullOrEmpty(remoteVersion))
                    {
                        updateAvailable = CompareVersions(localVersion, remoteVersion) < 0;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse remote package version: {e.Message}");
                    remoteVersion = "Error";
                }
            }
            else
            {
                Debug.LogError($"Failed to check for updates: {request.error}");
                remoteVersion = "Error";
            }
        }

        isCheckingForUpdates = false;
        Repaint();
    }

    private int CompareVersions(string v1, string v2)
    {
        var v1Parts = v1.Split('.');
        var v2Parts = v2.Split('.');

        for (int i = 0; i < Math.Max(v1Parts.Length, v2Parts.Length); i++)
        {
            int v1Part = i < v1Parts.Length ? int.Parse(v1Parts[i]) : 0;
            int v2Part = i < v2Parts.Length ? int.Parse(v2Parts[i]) : 0;

            if (v1Part < v2Part) return -1;
            if (v1Part > v2Part) return 1;
        }

        return 0;
    }

    private void OnGUI()
    {
        
        GUIStyle headerStyle = new GUIStyle(EditorStyles.largeLabel)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Overflow,
            normal = { textColor = Color.white }
        };

        GUIStyle versionStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.8f, 0.8f, 0.8f, 1f) }
        };

        GUIStyle sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            normal = { textColor = accentColor }
        };

        GUIStyle buttonStyle = new GUIStyle("button")
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };

        
        Rect headerRect = new Rect(0, 0, position.width, 120);
        EditorGUI.DrawRect(headerRect, new Color(0.1f, 0.1f, 0.2f, 1f));

        
        Rect logoRect = new Rect(position.width / 2 - 30, 10, 60, 60);

        EditorGUI.DrawRect(logoRect, accentColor);
        GUI.Label(logoRect, "NE", new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        });

        
        GUILayout.Space(80);
        EditorGUILayout.LabelField("Node Engine Setup", headerStyle);

        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField($"Local Version: {localVersion}", versionStyle);
        if (isCheckingForUpdates)
        {
            EditorGUILayout.LabelField("Checking for updates...", versionStyle);
        }
        else if (!string.IsNullOrEmpty(remoteVersion) && remoteVersion != "Error")
        {
            Color versionColor = updateAvailable ? warningColor : successColor;
            EditorGUILayout.LabelField($"Latest Version: {remoteVersion}", new GUIStyle(versionStyle)
            {
                normal = { textColor = versionColor }
            });

            if (updateAvailable)
            {
                EditorGUILayout.LabelField("Update Available!", new GUIStyle(versionStyle)
                {
                    normal = { textColor = warningColor },
                    fontStyle = FontStyle.Bold
                });
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        
        Rect accentLineRect = new Rect(0, 120, position.width, 3);
        EditorGUI.DrawRect(accentLineRect, accentColor);

        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        
        EditorGUILayout.Space(20);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Setup Options", sectionHeaderStyle);
        EditorGUILayout.Space(10);

        copyResources = EditorGUILayout.ToggleLeft(" Copy Resources and Fix References", copyResources);
        EditorGUILayout.HelpBox("Copies files from the package to 'Assets/Resources/NodeEngine' and updates all internal GUID references.", MessageType.Info);

        copyGraphExamples = EditorGUILayout.ToggleLeft(" Copy Graph Examples", copyGraphExamples);
        EditorGUILayout.HelpBox("Copies example graphs from the package to the 'Assets' root folder.", MessageType.Info);

        EditorGUILayout.Space(10);

        
        GUI.enabled = copyResources || copyGraphExamples;
        Color originalBgColor = GUI.backgroundColor;
        GUI.backgroundColor = accentColor;
        if (GUILayout.Button("Run Setup", GUILayout.Height(40)))
        {
            RunSetup();
        }
        GUI.backgroundColor = originalBgColor;
        GUI.enabled = true;

        EditorGUILayout.EndVertical();

        
        EditorGUILayout.Space(20);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Setup Status", sectionHeaderStyle);
        EditorGUILayout.Space(10);

        bool isSetupComplete = EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false);
        string statusText = isSetupComplete ? "✅ Setup Complete" : "❌ Setup Required";
        Color statusColor = isSetupComplete ? successColor : warningColor;

        GUIStyle statusStyle = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = statusColor },
            fontStyle = FontStyle.Bold,
            fontSize = 16
        };
        EditorGUILayout.LabelField(statusText, statusStyle);

        if (isSetupComplete)
        {
            EditorGUILayout.HelpBox("Node Engine is ready to use! You can access nodes through the Space key context menu.", MessageType.Info);

            GUI.backgroundColor = accentColor;
            if (GUILayout.Button("Open NodeEngine Folder", GUILayout.Height(30)))
            {
                OpenNodeEngineFolder();
            }
            GUI.backgroundColor = originalBgColor;
        }

        EditorGUILayout.EndVertical();

        
        if (updateAvailable)
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Update Available", sectionHeaderStyle);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox($"A new version ({remoteVersion}) of Node Engine is available. You are currently using version {localVersion}.", MessageType.Warning);

            GUI.backgroundColor = warningColor;
            if (GUILayout.Button("Get Latest Version", GUILayout.Height(30)))
            {
                Application.OpenURL("https://github.com/pppoe252110/Node-Engine/releases");
            }
            GUI.backgroundColor = originalBgColor;

            EditorGUILayout.EndVertical();
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

            GUI.backgroundColor = warningColor;
            if (GUILayout.Button("Force Re-Setup"))
            {
                ResetSetupStatus();
                RunSetup();
            }
            GUI.backgroundColor = originalBgColor;

            if (GUILayout.Button("Show NodeEngine in Explorer"))
                ShowInExplorer();

            if (GUILayout.Button("Check for Updates"))
            {
                CheckForUpdatesAsync().Forget();
            }

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

            if (copyGraphExamples)
            {
                float progress = copyResources ? 0.7f : 0.8f;
                EditorUtility.DisplayProgressBar("Node Engine Setup", "Copying GraphExamples...", progress);
                CopyGraphExamples();
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

        Directory.CreateDirectory(targetFullPath);

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

        foreach (string sourceFile in sourceFiles)
        {
            string relativePath = sourceFile.Substring(sourcePath.Length + 1);
            string targetFile = Path.Combine(targetFullPath, relativePath);
            string targetDir = Path.GetDirectoryName(targetFile);
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
            File.Copy(sourceFile, targetFile, true);
        }

        AssetDatabase.Refresh();

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

        AssetDatabase.Refresh();
        Debug.Log("Resource copy and reference fixing complete.");
    }

    private void CopyGraphExamples()
    {
        string sourcePath = FindPackageGraphExamplesPath();
        if (string.IsNullOrEmpty(sourcePath) || !Directory.Exists(sourcePath))
        {
            Debug.LogWarning($"GraphExamples not found at: {sourcePath}");
            return;
        }

        string targetPath = Application.dataPath;

        string[] sourceFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".meta")).ToArray();

        if (sourceFiles.Length == 0)
        {
            Debug.LogWarning("No files found in GraphExamples folder.");
            return;
        }

        foreach (string sourceFile in sourceFiles)
        {
            string relativePath = sourceFile.Substring(sourcePath.Length + 1);
            string targetFile = Path.Combine(targetPath, relativePath);
            string targetDir = Path.GetDirectoryName(targetFile);
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);
            File.Copy(sourceFile, targetFile, true);
            Debug.Log($"Copied GraphExample: {relativePath}");
        }

        Debug.Log($"Copied {sourceFiles.Length} GraphExample files to Assets folder.");
    }

    private string FindPackageGraphExamplesPath()
    {
        string packageCacheRoot = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Library", "PackageCache");
        if (!Directory.Exists(packageCacheRoot)) return null;

        string packageSearchPattern = "com.parity.nodeengine*";
        string[] packageFolders = Directory.GetDirectories(packageCacheRoot, packageSearchPattern);

        if (packageFolders.Length > 0)
        {
            string graphExamplesPath = Path.Combine(packageFolders[0], "GraphExamples");
            if (Directory.Exists(graphExamplesPath)) return graphExamplesPath;
        }

        return null;
    }

    private string FindPackageResourcesPath()
    {
        string packageCacheRoot = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Library", "PackageCache");
        if (!Directory.Exists(packageCacheRoot)) return null;

        string packageSearchPattern = "com.parity.nodeengine*";
        string[] packageFolders = Directory.GetDirectories(packageCacheRoot, packageSearchPattern);

        if (packageFolders.Length > 0)
        {
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
            UnityEngine.Object folder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);
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

    [System.Serializable]
    private class PackageInfo
    {
        public string name;
        public string version;
        public string description;
        public string unity;
        public string unityRelease;
    }
}