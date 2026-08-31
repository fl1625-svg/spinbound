using UnityEngine;

namespace Spinbound.Presentation.WorldMap
{
    public sealed class WorldMapAvatar : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 8f;
        private Vector3 _target;
        private Transform _visual;
        private float _bobClock;

        public bool IsMoving => (transform.position - _target).sqrMagnitude > .0025f;

        public static WorldMapAvatar Build(Transform parent)
        {
            var root = new GameObject("Orbital Explorer — World Map Avatar");
            root.transform.SetParent(parent, false);
            var avatar = root.AddComponent<WorldMapAvatar>();

            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Avatar Core";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(.58f, .30f, .58f);
            Object.Destroy(body.GetComponent<Collider>());

            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Avatar Rotor";
            ring.transform.SetParent(root.transform, false);
            ring.transform.localPosition = new Vector3(0f, .02f, 0f);
            ring.transform.localScale = new Vector3(1.15f, .035f, .16f);
            Object.Destroy(ring.GetComponent<Collider>());

            Material coreMaterial = CreateMaterial(
                "World Map Avatar Core",
                new Color(.93f, .97f, 1f),
                new Color(.20f, .68f, 1f) * 1.5f);
            Material rotorMaterial = CreateMaterial(
                "World Map Avatar Rotor",
                new Color(.20f, .56f, .92f),
                new Color(.18f, .62f, 1f) * 1.1f);
            body.GetComponent<Renderer>().sharedMaterial = coreMaterial;
            ring.GetComponent<Renderer>().sharedMaterial = rotorMaterial;

            avatar._visual = ring.transform;
            avatar._target = root.transform.position;
            return avatar;
        }

        public void SetTarget(Vector3 worldPosition, bool instant)
        {
            _target = worldPosition;
            if (instant)
                transform.position = worldPosition;
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, _target, _moveSpeed * Time.unscaledDeltaTime);
            _bobClock += Time.unscaledDeltaTime;
            if (_visual != null)
            {
                _visual.localRotation = Quaternion.Euler(0f, _bobClock * 95f, 0f);
                _visual.localPosition = new Vector3(0f, .07f + Mathf.Sin(_bobClock * 3.5f) * .035f, 0f);
            }
        }

        private static Material CreateMaterial(string name, Color baseColor, Color emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { name = name };
            material.color = baseColor;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", baseColor);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .72f);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
            }
            return material;
        }
    }
}
