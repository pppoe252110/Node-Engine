using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorVariableUIElement : VariableUIElement
{
    [SerializeField] private Image _colorPreview;
    [SerializeField] private TMP_InputField _hexInput;
    [SerializeField] private Button _colorPickerButton;
    [SerializeField] private GameObject _colorPickerPanel;
    [SerializeField] private Slider _redSlider;
    [SerializeField] private Slider _greenSlider;
    [SerializeField] private Slider _blueSlider;
    [SerializeField] private Slider _alphaSlider;
    [SerializeField] private Image _colorPickerPreview;

    private bool _isUpdating;
    private bool _isPickerOpen;

    public override bool CanBind(Type valueType)
    {
        return valueType == typeof(Color);
    }

    public override void Bind(IVariableNode node)
    {
        if (!CanBind(node.ValueType))
        {
            Debug.LogError($"[ColorVariableUI] Cannot bind to node of type {node.ValueType}. Expected Color.");
            return;
        }

        base.Bind(node);

        if (_hexInput != null)
        {
            _hexInput.onEndEdit.AddListener(OnHexInputChanged);
        }

        if (_colorPickerButton != null)
        {
            _colorPickerButton.onClick.AddListener(ToggleColorPicker);
        }

        SetupColorPickerSliders();

        if (_colorPickerPanel != null)
        {
            _colorPickerPanel.SetActive(false);
        }
    }

    private void SetupColorPickerSliders()
    {
        if (_redSlider != null)
        {
            _redSlider.onValueChanged.AddListener(OnSliderValueChanged);
            _redSlider.minValue = 0;
            _redSlider.maxValue = 1;
        }

        if (_greenSlider != null)
        {
            _greenSlider.onValueChanged.AddListener(OnSliderValueChanged);
            _greenSlider.minValue = 0;
            _greenSlider.maxValue = 1;
        }

        if (_blueSlider != null)
        {
            _blueSlider.onValueChanged.AddListener(OnSliderValueChanged);
            _blueSlider.minValue = 0;
            _blueSlider.maxValue = 1;
        }

        if (_alphaSlider != null)
        {
            _alphaSlider.onValueChanged.AddListener(OnSliderValueChanged);
            _alphaSlider.minValue = 0;
            _alphaSlider.maxValue = 1;
        }
    }

    protected override void OnDestroy()
    {
        if (_hexInput != null)
        {
            _hexInput.onEndEdit.RemoveListener(OnHexInputChanged);
        }

        if (_colorPickerButton != null)
        {
            _colorPickerButton.onClick.RemoveListener(ToggleColorPicker);
        }

        if (_redSlider != null) _redSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        if (_greenSlider != null) _greenSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        if (_blueSlider != null) _blueSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        if (_alphaSlider != null) _alphaSlider.onValueChanged.RemoveListener(OnSliderValueChanged);

        base.OnDestroy();
    }

    protected override void OnNodeValueChanged(object newValue)
    {
        if (_isUpdating) return;

        _isUpdating = true;

        if (newValue is Color color)
        {
            UpdateColorPreview(color);
            UpdateHexInput(color);
            UpdateColorPickerPreview(color);
        }

        _isUpdating = false;
    }

    private void UpdateColorPreview(Color color)
    {
        if (_colorPreview != null)
        {
            _colorPreview.color = color;
        }
    }

    private void UpdateHexInput(Color color)
    {
        if (_hexInput != null)
        {
            string hexColor = ColorUtility.ToHtmlStringRGBA(color);
            _hexInput.SetTextWithoutNotify("#" + hexColor);
        }
    }

    private void UpdateColorPickerPreview(Color color)
    {
        if (_colorPickerPreview != null)
        {
            _colorPickerPreview.color = color;
        }
    }

    private void OnHexInputChanged(string hex)
    {
        if (_isUpdating || TargetNode == null) return;

        if (string.IsNullOrEmpty(hex)) return;

        if (!hex.StartsWith("#"))
        {
            hex = "#" + hex;
        }

        if (ColorUtility.TryParseHtmlString(hex, out Color parsedColor))
        {
            SetColor(parsedColor);
        }
        else
        {
            // Revert to current value if hex is invalid
            OnNodeValueChanged(TargetNode.GetUntypedValue());
        }
    }

    private void ToggleColorPicker()
    {
        _isPickerOpen = !_isPickerOpen;

        if (_colorPickerPanel != null)
        {
            _colorPickerPanel.SetActive(_isPickerOpen);

            if (_isPickerOpen && TargetNode != null)
            {
                // Update sliders to current color
                var currentColor = (Color)TargetNode.GetUntypedValue();
                UpdateSlidersWithoutNotify(currentColor);
            }
        }
    }

    private void UpdateSlidersWithoutNotify(Color color)
    {
        if (_redSlider != null) _redSlider.SetValueWithoutNotify(color.r);
        if (_greenSlider != null) _greenSlider.SetValueWithoutNotify(color.g);
        if (_blueSlider != null) _blueSlider.SetValueWithoutNotify(color.b);
        if (_alphaSlider != null) _alphaSlider.SetValueWithoutNotify(color.a);
    }

    private void OnSliderValueChanged(float _)
    {
        if (_isUpdating || TargetNode == null) return;

        var newColor = new Color(
            _redSlider != null ? _redSlider.value : 0,
            _greenSlider != null ? _greenSlider.value : 0,
            _blueSlider != null ? _blueSlider.value : 0,
            _alphaSlider != null ? _alphaSlider.value : 1
        );

        SetColor(newColor);
    }

    private void SetColor(Color color)
    {
        if (TargetNode == null) return;

        _isUpdating = true;

        TargetNode.SetUntypedValue(color);
        UpdateColorPreview(color);
        UpdateHexInput(color);
        UpdateColorPickerPreview(color);

        if (_isPickerOpen)
        {
            UpdateSlidersWithoutNotify(color);
        }

        _isUpdating = false;
    }

    // This can be called from an external close button if needed
    public void CloseColorPicker()
    {
        _isPickerOpen = false;
        if (_colorPickerPanel != null)
        {
            _colorPickerPanel.SetActive(false);
        }
    }
}