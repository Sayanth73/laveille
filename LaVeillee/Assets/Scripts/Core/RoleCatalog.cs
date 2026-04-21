using System.Collections.Generic;

namespace LaVeillee.Core
{
    /// Catalogue des rôles disponibles en Epic 2. Les rôles effectivement jouables
    /// dépendent de l'Épopée 3+ ; on liste tous les rôles canoniques pour que la
    /// composition soit déjà persistable. Les IDs int sont stables — ne pas
    /// réordonner (sérialisés dans LobbyState.CompositionRoles).
    public enum RoleId
    {
        Villageois = 0,
        LoupGarou  = 1,
        Voyante    = 2,
        Sorciere   = 3,
        Chasseur   = 4,
        Cupidon    = 5,
        PetiteFille= 6,
        Salvateur  = 7,
        Maire      = 8, // élu en jeu, pas pré-assigné → exclu de la composition
        Ange       = 9,
    }

    public static class RoleCatalog
    {
        public static readonly RoleId[] SelectableRoles =
        {
            RoleId.Villageois,
            RoleId.LoupGarou,
            RoleId.Voyante,
            RoleId.Sorciere,
            RoleId.Chasseur,
            RoleId.Cupidon,
            RoleId.PetiteFille,
            RoleId.Salvateur,
            RoleId.Ange,
        };

        public static string LabelFor(RoleId r) => r switch
        {
            RoleId.Villageois  => "Villageois",
            RoleId.LoupGarou   => "Loup-Garou",
            RoleId.Voyante     => "Voyante",
            RoleId.Sorciere    => "Sorcière",
            RoleId.Chasseur    => "Chasseur",
            RoleId.Cupidon     => "Cupidon",
            RoleId.PetiteFille => "Petite Fille",
            RoleId.Salvateur   => "Salvateur",
            RoleId.Maire       => "Maire",
            RoleId.Ange        => "Ange",
            _                  => r.ToString(),
        };

        public static string EmojiFor(RoleId r) => r switch
        {
            RoleId.Villageois  => "🧑‍🌾",
            RoleId.LoupGarou   => "🐺",
            RoleId.Voyante     => "🔮",
            RoleId.Sorciere    => "🧪",
            RoleId.Chasseur    => "🎯",
            RoleId.Cupidon     => "💘",
            RoleId.PetiteFille => "👁️",
            RoleId.Salvateur   => "🛡️",
            RoleId.Maire       => "👑",
            RoleId.Ange        => "😇",
            _                  => "❔",
        };

        public static bool IsWolf(RoleId r) => r == RoleId.LoupGarou;

        /// Composition recommandée pour un nombre de joueurs donné (FR11 GDD simplifié).
        /// Retourne un dict RoleId → count tel que la somme = playerCount.
        public static Dictionary<RoleId, int> RecommendedFor(int playerCount)
        {
            var comp = new Dictionary<RoleId, int>();
            if (playerCount < 5) return comp;

            int wolves = playerCount switch
            {
                >= 5 and <= 6   => 1,
                >= 7 and <= 10  => 2,
                >= 11 and <= 14 => 3,
                _               => 4,
            };
            comp[RoleId.LoupGarou] = wolves;
            comp[RoleId.Voyante] = 1;
            if (playerCount >= 7)  comp[RoleId.Sorciere] = 1;
            if (playerCount >= 9)  comp[RoleId.Chasseur] = 1;
            if (playerCount >= 11) comp[RoleId.Cupidon] = 1;

            int used = 0;
            foreach (var kv in comp) used += kv.Value;
            int remaining = playerCount - used;
            if (remaining > 0) comp[RoleId.Villageois] = remaining;
            return comp;
        }

        public static bool IsValid(Dictionary<RoleId, int> comp, int playerCount, out string error)
        {
            int total = 0, wolves = 0, nonWolves = 0;
            foreach (var kv in comp)
            {
                total += kv.Value;
                if (IsWolf(kv.Key)) wolves += kv.Value;
                else nonWolves += kv.Value;
            }
            if (total != playerCount)
            {
                error = $"Total des rôles = {total}, il faut {playerCount}.";
                return false;
            }
            if (wolves < 1)
            {
                error = "Il faut au moins 1 Loup-Garou.";
                return false;
            }
            if (nonWolves < 1)
            {
                error = "Il faut au moins 1 rôle non-Loup.";
                return false;
            }
            error = "";
            return true;
        }
    }
}
