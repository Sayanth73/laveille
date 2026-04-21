using LaVeillee.Networking;
using Photon.Voice.Fusion;
using Photon.Voice.Unity;
using UnityEngine;

namespace LaVeillee.Core
{
    /// Singleton persistant (DontDestroyOnLoad) qui porte les composants réseau +
    /// vocal. Créé au boot, survit aux changements de scène (Home → Lobby → Game).
    ///
    /// Pattern repris de DevTestRoomController — extrait ici pour que tout écran
    /// puisse accéder au RoomManager / VoiceManager via un accesseur stable.
    public class GameServices : MonoBehaviour
    {
        public static GameServices Instance { get; private set; }

        public FusionRoomManager Room { get; private set; }
        public PhotonVoiceManager Voice { get; private set; }

        public static GameServices EnsureExists()
        {
            if (Instance != null) return Instance;

            var go = new GameObject("[GameServices]");
            DontDestroyOnLoad(go);
            var services = go.AddComponent<GameServices>();

            // Order matters : Voice d'abord car RequireComponent tire Recorder + FusionVoiceClient
            // + NetworkRunner. Puis Room réutilise le NetworkRunner existant.
            services.Voice = go.AddComponent<PhotonVoiceManager>();
            services.Room = go.AddComponent<FusionRoomManager>();

            // SpeakerPrefab : template actif pour que les clones inherit activeSelf = true
            // (sinon "Can not play a disabled audio source"). Le template lui-même ne
            // joue rien — il n'a pas de VoiceNetworkObject lié.
            var voiceClient = go.GetComponent<FusionVoiceClient>();
            if (voiceClient.SpeakerPrefab == null)
            {
                var speakerTemplate = new GameObject("SpeakerTemplate", typeof(AudioSource), typeof(Speaker));
                speakerTemplate.transform.SetParent(go.transform, false);
                voiceClient.SpeakerPrefab = speakerTemplate;
            }

            Instance = services;
            return services;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }
    }
}
