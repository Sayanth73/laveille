#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace LaVeillee.EditorTools
{
    public static class iOSBuildPostProcessor
    {
        const string LocalNetworkDescription =
            "La Veillee utilise le reseau local pour synchroniser les telephones en mode Campfire.";
        const string BluetoothAlwaysDescription =
            "La Veillee utilise le Bluetooth pour connecter les telephones en mode Campfire (sans internet).";
        const string BluetoothPeripheralDescription =
            "La Veillee utilise le Bluetooth pour partager l'etat de la partie en mode Campfire.";
        const string MicrophoneDescription =
            "La Veillee a besoin du micro pour faire entendre ta voix aux autres joueurs (Maitre du Jeu et discussions de village).";

        [PostProcessBuild(45)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            plist.root.SetString("NSLocalNetworkUsageDescription", LocalNetworkDescription);
            plist.root.SetString("NSBluetoothAlwaysUsageDescription", BluetoothAlwaysDescription);
            plist.root.SetString("NSBluetoothPeripheralUsageDescription", BluetoothPeripheralDescription);
            plist.root.SetString("NSMicrophoneUsageDescription", MicrophoneDescription);

            plist.WriteToFile(plistPath);
            Debug.Log("[iOSBuildPostProcessor] LocalNetwork + Bluetooth + Microphone usage descriptions injected.");
        }
    }
}
#endif
