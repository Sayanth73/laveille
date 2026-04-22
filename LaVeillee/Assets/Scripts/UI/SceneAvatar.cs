using LaVeillee.Core;
using TMPro;
using UnityEngine;

namespace LaVeillee.UI
{
    /// Story 5.2 — Avatar 3D posé sur un socle.
    /// Attaché au root de l'instance FBX personnage. Expose :
    ///   - SlotIndex (0..11) : position fixe dans le cercle de socles
    ///   - AssignedPlayerId  : PlayerRef.PlayerId affecté au runtime par GameSceneController
    ///   - IsAlive, IsPickable : commandent les visuels et les raycasts
    ///   - Highlight(color) : halo pour "peut être visé"
    ///   - ShowLabel(text)  : bulle texte au-dessus (pseudo, votes, rôle révélé)
    ///
    /// Les tints (base color) sont appliqués via MaterialPropertyBlock pour éviter
    /// de créer des matériaux uniques par instance (chaque personnage partage le
    /// même shader mais diverge par PlayerId).
    [RequireComponent(typeof(BoxCollider))]
    public class SceneAvatar : MonoBehaviour
    {
        public int SlotIndex { get; set; } = -1;
        public int AssignedPlayerId { get; private set; } = -1;
        public bool IsAlive { get; private set; } = true;
        public bool IsPickable { get; set; } = false;

        Renderer[] _renderers;
        TextMeshPro _label;
        MaterialPropertyBlock _mpb;
        Color _tint = Color.white;
        Color _highlightColor = Color.clear;
        GameObject _ring;

        static readonly int ColorProp = Shader.PropertyToID("_Color");
        static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");

        void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _mpb = new MaterialPropertyBlock();

            var col = GetComponent<BoxCollider>();
            if (col.size == Vector3.zero)
            {
                col.center = new Vector3(0f, 0.9f, 0f);
                col.size = new Vector3(0.9f, 1.9f, 0.6f);
            }
        }

        public void Assign(int playerId, int avatarSeed)
        {
            AssignedPlayerId = playerId;
            SetTintFromSeed(avatarSeed);
        }

        public void SetLabel(string text)
        {
            if (_label == null)
            {
                var go = new GameObject("Label");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 2.3f, 0f);
                _label = go.AddComponent<TextMeshPro>();
                _label.alignment = TextAlignmentOptions.Center;
                _label.fontSize = 2.4f;
                _label.color = new Color(1f, 0.95f, 0.85f);
                _label.outlineWidth = 0.25f;
                _label.outlineColor = new Color(0f, 0f, 0f, 1f);
                var rt = _label.rectTransform;
                rt.sizeDelta = new Vector2(4f, 1f);
            }
            _label.text = text ?? "";
            _label.gameObject.SetActive(!string.IsNullOrEmpty(text));
            BillboardLabel();
        }

        public void SetAlive(bool alive)
        {
            IsAlive = alive;
            ApplyTint();
            // Mort : s'incline légèrement au sol.
            transform.localRotation = alive
                ? Quaternion.identity
                : Quaternion.Euler(-80f, 0f, 0f);
            transform.localPosition = alive
                ? new Vector3(transform.localPosition.x, 0f, transform.localPosition.z)
                : new Vector3(transform.localPosition.x, 0.1f, transform.localPosition.z);
        }

        public void Highlight(Color c)
        {
            _highlightColor = c;
            EnsureRing();
            var ringMat = _ring.GetComponent<Renderer>();
            if (ringMat != null)
            {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor(ColorProp, c);
                mpb.SetColor(BaseColorProp, c);
                ringMat.SetPropertyBlock(mpb);
            }
            _ring.SetActive(true);
        }

        public void ClearHighlight()
        {
            _highlightColor = Color.clear;
            if (_ring != null) _ring.SetActive(false);
        }

        void EnsureRing()
        {
            if (_ring != null) return;
            _ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _ring.name = "HighlightRing";
            Destroy(_ring.GetComponent<Collider>());
            _ring.transform.SetParent(transform, false);
            _ring.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            _ring.transform.localScale = new Vector3(1.6f, 0.03f, 1.6f);
            // Shader unlit additif pour le halo.
            var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            var mat = new Material(shader);
            _ring.GetComponent<Renderer>().sharedMaterial = mat;
        }

        void SetTintFromSeed(int seed)
        {
            var rng = new System.Random(seed);
            float h = (float)rng.NextDouble();
            float s = 0.55f + (float)rng.NextDouble() * 0.25f;
            float v = 0.85f;
            _tint = Color.HSVToRGB(h, s, v);
            ApplyTint();
        }

        void ApplyTint()
        {
            var c = IsAlive ? _tint : new Color(0.35f, 0.35f, 0.4f, 1f);
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor(ColorProp, c);
                _mpb.SetColor(BaseColorProp, c);
                r.SetPropertyBlock(_mpb);
            }
        }

        void LateUpdate()
        {
            if (_label != null && _label.gameObject.activeSelf) BillboardLabel();
        }

        void BillboardLabel()
        {
            var cam = Camera.main;
            if (cam == null || _label == null) return;
            _label.transform.rotation = Quaternion.LookRotation(
                _label.transform.position - cam.transform.position);
        }
    }
}
