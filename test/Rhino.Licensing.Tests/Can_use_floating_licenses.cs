namespace Rhino.Licensing.Tests
{
    using System;
    using System.IO;
    using System.ServiceModel;
    using Xunit;
    using Rhino.Licensing;

    public class Can_use_floating_licenses : BaseLicenseTest
    {
        [Fact]
        public void Can_generate_floating_license()
        {
            var server_public_key = @"<license-server-public-key>&lt;RSAKeyValue&gt;&lt;Modulus&gt;tOAa81fDkKAIcmBx5SybBQM34OG12Qsbm0V8H10Q5iL3bFIco1S6BFyKRK84LKitSPczY3z62imwNkanDVfXhnhl2UFTS0MTkhXM+yG9xFRGc3QwIcNE1j7UFAENo7RS1eguVQaYm26uaqgYXWHJn352CzddV7Lv4M3lAe6oh2M=&lt;/Modulus&gt;&lt;Exponent&gt;AQAB&lt;/Exponent&gt;&lt;/RSAKeyValue&gt;</license-server-public-key>";
            var owner_name = "<name>ayende</name>";

            var generator = new LicenseGenerator(public_and_private);
            var license = generator.GenerateFloatingLicense("ayende", floating_public);

            Assert.Contains(server_public_key, license);
            Assert.Contains(owner_name, license);
        }

#if DESKTOP

        [SkipOnTeamCityFact]
        public void Can_validate_floating_license()
        {
            string fileName = WriteFloatingLicenseFile();

            GenerateLicenseFileInLicensesDirectory();

            LicensingService.SoftwarePublicKey = public_only;
            LicensingService.LicenseServerPrivateKey = floating_private;

            var host = new ServiceHost(typeof(LicensingService));
            string address = "http://localhost:19292/" + Guid.NewGuid();
            host.AddServiceEndpoint(typeof(ILicensingService), new BasicHttpBinding(), address);

            host.Open();
            try
            {
                var validator = new LicenseValidator(public_only, fileName, address, Guid.NewGuid());
                validator.AssertValidLicense();
            }
            finally
            {
                host.Close();
            }
        }

        [SkipOnTeamCityFact]
        public void Can_only_get_license_per_allocated_licenses()
        {
            string fileName = WriteFloatingLicenseFile();

            GenerateLicenseFileInLicensesDirectory();

            LicensingService.SoftwarePublicKey = public_only;
            LicensingService.LicenseServerPrivateKey = floating_private;

            var host = new ServiceHost(typeof(LicensingService));
            string address = "http://localhost:19292/" + Guid.NewGuid();
            host.AddServiceEndpoint(typeof(ILicensingService), new BasicHttpBinding(), address);

            host.Open();

            try
            {
                var validator = new LicenseValidator(public_only, fileName, address, Guid.NewGuid());
                validator.AssertValidLicense();

                var validator2 = new LicenseValidator(public_only, fileName, address, Guid.NewGuid());
                Assert.Throws<FloatingLicenseNotAvailableException>(() => validator2.AssertValidLicense());
            }
            finally
            {
                host.Close();
            }
        }

#endif

        private void GenerateLicenseFileInLicensesDirectory()
        {
            var generator = new LicenseGenerator(public_and_private);
            var generate = generator.Generate("ayende", Guid.NewGuid(), DateTime.MaxValue, LicenseType.Standard);
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Licenses");
            if (Directory.Exists(dir) == false)
                Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "ayende.xml"), generate);
        }

        private string WriteFloatingLicenseFile()
        {
            var generator = new LicenseGenerator(public_and_private);
            var license = generator.GenerateFloatingLicense("ayende", floating_public);
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            var fileName = Path.Combine(dir, Guid.NewGuid().ToString());
            File.WriteAllText(fileName, license);
            return fileName;
        }
    }
}
