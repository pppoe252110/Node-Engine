using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LocalGraphStorage : IGraphStorage
{
    private readonly string _saveDirectory;
    private readonly string _extension;

    public LocalGraphStorage(string customDirectory = null, string extension = ".json")
    {
        _saveDirectory = customDirectory ?? Application.persistentDataPath + "/NodeGraphs/";
        _extension = extension;

        if (!Directory.Exists(_saveDirectory))
            Directory.CreateDirectory(_saveDirectory);
    }

    public void Save(string saveName, string data) => File.WriteAllText(GetFilePath(saveName), data);
    public string Load(string saveName)
    {
        Debug.Log("Loading graph from: " + GetFilePath(saveName));
        return File.ReadAllText(GetFilePath(saveName));

    }
    public bool Exists(string saveName) => File.Exists(GetFilePath(saveName));

    public void Delete(string saveName)
    {
        if (Exists(saveName)) File.Delete(GetFilePath(saveName));
    }

    public void DeleteAll()
    {
        foreach (string file in Directory.GetFiles(_saveDirectory, "*" + _extension))
            File.Delete(file);
    }

    public List<string> GetAllSaveNames()
    {
        if (!Directory.Exists(_saveDirectory)) return new List<string>();
        return Directory.GetFiles(_saveDirectory, "*" + _extension)
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(f => f).ToList();
    }

    private string GetFilePath(string saveName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            saveName = saveName.Replace(c, '_');
        return Path.Combine(_saveDirectory, saveName + _extension);
    }
}