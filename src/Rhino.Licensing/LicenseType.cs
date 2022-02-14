namespace Rhino.Licensing
{
    /// <summary>
    /// License Type
    /// </summary>
    public enum LicenseType
    {
        /// <summary>
        /// No type specified
        /// </summary>
        None,

        /// <summary>
        /// For trial use
        /// </summary>
        Trial,

        /// <summary>
        /// Standard license
        /// </summary>
        Standard,

        /// <summary>
        /// For personal use
        /// </summary>
        Personal,

        /// <summary>
        /// Professional license (subscription)
        /// </summary>
        Professional,

        /// <summary>
        /// Architect license (subscription)
        /// </summary>
        Architect,

        /// <summary>
        /// MSP license (subscription)
        /// </summary>
        ManagedServiceProvider,

        /// <summary>
        /// Educational license (subscription)
        /// </summary>
        Education,

        /// <summary>
        /// Business license (subscription)
        /// </summary>
        Business,

        /// <summary>
        /// Enterprise license (subscription)
        /// </summary>
        Enterprise,

        /// <summary>
        /// Floating license
        /// </summary>
        Floating,

        /// <summary>
        /// Subscription based license
        /// </summary>
        Subscription,
    }
}