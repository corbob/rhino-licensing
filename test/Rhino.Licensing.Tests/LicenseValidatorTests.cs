using System.IO;
using Xunit;

namespace Rhino.Licensing.Tests
{
    public class LicenseValidatorTests : BaseLicenseTest
    {
        [Fact]
        public void ThrowsLicenseNotFoundExceptionOnEmptyLicenseFile()
        {
            var validator = new LicenseValidator(PublicOnlyKey, Path.GetTempFileName());
            Assert.Throws<LicenseNotFoundException>(() => validator.AssertValidLicense());
        }


        [Fact]
        public void ThrowsLicenseFileNotFoundExceptionOnMissingLicenseFile()
        {
            var validator = new LicenseValidator(PublicOnlyKey, "not_there");
            Assert.Throws<LicenseFileNotFoundException>(() => validator.AssertValidLicense());
        }

        [Theory]
        [MemberData(nameof(GetAllLicenseTypesWithSigningAlgorithms))]
        public void ThrowsLicenseExpiredExceptionWhenLicenseExpired(LicenseType licenseType, SigningAlgorithm signingAlgorithm)
        {
            var validator = new LicenseValidator(PublicOnlyKey, WriteLicenseFile($"LicenseFiles.{licenseType}-{signingAlgorithm}-Expired"));
            Assert.Throws<LicenseExpiredException>(() => validator.AssertValidLicense());
        }

        [Theory]
        [MemberData(nameof(GetAllLicenseTypesWithSigningAlgorithms))]
        public void CanValidateLicenseUsingSigningAlgorithm(LicenseType licenseType, SigningAlgorithm signingAlgorithm)
        {
            var validator = new LicenseValidator(PublicOnlyKey, WriteLicenseFile($"LicenseFiles.{licenseType}-{signingAlgorithm}"));
            
            validator.AssertValidLicense();

            Assert.Equal(TestGuid, validator.UserId);
            Assert.Equal(Expiration, validator.ExpirationDate);
            Assert.Equal(CustomerName, validator.Name);
            Assert.Equal(licenseType, validator.LicenseType);
        }

        [Theory]
        [MemberData(nameof(GetAllLicenseTypes))]
        public void CanValidateLicenseWithAttributes(LicenseType licenseType)
        {
            var validator = new LicenseValidator(PublicOnlyKey, WriteLicenseFile($"LicenseFiles.{licenseType}-WithAttributes"));
            
            validator.AssertValidLicense();

            Assert.Equal(TestGuid, validator.UserId);
            Assert.Equal(Expiration, validator.ExpirationDate);
            Assert.Equal(CustomerName, validator.Name);
            Assert.Equal(licenseType, validator.LicenseType);
            Assert.Equal("llb", validator.LicenseAttributes["prof"]);
            Assert.Equal("on", validator.LicenseAttributes["reporting"]);
        }

        [Fact]
        public void ThrowsLicenseNotFoundExceptionOnInvalidLicense()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, Resources["LicenseFiles.Invalid"]);

            var validator = new LicenseValidator(PublicOnlyKey, path);
            Assert.Throws<LicenseNotFoundException>(() => validator.AssertValidLicense());
        }
    }
}