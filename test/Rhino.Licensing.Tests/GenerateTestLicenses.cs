using System;
using System.IO;
using Xunit;

namespace Rhino.Licensing.Tests
{
    public class GenerateTestLicenses : BaseLicenseTest
    {
        private static readonly LicenseGenerator Generator = new LicenseGenerator(PublicAndPrivateKey);

        [GenerateTestLicensesTheory]
        [MemberData(nameof(GetAllLicenseTypesWithSigningAlgorithms))]
        public void GeneratesTestLicenses(LicenseType licenseType, SigningAlgorithm signingAlgorithm)
        {
            var path = Environment.GetEnvironmentVariable("GENERATE_TEST_LICENSES");

            // Never expiring, specifically signed licenses
            var key = Generator.Generate(CustomerName, TestGuid, Expiration, licenseType, signingAlgorithm);
            File.WriteAllText($"{path}\\{licenseType}-{signingAlgorithm}.xml", key);

            // Never expiring, default (SHA1) signed licenses
            key = Generator.Generate(CustomerName, TestGuid, Expiration, licenseType);
            File.WriteAllText($"{path}\\{licenseType}.xml", key);

            // Expired, specifically signed licenses
            key = Generator.Generate(CustomerName, TestGuid, DateTime.Parse("12-12-2013 16:47:34 -0600"), licenseType, signingAlgorithm);
            File.WriteAllText($"{path}\\{licenseType}-{signingAlgorithm}-Expired.xml", key);

        }
    }
}