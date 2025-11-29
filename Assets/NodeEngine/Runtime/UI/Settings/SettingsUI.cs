using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : BasePanel
{
    [SerializeField] private CanvasScaler[] _canvasScalers;
    [SerializeField] private SliderEndEdit _uiScaleSlider;
    [SerializeField] private float referenceUIScale = 1080f;

    private void Start()
    {
        PanelManager.Instance.RegisterPanel(this);
    }

    private void OnEnable()
    {
        _uiScaleSlider.OnSliderEditEnd += SetUIScale;
    }

    private void OnDisable()
    {
        _uiScaleSlider.OnSliderEditEnd -= SetUIScale;
    }

    public void SetUIScale(float scale)
    {
        foreach (var canvasScaler in _canvasScalers)
        {
            canvasScaler.referenceResolution = new Vector2(canvasScaler.referenceResolution.x, referenceUIScale * scale);
        }
    }
}