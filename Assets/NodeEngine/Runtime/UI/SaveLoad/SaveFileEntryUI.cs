using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SaveFileEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _fileNameText;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _deleteButton;

    private string _saveName;
    private GraphSaveLoadCoordinator _coordinator;

    [Inject]
    public void Construct(GraphSaveLoadCoordinator coordinator)
    {
        _coordinator = coordinator;
    }

    public void Initialize(string saveName, System.Action<string> onLoad, System.Action<string> onDelete)
    {
        _saveName = saveName;

        _fileNameText.text = saveName;

        _loadButton.onClick.AddListener(() => onLoad?.Invoke(_saveName));
        _deleteButton.onClick.AddListener(() => onDelete?.Invoke(_saveName));
    }
}
