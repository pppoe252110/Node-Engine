using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubgraphListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Button _editButton;
    [SerializeField] private Button _deleteButton;

    private SubgraphDefinition _definition;
    private System.Action<SubgraphDefinition> _onEdit, _onDelete, _onInstantiate;

    public void Initialize(SubgraphDefinition def,
        System.Action<SubgraphDefinition> onEdit,
        System.Action<SubgraphDefinition> onDelete,
        System.Action<SubgraphDefinition> onInstantiate)
    {
        _definition = def;
        _onEdit = onEdit;
        _onDelete = onDelete;
        _onInstantiate = onInstantiate;

        _nameText.text = def.subgraphName;

        _editButton.onClick.AddListener(() => _onEdit?.Invoke(_definition));
        _deleteButton.onClick.AddListener(() => _onDelete?.Invoke(_definition));
    }
}