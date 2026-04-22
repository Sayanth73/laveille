using LaVeillee.UI;
using UnityEngine;

namespace LaVeillee.Core
{
    /// Entry point de la scène Main — initialise services, construit les écrans,
    /// affiche Home (ou JoinRoom si deeplink pending).
    ///
    /// Placé manuellement via Tools → La Veillée → Bootstrap Main Scene (script éditeur),
    /// ou directement en attachant ce script à un GameObject vide dans la scène.
    public class AppBootstrap : MonoBehaviour
    {
        void Awake()
        {
#if UNITY_STANDALONE
            Screen.SetResolution(540, 960, FullScreenMode.Windowed);
#endif

            DeeplinkHandler.EnsureExists();
            NavigationService.EnsureExists();
            GameServices.EnsureExists();

            // Force l'init de l'identité locale (lazy → évalue au moindre accès).
            _ = PlayerIdentityService.Current;

            BuildScreens();

            // Route initiale : deeplink > Home
            if (!string.IsNullOrEmpty(DeeplinkHandler.PendingCode))
                NavigationService.Instance.Show<JoinRoomScreen>();
            else
                NavigationService.Instance.Show<HomeScreen>();
        }

        static void BuildScreens()
        {
            var home = new GameObject("HomeScreen").AddComponent<HomeScreen>();
            var create = new GameObject("CreateRoomScreen").AddComponent<CreateRoomScreen>();
            var join = new GameObject("JoinRoomScreen").AddComponent<JoinRoomScreen>();
            var lobby = new GameObject("LobbyScreen").AddComponent<LobbyScreen>();
            var game = new GameObject("GameScreen").AddComponent<GameScreen>();
            var solo = new GameObject("DevSoloScreen").AddComponent<DevSoloScreen>();

            // Parent sous un conteneur pour hiérarchie propre
            var root = new GameObject("[Screens]");
            home.transform.SetParent(root.transform, false);
            create.transform.SetParent(root.transform, false);
            join.transform.SetParent(root.transform, false);
            lobby.transform.SetParent(root.transform, false);
            game.transform.SetParent(root.transform, false);
            solo.transform.SetParent(root.transform, false);
        }
    }
}
