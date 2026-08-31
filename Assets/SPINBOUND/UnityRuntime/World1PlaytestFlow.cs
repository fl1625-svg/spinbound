using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Spinbound.Core.Gameplay;
using Spinbound.Core.Simulation;
using Spinbound.Meta;
using Spinbound.Presentation.UI;
using Spinbound.UnityRuntime.Save;
using Spinbound.Worlds;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.UnityRuntime
{
    /// <summary>
    /// Stage-side results/retry/map/settings flow. Authoritative simulation stays in UnityRotorGameHost.
    /// </summary>
    public sealed class World1PlaytestFlow : MonoBehaviour
    {
        public const string WorldMapSceneName = "WorldMap_W01";

        [SerializeField] private string _stageId = W01_01_FirstSpin.Id;
        private readonly LocalProgressStore _store = new();
        private PlayerProgress _progress;
        private AccessibilitySettings _settings;
        private Canvas _canvas;
        private ResultsPanel _resultsPanel;
        private SettingsPanel _settingsPanel;
        private string _nextStageId;
        private bool _completed;
        private bool _settingsOpen;

        public event Action<bool> ModalPauseChanged;
        public event Action<AccessibilitySettings> SettingsChanged;
        public AccessibilitySettings Settings => _settings;

        public static World1PlaytestFlow Build(StageDefinition stage)
        {
            if (stage == null) throw new ArgumentNullException(nameof(stage));
            var root = new GameObject("World 1 Stage Flow");
            var flow = root.AddComponent<World1PlaytestFlow>();
            flow.ConfigureStage(stage);
            return flow;
        }

        public void ConfigureStage(StageDefinition stage)
        {
            if (stage == null) throw new ArgumentNullException(nameof(stage));
            _stageId = stage.Id;
        }

        private void Awake()
        {
            _progress = _store.Load();
            _settings = AccessibilitySettings.Load();
            EnsureUi();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
                ToggleSettings();

            if (!_settingsOpen && Input.GetKeyDown(KeyCode.M))
                LoadWorldMap();
        }

        private void OnDisable()
        {
            if (_settingsOpen)
            {
                _settingsOpen = false;
                ModalPauseChanged?.Invoke(false);
            }
        }

        public void CompleteStage(StageDefinition stage, float elapsedSeconds, int hits, RotorMode mode)
        {
            if (stage == null) throw new ArgumentNullException(nameof(stage));
            if (_completed) return;

            _completed = true;
            _stageId = stage.Id;
            _progress ??= _store.Load();

            var result = new RunResult(
                stage.Id,
                rawTimeSeconds: Mathf.Max(0f, elapsedSeconds),
                penaltySeconds: 0f,
                damageCount: Mathf.Max(0, hits),
                orbitCoreIds: Array.Empty<string>(),
                mode: mode,
                practice: false,
                completed: true);

            StageProgressRecord record = _progress.Merge(result, Array.Empty<string>(), stage.MasterTimeSeconds);
            _store.Save(_progress);

            _nextStageId = ProgressionRules.GetMainNextStageId(stage.Id);
            StageDefinition nextStage = null;
            if (!string.IsNullOrEmpty(_nextStageId) && ProgressionRules.IsUnlocked(_nextStageId, _progress))
                nextStage = W01ReferenceRoutes.Get(_nextStageId).Stage;

            EnsureUi();
            _resultsPanel.Show(stage, result, record, nextStage);
        }

        public void LoadNextStage()
        {
            if (!string.IsNullOrEmpty(_nextStageId) && ProgressionRules.IsUnlocked(_nextStageId, _progress))
            {
                LoadStage(_nextStageId);
                return;
            }

            LoadWorldMap();
        }

        public void RetryStage() => LoadStage(_stageId);

        public void LoadWorldMap()
        {
            CloseSettings();
            Time.timeScale = 1f;
            SceneManager.LoadScene(WorldMapSceneName, LoadSceneMode.Single);
        }

        public void ToggleSettings()
        {
            EnsureUi();
            _settingsOpen = !_settingsOpen;
            if (_settingsOpen) _settingsPanel.Show();
            else _settingsPanel.Hide();
            ModalPauseChanged?.Invoke(_settingsOpen);
        }

        public void CloseSettings()
        {
            if (!_settingsOpen) return;
            _settingsOpen = false;
            _settingsPanel?.Hide();
            ModalPauseChanged?.Invoke(false);
        }

        private void OnSettingsChanged(AccessibilitySettings value)
        {
            _settings = value ?? AccessibilitySettings.Default();
            SettingsChanged?.Invoke(_settings);
        }

        private static void LoadStage(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId)) return;
            Time.timeScale = 1f;
            SceneManager.LoadScene(stageId, LoadSceneMode.Single);
        }

        private void EnsureUi()
        {
            if (_canvas != null) return;
            EnsureEventSystem();

            var canvasObject = new GameObject("World 1 Stage Flow Canvas");
            canvasObject.transform.SetParent(transform, false);
            _canvas = canvasObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 120;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = .5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            CreateCornerButton(
                canvasObject.transform,
                "World Map Button",
                "WORLD MAP  [M]",
                new Vector2(-40f, -112f),
                new Vector2(220f, 48f),
                LoadWorldMap);

            CreateCornerButton(
                canvasObject.transform,
                "Settings Button",
                "SETTINGS  [P]",
                new Vector2(-40f, -168f),
                new Vector2(220f, 48f),
                ToggleSettings);

            _resultsPanel = ResultsPanel.Build(canvasObject.transform, RetryStage, LoadNextStage, LoadWorldMap);
            _settingsPanel = SettingsPanel.Build(canvasObject.transform, _settings, OnSettingsChanged, CloseSettings);
        }

        private static Button CreateCornerButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Action action)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = go.AddComponent<Image>();
            image.color = new Color(.03f, .07f, .10f, .84f);
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
            text.fontSize = 17;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return button;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("Event System");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }
    }
}
