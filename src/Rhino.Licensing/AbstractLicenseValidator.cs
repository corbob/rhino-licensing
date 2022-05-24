using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.Threading;
using System.Xml;
using Rhino.Licensing.Logging;

namespace Rhino.Licensing
{
    /// <summary>
    /// Base license validator.
    /// </summary>
    public abstract class AbstractLicenseValidator
    {
        /// <summary>
        /// License validator logger
        /// </summary>
        private static readonly ILog Log = LogProvider.GetLogger(typeof(AbstractLicenseValidator));

        /// <summary>
        /// Standard Time servers
        /// </summary>
        protected static readonly string[] TimeServers =
        {
            "time.nist.gov",
            "time-nw.nist.gov",
            "time-a.nist.gov",
            "time-b.nist.gov",
            "time-a.timefreq.bldrdoc.gov",
            "time-b.timefreq.bldrdoc.gov",
            "time-c.timefreq.bldrdoc.gov",
            "utcnist.colorado.edu",
            "nist1.datum.com",
            "nist1.dc.certifiedtime.com",
            "nist1.nyc.certifiedtime.com",
            "nist1.sjc.certifiedtime.com"
        };

        private readonly string publicKey;

        /// <summary>
        /// Fired when license is expired
        /// </summary>
        public event Action<DateTime> LicenseExpired;

        /// <summary>
        /// Gets the expiration date of the license
        /// </summary>
        public DateTime ExpirationDate { get; private set; }

        /// <summary>
        /// Gets the Type of the license
        /// </summary>
        public LicenseType LicenseType { get; private set; }

        /// <summary>
        /// Gets the Id of the license holder
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Gets the name of the license holder
        /// </summary>
        public string Name { get; private set; }

        public IDictionary<string, string> LicenseAttributes { get; private set; }
        /// <summary>
        /// Gets or Sets the license content
        /// </summary>
        protected abstract string License { get; set; }

        /// <summary>
        /// Creates a license validator with specfied public key.
        /// </summary>
        /// <param name="publicKey">public key</param>
        protected AbstractLicenseValidator(string publicKey)
        {
            LicenseAttributes = new Dictionary<string, string>();
            this.publicKey = publicKey;
        }

        /// <summary>
        /// Validates loaded license
        /// </summary>
        public virtual void AssertValidLicense()
        {
            LicenseAttributes.Clear();
            if (HasExistingLicense())
            {
                return;
            }

            Log.WarnFormat("[Licensing] Could not validate existing license\r\n{0}", License);
            throw new LicenseNotFoundException("Could not validate existing license");
        }

        private bool HasExistingLicense()
        {
            try
            {
                if (TryLoadingLicenseValuesFromValidatedXml() == false)
                {
                    Log.WarnFormat("[Licensing] Failed validating license:\r\n{0}", License);
                    return false;
                }
                Log.DebugFormat("[Licensing] License expiration date is {0}", ExpirationDate);

                var result = DateTime.UtcNow < ExpirationDate;

                if (result)
                {
                    ValidateUsingNetworkTime();
                }
                else
                {
                    if (LicenseExpired == null) throw new LicenseExpiredException("Expiration Date : " + ExpirationDate);

                    LicenseExpired(ExpirationDate);
                }

                return true;
            }
            catch (RhinoLicensingException)
            {
                throw;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ValidateUsingNetworkTime()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return;

            if (LicenseType == LicenseType.Business
                || LicenseType == LicenseType.Architect
                || LicenseType == LicenseType.Education
                || LicenseType == LicenseType.Enterprise
                || LicenseType == LicenseType.Trial
                )
            {
                // Many organizations have blocked NTP traffic, so this
                // check creates additional noise that is cause for
                // concern on enterprise security teams.
                // Since the traffic is already blocked,
                // this check would not produce a desired result
                // anyway.
                return;
            }

            var sntp = new SntpClient(GetTimeServers());
            sntp.BeginGetDate(time =>
            {
                // TODO: RaiseLicenseInvalidated only ever ended up in throwing this exception. We should make it a better exception
                if (time > ExpirationDate)
                    throw new InvalidOperationException("License was invalidated, but there is no one subscribe to the LicenseInvalidated event");
            },
            () =>
            {
                /* ignored */
            });
        }

        /// <summary>
        /// Extension point to return different time servers
        /// </summary>
        /// <returns></returns>
        protected virtual string[] GetTimeServers()
        {
            return TimeServers;
        }

        /// <summary>
        /// Loads license data from validated license file.
        /// </summary>
        /// <returns></returns>
        public bool TryLoadingLicenseValuesFromValidatedXml()
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(License);

                if (TryGetValidDocument(publicKey, doc) == false)
                {
                    Log.WarnFormat("[Licensing] Could not validate xml signature of:\r\n{0}", License);
                    return false;
                }

                if (doc.FirstChild == null)
                {
                    Log.WarnFormat("[Licensing] Could not find first child of:\r\n{0}", License);
                    return false;
                }

                var result = ValidateXmlDocumentLicense(doc);

                return result;
            }
            catch (RhinoLicensingException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error("[Licensing] Could not validate license", e);
                return false;
            }
        }

        internal bool ValidateXmlDocumentLicense(XmlDocument doc)
        {
            var id = doc.SelectSingleNode("/license/@id");
            if (id == null)
            {
                Log.WarnFormat("[Licensing] Could not find id attribute in license:\r\n{0}", License);
                return false;
            }

            UserId = new Guid(id.Value);

            var date = doc.SelectSingleNode("/license/@expiration");
            if (date == null)
            {
                Log.WarnFormat("[Licensing] Could not find expiration in license:\r\n{0}", License);
                return false;
            }

            ExpirationDate = DateTime.ParseExact(date.Value, "yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);

            var licenseType = doc.SelectSingleNode("/license/@type");
            if (licenseType == null)
            {
                Log.WarnFormat("[Licensing] Could not find license type in {0}", licenseType);
                return false;
            }

            LicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), licenseType.Value);

            var name = doc.SelectSingleNode("/license/name/text()");
            if (name == null)
            {
                Log.WarnFormat("[Licensing] Could not find licensee's name in license:\r\n{0}", License);
                return false;
            }

            Name = name.Value;

            var license = doc.SelectSingleNode("/license");
            foreach (XmlAttribute attrib in license.Attributes)
            {
                if (attrib.Name == "type" || attrib.Name == "expiration" || attrib.Name == "id")
                    continue;

                LicenseAttributes[attrib.Name] = attrib.Value;
            }

            return true;
        }

        private bool TryGetValidDocument(string licensePublicKey, XmlDocument doc)
        {
            var rsa = new RSACryptoServiceProvider();
            RSAKeyExtensions.FromXmlString(rsa,licensePublicKey);

            var nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("sig", "http://www.w3.org/2000/09/xmldsig#");

            var signedXml = new SignedXml(doc);
            var sig = (XmlElement)doc.SelectSingleNode("//sig:Signature", nsMgr);
            if (sig == null)
            {
                Log.WarnFormat("[Licensing] Could not find this signature node on license:\r\n{0}", License);
                return false;
            }
            signedXml.LoadXml(sig);

            return signedXml.CheckSignature(rsa);
        }

        /// <summary>
        /// Disables further license checks for the session.
        /// </summary>
        [Obsolete("This function does nothing, it is left in to prevent errors.")]
        public void DisableFutureChecks()
        {
            // Do nothing
        }
    }
}
