using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Collections.Generic;

namespace Rhino.Licensing
{
    /// <summary>
    /// LicenseGenerator generates and signs license.
    /// </summary>
    public class LicenseGenerator
    {
        private readonly string privateKey;

        /// <summary>
        /// Creates a new instance of <see cref="LicenseGenerator"/>.
        /// </summary>
        /// <param name="privateKey">The private key of the product</param>
        public LicenseGenerator(string privateKey)
        {
            this.privateKey = privateKey;
        }

        /// <summary>
        /// Generates a new floating license.
        /// </summary>
        /// <param name="name">Name of the license holder</param>
        /// <param name="publicKey">The public key of the license server</param>
        /// <returns>The generated license XML string</returns>
        public string GenerateFloatingLicense(string name, string publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                RSAKeyExtensions.FromXmlString(rsa,privateKey);
                var doc = new XmlDocument();
                var license = doc.CreateElement("floating-license");
                doc.AppendChild(license);

                var publicKeyEl = doc.CreateElement("license-server-public-key");
                license.AppendChild(publicKeyEl);
                publicKeyEl.InnerText = publicKey;

                var nameEl = doc.CreateElement("name");
                license.AppendChild(nameEl);
                nameEl.InnerText = name;

                var signature = GetXmlDigitalSignature(doc, rsa);
                doc.FirstChild.AppendChild(doc.ImportNode(signature, true));

                var ms = new MemoryStream();
                var writer = XmlWriter.Create(ms, new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = Encoding.UTF8
                });
                doc.Save(writer);
                ms.Position = 0;
                return new StreamReader(ms).ReadToEnd();
            }
        }

        /// <summary>
        /// Generates a new license with no attributes using SHA1 as the signing algorithm.
        /// </summary>
        /// <param name="name">Name of the license holder</param>
        /// <param name="id">Id of the license holder</param>
        /// <param name="expirationDate">License expiry date</param>
        /// <param name="licenseType">Type of the license</param>
        /// <returns>The generated license XML string</returns>
        public string Generate(string name, Guid id, DateTime expirationDate, LicenseType licenseType)
        {
            return Generate(name, id, expirationDate, new Dictionary<string, string>(), licenseType);
        }

        /// <summary>
        /// Generates a new license using SHA1 as the signing algorithm.
        /// </summary>
        /// <param name="name">Name of the license holder</param>
        /// <param name="id">Id of the license holder</param>
        /// <param name="expirationDate">License expiry date</param>
        /// <param name="licenseType">Type of the license</param>
        /// <param name="attributes">Extra information stored as key/value in the license file</param>
        /// <returns>The generated license XML string</returns>
        public string Generate(string name, Guid id, DateTime expirationDate, IDictionary<string, string> attributes, LicenseType licenseType)
            => Generate(name, id, expirationDate, attributes, licenseType, SigningAlgorithm.SHA1);

        /// <summary>
        /// Generates a new license with no attributes using a specified signing <paramref name="algorithm"/>
        /// </summary>
        /// <param name="name">The name of the license holder</param>
        /// <param name="id">Id of the license holder</param>
        /// <param name="expirationDate">License expiry date</param>
        /// <param name="licenseType">Type of the license</param>
        /// <param name="algorithm">Signing algorithm to use for signing the XML</param>
        /// <returns>The generated license XML string</returns>
        public string Generate(string name, Guid id, DateTime expirationDate, LicenseType licenseType, SigningAlgorithm algorithm)
            => Generate(name, id, expirationDate, new Dictionary<string, string>(), licenseType, algorithm);

        /// <summary>
        /// Generates a new license using a specified signing <paramref name="algorithm"/>
        /// </summary>
        /// <param name="name">The name of the license holder</param>
        /// <param name="id">Id of the license holder</param>
        /// <param name="expirationDate">License expiry date</param>
        /// <param name="attributes">Extra information stored as key/value in the license file</param>
        /// <param name="licenseType">Type of the license</param>
        /// <param name="algorithm">Signing algorithm to use for signing the XML</param>
        /// <returns>The generated license string</returns>
        public string Generate(string name, Guid id, DateTime expirationDate, IDictionary<string, string> attributes, LicenseType licenseType, SigningAlgorithm algorithm)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                RSAKeyExtensions.FromXmlString(rsa, privateKey);
                var doc = CreateDocument(id, name, expirationDate, attributes, licenseType);

                var signature = GetXmlDigitalSignature(doc, rsa, algorithm);
                doc.FirstChild.AppendChild(doc.ImportNode(signature, true));

                var ms = new MemoryStream();
                var writer = XmlWriter.Create(ms, new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = Encoding.UTF8
                });
                doc.Save(writer);
                ms.Position = 0;
                return new StreamReader(ms).ReadToEnd();
            }
        }

        private static string GetDigestSigningMethod(SigningAlgorithm algorithm)
            => algorithm switch
            {
                SigningAlgorithm.SHA1 => SignedXml.XmlDsigSHA1Url,
#if !NET40
                SigningAlgorithm.SHA256 => SignedXml.XmlDsigSHA256Url,
                SigningAlgorithm.SHA384 => SignedXml.XmlDsigSHA384Url,
                SigningAlgorithm.SHA512 => SignedXml.XmlDsigSHA512Url,
#endif
                _ => throw new ArgumentException(nameof(algorithm))
            };

        private static string GetRsaSigningMethod(SigningAlgorithm algorithm)
            => algorithm switch
            {
                SigningAlgorithm.SHA1 => SignedXml.XmlDsigRSASHA1Url,
#if !NET40
                SigningAlgorithm.SHA256 => SignedXml.XmlDsigRSASHA256Url,
                SigningAlgorithm.SHA384 => SignedXml.XmlDsigRSASHA384Url,
                SigningAlgorithm.SHA512 => SignedXml.XmlDsigRSASHA512Url,
#endif
                _ => throw new ArgumentException(nameof(algorithm))
            };

        private static XmlElement GetXmlDigitalSignature(XmlDocument x, AsymmetricAlgorithm key, SigningAlgorithm algorithm = SigningAlgorithm.SHA1)
        {
            var signedXml = new SignedXml(x) { SigningKey = key };
            var reference = new Reference { Uri = "" };

            reference.DigestMethod = GetDigestSigningMethod(algorithm);
            signedXml.SignedInfo.SignatureMethod = GetRsaSigningMethod(algorithm);

            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);
            signedXml.ComputeSignature();
            return signedXml.GetXml();
        }

        private static XmlDocument CreateDocument(Guid id, string name, DateTime expirationDate,  IDictionary<string,string> attributes, LicenseType licenseType)
        {
            var doc = new XmlDocument();
            var license = doc.CreateElement("license");
            doc.AppendChild(license);
            var idAttr = doc.CreateAttribute("id");
            license.Attributes.Append(idAttr);
            idAttr.Value = id.ToString();

            var expirDateAttr = doc.CreateAttribute("expiration");
            license.Attributes.Append(expirDateAttr);
            expirDateAttr.Value = expirationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);

            var licenseAttr = doc.CreateAttribute("type");
            license.Attributes.Append(licenseAttr);
            licenseAttr.Value = licenseType.ToString();

            var nameEl = doc.CreateElement("name");
            license.AppendChild(nameEl);
            nameEl.InnerText = name;

            foreach (var attribute in attributes)
            {
                var attrib = doc.CreateAttribute(attribute.Key);
                attrib.Value = attribute.Value;
                license.Attributes.Append(attrib);
            }

            return doc;
        }
    }
}
