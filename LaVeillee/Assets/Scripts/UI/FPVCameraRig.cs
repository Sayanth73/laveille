using UnityEngine;

namespace LaVeillee.UI
{
    /// Story 5.7 — Vue première personne depuis le socle du joueur local.
    /// Attache au Main Camera ; position pilotée par GameSceneController, rotation
    /// pilotée par drag souris (desktop) ou swipe tactile (mobile).
    ///
    /// Contrôles :
    ///   - Desktop : clic gauche maintenu + drag → rotation. Clic simple (pas de drag)
    ///     → GameSceneController gère le raycast de sélection.
    ///   - Mobile : swipe pour regarder, tap pour sélectionner.
    [DefaultExecutionOrder(100)]
    public class FPVCameraRig : MonoBehaviour
    {
        public float LookSensitivity = 0.25f;
        public float PitchMin = -35f;
        public float PitchMax = 45f;
        public Vector3 EyeOffset = new Vector3(0f, 1.55f, 0f);

        public Transform Anchor;          // socle local (position world = pivot)
        public bool IsDragging { get; private set; }
        public Vector2 DragTotalDelta { get; private set; }

        float _yaw;
        float _pitch;
        Vector2 _lastPointer;
        bool _dragStarted;

        public void SetInitialLook(Vector3 lookAt)
        {
            var dir = lookAt - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                _yaw = Quaternion.LookRotation(dir).eulerAngles.y;
            _pitch = 0f;
        }

        void LateUpdate()
        {
            HandleInput();
            if (Anchor != null)
                transform.position = Anchor.position + EyeOffset;
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        void HandleInput()
        {
            // Pas de look-around si le pointer est sur un élément UI (évite de viser en
            // cliquant un bouton).
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                IsDragging = false;
                return;
            }

#if UNITY_IOS || UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);
                if (t.phase == UnityEngine.TouchPhase.Began)
                {
                    _lastPointer = t.position;
                    IsDragging = true;
                    DragTotalDelta = Vector2.zero;
                    _dragStarted = false;
                }
                else if (t.phase == UnityEngine.TouchPhase.Moved && IsDragging)
                {
                    Vector2 delta = t.position - _lastPointer;
                    ApplyLookDelta(delta);
                    DragTotalDelta += delta;
                    _lastPointer = t.position;
                    _dragStarted = true;
                }
                else if (t.phase == UnityEngine.TouchPhase.Ended || t.phase == UnityEngine.TouchPhase.Canceled)
                {
                    IsDragging = false;
                }
            }
#else
            if (Input.GetMouseButtonDown(0))
            {
                IsDragging = true;
                _lastPointer = Input.mousePosition;
                DragTotalDelta = Vector2.zero;
                _dragStarted = false;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                IsDragging = false;
            }
            else if (IsDragging && Input.GetMouseButton(0))
            {
                Vector2 cur = Input.mousePosition;
                Vector2 delta = cur - _lastPointer;
                ApplyLookDelta(delta);
                DragTotalDelta += new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
                _lastPointer = cur;
                if (DragTotalDelta.sqrMagnitude > 100f) _dragStarted = true;
            }
#endif
        }

        void ApplyLookDelta(Vector2 delta)
        {
            _yaw += delta.x * LookSensitivity;
            _pitch -= delta.y * LookSensitivity;
            _pitch = Mathf.Clamp(_pitch, PitchMin, PitchMax);
        }

        /// Retourne true si le drag total est assez grand pour être considéré un look,
        /// pas un tap. GameSceneController appelle ça pour décider de faire raycast ou non.
        public bool WasDrag() => _dragStarted;
    }
}
