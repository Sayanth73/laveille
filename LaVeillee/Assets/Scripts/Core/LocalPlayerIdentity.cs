using System;
using UnityEngine;

namespace LaVeillee.Core
{
    /// Mock persistant de l'identité joueur — généré à la première utilisation,
    /// stocké dans PlayerPrefs. À remplacer par un provider basé sur Sign in with
    /// Apple quand Story 1.2 débloque.
    public sealed class LocalPlayerIdentity : IPlayerIdentity
    {
        const string PrefUserId = "laveille.userId";
        const string PrefPseudo = "laveille.pseudo";
        const string PrefSeed   = "laveille.avatarSeed";

        public string UserId { get; }
        public int AvatarColorSeed { get; }
        public bool IsMocked => true;

        string _pseudo;
        public string Pseudo
        {
            get => _pseudo;
            set
            {
                var clean = Sanitize(value);
                if (clean == _pseudo) return;
                _pseudo = clean;
                PlayerPrefs.SetString(PrefPseudo, clean);
                PlayerPrefs.Save();
            }
        }

        public LocalPlayerIdentity()
        {
            if (!PlayerPrefs.HasKey(PrefUserId))
            {
                PlayerPrefs.SetString(PrefUserId, Guid.NewGuid().ToString("N"));
                PlayerPrefs.SetString(PrefPseudo, GeneratePseudo());
                PlayerPrefs.SetInt(PrefSeed, UnityEngine.Random.Range(0, 10000));
                PlayerPrefs.Save();
            }
            UserId = PlayerPrefs.GetString(PrefUserId);
            _pseudo = PlayerPrefs.GetString(PrefPseudo);
            AvatarColorSeed = PlayerPrefs.GetInt(PrefSeed);
        }

        static string GeneratePseudo()
        {
            // Format "Joueur-XXXX" — 4 chars alphanum faciles à lire (pas 0/O, 1/I).
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            Span<char> code = stackalloc char[4];
            var bytes = Guid.NewGuid().ToByteArray();
            for (int i = 0; i < 4; i++) code[i] = alphabet[bytes[i] % alphabet.Length];
            return $"Joueur-{new string(code)}";
        }

        static string Sanitize(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Joueur";
            var trimmed = raw.Trim();
            return trimmed.Length > 20 ? trimmed.Substring(0, 20) : trimmed;
        }
    }
}
