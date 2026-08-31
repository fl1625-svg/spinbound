using UnityEngine;
using Spinbound.Meta;
using Spinbound.Worlds;
using Spinbound.Worlds.W01.DaisyMeadow;

namespace Spinbound.Presentation.WorldMap
{
    public sealed class StageNodeView : MonoBehaviour
    {
        private Renderer _padRenderer;
        private Renderer _beaconRenderer;
        private Material _padMaterial;
        private Material _beaconMaterial;
        private TextMesh _label;
        private Transform _beacon;
        private bool _selected;
        private float _clock;

        public string StageId { get; private set; }
        public Vector3 WorldPosition => transform.position + Vector3.up * .48f;

        public static StageNodeView Build(WorldMapNode node, Transform parent)
        {
            var root = new GameObject($"Map Node — {node.StageId}");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(node.Position.X, .15f, node.Position.Y);

            var view = root.AddComponent<StageNodeView>();
            view.StageId = node.StageId;

            var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pad.name = "Node Pedestal";
            pad.transform.SetParent(root.transform, false);
            pad.transform.localScale = node.Kind == WorldMapNodeKind.Boss
                ? new Vector3(1.25f, .16f, 1.25f)
                : new Vector3(.82f, .14f, .82f);
            Object.Destroy(pad.GetComponent<Collider>());
            view._padRenderer = pad.GetComponent<Renderer>();
            view._padMaterial = CreateMaterial(node.StageId + " Pad");
            view._padRenderer.sharedMaterial = view._padMaterial;

            var beacon = GameObject.CreatePrimitive(
                node.Kind == WorldMapNodeKind.Boss ? PrimitiveType.Cube : PrimitiveType.Sphere);
            beacon.name = "Node Beacon";
            beacon.transform.SetParent(root.transform, false);
            beacon.transform.localPosition = new Vector3(0f, .55f, 0f);
            beacon.transform.localScale = node.Kind == WorldMapNodeKind.Boss
                ? new Vector3(.72f, .72f, .72f)
                : new Vector3(.44f, .44f, .44f);
            Object.Destroy(beacon.GetComponent<Collider>());
            view._beacon = beacon.transform;
            view._beaconRenderer = beacon.GetComponent<Renderer>();
            view._beaconMaterial = CreateMaterial(node.StageId + " Beacon");
            view._beaconRenderer.sharedMaterial = view._beaconMaterial;

            var labelGo = new GameObject("Node Label");
            labelGo.transform.SetParent(root.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 1.12f, 0f);
            view._label = labelGo.AddComponent<TextMesh>();
            StageDefinition stage = W01ReferenceRoutes.Get(node.StageId).Stage;
            view._label.text = stage.Kind == StageKind.Trial
                ? "TRIAL"
                : stage.Kind == StageKind.Boss ? "BOSS" : node.StageId.Replace("W01-", string.Empty);
            view._label.anchor = TextAnchor.MiddleCenter;
            view._label.alignment = TextAlignment.Center;
            view._label.fontSize = 42;
            view._label.characterSize = .055f;
            view._label.fontStyle = FontStyle.Bold;
            view._label.color = Color.white;

            return view;
        }

        public void SetState(bool visible, bool unlocked, bool cleared, bool selected)
        {
            gameObject.SetActive(visible);
            if (!visible) return;

            _selected = selected;
            Color pad;
            Color beacon;

            if (!unlocked)
            {
                pad = new Color(.11f, .14f, .16f);
                beacon = new Color(.18f, .22f, .24f);
            }
            else if (cleared)
            {
                pad = new Color(.22f, .53f, .25f);
                beacon = new Color(.65f, .96f, .34f);
            }
            else
            {
                pad = new Color(.12f, .34f, .56f);
                beacon = new Color(.30f, .72f, 1f);
            }

            if (selected)
            {
                pad = Color.Lerp(pad, new Color(.90f, .78f, .28f), .55f);
                beacon = new Color(1f, .88f, .34f);
            }

            Apply(_padMaterial, pad, selected ? pad * 1.3f : pad * .35f);
            Apply(_beaconMaterial, beacon, unlocked ? beacon * (selected ? 1.8f : .9f) : Color.black);
            if (_label != null)
                _label.color = unlocked ? Color.white : new Color(.52f, .57f, .60f);
        }

        private void Update()
        {
            _clock += Time.unscaledDeltaTime;
            if (_beacon != null)
            {
                float scale = _selected ? 1f + Mathf.Sin(_clock * 5f) * .10f : 1f;
                Vector3 baseScale = StageId == W01_Boss_BloomEngine.Id
                    ? new Vector3(.72f, .72f, .72f)
                    : new Vector3(.44f, .44f, .44f);
                _beacon.localScale = baseScale * scale;
                _beacon.Rotate(Vector3.up, (_selected ? 75f : 28f) * Time.unscaledDeltaTime, Space.Self);
            }
        }

        private void LateUpdate()
        {
            if (_label == null || Camera.main == null) return;
            Vector3 away = _label.transform.position - Camera.main.transform.position;
            if (away.sqrMagnitude > .001f)
                _label.transform.rotation = Quaternion.LookRotation(away.normalized, Vector3.up);
        }

        private static Material CreateMaterial(string name)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { name = name };
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .55f);
            return material;
        }

        private static void Apply(Material material, Color baseColor, Color emission)
        {
            if (material == null) return;
            material.color = baseColor;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", baseColor);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
            }
        }
    }
}
