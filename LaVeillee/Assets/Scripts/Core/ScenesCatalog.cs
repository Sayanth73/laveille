namespace LaVeillee.Core
{
    /// Noms de scènes centralisés — évite les typos et rend les refactors triviaux.
    /// Les scènes elles-mêmes sont générées par un script éditeur (Epic2SceneBootstrap)
    /// et doivent être référencées dans EditorBuildSettings pour être chargeables.
    public static class ScenesCatalog
    {
        // Epic 2 tient dans une seule scène : panel switching entre Home / Create / Join /
        // Lobby / Game (stub). Évite la complexité de NetworkSceneManager en Shared mode.
        // Split de scènes reporté à Épopée 3 si la taille le justifie.
        public const string Main = "Main";
    }
}
