namespace LaVeillee.Core
{
    /// Identité locale du joueur. Abstraction au-dessus de la source d'auth pour
    /// que le reste du code ne soit pas couplé à Sign in with Apple (Story 1.2 bloquée
    /// sur l'ADP). L'implémentation actuelle est un mock local persisté en PlayerPrefs ;
    /// swap en place le jour où 1.2 débloque — aucun changement ailleurs.
    public interface IPlayerIdentity
    {
        string UserId { get; }      // stable entre sessions
        string Pseudo { get; set; } // modifiable par le joueur
        int AvatarColorSeed { get; }
        bool IsMocked { get; }
    }
}
