using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LaVeillee.Core
{
    /// Fade overlay + scene loading + panel switching.
    ///
    /// Stratégie de navigation hybride :
    ///   - Entre écrans "statiques" (Home/Create/Join) → panel switch (instant, < 500ms facile).
    ///   - Entre contextes (Lobby / Game) → scene load avec fade (isole le NetworkRunner).
    ///
    /// Persiste via DontDestroyOnLoad pour que le fade reste visible pendant un changement de scène.
    [DefaultExecutionOrder(-1000)]
    public class NavigationService : MonoBehaviour
    {
        public static NavigationService Instance { get; private set; }

        readonly List<IScreen> _registeredScreens = new();
        IScreen _currentScreen;

        Canvas _fadeCanvas;
        Image _fadeImage;

        public static NavigationService EnsureExists()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("[NavigationService]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<NavigationService>();
            Instance.SetupFadeOverlay();
            return Instance;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupFadeOverlay();
        }

        void SetupFadeOverlay()
        {
            if (_fadeCanvas != null) return;
            var go = new GameObject("FadeCanvas", typeof(Canvas), typeof(CanvasScaler));
            go.transform.SetParent(transform, false);
            _fadeCanvas = go.GetComponent<Canvas>();
            _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _fadeCanvas.sortingOrder = 10000; // always on top

            var imgGo = new GameObject("FadeImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imgGo.transform.SetParent(go.transform, false);
            var rt = imgGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _fadeImage = imgGo.GetComponent<Image>();
            _fadeImage.color = new Color(0f, 0f, 0f, 0f); // transparent by default
            _fadeImage.raycastTarget = false;
        }

        public void Register(IScreen screen)
        {
            if (!_registeredScreens.Contains(screen))
                _registeredScreens.Add(screen);
        }

        public void Unregister(IScreen screen) => _registeredScreens.Remove(screen);

        /// Switch vers un écran enregistré dans la scène courante (pas de scene load).
        public void Show<T>() where T : IScreen
        {
            IScreen target = null;
            foreach (var s in _registeredScreens)
            {
                if (s is T) { target = s; break; }
            }
            if (target == null)
            {
                Debug.LogError($"[Nav] Screen of type {typeof(T).Name} not registered");
                return;
            }
            if (_currentScreen == target) return;
            _currentScreen?.OnHide();
            _currentScreen = target;
            foreach (var s in _registeredScreens)
                s.SetVisible(s == target);
            target.OnShow();
        }

        /// Charge une scène en fade out→in. onLoaded est invoqué après la scene load,
        /// avant le fade in, pour permettre au caller d'initialiser son état.
        public void LoadScene(string sceneName, Action onLoaded = null)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName, onLoaded));
        }

        IEnumerator LoadSceneCoroutine(string sceneName, Action onLoaded)
        {
            yield return FadeTo(1f, DesignTokens_Motion_Slow());
            var op = SceneManager.LoadSceneAsync(sceneName);
            while (op != null && !op.isDone) yield return null;
            onLoaded?.Invoke();
            yield return FadeTo(0f, DesignTokens_Motion_Slow());
        }

        IEnumerator FadeTo(float targetAlpha, float duration)
        {
            float start = _fadeImage.color.a;
            float t = 0f;
            _fadeImage.raycastTarget = targetAlpha > 0.01f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                var a = Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(t / duration));
                _fadeImage.color = new Color(0f, 0f, 0f, a);
                yield return null;
            }
            _fadeImage.color = new Color(0f, 0f, 0f, targetAlpha);
            _fadeImage.raycastTarget = targetAlpha > 0.01f;
        }

        // Evite une dépendance au namespace UI depuis Core — duplique la constante motion.slow.
        static float DesignTokens_Motion_Slow() => 0.5f;
    }

    /// Contrat d'un écran enregistré auprès du NavigationService.
    public interface IScreen
    {
        void SetVisible(bool visible);
        void OnShow();
        void OnHide();
    }
}
