using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Spinbound.Worlds;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.UnityRuntime
{
    /// <summary>
    /// Fast browser-playtest shell for the complete authored World 1 route.
    /// It keeps stage navigation presentation-only: authoritative gameplay remains in Core/Worlds.
    /// </summary>
    public sealed class World1PlaytestFlow : MonoBehaviour
    {
        [SerializeField] private string _stageId = W01_01_FirstSpin.Id;

        private Canvas _canvas;
        private GameObject _stageSelectPanel;
        private GameObject _resultsPanel;
        private Text _resultsTitle;
        private Text _resultsBody;
        private Text _nextButtonText;
        private bool _completed;

        public static World1PlaytestFlow Build(StageDefinition stage)
        {
            if (stage == null) throw new ArgumentNullException(nameof(stage));
            var root = new GameObject("World 1 Playtest Flow");
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
            EnsureUi();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (_stageSelectPanel.activeSelf) HideStageSelect();
                else ShowStageSelect();
            }
        }

        public void CompleteStage(StageDefinition stage, float elapsedSeconds, int hits)
        {
            if (stage == null) throw new ArgumentNullException(nameof(stage));
            if (_completed) return;

            _completed = true;
            _stageId = stage.Id;
            EnsureUi();

            bool hasNext = World1StageSequence.TryGetNext(stage.Id, out StageDefinition next);
            _resultsTitle.text = stage.Kind == StageKind.Boss ? "WORLD 1 CLEAR" : "COURSE CLEAR";
            _resultsBody.text = BuildResultsBody(stage, elapsedSeconds, hits, hasNext ? next : null);
            _nextButtonText.text = hasNext ? "NEXT COURSE" : "REPLAY WORLD 1";
            _stageSelectPanel.SetActive(false);
            _resultsPanel.SetActive(true);
        }

        public void LoadNextStage()
        {
            if (World1StageSequence.TryGetNext(_stageId, out StageDefinition next))
            {
                LoadStage(next.Id);
                return;
            }

            LoadStage(World1StageSequence.Get(0).Id);
        }

        public void RetryStage()
        {
            LoadStage(_stageId);
        }

        public void ShowStageSelect()
        {
            EnsureUi();
            _resultsPanel.SetActive(false);
            _stageSelectPanel.SetActive(true);
        }

        public void HideStageSelect()
        {
            if (_stageSelectPanel != null)
                _stageSelectPanel.SetActive(false);
        }

        public void LoadStageByIndex(int index)
        {
            LoadStage(World1StageSequence.Get(index).Id);
        }

        private static void LoadStage(string stageId)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(stageId, LoadSceneMode.Single);
        }

        private void EnsureUi()
        {
            if (_canvas != null) return;

            EnsureEventSystem();

            var canvasGo = new GameObject("World 1 Flow Canvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 120;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = .5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            CreateStageMenuButton(canvasGo.transform);
            _stageSelectPanel = BuildStageSelectPanel(canvasGo.transform);
            _resultsPanel = BuildResultsPanel(canvasGo.transform);
            _stageSelectPanel.SetActive(false);
            _resultsPanel.SetActive(false);
        }

        private void CreateStageMenuButton(Transform parent)
        {
            var button = CreateButton(parent, "Stage Menu Button", "STAGES  [M]", new Vector2(-40f, -112f), new Vector2(190f, 48f), true, ShowStageSelect);
            var image = button.GetComponent<Image>();
            image.color = new Color(.03f, .07f, .10f, .82f);
        }

        private GameObject BuildStageSelectPanel(Transform parent)
        {
            var panel = CreateModalPanel(parent, "World 1 Stage Select", new Vector2(900f, 660f));
            CreateText(panel.transform, "Stage Select Title", "WORLD 1 — DAISY MEADOW", new Vector2(0f, -38f), new Vector2(760f, 58f), TextAnchor.MiddleCenter, 34, FontStyle.Bold, Color.white);
            CreateText(panel.transform, "Stage Select Caption", "PLAYTEST ROUTE  •  8 AUTHORED COURSES", new Vector2(0f, -96f), new Vector2(720f, 36f), TextAnchor.MiddleCenter, 17, FontStyle.Bold, new Color(.60f, .84f, 1f, 1f));

            for (int i = 0; i < World1StageSequence.Count; i++)
            {
                int capturedIndex = i;
                StageDefinition stage = World1StageSequence.Get(i);
                int column = i % 2;
                int row = i / 2;
                float x = column == 0 ? -215f : 215f;
                float y = -166f - row * 96f;
                string label = $"{stage.Id}\n{stage.DisplayName.ToUpperInvariant()}";
                Button button = CreateCenteredButton(panel.transform, $"Stage {stage.Id}", label, new Vector2(x, y), new Vector2(390f, 76f), () => LoadStageByIndex(capturedIndex));
                if (string.Equals(stage.Id, _stageId, StringComparison.Ordinal))
                    button.GetComponent<Image>().color = new Color(.19f, .48f, .23f, .96f);
            }

            CreateCenteredButton(panel.transform, "Close Stage Select", "BACK TO COURSE", new Vector2(0f, -566f), new Vector2(280f, 52f), HideStageSelect);
            return panel;
        }

        private GameObject BuildResultsPanel(Transform parent)
        {
            var panel = CreateModalPanel(parent, "Course Results", new Vector2(720f, 520f));
            _resultsTitle = CreateText(panel.transform, "Results Title", "COURSE CLEAR", new Vector2(0f, -52f), new Vector2(600f, 62f), TextAnchor.MiddleCenter, 40, FontStyle.Bold, new Color(.71f, .98f, .42f, 1f));
            _resultsBody = CreateText(panel.transform, "Results Body", string.Empty, new Vector2(0f, -142f), new Vector2(590f, 154f), TextAnchor.MiddleCenter, 22, FontStyle.Normal, Color.white);

            Button next = CreateCenteredButton(panel.transform, "Next Course", "NEXT COURSE", new Vector2(0f, -332f), new Vector2(360f, 62f), LoadNextStage);
            _nextButtonText = next.GetComponentInChildren<Text>();
            next.GetComponent<Image>().color = new Color(.22f, .57f, .22f, .98f);

            CreateCenteredButton(panel.transform, "Retry Course", "RETRY", new Vector2(-168f, -414f), new Vector2(280f, 54f), RetryStage);
            CreateCenteredButton(panel.transform, "Choose Stage", "STAGE SELECT", new Vector2(168f, -414f), new Vector2(280f, 54f), ShowStageSelect);
            return panel;
        }

        private static string BuildResultsBody(StageDefinition stage, float elapsedSeconds, int hits, StageDefinition next)
        {
            float safe = Mathf.Max(0f, elapsedSeconds);
            int minutes = Mathf.FloorToInt(safe / 60f);
            float seconds = safe - minutes * 60f;
            string damage = hits == 0 ? "PERFECT — NO DAMAGE" : $"HITS  {hits}";
            string mastery = stage.MasterTimeSeconds > 0f && safe <= stage.MasterTimeSeconds ? "MASTER TIME" : "CLEAR";
            string nextLine = next == null ? "BLOOM ENGINE COMPLETE" : $"NEXT  {next.Id}  {next.DisplayName.ToUpperInvariant()}";
            return $"{stage.Id}  {stage.DisplayName.ToUpperInvariant()}\n\nTIME  {minutes:00}:{seconds:00.000}    {damage}\n{mastery}\n\n{nextLine}";
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

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size, bool rightAnchored, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            Vector2 anchor = rightAnchored ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = rightAnchored ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
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
            CreateStretchText(go.transform, label, label.Contains("\n") ? 16 : 19);
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
