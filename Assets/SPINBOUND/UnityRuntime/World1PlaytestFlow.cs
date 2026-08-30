using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Spinbound.Core.Gameplay;
using Spinbound.Core.Simulation;
using Spinbound.Meta;
using Spinbound.UnityRuntime.Save;
using Spinbound.Worlds;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.UnityRuntime
{
    /// <summary>
    /// Stage-side results/retry/map flow. World selection itself now lives in the playable 3D diorama map.
    /// </summary>
    public sealed class World1PlaytestFlow : MonoBehaviour
    {
        public const string WorldMapSceneName = "WorldMap_W01";

        [SerializeField] private string _stageId = W01_01_FirstSpin.Id;
        private readonly LocalProgressStore _store = new();
        private PlayerProgress _progress;
        private Canvas _canvas;
        private GameObject _resultsPanel;
        private Text _resultsTitle;
        private Text _resultsBody;
        private Text _nextButtonText;
        private string _nextStageId;
        private bool _completed;

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
            EnsureUi();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
                LoadWorldMap();
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
            bool hasNext = !string.IsNullOrEmpty(_nextStageId) && ProgressionRules.IsUnlocked(_nextStageId, _progress);

            EnsureUi();
            _resultsTitle.text = stage.Kind == StageKind.Boss ? "WORLD 1 CLEAR" : "COURSE CLEAR";
            _resultsBody.text = BuildResultsBody(stage, result, record, hasNext ? W01ReferenceRoutes.Get(_nextStageId).Stage : null);
            _nextButtonText.text = hasNext ? "NEXT COURSE" : "WORLD MAP";
            _resultsPanel.SetActive(true);
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
            Time.timeScale = 1f;
            SceneManager.LoadScene(WorldMapSceneName, LoadSceneMode.Single);
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

            var canvasGo = new GameObject("World 1 Stage Flow Canvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 120;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = .5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            Button mapButton = CreateButton(
                canvasGo.transform,
                "World Map Button",
                "WORLD MAP  [M]",
                new Vector2(-40f, -112f),
                new Vector2(220f, 48f),
                LoadWorldMap);
            mapButton.GetComponent<Image>().color = new Color(.03f, .07f, .10f, .82f);

            _resultsPanel = BuildResultsPanel(canvasGo.transform);
            _resultsPanel.SetActive(false);
        }

        private GameObject BuildResultsPanel(Transform parent)
        {
            GameObject backdrop = CreateModalPanel(parent, "Course Results", new Vector2(760f, 540f));
            Transform content = backdrop.transform.GetChild(0);
            _resultsTitle = CreateText(content, "Results Title", "COURSE CLEAR", new Vector2(0f, -52f), new Vector2(640f, 62f), TextAnchor.MiddleCenter, 40, FontStyle.Bold, new Color(.71f, .98f, .42f, 1f));
            _resultsBody = CreateText(content, "Results Body", string.Empty, new Vector2(0f, -142f), new Vector2(640f, 176f), TextAnchor.MiddleCenter, 21, FontStyle.Normal, Color.white);

            Button next = CreateCenteredButton(content, "Next Course", "NEXT COURSE", new Vector2(0f, -350f), new Vector2(360f, 62f), LoadNextStage);
            _nextButtonText = next.GetComponentInChildren<Text>();
            next.GetComponent<Image>().color = new Color(.22f, .57f, .22f, .98f);

            CreateCenteredButton(content, "Retry Course", "RETRY", new Vector2(-168f, -432f), new Vector2(280f, 54f), RetryStage);
            CreateCenteredButton(content, "Return To Map", "WORLD MAP", new Vector2(168f, -432f), new Vector2(280f, 54f), LoadWorldMap);
            return backdrop;
        }

        private static string BuildResultsBody(StageDefinition stage, RunResult result, StageProgressRecord record, StageDefinition next)
        {
            string damage = result.DamageCount == 0 ? "PERFECT — NO DAMAGE" : $"HITS  {result.DamageCount}";
            string mastery = record.MasteryFlags.HasFlag(StageMasteryFlags.Crown)
                ? "CROWN"
                : record.MasteryFlags.HasFlag(StageMasteryFlags.MasterTime) ? "MASTER TIME" : "CLEAR";
            string best = record.BestValidTimeSeconds > 0f ? FormatTime(record.BestValidTimeSeconds) : "ASSIST CLEAR — NO STANDARD PB";
            string nextLine = next == null ? "RETURN TO DAISY MEADOW" : $"NEXT  {next.Id}  {next.DisplayName.ToUpperInvariant()}";
            return $"{stage.Id}  {stage.DisplayName.ToUpperInvariant()}\n\nTIME  {FormatTime(result.DisplayTimeSeconds)}    {damage}\n{mastery}    BEST  {best}\n\n{nextLine}";
        }

        private static string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            float remaining = seconds - minutes * 60f;
            return $"{minutes:00}:{remaining:00.000}";
        }

        private static GameObject CreateModalPanel(Transform parent, string name, Vector2 size)
        {
            var backdrop = new GameObject(name + " Backdrop");
            backdrop.transform.SetParent(parent, false);
            var backdropRect = backdrop.AddComponent<RectTransform>();
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;
            var backdropImage = backdrop.AddComponent<Image>();
            backdropImage.color = new Color(.01f, .025f, .04f, .72f);

            var panel = new GameObject(name);
            panel.transform.SetParent(backdrop.transform, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            var image = panel.AddComponent<Image>();
            image.color = new Color(.025f, .060f, .085f, .97f);
            var outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(.39f, .76f, .98f, .55f);
            outline.effectDistance = new Vector2(2f, -2f);
            return backdrop;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.AddComponent<Image>();
            image.color = new Color(.08f, .18f, .24f, .96f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            CreateStretchText(go.transform, label, 18);
            return button;
        }

        private static Button CreateCenteredButton(Transform parent, string name, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
            rect.pivot = new Vector2(.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.AddComponent<Image>();
            image.color = new Color(.075f, .18f, .24f, .98f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            CreateStretchText(go.transform, label, 19);
            return button;
        }

        private static void CreateStretchText(Transform parent, string value, int fontSize)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(10f, 5f);
            rect.offsetMax = new Vector2(-10f, -5f);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
        }

        private static Text CreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, TextAnchor alignment, int fontSize, FontStyle style, Color color)
        {
            var go = new GameObject(name);
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

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("Event System");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }
    }
}
