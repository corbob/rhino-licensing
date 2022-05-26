using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rhino.Licensing.Tests
{
    public class BaseLicenseTest
    {
        protected static readonly Dictionary<string, string> Resources = new Dictionary<string, string>();
        protected static readonly string PublicAndPrivateKey;
        protected static readonly string PublicOnlyKey;
        protected static readonly Guid TestGuid = new Guid("0c6b2abc-5ba1-4f1c-af11-8c3a01147124");
        protected static readonly DateTime Expiration = DateTime.MaxValue;
        protected static readonly string CustomerName = "Oren Eini";

        static BaseLicenseTest()
        {
            foreach (var resourceName in typeof(BaseLicenseTest).Assembly.GetManifestResourceNames().Where(name => name.EndsWith(".xml")))
            {
                var resourceNameSplit = resourceName.Split('.');
                var resourceNameLength = resourceNameSplit.Length;
                Resources.Add($"{resourceNameSplit[resourceNameLength - 3]}.{resourceNameSplit[resourceNameLength - 2]}", ReadResourceFile(resourceName));
            }

            PublicAndPrivateKey = Resources["SigningKeys.PublicAndPrivate"];
            PublicOnlyKey = Resources["SigningKeys.PublicOnly"];
        }

        private static string ReadResourceFile (string resourceName)
        {
            var stream = typeof(LicenseGeneratorTests)
                .Assembly
                .GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                throw new Exception($"Something really bad happened. We couldn't load {resourceName}");
            }

            return new StreamReader(stream).ReadToEnd();
        }

        protected static string WriteLicenseFile(string resourceName)
        {
            var path = $"{Path.GetTempPath()}\\{resourceName}.xml";
            File.WriteAllText(path, Resources[resourceName]);
            return path;
        }

        public static IEnumerable<object[]> GetAllLicenseTypes()
        {
            foreach (var licenseType in Enum.GetValues(typeof(LicenseType)))
            {
                yield return new object[] { licenseType };
            }
        }

        public static IEnumerable<object[]> GetAllLicenseTypesWithSigningAlgorithms()
        {
            foreach (var licenseType in Enum.GetValues(typeof(LicenseType)))
            {
                foreach (var signingAlgorithm in Enum.GetValues(typeof(SigningAlgorithm)))
                {
                    yield return new object[] { licenseType, signingAlgorithm };
                }
            }
        }
    }
}