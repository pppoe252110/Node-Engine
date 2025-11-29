using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class SaveFileEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _fileNameText;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _deleteButton;

    private string _saveName;

    public void Initialize(string saveName, System.Action<string> onLoad, System.Action<string> onDelete)
    {
        _saveName = saveName;

        _fileNameText.text = saveName;
        UpdateFileInfo();

        _loadButton.onClick.AddListener(() => onLoad?.Invoke(_saveName));
        _deleteButton.onClick.AddListener(() => onDelete?.Invoke(_saveName));
    }

    private void UpdateFileInfo()
    {
        string filePath = Path.Combine(Application.dataPath, $"{_saveName}.json");

        if (!File.Exists(filePath))
        {
            _loadButton.interactable = false;
        }
    }
}
