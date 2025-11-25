using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleEntryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image typeIcon;

    [Header("Icons")]
    [SerializeField] private Sprite logIcon;
    [SerializeField] private Sprite warningIcon;
    [SerializeField] private Sprite errorIcon;

    public void Initialize(ConsoleEntry entry)
    {
        messageText.text = entry.DisplayMessage;
        messageText.color = entry.Color;
        timeText.text = entry.TimeString;  
        countText.text = entry.count > 1 ? entry.count.ToString() : "";
        countText.gameObject.SetActive(entry.count > 1);

        
        typeIcon.sprite = entry.logType switch
        {
            LogType.Warning => warningIcon,
            LogType.Error or LogType.Exception => errorIcon,
            _ => logIcon
        };
    }
}