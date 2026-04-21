using LaVeillee.Core;
using UnityEngine;

namespace LaVeillee.UI
{
    /// Base d'un écran (panel) : gère visibilité via activeSelf du GameObject root.
    /// Sous-classe doit construire son UI dans BuildUI() (appelé en Awake).
    public abstract class ScreenBase : MonoBehaviour, IScreen
    {
        protected RectTransform Root;
        bool _builtUI;

        protected virtual void Awake()
        {
            if (!_builtUI)
            {
                BuildUI();
                _builtUI = true;
            }
            NavigationService.EnsureExists().Register(this);
            SetVisible(false);
        }

        protected virtual void OnDestroy()
        {
            if (NavigationService.Instance != null)
                NavigationService.Instance.Unregister(this);
        }

        protected abstract void BuildUI();

        public virtual void SetVisible(bool visible)
        {
            if (Root != null) Root.gameObject.SetActive(visible);
        }

        public virtual void OnShow() { }
        public virtual void OnHide() { }
    }
}
