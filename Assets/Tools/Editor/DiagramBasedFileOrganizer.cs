using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class DiagramBasedFileOrganizer : EditorWindow
{
    private string _diagramInput = "";
    private string _rootPath = "Assets/NodeEngine";
    private bool _dryRun = true;
    private Vector2 _scrollPosition;
    private List<OrganizationResult> _results = new List<OrganizationResult>();
    private Dictionary<string, List<string>> _fileMappings = new Dictionary<string, List<string>>();
    private bool _scanIncludeEmptyFolders = true;
    private bool _scanIncludeFileDetails = false;

    [MenuItem("Tools/NodeEngine/Organize from Diagram")]
    public static void ShowWindow()
    {
        GetWindow<DiagramBasedFileOrganizer>("Diagram Organizer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Folder Diagram File Organizer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Paste your folder diagram with file names (e.g., '├── FileName.cs')\n" +
            "The script will find these files anywhere in the project and move them to the specified folders.",
            MessageType.Info);

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Root Path:", EditorStyles.boldLabel);
        _rootPath = EditorGUILayout.TextField(_rootPath);

        EditorGUILayout.Space(10);

        _dryRun = EditorGUILayout.Toggle("Dry Run (Preview Only)", _dryRun);

        EditorGUILayout.Space(10);

        // Scan Project Section
        EditorGUILayout.LabelField("Current Project Structure:", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("📂 Scan Project", GUILayout.Height(30)))
        {
            ScanProjectToDiagram();
        }

        if (GUILayout.Button("📋 Copy to Clipboard", GUILayout.Height(30)))
        {
            EditorGUIUtility.systemCopyBuffer = _diagramInput;
            _results.Add(new OrganizationResult("✓ Diagram copied to clipboard!"));
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        _scanIncludeEmptyFolders = EditorGUILayout.Toggle("Include Empty Folders", _scanIncludeEmptyFolders);
        _scanIncludeFileDetails = EditorGUILayout.Toggle("Include Comments", _scanIncludeFileDetails);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Diagram Editor:", EditorStyles.boldLabel);
        _diagramInput = EditorGUILayout.TextArea(_diagramInput, GUILayout.Height(300));

        EditorGUILayout.Space(10);

        // Action Buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("1. Parse Diagram", GUILayout.Height(40)))
        {
            ParseDiagramAndFindFiles();
        }

        if (GUILayout.Button("2. Create Folders", GUILayout.Height(40)))
        {
            CreateAllFolders();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        if (_fileMappings != null && _fileMappings.Count > 0)
        {
            int totalFiles = _fileMappings.Sum(kvp => kvp.Value.Count);
            string buttonText = _dryRun ? "3. Preview File Moves" : "3. Execute File Moves";

            if (GUILayout.Button($"{buttonText} ({totalFiles} files)", GUILayout.Height(40)))
            {
                ExecuteFileMoves();
            }
        }

        // Results Section
        if (_results.Count > 0)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Results:", EditorStyles.boldLabel);

            if (GUILayout.Button("Clear Results", GUILayout.Width(100)))
            {
                _results.Clear();
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));

            foreach (var result in _results)
            {
                Color oldColor = GUI.color;
                GUI.color = result.IsError ? Color.red : (result.IsWarning ? Color.yellow : Color.white);
                EditorGUILayout.LabelField(result.Message, EditorStyles.wordWrappedLabel);
                GUI.color = oldColor;
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void ScanProjectToDiagram()
    {
        _results.Clear();

        if (!Directory.Exists(_rootPath))
        {
            _results.Add(new OrganizationResult($"Root path not found: {_rootPath}", true));
            return;
        }

        EditorUtility.DisplayProgressBar("Scanning", "Analyzing project structure...", 0);

        try
        {
            var diagram = GenerateDiagram(_rootPath);
            _diagramInput = diagram;

            _results.Add(new OrganizationResult("✓ Project scanned successfully!"));
            _results.Add(new OrganizationResult($"Generated diagram with {diagram.Split('\n').Length} lines"));

            // Count files
            var allFiles = Directory.GetFiles(_rootPath, "*.*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".meta"))
                .ToList();

            _results.Add(new OrganizationResult($"Found {allFiles.Count} files in project"));

            // Count folders
            var allFolders = Directory.GetDirectories(_rootPath, "*", SearchOption.AllDirectories);
            _results.Add(new OrganizationResult($"Found {allFolders.Length} folders in project"));
        }
        catch (System.Exception ex)
        {
            _results.Add(new OrganizationResult($"Error scanning project: {ex.Message}", true));
            Debug.LogError($"Scan error: {ex}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private string GenerateDiagram(string rootPath)
    {
        var sb = new StringBuilder();
        string rootName = Path.GetFileName(rootPath);

        sb.AppendLine($"{rootName}/");

        var directoryInfo = new DirectoryInfo(rootPath);
        GenerateDirectoryDiagram(directoryInfo, sb, "", true);

        return sb.ToString();
    }

    private void GenerateDirectoryDiagram(DirectoryInfo directory, StringBuilder sb, string indent, bool isLast = false)
    {
        var directories = directory.GetDirectories()
            .Where(d => !d.Name.StartsWith(".") && d.Name != "obj" && d.Name != "bin")
            .OrderBy(d => d.Name)
            .ToList();

        var files = directory.GetFiles()
            .Where(f => !f.Name.EndsWith(".meta") && !f.Name.StartsWith("."))
            .OrderBy(f => f.Name)
            .ToList();

        // If no contents and not including empty folders, skip
        if (!_scanIncludeEmptyFolders && directories.Count == 0 && files.Count == 0)
            return;

        for (int i = 0; i < directories.Count; i++)
        {
            var dir = directories[i];
            bool isLastDir = (i == directories.Count - 1) && files.Count == 0;

            string prefix = isLastDir ? "└── " : "├── ";
            string childIndent = isLastDir ? "    " : "│   ";

            sb.AppendLine($"{indent}{prefix}{dir.Name}/");

            GenerateDirectoryDiagram(dir, sb, indent + childIndent, isLastDir);
        }

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];
            bool isLastFile = (i == files.Count - 1);

            string prefix = isLastFile ? "└── " : "├── ";

            sb.Append($"{indent}{prefix}{file.Name}");

            if (_scanIncludeFileDetails)
            {
                // Add file size info
                long fileSizeKB = file.Length / 1024;
                if (fileSizeKB > 0)
                {
                    sb.Append($"  # {fileSizeKB} KB");
                }
            }

            sb.AppendLine();
        }
    }

    private string GenerateDetailedDiagram(string rootPath)
    {
        var sb = new StringBuilder();

        // Get all directories relative to root
        var allDirs = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories)
            .Select(d => d.Replace("\\", "/"))
            .OrderBy(d => d)
            .ToList();

        var allFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".meta"))
            .Select(f => f.Replace("\\", "/"))
            .OrderBy(f => f)
            .ToList();

        // Build tree structure
        var tree = new Dictionary<string, List<string>>();

        foreach (var dir in allDirs)
        {
            string relativePath = dir.Substring(rootPath.Length + 1);
            string parent = Path.GetDirectoryName(relativePath)?.Replace("\\", "/") ?? "";

            if (!tree.ContainsKey(parent))
                tree[parent] = new List<string>();

            tree[parent].Add(Path.GetFileName(relativePath) + "/");
        }

        foreach (var file in allFiles)
        {
            string relativePath = file.Substring(rootPath.Length + 1);
            string parent = Path.GetDirectoryName(relativePath)?.Replace("\\", "/") ?? "";

            if (!tree.ContainsKey(parent))
                tree[parent] = new List<string>();

            tree[parent].Add(Path.GetFileName(relativePath));
        }

        // Generate diagram
        sb.AppendLine($"{Path.GetFileName(rootPath)}/");
        GenerateTreeDiagram(tree, "", sb, "");

        return sb.ToString();
    }

    private void GenerateTreeDiagram(Dictionary<string, List<string>> tree, string currentPath, StringBuilder sb, string indent)
    {
        if (!tree.ContainsKey(currentPath))
            return;

        var items = tree[currentPath].OrderBy(i => i).ToList();

        for (int i = 0; i < items.Count; i++)
        {
            string item = items[i];
            bool isLast = (i == items.Count - 1);

            string prefix = isLast ? "└── " : "├── ";
            string childIndent = indent + (isLast ? "    " : "│   ");

            sb.AppendLine($"{indent}{prefix}{item}");

            string childPath = string.IsNullOrEmpty(currentPath) ? item.TrimEnd('/') : $"{currentPath}/{item.TrimEnd('/')}";

            if (item.EndsWith("/"))
            {
                GenerateTreeDiagram(tree, childPath, sb, childIndent);
            }
        }
    }

    private void ParseDiagramAndFindFiles()
    {
        _results.Clear();
        _fileMappings.Clear();

        if (string.IsNullOrWhiteSpace(_diagramInput))
        {
            _results.Add(new OrganizationResult("Please paste a diagram first!", true));
            return;
        }

        if (!Directory.Exists(_rootPath))
        {
            _results.Add(new OrganizationResult($"Root path not found: {_rootPath}", true));
            return;
        }

        EditorUtility.DisplayProgressBar("Parsing", "Analyzing diagram...", 0);

        try
        {
            // Parse the diagram
            var fileTargets = ParseDiagram(_diagramInput);

            _results.Add(new OrganizationResult($"Parsed {fileTargets.Count} file targets from diagram"));

            // Find all files in the project (not just .cs)
            var allFiles = Directory.GetFiles(_rootPath, "*.*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".meta"))
                .Select(f => f.Replace("\\", "/"))
                .ToList();

            _results.Add(new OrganizationResult($"Found {allFiles.Count} files in project"));

            EditorUtility.DisplayProgressBar("Matching", "Finding file matches...", 0.5f);

            // Match files to targets
            foreach (var target in fileTargets)
            {
                string targetFolder = target.Key;
                List<string> expectedFiles = target.Value;

                foreach (var expectedFile in expectedFiles)
                {
                    // Find the actual file
                    var matches = allFiles.Where(f => Path.GetFileName(f) == expectedFile).ToList();

                    if (matches.Count == 1)
                    {
                        if (!_fileMappings.ContainsKey(targetFolder))
                            _fileMappings[targetFolder] = new List<string>();

                        _fileMappings[targetFolder].Add(matches[0]);
                        _results.Add(new OrganizationResult($"✓ Found: {expectedFile} → {targetFolder}"));
                    }
                    else if (matches.Count > 1)
                    {
                        _results.Add(new OrganizationResult($"⚠ Multiple matches for {expectedFile}, using first", true, true));
                        if (!_fileMappings.ContainsKey(targetFolder))
                            _fileMappings[targetFolder] = new List<string>();

                        _fileMappings[targetFolder].Add(matches[0]);
                    }
                    else
                    {
                        _results.Add(new OrganizationResult($"✗ Not found: {expectedFile}", true));
                    }
                }
            }

            EditorUtility.ClearProgressBar();

            int totalMapped = _fileMappings.Sum(kvp => kvp.Value.Count);
            _results.Add(new OrganizationResult($"\n✓ Successfully mapped {totalMapped} files to {_fileMappings.Count} folders"));
        }
        catch (System.Exception ex)
        {
            EditorUtility.ClearProgressBar();
            _results.Add(new OrganizationResult($"Error: {ex.Message}", true));
            Debug.LogError($"Parse error: {ex}");
        }

        AssetDatabase.Refresh();
    }

    private Dictionary<string, List<string>> ParseDiagram(string diagram)
    {
        var fileTargets = new Dictionary<string, List<string>>();
        var lines = diagram.Split('\n');

        Stack<string> folderStack = new Stack<string>();
        int lastDepth = -1;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Calculate depth by counting tree characters
            int depth = 0;
            foreach (char c in line)
            {
                if (c == ' ' || c == '│' || c == '├' || c == '└' || c == '─')
                    depth++;
                else
                    break;
            }
            depth = depth / 4; // Approximate depth level

            // Clean the line
            string cleanLine = line.Replace("├──", "")
                                  .Replace("└──", "")
                                  .Replace("│", "")
                                  .Replace("─", "")
                                  .Trim();

            // Skip comments and empty
            if (cleanLine.StartsWith("#") || string.IsNullOrEmpty(cleanLine))
                continue;

            // Adjust stack based on depth
            if (depth <= lastDepth)
            {
                int popCount = lastDepth - depth + 1;
                for (int i = 0; i < popCount && folderStack.Count > 0; i++)
                    folderStack.Pop();
            }

            // Check if it's a file (has extension) or folder
            if (cleanLine.Contains("."))
            {
                // It's a file
                string fileName = cleanLine;

                // Clean up any trailing comments
                if (fileName.Contains("#"))
                    fileName = fileName.Split('#')[0].Trim();

                if (!string.IsNullOrEmpty(fileName))
                {
                    // Build the folder path from stack
                    string folderPath = string.Join("/", folderStack.Reverse());

                    if (!fileTargets.ContainsKey(folderPath))
                        fileTargets[folderPath] = new List<string>();

                    fileTargets[folderPath].Add(fileName);
                }
            }
            else
            {
                // It's a folder
                string folderName = cleanLine;

                // Clean up comments
                if (folderName.Contains("#"))
                    folderName = folderName.Split('#')[0].Trim();

                if (!string.IsNullOrEmpty(folderName) && !folderName.EndsWith("/"))
                {
                    folderStack.Push(folderName);
                    lastDepth = depth;
                }
            }
        }

        return fileTargets;
    }

    private void CreateAllFolders()
    {
        _results.Clear();

        if (_fileMappings == null || _fileMappings.Count == 0)
        {
            ParseDiagramAndFindFiles();
        }

        if (_fileMappings.Count == 0)
        {
            _results.Add(new OrganizationResult("No folders to create. Parse diagram first.", true));
            return;
        }

        foreach (var folder in _fileMappings.Keys)
        {
            string fullPath = $"{_rootPath}/{folder}";

            if (!Directory.Exists(fullPath))
            {
                if (!_dryRun)
                {
                    Directory.CreateDirectory(fullPath);
                }
                _results.Add(new OrganizationResult($"Created: {fullPath}"));
            }
            else
            {
                _results.Add(new OrganizationResult($"Exists: {fullPath}"));
            }
        }

        AssetDatabase.Refresh();

        if (!_dryRun)
        {
            _results.Add(new OrganizationResult($"\n✓ Created all folders!"));
        }
    }

    private void ExecuteFileMoves()
    {
        _results.Clear();

        if (_fileMappings == null || _fileMappings.Count == 0)
        {
            ParseDiagramAndFindFiles();
        }

        if (_fileMappings.Count == 0)
        {
            _results.Add(new OrganizationResult("No files to move. Parse diagram first.", true));
            return;
        }

        int totalFiles = _fileMappings.Sum(kvp => kvp.Value.Count);
        int processed = 0;
        int moved = 0;

        foreach (var mapping in _fileMappings)
        {
            string targetFolder = $"{_rootPath}/{mapping.Key}";

            // Ensure folder exists
            if (!Directory.Exists(targetFolder))
            {
                if (!_dryRun)
                {
                    Directory.CreateDirectory(targetFolder);
                }
                _results.Add(new OrganizationResult($"Created folder: {targetFolder}"));
            }

            foreach (var sourceFile in mapping.Value)
            {
                processed++;

                EditorUtility.DisplayProgressBar("Moving Files",
                    $"Moving {Path.GetFileName(sourceFile)} ({processed}/{totalFiles})",
                    (float)processed / totalFiles);

                string fileName = Path.GetFileName(sourceFile);
                string targetPath = $"{targetFolder}/{fileName}";

                // Skip if already in correct location
                if (sourceFile == targetPath)
                {
                    _results.Add(new OrganizationResult($"✓ Already in place: {fileName}"));
                    continue;
                }

                // Handle duplicate names
                if (File.Exists(targetPath))
                {
                    string newName = $"{Path.GetFileNameWithoutExtension(fileName)}_Duplicate{Path.GetExtension(fileName)}";
                    targetPath = $"{targetFolder}/{newName}";
                    _results.Add(new OrganizationResult($"⚠ Duplicate detected: {fileName} → {newName}", true, true));
                }

                if (!_dryRun)
                {
                    try
                    {
                        // Move the file and its meta file
                        string sourceMeta = sourceFile + ".meta";
                        string targetMeta = targetPath + ".meta";

                        File.Move(sourceFile, targetPath);
                        if (File.Exists(sourceMeta))
                        {
                            File.Move(sourceMeta, targetMeta);
                        }

                        moved++;
                        _results.Add(new OrganizationResult($"✓ Moved: {GetRelativePath(sourceFile)} → {mapping.Key}/{fileName}"));
                    }
                    catch (System.Exception ex)
                    {
                        _results.Add(new OrganizationResult($"✗ Failed to move {fileName}: {ex.Message}", true));
                    }
                }
                else
                {
                    moved++;
                    _results.Add(new OrganizationResult($"[PREVIEW] Move: {GetRelativePath(sourceFile)} → {mapping.Key}/{fileName}"));
                }
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

        string action = _dryRun ? "Would move" : "Successfully moved";
        _results.Add(new OrganizationResult($"\n✓ {action} {moved} files"));

        if (!_dryRun && moved > 0)
        {
            EditorUtility.DisplayDialog("Complete", $"Successfully moved {moved} files!", "OK");
        }
    }

    private string GetRelativePath(string fullPath)
    {
        if (fullPath.StartsWith(_rootPath))
            return fullPath.Substring(_rootPath.Length + 1);
        return fullPath;
    }

    private class OrganizationResult
    {
        public string Message;
        public bool IsError;
        public bool IsWarning;

        public OrganizationResult(string message, bool isError = false, bool isWarning = false)
        {
            Message = message;
            IsError = isError;
            IsWarning = isWarning;
        }
    }
}