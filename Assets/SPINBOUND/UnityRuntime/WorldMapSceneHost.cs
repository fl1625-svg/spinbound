using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Spinbound.Presentation.UI;
using Spinbound.Presentation.WorldMap;
using Spinbound.UnityRuntime.Save;

namespace Spinbound.UnityRuntime
{
    public sealed class WorldMapSceneHost : MonoBehaviour
    {
        [SerializeField] private WorldMapController _controller;
        private readonly LocalProgressStore _store = new();
        private Canvas _settingsCanvas;
        private SettingsPanel _settingsPanel;
        private bool _settingsOpen;

        public void Configure(WorldMapController controller)
        {
            _controller = controller;
        }

        private void Awake()
        {
            _controller ??= FindFirstObjectByType<WorldMapController>();
            if (_controller == null)
            {
                Debug.LogError("SPINBOUND WorldMapSceneHost requires a WorldMapController.");
                enabled = false;
                return;
            }

            _controller.ConfigureProgress(_store.Load());
            _controller.StageRequested += LoadStage;
            BuildSettingsUi();
        }

        private void Update()
        {
            if (_settingsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseSettings();
                return;
            }

            if (Input.GetKeyDown(KeyCode.P))
                ToggleSettings();
        }

        private void OnDestroy()
        {
            if (_controller != null)
                _controller.StageRequested -= LoadStage;
        }

        public void ToggleSettings()
        {
            if (_settingsPanel == null) BuildSettingsUi();
            _settingsOpen = !_settingsOpen;
            if (_controller != null) _controller.enabled = !_settingsOpen;
            if (_settingsOpen) _settingsPanel.Show(); else _settingsPanel.Hide();
        }

        public void CloseSettings()
        {
            if (!_settingsOpen) return;
            _settingsOpen = false;
            if (_controller != null) _controller.enabled = true;
            _settingsPanel?.Hide();
        }

        private void BuildSettingsUi()
        {
            if (_settingsCanvas != null) return;
            EnsureEventSystem();

            var root = new GameObject("World Map Settings Canvas");
            root.transform.SetParent(transform, false);
            _settingsCanvas = root.AddComponent<Canvas>();
            _settingsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _settingsCanvas.sortingOrder = 120;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = .5f;
            root.AddComponent<GraphicRaycaster>();

            CreateCornerButton(root.transform, "SETTINGS  [P]", ToggleSettings);
            _settingsPanel = SettingsPanel.Build(root.transform, AccessibilitySettings.Load(), null, CloseSettings);
        }

        private static void CreateCornerButton(Transform parent, string label, Action action)
        {
            var go = new GameObject("World Map Settings Button");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-38f, -38f);
            rect.sizeDelta = new Vector2(220f, 50f);

            var image = go.AddComponent<Image>();
            image.color = new Color(.018f, .045f, .067f, .90f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            if (action != null) button.onClick.AddListener(() => action());

            var textObject = new GameObject("Label");
            textObject.transform.SetParent(go.transform, false);
            var textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 4f);
            textRect.offsetMax = new Vector2(-8f, -4f);
            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label;
            text.fontSize = 17;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("Event System");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static void LoadStage(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId)) return;
            Time.timeScale = 1f;
            SceneManager.LoadScene(stageId, LoadSceneMode.Single);
        }
    }
}
