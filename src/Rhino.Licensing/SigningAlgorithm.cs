namespace Rhino.Licensing
{
    /// <summary>
    /// The algorithm to use when signing licenses.
    /// </summary>
    public enum SigningAlgorithm
    {
        SHA1,

#if !NET40
        SHA256,

        SHA384,

        SHA512
#endif
    }
}
