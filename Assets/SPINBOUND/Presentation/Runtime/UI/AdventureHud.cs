using UnityEngine;
using UnityEngine.UI;
using Spinbound.Core.Simulation;

namespace Spinbound.Presentation.UI
{
    public sealed class AdventureHud : MonoBehaviour
    {
        private Text _worldBadge;
        private Text _time;
        private Text _hearts;
        private Text _course;
        private Text _cores;
        private Text _speedDots;

        public Text Hearts => _hearts;

        public void SetCourse(string stageId, string displayName)
        {
            if (_worldBadge != null)
                _worldBadge.text = string.IsNullOrWhiteSpace(stageId) ? "W1" : stageId;
            if (_course != null)
                _course.text = string.IsNullOrWhiteSpace(displayName) ? "DAISY MEADOW" : displayName.ToUpperInvariant();
        }

        public void SetTime(float seconds)
        {
            if (_time == null) return;
            float safe = Mathf.Max(0f, seconds);
            int minutes = Mathf.FloorToInt(safe / 60f);
            float remaining = safe - minutes * 60f;
            _time.text = $"{minutes:00}:{remaining:00.000}";
        }

        public void SetHearts(int value)
        {
            if (_hearts == null) return;
            int hearts = Mathf.Clamp(value, 0, 5);
            _hearts.text = hearts <= 0 ? "—" : new string('♥', hearts);
        }

        public void SetOrbitCores(int collected, int total = 3)
        {
            if (_cores == null) return;
            int safeTotal = Mathf.Max(0, total);
            int safeCollected = Mathf.Clamp(collected, 0, safeTotal);
            _cores.text = $"{safeCollected} / {safeTotal}";
        }

        public void SetSpeedTier(SpeedTier tier)
        {
            if (_speedDots == null) return;
            _speedDots.text = tier switch
            {
                SpeedTier.Speed1 => "●  ○  ○",
                SpeedTier.Speed2 => "●  ●  ○",
                _ => "●  ●  ●",
            };
        }

        public static AdventureHud Build()
        {
            var root = new GameObject("Adventure HUD");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            root.AddComponent<GraphicRaycaster>();

            var hud = root.AddComponent<AdventureHud>();
            hud._course = CreateCourseCard(root.transform, out hud._worldBadge);
            hud._time = CreateTimeCard(root.transform);
            hud._hearts = CreateHeartsCard(root.transform);
            hud._cores = CreateCoresCard(root.transform);
            hud._speedDots = CreateSpeedCard(root.transform);
            hud.SetOrbitCores(0, 3);
            hud.SetSpeedTier(SpeedTier.Speed1);
            return hud;
        }

        private static Text CreateCourseCard(Transform parent, out Text worldBadge)
        {
            var panel = CreateCard(parent, "Course Card", new Vector2(40, -38), new Vector2(520, 62), false, new Color(.025f, .055f, .080f, .72f), new Color(.45f, .91f, .34f, 1f));
            worldBadge = CreateBadge(panel, "World Badge", "W01-01", new Vector2(16, 9), new Vector2(100, 44), new Color(.62f, .94f, .38f, .96f), new Color(.08f, .17f, .12f, 1f));
            var title = CreateText(panel, "Course Title", "FIRST SPIN", new Vector2(132, 8), new Vector2(364, 46), TextAnchor.MiddleLeft, 25, FontStyle.Bold, Color.white);
            AddTextOutline(title, new Color(0f, 0f, 0f, .34f), new Vector2(1, -1));
            return title;
        }

        private static Text CreateTimeCard(Transform parent)
        {
            var panel = CreateCard(parent, "Time Card", new Vector2(40, -108), new Vector2(238, 48), false, new Color(.025f, .055f, .080f, .62f), new Color(.33f, .74f, 1f, 1f));
            var caption = CreateText(panel, "Time Caption", "TIME", new Vector2(15, 5), new Vector2(58, 38), TextAnchor.MiddleLeft, 14, FontStyle.Bold, new Color(.66f, .84f, .98f, 1f));
            caption.horizontalOverflow = HorizontalWrapMode.Overflow;
            var value = CreateText(panel, "Time Value", "00:00.000", new Vector2(78, 5), new Vector2(144, 38), TextAnchor.MiddleRight, 21, FontStyle.Bold, Color.white);
            AddTextOutline(value, new Color(0f, 0f, 0f, .32f), new Vector2(1, -1));
            return value;
        }

        private static Text CreateHeartsCard(Transform parent)
        {
            var panel = CreateCard(parent, "Hearts Card", new Vector2(-40, -38), new Vector2(192, 62), true, new Color(.025f, .055f, .080f, .70f), new Color(1f, .46f, .60f, 1f));
            var caption = CreateText(panel, "Heart Caption", "ENERGY", new Vector2(14, 7), new Vector2(72, 48), TextAnchor.MiddleLeft, 13, FontStyle.Bold, new Color(1f, .72f, .78f, 1f));
            caption.horizontalOverflow = HorizontalWrapMode.Overflow;
            var value = CreateText(panel, "Heart Value", "♥♥♥", new Vector2(86, 7), new Vector2(88, 48), TextAnchor.MiddleRight, 25, FontStyle.Bold, new Color(1f, .52f, .64f, 1f));
            AddTextOutline(value, new Color(.20f, .01f, .04f, .42f), new Vector2(1, -1));
            return value;
        }

        private static Text CreateCoresCard(Transform parent)
        {
            var panel = CreateCard(parent, "Orbit Cores Card", new Vector2(-40, -108), new Vector2(192, 48), true, new Color(.025f, .055f, .080f, .62f), new Color(.92f, .75f, .28f, 1f));
            var caption = CreateText(panel, "Cores Caption", "CORES", new Vector2(14, 5), new Vector2(76, 38), TextAnchor.MiddleLeft, 13, FontStyle.Bold, new Color(1f, .86f, .49f, 1f));
            caption.horizontalOverflow = HorizontalWrapMode.Overflow;
            var value = CreateText(panel, "Cores Value", "0 / 3", new Vector2(88, 5), new Vector2(86, 38), TextAnchor.MiddleRight, 20, FontStyle.Bold, Color.white);
            AddTextOutline(value, new Color(0f, 0f, 0f, .28f), new Vector2(1, -1));
            return value;
        }

        private static Text CreateSpeedCard(Transform parent)
        {
            var panel = CreateCard(parent, "Speed Card", new Vector2(-40, -164), new Vector2(192, 42), true, new Color(.025f, .055f, .080f, .55f), new Color(.35f, .78f, 1f, 1f));
            var caption = CreateText(panel, "Speed Caption", "SPEED", new Vector2(14, 4), new Vector2(62, 32), TextAnchor.MiddleLeft, 12, FontStyle.Bold, new Color(.63f, .83f, .98f, 1f));
            caption.horizontalOverflow = HorizontalWrapMode.Overflow;
            return CreateText(panel, "Speed Dots", "●  ○  ○", new Vector2(74, 4), new Vector2(100, 32), TextAnchor.MiddleRight, 16, FontStyle.Bold, Color.white);
        }

        private static RectTransform CreateCard(Transform parent, string name, Vector2 pos, Vector2 size, bool right, Color background, Color accent)
        {
            var shadow = new GameObject(name + " Shadow");
            shadow.transform.SetParent(parent, false);
            var sr = shadow.AddComponent<RectTransform>();
            ConfigureRect(sr, pos + new Vector2(right ? -4f : 4f, -5f), size, right);
            var si = shadow.AddComponent<Image>();
            si.color = new Color(0f, .02f, .04f, .24f);
            si.raycastTarget = false;

            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var pr = panel.AddComponent<RectTransform>();
            ConfigureRect(pr, pos, size, right);
            var image = panel.AddComponent<Image>();
            image.color = background;
            image.raycastTarget = false;

            var accentBar = new GameObject("Accent");
            accentBar.transform.SetParent(panel.transform, false);
            var ar = accentBar.AddComponent<RectTransform>();
            ar.anchorMin = right ? new Vector2(1, 0) : new Vector2(0, 0);
            ar.anchorMax = right ? new Vector2(1, 1) : new Vector2(0, 1);
            ar.pivot = right ? new Vector2(1, .5f) : new Vector2(0, .5f);
            ar.anchoredPosition = Vector2.zero;
            ar.sizeDelta = new Vector2(6, 0);
            var ai = accentBar.AddComponent<Image>();
            ai.color = accent;
            ai.raycastTarget = false;

            var highlight = new GameObject("Glass Highlight");
            highlight.transform.SetParent(panel.transform, false);
            var hr = highlight.AddComponent<RectTransform>();
            hr.anchorMin = new Vector2(0, 1);
            hr.anchorMax = new Vector2(1, 1);
            hr.pivot = new Vector2(.5f, 1);
            hr.anchoredPosition = Vector2.zero;
            hr.sizeDelta = new Vector2(-12, 1);
            var hi = highlight.AddComponent<Image>();
            hi.color = new Color(1f, 1f, 1f, .16f);
            hi.raycastTarget = false;
            return pr;
        }

        private static Text CreateBadge(RectTransform parent, string name, string value, Vector2 pos, Vector2 size, Color background, Color foreground)
        {
            var badge = new GameObject(name);
            badge.transform.SetParent(parent, false);
            var rect = badge.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(pos.x, -pos.y);
            rect.sizeDelta = size;
            var image = badge.AddComponent<Image>();
            image.color = background;
            image.raycastTarget = false;

            var text = CreateText(rect, "World Tag", value, Vector2.zero, size, TextAnchor.MiddleCenter, 17, FontStyle.Bold, foreground);
            var tr = text.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.pivot = new Vector2(.5f, .5f);
            tr.anchoredPosition = Vector2.zero;
            tr.sizeDelta = Vector2.zero;
            return text;
        }

        private static void ConfigureRect(RectTransform rect, Vector2 pos, Vector2 size, bool right)
        {
            var anchor = right ? new Vector2(1, 1) : new Vector2(0, 1);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = right ? new Vector2(1, 1) : new Vector2(0, 1);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }

        private static Text CreateText(RectTransform parent, string name, string value, Vector2 pos, Vector2 size, TextAnchor alignment, int fontSize, FontStyle style, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(pos.x, -pos.y);
            rect.sizeDelta = size;

            var text = go.AddComponent<Text>();
            text.text = value;
            text.alignment = alignment;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget = false;
            text.resizeTextForBestFit = false;
            return text;
        }

        private static void AddTextOutline(Text text, Color color, Vector2 distance)
        {
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }
    }
}
