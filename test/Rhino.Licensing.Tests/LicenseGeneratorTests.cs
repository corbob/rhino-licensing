using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Rhino.Licensing.Tests
{
    public class LicenseGeneratorTests : BaseLicenseTest
    {
        private static readonly LicenseGenerator Generator = new LicenseGenerator(PublicAndPrivateKey);

        [Theory]
        [MemberData(nameof(GetAllLicenseTypes))]
        public void CanGenerateLicenseType(LicenseType licenseType)
        {
            var key = Generator.Generate(CustomerName, TestGuid, Expiration, licenseType);

            Assert.Equal(Resources[$"LicenseFiles.{licenseType}"], key);
        }

        [Theory]
        [MemberData(nameof(GetAllLicenseTypes))]
        public void CanGenerateLicenseTypeWithAttributes(LicenseType licenseType)
        {
            var key = Generator.Generate(CustomerName, TestGuid, Expiration,
                                         new Dictionary<string, string>
                                         {
                                            {"prof", "llb"},
                                            {"reporting", "on"}
                                         }, licenseType);

            Assert.Equal(Resources[$"LicenseFiles.{licenseType}-WithAttributes"], key);
        }

        [Theory]
        [MemberData(nameof(GetAllLicenseTypesWithSigningAlgorithms))]
        public void CanGenerateLicenseTypeUsingSigningAlgorithm(LicenseType licenseType, SigningAlgorithm signingAlgorithm)
        {
            var key = Generator.Generate(CustomerName, TestGuid, Expiration, licenseType, signingAlgorithm);

            Assert.Equal(Resources[$"LicenseFiles.{licenseType}-{signingAlgorithm}"], key);
        }
    }
}
