using System.ServiceModel;

namespace Rhino.Licensing
{
    /// <summary>
    /// Service contract of subscription server.
    /// </summary>
    [ServiceContract]
    public interface ISubscriptionLicensingService
    {
        /// <summary>
        /// Issues an updated subscription license
        /// </summary>
        /// <param name="previousLicense">The current or previous license.</param>
        /// <param name="passcode">optional passcode for server license leasing</param>
        /// <returns></returns>
        [OperationContract]
        string LeaseLicense(string previousLicense, string passcode);
    }
}