using UnityEngine;

namespace LaVeillee.UI
{
    /// Design tokens — palette, spacing, radius, shadows, motion.
    /// Source of truth : _bmad-output/planning-artifacts/ux-design-specification.md §5.2
    /// Direction A "Veillée intime" (retenue). Pure C# static — no ScriptableObject
    /// to keep things simple and dependency-free. Custom fonts (Manrope/Cormorant)
    /// are deferred: TMP's default LiberationSans ships out-of-the-box.
    public static class DesignTokens
    {
        public static class Colors
        {
            // Fire — orange chaud, CTAs primaires, feu
            public static readonly Color Fire500 = Hex(0xE87B2F);
            public static readonly Color Fire700 = Hex(0xA8501A); // hover/pressed
            public static readonly Color Fire300 = Hex(0xF5B380); // highlights

            // Night — bleu nuit profond, fonds
            public static readonly Color Night900 = Hex(0x0B1224);
            public static readonly Color Night700 = Hex(0x1A2440); // overlay cards
            public static readonly Color Night500 = Hex(0x2E3A5F); // borders, dividers

            // Moon — blanc lune, texte
            public static readonly Color Moon100 = Hex(0xF4F1E8); // principal
            public static readonly Color Moon300 = Hex(0xC9C0AC); // secondaire

            // Fog — gris brume, états inactifs
            public static readonly Color Fog500 = Hex(0x6B6B7A);

            // Accents contextuels
            public static readonly Color Forest700 = Hex(0x1C3D2E); // mode Campfire
            public static readonly Color Blood500  = Hex(0x9B1F2D); // alertes/mort
            public static readonly Color Gold500   = Hex(0xD4A436); // Maire
            public static readonly Color Crystal500= Hex(0x5BA9CF); // Voyante
            public static readonly Color Poison500 = Hex(0x7A4FB5); // Sorcière

            static Color Hex(int rgb)
            {
                byte r = (byte)((rgb >> 16) & 0xFF);
                byte g = (byte)((rgb >> 8) & 0xFF);
                byte b = (byte)(rgb & 0xFF);
                return new Color32(r, g, b, 0xFF);
            }

            /// Couleurs d'avatar placeholder en attendant les avatars custom.
            /// Couvrent un spectre visuellement distinct pour jusqu'à 12 joueurs
            /// — au-delà on boucle (acceptable, lobby affiche surtout le pseudo).
            public static readonly Color[] AvatarPalette =
            {
                Hex(0xE87B2F), // fire
                Hex(0x5BA9CF), // crystal
                Hex(0x7A4FB5), // poison
                Hex(0xD4A436), // gold
                Hex(0x1C3D2E), // forest
                Hex(0x9B1F2D), // blood
                Hex(0xA8501A), // fire700
                Hex(0x2E8B57), // seagreen
                Hex(0x4682B4), // steelblue
                Hex(0xCD853F), // peru
                Hex(0x6A5ACD), // slateblue
                Hex(0xB8860B), // darkgoldenrod
            };

            public static Color AvatarFor(int seed)
            {
                int i = (seed % AvatarPalette.Length + AvatarPalette.Length) % AvatarPalette.Length;
                return AvatarPalette[i];
            }
        }

        public static class Spacing
        {
            public const float Xs  = 4f;
            public const float Sm  = 8f;
            public const float Md  = 16f;
            public const float Lg  = 24f;
            public const float Xl  = 32f;
            public const float Xxl = 48f;
        }

        public static class Radius
        {
            public const float Sm   = 6f;
            public const float Md   = 12f;
            public const float Lg   = 20f;
            public const float Full = 999f;
        }

        public static class Text
        {
            // Tailles — doivent matcher §5.2 du UX spec
            public const float Display = 36f;
            public const float H1      = 28f;
            public const float H2      = 22f;
            public const float H3      = 18f;
            public const float Body    = 16f;
            public const float Caption = 13f;
            public const float Label   = 12f;
        }

        public static class Motion
        {
            public const float Fast     = 0.15f;
            public const float Medium   = 0.30f;
            public const float Slow     = 0.50f;
            public const float Dramatic = 1.50f;
        }

        public static class Layout
        {
            // iPhone safe margins (iPad overrides géré ailleurs)
            public const float ScreenMarginX = 16f;
            public const float ScreenMarginHero = 24f;
            public const float HomeIndicatorBottom = 34f; // Apple HIG
        }
    }
}
