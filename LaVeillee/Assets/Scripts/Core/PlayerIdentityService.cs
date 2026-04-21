namespace LaVeillee.Core
{
    /// Accesseur statique de l'identité locale. Lazy-init — l'instance est créée au
    /// premier accès, sans dépendance à un MonoBehaviour de scène. Thread-safe via
    /// la garantie du CLR sur l'init de champ static readonly (local runtime Unity =
    /// single-thread sur main, donc suffisant).
    public static class PlayerIdentityService
    {
        static IPlayerIdentity _current;

        public static IPlayerIdentity Current
        {
            get
            {
                _current ??= new LocalPlayerIdentity();
                return _current;
            }
        }

        /// Hook de test pour substituer une implémentation (p.ex. un stub déterministe).
        public static void Override(IPlayerIdentity identity) => _current = identity;
    }
}
