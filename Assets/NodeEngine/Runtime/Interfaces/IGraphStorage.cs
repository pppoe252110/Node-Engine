using System.Collections.Generic;

public interface IGraphStorage
{
    void Save(string saveName, string data);
    string Load(string saveName);
    bool Exists(string saveName);
    void Delete(string saveName);
    void DeleteAll();
    List<string> GetAllSaveNames();
}
