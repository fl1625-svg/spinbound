using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spinbound.Meta;
using Spinbound.Worlds;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.Presentation.WorldMap
{
    public sealed class WorldMapController : MonoBehaviour
    {
        private WorldMapGraph _graph;
        private PlayerProgress _progress;
        private readonly Dictionary<string, StageNodeView> _views = new(StringComparer.Ordinal);
        private readonly Dictionary<string, GameObject> _conditionalPaths = new(StringComparer.Ordinal);
        private WorldMapAvatar _avatar;
        private string _currentStageId;
        private Text _stageTitle;
        private Text _stageMeta;
        private Text _actionHint;
        private bool _built;

        public event Action<string> StageRequested;
        public string CurrentStageId => _currentStageId;

        private void Awake()
        {
            _graph = WorldMapGraph.CreateWorld1();
            _progress = new PlayerProgress();
            BuildMap();
            SelectInitialStage(instant: true);
        }

        public void ConfigureProgress(PlayerProgress progress)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            if (!_built) BuildMap();
            SelectInitialStage(instant: true);
            RefreshViews();
        }

        private void Update()
        {
            if (!_built || _avatar == null || _avatar.IsMoving) return;

            Vector2 direction = Vector2.zero;
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) direction.x -= 1f;
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) direction.x += 1f;
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) direction.y += 1f;
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) direction.y -= 1f;

            if (direction.sqrMagnitude > .01f)
                MoveSelection(direction.normalized);

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
                RequestCurrentStage();
        }

        public void RequestCurrentStage()
        {
            if (string.IsNullOrEmpty(_currentStageId) || !ProgressionRules.IsUnlocked(_currentStageId, _progress))
                return;

            StageRequested?.Invoke(_currentStageId);
        }

        private void MoveSelection(Vector2 desiredDirection)
        {
            WorldMapNode current = _graph.Get(_currentStageId);
            string bestId = string.Empty;
            float bestDot = .22f;

            for (int i = 0; i < current.Neighbors.Count; i++)
            {
                string neighborId = current.Neighbors[i];
                if (!ProgressionRules.IsVisible(neighborId, _progress) || !ProgressionRules.IsUnlocked(neighborId, _progress))
                    continue;

                WorldMapNode neighbor = _graph.Get(neighborId);
                Vector2 delta = new Vector2(neighbor.Position.X - current.Position.X, neighbor.Position.Y - current.Position.Y);
                if (delta.sqrMagnitude < .001f) continue;

                float dot = Vector2.Dot(delta.normalized, desiredDirection);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestId = neighborId;
                }
            }

            if (!string.IsNullOrEmpty(bestId))
                Select(bestId, instant: false);
        }

        private void SelectInitialStage(bool instant)
        {
            string candidate = W01_01_FirstSpin.Id;
            while (true)
            {
                if (ProgressionRules.IsUnlocked(candidate, _progress) && !_progress.HasCleared(candidate))
                    break;

                string next = ProgressionRules.GetMainNextStageId(candidate);
                if (string.IsNullOrEmpty(next) || !ProgressionRules.IsUnlocked(next, _progress))
                    break;
                candidate = next;
            }

            Select(candidate, instant);
        }

        private void Select(string stageId, bool instant)
        {
            if (!_views.TryGetValue(stageId, out StageNodeView view)) return;
            if (!ProgressionRules.IsVisible(stageId, _progress)) return;

            _currentStageId = stageId;
            _avatar.SetTarget(view.WorldPosition, instant);
            RefreshViews();
        }

        private void RefreshViews()
        {
            if (!_built || _progress == null) return;

            foreach (WorldMapNode node in _graph.Nodes)
            {
                bool visible = ProgressionRules.IsVisible(node.StageId, _progress);
                bool unlocked = ProgressionRules.IsUnlocked(node.StageId, _progress);
                bool cleared = _progress.HasCleared(node.StageId);
                bool selected = string.Equals(node.StageId, _currentStageId, StringComparison.Ordinal);
                _views[node.StageId].SetState(visible, unlocked, cleared, selected);
            }

            bool trialVisible = ProgressionRules.IsVisible(W01_Trial_PerfectCorner.Id, _progress);
            if (_conditionalPaths.TryGetValue(W01_Trial_PerfectCorner.Id, out GameObject trialPath))
                trialPath.SetActive(trialVisible);

            StageDefinition stage = W01ReferenceRoutes.Get(_currentStageId).Stage;
            StageProgressRecord record = _progress.Get(_currentStageId);
            _stageTitle.text = $"{stage.Id}  {stage.DisplayName.ToUpperInvariant()}";

            string status = record.HasCleared ? "CLEAR" : "NEW";
            if (record.MasteryFlags.HasFlag(StageMasteryFlags.Crown)) status = "CROWN";
            else if (record.MasteryFlags.HasFlag(StageMasteryFlags.Perfect)) status = "PERFECT";

            string best = record.BestValidTimeSeconds > 0f ? FormatTime(record.BestValidTimeSeconds) : "--:--.---";
            _stageMeta.text = $"{status}    BEST {best}";
            _actionHint.text = ProgressionRules.IsUnlocked(_currentStageId, _progress)
                ? "WASD / ARROWS  MOVE     ENTER / SPACE  PLAY"
                : "ROUTE LOCKED";
        }

        private void BuildMap()
        {
            if (_built) return;
            _built = true;

            Transform worldRoot = new GameObject("Daisy Meadow Diorama").transform;
            worldRoot.SetParent(transform, false);
            BuildGround(worldRoot);
            BuildPaths(worldRoot);

            Transform nodeRoot = new GameObject("Stage Nodes").transform;
            nodeRoot.SetParent(worldRoot, false);
            foreach (WorldMapNode node in _graph.Nodes)
                _views[node.StageId] = StageNodeView.Build(node, nodeRoot);

            _avatar = WorldMapAvatar.Build(worldRoot);
            BuildHud();
        }

        private void BuildGround(Transform parent)
        {
            var island = GameObject.CreatePrimitive(PrimitiveType.Cube);
            island.name = "Daisy Meadow Floating Island";
            island.transform.SetParent(parent, false);
            island.transform.localPosition = new Vector3(2f, -.55f, -.6f);
            island.transform.localScale = new Vector3(21f, .65f, 10.5f);
            Destroy(island.GetComponent<Collider>());
            island.GetComponent<Renderer>().sharedMaterial = CreateMaterial(
                "Map Grass",
                new Color(.25f, .58f, .20f),
                new Color(.05f, .14f, .04f));

            var soil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            soil.name = "Diorama Earth Base";
            soil.transform.SetParent(parent, false);
            soil.transform.localPosition = new Vector3(2f, -1.10f, -.6f);
            soil.transform.localScale = new Vector3(20.4f, .60f, 9.9f);
            Destroy(soil.GetComponent<Collider>());
            soil.GetComponent<Renderer>().sharedMaterial = CreateMaterial(
                "Map Earth",
                new Color(.34f, .22f, .12f),
                Color.black);

            for (int i = 0; i < 18; i++)
            {
                var flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flower.name = "Map Flower";
                flower.transform.SetParent(parent, false);
                float x = -7.5f + (i % 9) * 2.2f;
                float z = -3.8f + (i / 9) * 7.2f + Mathf.Sin(i * 1.7f) * .45f;
                flower.transform.localPosition = new Vector3(x, -.05f, z);
                flower.transform.localScale = Vector3.one * (.14f + (i % 3) * .035f);
                Destroy(flower.GetComponent<Collider>());
                Color c = i % 2 == 0 ? new Color(1f, .82f, .28f) : new Color(1f, .55f, .72f);
                flower.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Map Flower", c, c * .25f);
            }
        }

        private void BuildPaths(Transform parent)
        {
            var made = new HashSet<string>(StringComparer.Ordinal);
            foreach (WorldMapNode node in _graph.Nodes)
            {
                for (int i = 0; i < node.Neighbors.Count; i++)
                {
                    string otherId = node.Neighbors[i];
                    string key = string.CompareOrdinal(node.StageId, otherId) < 0
                        ? node.StageId + "|" + otherId
                        : otherId + "|" + node.StageId;
                    if (!made.Add(key)) continue;

                    WorldMapNode other = _graph.Get(otherId);
                    GameObject path = BuildPathSegment(parent, node, other);
                    if (node.StageId == W01_Trial_PerfectCorner.Id || otherId == W01_Trial_PerfectCorner.Id)
                        _conditionalPaths[W01_Trial_PerfectCorner.Id] = path;
                }
            }
        }

        private static GameObject BuildPathSegment(Transform parent, WorldMapNode a, WorldMapNode b)
        {
            Vector3 start = new Vector3(a.Position.X, .02f, a.Position.Y);
            Vector3 end = new Vector3(b.Position.X, .02f, b.Position.Y);
            Vector3 delta = end - start;

            var path = GameObject.CreatePrimitive(PrimitiveType.Cube);
            path.name = $"Map Route — {a.StageId} → {b.StageId}";
            path.transform.SetParent(parent, false);
            path.transform.position = (start + end) * .5f;
            path.transform.localScale = new Vector3(.28f, .07f, delta.magnitude);
            path.transform.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            Destroy(path.GetComponent<Collider>());
            path.GetComponent<Renderer>().sharedMaterial = CreateMaterial(
                "Map Route",
                new Color(.82f, .74f, .52f),
                new Color(.12f, .09f, .03f));
            return path;
        }

        private void BuildHud()
        {
            var root = new GameObject("World Map HUD");
            root.transform.SetParent(transform, false);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = .5f;
            root.AddComponent<GraphicRaycaster>();

            RectTransform titlePanel = CreatePanel(root.transform, "World Header", new Vector2(38f, -38f), new Vector2(650f, 104f), new Color(.018f, .045f, .067f, .88f));
            CreateText(titlePanel, "WORLD 1  ·  DAISY MEADOW", new Vector2(22f, -10f), new Vector2(600f, 38f), 26, FontStyle.Bold, new Color(.72f, .96f, .44f), TextAnchor.MiddleLeft);
            _stageTitle = CreateText(titlePanel, string.Empty, new Vector2(22f, -48f), new Vector2(600f, 42f), 22, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);

            RectTransform footer = CreatePanel(root.transform, "World Footer", new Vector2(38f, 38f), new Vector2(720f, 82f), new Color(.018f, .045f, .067f, .86f), bottom: true);
            _stageMeta = CreateText(footer, string.Empty, new Vector2(20f, -7f), new Vector2(680f, 30f), 17, FontStyle.Bold, new Color(.68f, .86f, 1f), TextAnchor.MiddleLeft);
            _actionHint = CreateText(footer, string.Empty, new Vector2(20f, -39f), new Vector2(680f, 28f), 15, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft);
        }

        private static RectTransform CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool bottom = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            Vector2 anchor = bottom ? new Vector2(0f, 0f) : new Vector2(0f, 1f);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = bottom ? new Vector2(0f, 0f) : new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.AddComponent<Image>();
            image.color = color;
            return rect;
        }

        private static Text CreateText(RectTransform parent, string value, Vector2 position, Vector2 size, int fontSize, FontStyle style, Color color, TextAnchor anchor)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.raycastTarget = false;
            return text;
        }

        private static Material CreateMaterial(string name, Color baseColor, Color emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { name = name };
            material.color = baseColor;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", baseColor);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .34f);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
            }
            return material;
        }

        private static string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            float remaining = seconds - minutes * 60f;
            return $"{minutes:00}:{remaining:00.000}";
        }
    }
}
