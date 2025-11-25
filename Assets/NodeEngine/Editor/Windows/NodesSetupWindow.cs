using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class NodesSetupWindow : EditorWindow
{
    private const string SETUP_COMPLETE_KEY = "NodeEngine_SetupComplete";
    private const string NODE_ENGINE_FOLDER = "NodeEngine";
    private const string PACKAGES_RESOURCE_PATH = "Packages/com.parity.nodeengine/Resources/"; // Updated path
    private const string TARGET_RESOURCE_PATH = "Assets/Resources/NodeEngine/"; // Changed to NodeEngine folder

    private bool copyResources = true;
    private bool createFolderStructure = true;
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
        // Check if setup is complete on editor load
        if (!EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false))
        {
            // Small delay to ensure editor is fully loaded
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

        // Header
        EditorGUILayout.Space(20);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.largeLabel)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("Node Engine Setup", headerStyle);
        EditorGUILayout.Space(10);

        // Description
        GUIStyle descStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
        {
            fontSize = 12,
            richText = true
        };
        EditorGUILayout.LabelField("Welcome to Node Engine! This setup will copy the necessary resources from the package to your project.", descStyle);
        EditorGUILayout.Space(20);

        // Setup Options
        EditorGUILayout.LabelField("Setup Options", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        createFolderStructure = EditorGUILayout.ToggleLeft(" Create NodeEngine Folder Structure", createFolderStructure);
        EditorGUILayout.HelpBox("Creates a 'NodeEngine' folder in your Assets with organized subfolders.", MessageType.Info);

        copyResources = EditorGUILayout.ToggleLeft(" Copy Resources from Package", copyResources);
        EditorGUILayout.HelpBox("Copies the entire Resources folder from the package to your project.", MessageType.Info);

        EditorGUILayout.Space(30);

        // Setup Button
        GUI.enabled = createFolderStructure || copyResources;
        if (GUILayout.Button("Run Setup", GUILayout.Height(40)))
        {
            RunSetup();
        }
        GUI.enabled = true;

        EditorGUILayout.Space(20);

        // Status
        EditorGUILayout.LabelField("Setup Status", EditorStyles.boldLabel);
        bool isSetupComplete = EditorPrefs.GetBool(SETUP_COMPLETE_KEY, false);
        string statusText = isSetupComplete ? "✅ Setup Complete" : "❌ Setup Required";
        Color statusColor = isSetupComplete ? Color.green : Color.yellow;

        GUIStyle statusStyle = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = statusColor },
            fontStyle = FontStyle.Bold
        };
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

        // Package Resources Preview
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        showPackageResources = EditorGUILayout.Foldout(showPackageResources, "Package Resources", true);
        if (showPackageResources)
        {
            EditorGUILayout.Space(10);
            ShowPackageResourcesPreview();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(20);

        // Advanced Section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options", true);
        if (showAdvancedOptions)
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox("Use these options to reset or manually manage the setup.", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Setup Status"))
            {
                ResetSetupStatus();
            }

            if (GUILayout.Button("Force Re-Setup"))
            {
                ResetSetupStatus();
                RunSetup();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Show NodeEngine in Explorer"))
            {
                ShowInExplorer();
            }

            EditorGUILayout.Space(10);

            // Manual path configuration
            EditorGUILayout.LabelField("Manual Path Configuration", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"Package Path: {PACKAGES_RESOURCE_PATH}", EditorStyles.miniLabel);
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

            // 1. Create folder structure
            if (createFolderStructure)
            {
                EditorUtility.DisplayProgressBar("Node Engine Setup", "Creating folder structure...", 0.3f);
                CreateFolderStructure();
            }

            // 2. Copy resources from package
            if (copyResources)
            {
                EditorUtility.DisplayProgressBar("Node Engine Setup", "Copying resources from package...", 0.7f);
                CopyResourcesFromPackage();
            }

            // Mark setup as complete
            EditorPrefs.SetBool(SETUP_COMPLETE_KEY, true);
            EditorUtility.DisplayProgressBar("Node Engine Setup", "Finalizing...", 1f);

            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();

            // Show success message with what was created
            string message = "Node Engine has been successfully set up!\n\nWhat was created:";
            if (createFolderStructure) message += "\n• NodeEngine folder structure";
            if (copyResources) message += "\n• Resources from package";
            message += "\n\nYou can now press Space to open the nodes menu!";

            EditorUtility.DisplayDialog("Setup Complete", message, "OK");

            // Highlight the created folder in Project window
            HighlightCreatedFolder();

            this.Repaint();
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Setup Failed", $"Setup encountered an error: {e.Message}", "OK");
            Debug.LogError($"Node Engine Setup Failed: {e}");
        }
    }

    private void CreateFolderStructure()
    {
        // Main NodeEngine folder
        if (!AssetDatabase.IsValidFolder($"Assets/{NODE_ENGINE_FOLDER}"))
        {
            AssetDatabase.CreateFolder("Assets", NODE_ENGINE_FOLDER);
            Debug.Log($"Created folder: Assets/{NODE_ENGINE_FOLDER}");
        }

        // Subfolders
        string[] subfolders = {
            "Resources",
            "Scripts",
            "Prefabs",
            "Scenes",
            "Art"
        };

        foreach (string folder in subfolders)
        {
            string fullPath = $"Assets/{NODE_ENGINE_FOLDER}/{folder}";
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder($"Assets/{NODE_ENGINE_FOLDER}", folder);
                Debug.Log($"Created folder: {fullPath}");
            }
        }

        Debug.Log("Node Engine folder structure created successfully.");
    }

    private void CopyResourcesFromPackage()
    {
        // Check if package resources exist
        if (!Directory.Exists(Path.Combine(Application.dataPath, "../", PACKAGES_RESOURCE_PATH)) &&
            !AssetDatabase.IsValidFolder(PACKAGES_RESOURCE_PATH))
        {
            string errorMessage = $"Package resources not found at: {PACKAGES_RESOURCE_PATH}\n\n" +
                                "Please make sure:\n" +
                                "1. The Node Engine package is properly installed\n" +
                                "2. The package contains a Resources folder\n" +
                                "3. The package name in the path is correct";

            Debug.LogError(errorMessage);
            EditorUtility.DisplayDialog("Resources Not Found", errorMessage, "OK");
            return;
        }

        // Ensure target directory exists
        string targetParentPath = "Assets/NodeEngine";
        if (!AssetDatabase.IsValidFolder(targetParentPath))
        {
            Debug.LogError($"NodeEngine folder doesn't exist. Please enable 'Create Folder Structure'.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(TARGET_RESOURCE_PATH))
        {
            AssetDatabase.CreateFolder(targetParentPath, "Resources");
            Debug.Log($"Created folder: {TARGET_RESOURCE_PATH}");
        }

        // Copy entire Resources folder recursively
        int copiedFiles = CopyFolderRecursive(PACKAGES_RESOURCE_PATH, TARGET_RESOURCE_PATH);

        if (copiedFiles > 0)
        {
            Debug.Log($"Successfully copied {copiedFiles} files from package to: {TARGET_RESOURCE_PATH}");
        }
        else
        {
            Debug.LogWarning("No files were copied. The package Resources folder might be empty.");
        }
    }

    private int CopyFolderRecursive(string sourcePath, string targetPath)
    {
        int filesCopied = 0;

        // Use AssetDatabase to find all assets in the source path
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { sourcePath });

        foreach (string guid in assetGuids)
        {
            string sourceAssetPath = AssetDatabase.GUIDToAssetPath(guid);

            // Skip .meta files
            if (sourceAssetPath.EndsWith(".meta"))
                continue;

            string relativePath = sourceAssetPath.Substring(sourcePath.Length);
            string targetAssetPath = targetPath + relativePath;

            // Create directory if it doesn't exist
            string targetDirectory = Path.GetDirectoryName(targetAssetPath);
            if (!AssetDatabase.IsValidFolder(targetDirectory))
            {
                string parentFolder = Path.GetDirectoryName(targetDirectory);
                string folderName = Path.GetFileName(targetDirectory);
                if (!string.IsNullOrEmpty(parentFolder) && !string.IsNullOrEmpty(folderName))
                {
                    AssetDatabase.CreateFolder(parentFolder, folderName);
                }
            }

            // Copy the asset if it doesn't exist
            if (!File.Exists(targetAssetPath))
            {
                AssetDatabase.CopyAsset(sourceAssetPath, targetAssetPath);
                Debug.Log($"Copied: {Path.GetFileName(sourceAssetPath)}");
                filesCopied++;
            }
            else
            {
                Debug.Log($"Skipped (already exists): {Path.GetFileName(sourceAssetPath)}");
            }
        }

        return filesCopied;
    }

    private void ShowPackageResourcesPreview()
    {
        bool packageExists = AssetDatabase.IsValidFolder(PACKAGES_RESOURCE_PATH) ||
                           Directory.Exists(Path.Combine(Application.dataPath, "../", PACKAGES_RESOURCE_PATH));

        if (!packageExists)
        {
            EditorGUILayout.HelpBox($"Package not found at: {PACKAGES_RESOURCE_PATH}\n\nPlease check:\n• Package installation\n• Package name in the path", MessageType.Error);
            return;
        }

        // Try to find assets using AssetDatabase
        string[] packageAssets = AssetDatabase.FindAssets("", new[] { PACKAGES_RESOURCE_PATH });

        if (packageAssets.Length == 0)
        {
            EditorGUILayout.HelpBox("No resources found in package. The package might be empty or the path is incorrect.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("Package Resources Found:", EditorStyles.boldLabel);

        foreach (string guid in packageAssets.Take(15)) // Show first 15 to avoid spam
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith(".meta")) continue;

            string assetName = Path.GetFileName(assetPath);
            EditorGUILayout.LabelField($"• {assetName}", EditorStyles.miniLabel);
        }

        if (packageAssets.Length > 15)
        {
            EditorGUILayout.LabelField($"... and {packageAssets.Length - 15} more files", EditorStyles.miniLabel);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox($"Total files in package: {packageAssets.Length}", MessageType.Info);
    }

    private void ResetSetupStatus()
    {
        EditorPrefs.DeleteKey(SETUP_COMPLETE_KEY);
        Debug.Log("Setup status reset. The setup window will appear on next editor load.");
        this.Repaint();
    }

    private void OpenNodeEngineFolder()
    {
        string folderPath = "Assets/NodeEngine";
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
        string folderPath = "Assets/Resources/NodeEngine";
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
            Selection.activeObject = folder;
            EditorGUIUtility.PingObject(folder);
        }
    }

    private void ShowInExplorer()
    {
        string path = Path.Combine(Application.dataPath, NODE_ENGINE_FOLDER);
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