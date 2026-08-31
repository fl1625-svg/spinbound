using System;
using UnityEngine;
using UnityEngine.UI;

namespace Spinbound.Presentation.UI
{
    public sealed class SettingsPanel : MonoBehaviour
    {
        private AccessibilitySettings _settings;
        private Action<AccessibilitySettings> _onChanged;
        private Text _shakeValue;
        private Text _reduceMotionValue;
        private Text _classicValue;
        private Text _colorVisionValue;

        public AccessibilitySettings Settings => _settings;

        public static SettingsPanel Build(Transform parent, AccessibilitySettings settings, Action<AccessibilitySettings> onChanged = null, Action onClose = null)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            var backdrop = new GameObject("Settings Backdrop");
            backdrop.transform.SetParent(parent, false);
            var backdropRect = backdrop.AddComponent<RectTransform>();
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;
            var backdropImage = backdrop.AddComponent<Image>();
            backdropImage.color = new Color(.006f, .016f, .028f, .80f);

            var panelObject = new GameObject("Settings Panel");
            panelObject.transform.SetParent(backdrop.transform, false);
            var panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(.5f, .5f);
            panelRect.pivot = new Vector2(.5f, .5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(980f, 860f);
            var panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(.020f, .050f, .073f, .99f);
            var outline = panelObject.AddComponent<Outline>();
            outline.effectColor = new Color(.42f, .80f, 1f, .48f);
            outline.effectDistance = new Vector2(2f, -2f);

            var panel = backdrop.AddComponent<SettingsPanel>();
            panel._settings = (settings ?? AccessibilitySettings.Default()).Sanitized();
            panel._onChanged = onChanged;

            CreateText(panelRect, "SETTINGS & ACCESSIBILITY", new Vector2(0f, -28f), new Vector2(850f, 50f), 31, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            CreateText(panelRect, "Presentation-only comfort options — gameplay physics stay unchanged", new Vector2(0f, -76f), new Vector2(850f, 34f), 15, FontStyle.Normal, new Color(.61f, .80f, .94f), TextAnchor.MiddleCenter);

            float leftX = -225f;
            float rightX = 225f;
            float y = -138f;
            panel._shakeValue = CreateCycleRow(panelRect, "CAMERA SHAKE", leftX, y, panel.CycleShake);
            panel._reduceMotionValue = CreateCycleRow(panelRect, "REDUCE MOTION", rightX, y, panel.ToggleReduceMotion);
            y -= 82f;
            panel._classicValue = CreateCycleRow(panelRect, "CLASSIC VIEW", leftX, y, panel.ToggleClassicView);
            panel._colorVisionValue = CreateCycleRow(panelRect, "COLOR VISION", rightX, y, panel.CycleColorVision);

            y -= 104f;
            CreateSliderRow(panelRect, "CAMERA SENSITIVITY", leftX, y, .35f, 2.5f, panel._settings.CameraSensitivity, v => { panel._settings.CameraSensitivity = v; panel.Commit(); });
            CreateSliderRow(panelRect, "VFX INTENSITY", rightX, y, 0f, 1f, panel._settings.VfxIntensity, v => { panel._settings.VfxIntensity = v; panel.Commit(); });
            y -= 102f;
            CreateSliderRow(panelRect, "MUSIC", leftX, y, 0f, 1f, panel._settings.MusicVolume, v => { panel._settings.MusicVolume = v; panel.Commit(); });
            CreateSliderRow(panelRect, "SFX", rightX, y, 0f, 1f, panel._settings.SfxVolume, v => { panel._settings.SfxVolume = v; panel.Commit(); });
            y -= 102f;
            CreateSliderRow(panelRect, "AMBIENCE", leftX, y, 0f, 1f, panel._settings.AmbienceVolume, v => { panel._settings.AmbienceVolume = v; panel.Commit(); });
            CreateSliderRow(panelRect, "UI", rightX, y, 0f, 1f, panel._settings.UiVolume, v => { panel._settings.UiVolume = v; panel.Commit(); });
            y -= 102f;
            CreateSliderRow(panelRect, "TOUCH CONTROL SIZE", 0f, y, .75f, 1.5f, panel._settings.TouchSize, v => { panel._settings.TouchSize = v; panel.Commit(); }, 470f);

            CreateButton(panelRect, "Close Settings", "DONE", new Vector2(0f, -766f), new Vector2(300f, 56f), () =>
            {
                panel.Hide();
                if (onClose != null) onClose();
            });

            panel.RefreshLabels();
            backdrop.SetActive(false);
            return panel;
        }

        public void Show()
        {
            RefreshLabels();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (gameObject.activeSelf) Hide(); else Show();
        }

        private void CycleShake()
        {
            _settings.CameraShake = _settings.CameraShake switch
            {
                CameraShakeLevel.Off => CameraShakeLevel.Low,
                CameraShakeLevel.Low => CameraShakeLevel.Full,
                _ => CameraShakeLevel.Off,
            };
            Commit();
        }

        private void ToggleReduceMotion()
        {
            _settings.ReduceMotion = !_settings.ReduceMotion;
            Commit();
        }

        private void ToggleClassicView()
        {
            _settings.ClassicView = !_settings.ClassicView;
            Commit();
        }

        private void CycleColorVision()
        {
            _settings.ColorVision = _settings.ColorVision switch
            {
                ColorVisionMode.Standard => ColorVisionMode.Deuteranopia,
                ColorVisionMode.Deuteranopia => ColorVisionMode.Protanopia,
                ColorVisionMode.Protanopia => ColorVisionMode.Tritanopia,
                _ => ColorVisionMode.Standard,
            };
            Commit();
        }

        private void Commit()
        {
            _settings = _settings.Sanitized();
            _settings.Save();
            RefreshLabels();
            _onChanged?.Invoke(_settings);
        }

        private void RefreshLabels()
        {
            if (_settings == null) return;
            if (_shakeValue != null) _shakeValue.text = _settings.CameraShake.ToString().ToUpperInvariant();
            if (_reduceMotionValue != null) _reduceMotionValue.text = _settings.ReduceMotion ? "ON" : "OFF";
            if (_classicValue != null) _classicValue.text = _settings.ClassicView ? "ON" : "OFF";
            if (_colorVisionValue != null) _colorVisionValue.text = FormatEnum(_settings.ColorVision.ToString());
        }

        private static string FormatEnum(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.ToUpperInvariant();
        }

        private static Text CreateCycleRow(RectTransform parent, string caption, float x, float y, Action action)
        {
            CreateText(parent, caption, new Vector2(x, y), new Vector2(390f, 26f), 14, FontStyle.Bold, new Color(.62f, .81f, .95f), TextAnchor.MiddleCenter);
            Button button = CreateButton(parent, caption + " Button", string.Empty, new Vector2(x, y - 34f), new Vector2(390f, 44f), action);
            Text value = button.GetComponentInChildren<Text>();
            value.fontSize = 18;
            return value;
        }

        private static void CreateSliderRow(RectTransform parent, string caption, float x, float y, float min, float max, float value, Action<float> changed, float width = 390f)
        {
            CreateText(parent, caption, new Vector2(x, y), new Vector2(width, 26f), 14, FontStyle.Bold, new Color(.62f, .81f, .95f), TextAnchor.MiddleCenter);

            var sliderObject = new GameObject(caption + " Slider");
            sliderObject.transform.SetParent(parent, false);
            var rect = sliderObject.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
            rect.pivot = new Vector2(.5f, 1f);
            rect.anchoredPosition = new Vector2(x, y - 42f);
            rect.sizeDelta = new Vector2(width, 34f);

            var backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(sliderObject.transform, false);
            var backgroundRect = backgroundObject.AddComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, .5f);
            backgroundRect.anchorMax = new Vector2(1f, .5f);
            backgroundRect.sizeDelta = new Vector2(-20f, 8f);
            var backgroundImage = backgroundObject.AddComponent<Image>();
            backgroundImage.color = new Color(.12f, .22f, .28f, 1f);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObject.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, .5f);
            fillAreaRect.anchorMax = new Vector2(1f, .5f);
            fillAreaRect.offsetMin = new Vector2(10f, -4f);
            fillAreaRect.offsetMax = new Vector2(-10f, 4f);

            var fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(fillArea.transform, false);
            var fillRect = fillObject.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
            var fillImage = fillObject.AddComponent<Image>();
            fillImage.color = new Color(.38f, .77f, 1f, 1f);

            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObject.transform, false);
            var handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10f, 0f);
            handleAreaRect.offsetMax = new Vector2(-10f, 0f);

            var handleObject = new GameObject("Handle");
            handleObject.transform.SetParent(handleArea.transform, false);
            var handleRect = handleObject.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(22f, 22f);
            var handleImage = handleObject.AddComponent<Image>();
            handleImage.color = Color.white;

            var slider = sliderObject.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = Mathf.Clamp(value, min, max);
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.onValueChanged.AddListener(v => changed?.Invoke(v));
        }

        private static Button CreateButton(RectTransform parent, string name, string label, Vector2 position, Vector2 size, Action action)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
            rect.pivot = new Vector2(.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.AddComponent<Image>();
            image.color = new Color(.07f, .18f, .24f, .98f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            if (action != null) button.onClick.AddListener(() => action());

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(go.transform, false);
            var labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 4f);
            labelRect.offsetMax = new Vector2(-8f, -4f);
            var text = labelObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label;
            text.fontSize = 18;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return button;
        }

        private static Text CreateText(RectTransform parent, string value, Vector2 position, Vector2 size, int fontSize, FontStyle style, Color color, TextAnchor alignment)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
            rect.pivot = new Vector2(.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }
    }
}
