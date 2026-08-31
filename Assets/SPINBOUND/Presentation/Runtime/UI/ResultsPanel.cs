using System;
using UnityEngine;
using UnityEngine.UI;
using Spinbound.Core.Gameplay;
using Spinbound.Meta;
using Spinbound.Worlds;

namespace Spinbound.Presentation.UI
{
    public sealed class ResultsPanel : MonoBehaviour
    {
        private Text _title;
        private Text _timeValue;
        private Text _damageValue;
        private Text _masteryValue;
        private Text _bestValue;
        private Text _coresValue;
        private Text _nextLabel;
        private Button _nextButton;

        public RunResult CurrentResult { get; private set; }
        public StageProgressRecord CurrentRecord { get; private set; }

        public static ResultsPanel Build(Transform parent, Action retry, Action next, Action worldMap)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            var backdrop = new GameObject("Course Results Backdrop");
            backdrop.transform.SetParent(parent, false);
            var backdropRect = backdrop.AddComponent<RectTransform>();
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;
            var backdropImage = backdrop.AddComponent<Image>();
            backdropImage.color = new Color(.008f, .020f, .035f, .76f);

            var panelObject = new GameObject("Course Results");
            panelObject.transform.SetParent(backdrop.transform, false);
            var panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(.5f, .5f);
            panelRect.pivot = new Vector2(.5f, .5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(820f, 600f);
            var panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(.022f, .055f, .080f, .985f);
            var outline = panelObject.AddComponent<Outline>();
            outline.effectColor = new Color(.43f, .82f, 1f, .55f);
            outline.effectDistance = new Vector2(2f, -2f);

            var panel = backdrop.AddComponent<ResultsPanel>();
            panel._title = CreateText(panelRect, "Results Title", "COURSE CLEAR", new Vector2(0f, -40f), new Vector2(700f, 58f), 38, FontStyle.Bold, new Color(.72f, .98f, .42f), TextAnchor.MiddleCenter);

            CreateCaption(panelRect, "TIME", new Vector2(-260f, -126f));
            panel._timeValue = CreateValue(panelRect, "00:00.000", new Vector2(-260f, -164f));
            CreateCaption(panelRect, "DAMAGE", new Vector2(0f, -126f));
            panel._damageValue = CreateValue(panelRect, "PERFECT", new Vector2(0f, -164f));
            CreateCaption(panelRect, "ORBIT CORES", new Vector2(260f, -126f));
            panel._coresValue = CreateValue(panelRect, "0 / 3", new Vector2(260f, -164f));

            CreateCaption(panelRect, "MASTERY", new Vector2(-195f, -248f));
            panel._masteryValue = CreateValue(panelRect, "CLEAR", new Vector2(-195f, -286f));
            CreateCaption(panelRect, "BEST", new Vector2(195f, -248f));
            panel._bestValue = CreateValue(panelRect, "--:--.---", new Vector2(195f, -286f));

            panel._nextButton = CreateButton(panelRect, "Next Course", "NEXT COURSE", new Vector2(0f, -386f), new Vector2(390f, 64f), next, new Color(.20f, .58f, .22f, .98f));
            panel._nextLabel = panel._nextButton.GetComponentInChildren<Text>();
            CreateButton(panelRect, "Retry", "RETRY", new Vector2(-176f, -474f), new Vector2(300f, 56f), retry, new Color(.07f, .18f, .24f, .98f));
            CreateButton(panelRect, "World Map", "WORLD MAP", new Vector2(176f, -474f), new Vector2(300f, 56f), worldMap, new Color(.07f, .18f, .24f, .98f));

            backdrop.SetActive(false);
            return panel;
        }

        public void Show(StageDefinition stage, RunResult result, StageProgressRecord record, StageDefinition nextStage)
        {
            if (stage == null) throw new ArgumentNullException(nameof(stage));
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (record == null) throw new ArgumentNullException(nameof(record));

            CurrentResult = result;
            CurrentRecord = record;
            _title.text = stage.Kind == StageKind.Boss ? "WORLD 1 CLEAR" : "COURSE CLEAR";
            _timeValue.text = FormatTime(result.DisplayTimeSeconds);
            _damageValue.text = result.DamageCount == 0 ? "PERFECT" : $"{result.DamageCount} HIT{(result.DamageCount == 1 ? string.Empty : "S")}";
            _coresValue.text = $"{result.OrbitCoreIds.Count} / 3";
            _masteryValue.text = MasteryLabel(record.MasteryFlags);
            _bestValue.text = record.BestValidTimeSeconds > 0f ? FormatTime(record.BestValidTimeSeconds) : "NO STANDARD PB";
            _nextLabel.text = nextStage == null ? "WORLD MAP" : "NEXT COURSE";
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private static string MasteryLabel(StageMasteryFlags flags)
        {
            if (flags.HasFlag(StageMasteryFlags.Crown)) return "CROWN";
            if (flags.HasFlag(StageMasteryFlags.MasterTime)) return "MASTER TIME";
            if (flags.HasFlag(StageMasteryFlags.Perfect)) return "PERFECT";
            return "CLEAR";
        }

        private static string FormatTime(float seconds)
        {
            float safe = Mathf.Max(0f, seconds);
            int minutes = Mathf.FloorToInt(safe / 60f);
            float remaining = safe - minutes * 60f;
            return $"{minutes:00}:{remaining:00.000}";
        }

        private static void CreateCaption(RectTransform parent, string value, Vector2 position)
        {
            CreateText(parent, value + " Caption", value, position, new Vector2(210f, 30f), 14, FontStyle.Bold, new Color(.58f, .78f, .94f), TextAnchor.MiddleCenter);
        }

        private static Text CreateValue(RectTransform parent, string value, Vector2 position)
        {
            return CreateText(parent, value + " Value", value, position, new Vector2(230f, 44f), 22, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        }

        private static Text CreateText(RectTransform parent, string name, string value, Vector2 position, Vector2 size, int fontSize, FontStyle style, Color color, TextAnchor alignment)
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

        private static Button CreateButton(RectTransform parent, string name, string label, Vector2 position, Vector2 size, Action action, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
            rect.pivot = new Vector2(.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.AddComponent<Image>();
            image.color = color;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            if (action != null) button.onClick.AddListener(() => action());

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(go.transform, false);
            var labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10f, 5f);
            labelRect.offsetMax = new Vector2(-10f, -5f);
            var text = labelObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label;
            text.fontSize = 19;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return button;
        }
    }
}
