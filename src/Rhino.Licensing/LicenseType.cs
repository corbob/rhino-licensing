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

        // Removed Standard and Personal. Professional needs to continue on with index of 4.
        /// <summary>
        /// Professional license (subscription)
        /// </summary>
        Professional = 4,

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
    }
}