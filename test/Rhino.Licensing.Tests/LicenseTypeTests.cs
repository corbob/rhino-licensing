using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Rhino.Licensing.Tests
{
    public class LicenseTypeTests : BaseLicenseTest
    {
        private static Dictionary<int, string> licenseMappings = new Dictionary<int, string>(){
            { 0, "None" },
            { 1, "Trial" },
            // Leave these in so that they fail should any other license types map to them.
            { 2, "REMOVED - Standard" },
            { 3, "REMOVED - Personal" },
            { 4, "Professional" },
            { 5, "Architect" },
            { 6, "ManagedServiceProvider" },
            { 7, "Education" },
            { 8, "Business" },
            { 9, "Enterprise" },
            // Leave these in so that they fail should any other license types map to them.
            { 10, "REMOVED - Floating" },
            { 11, "REMOVED - Subscription" },
        };

        [Theory]
        [MemberData(nameof(GetAllLicenseTypes))]
        public void CorrectlyMapsLicenseType(LicenseType licenseType)
        {
            var typeInt = (int)licenseType;

            Assert.Equal(licenseMappings[typeInt], licenseType.ToString());
        }
    }
}
