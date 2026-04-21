using UnityEngine;

namespace LaVeillee.Core
{
    /// Capture les deeplinks `laveillee://join/{code}` ou `https://laveillee.app/join/{code}`
    /// et expose le code extrait au reste du code. URL scheme iOS déclaré dans Info.plist
    /// via iOSBuildPostProcessor (CFBundleURLTypes).
    ///
    /// Unity invoke `Application.deepLinkActivated` quand l'app est ouverte par un deeplink —
    /// en cold-start l'URL est dans `Application.absoluteURL` au démarrage.
    public class DeeplinkHandler : MonoBehaviour
    {
        public static string PendingCode { get; private set; }

        public static DeeplinkHandler EnsureExists()
        {
            var existing = FindFirstObjectByType<DeeplinkHandler>();
            if (existing != null) return existing;
            var go = new GameObject("[DeeplinkHandler]");
            DontDestroyOnLoad(go);
            return go.AddComponent<DeeplinkHandler>();
        }

        public static void ConsumePending() => PendingCode = null;

        void Awake()
        {
            Application.deepLinkActivated += OnDeeplinkActivated;
            if (!string.IsNullOrEmpty(Application.absoluteURL))
                OnDeeplinkActivated(Application.absoluteURL);
        }

        void OnDestroy()
        {
            Application.deepLinkActivated -= OnDeeplinkActivated;
        }

        void OnDeeplinkActivated(string url)
        {
            Debug.Log($"[Deeplink] Reçu : {url}");
            var code = ExtractCode(url);
            if (string.IsNullOrEmpty(code)) return;
            PendingCode = code;
            // Si on est déjà sur JoinRoomScreen, on l'affiche. Sinon le prochain OnShow le consommera.
            NavigationService.EnsureExists();
            // Pas de Show<JoinRoomScreen>() ici : la scène peut ne pas encore avoir enregistré
            // ses écrans. JoinRoomScreen.OnShow() lira PendingCode quand navigué manuellement.
        }

        /// Extrait le code 6 chiffres d'une URL :
        ///   - laveillee://join/123456
        ///   - https://laveillee.app/join/123456
        ///   - https://laveillee.app/join/?code=123456
        static string ExtractCode(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            // Find "/join/" segment
            var key = "/join/";
            var idx = url.IndexOf(key, System.StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            var tail = url.Substring(idx + key.Length);
            // strip query string / trailing slash
            var q = tail.IndexOf('?');
            if (q >= 0) tail = tail.Substring(0, q);
            tail = tail.TrimEnd('/');

            // If tail contains only digits, good
            var digits = new System.Text.StringBuilder();
            foreach (var c in tail)
            {
                if (char.IsDigit(c)) digits.Append(c);
                if (digits.Length == 6) break;
            }
            return digits.Length == 6 ? digits.ToString() : null;
        }
    }
}
