using System;
using Xunit;

namespace Rhino.Licensing.Tests
{
    public sealed class GenerateTestLicensesTheory : TheoryAttribute
    {
        public GenerateTestLicensesTheory()
        {
            if (!ShouldGenerateTestLicenses) {
                Skip = "Ignore generating test licenses when we don't want them";
            }
        }

        private static bool ShouldGenerateTestLicenses { get => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GENERATE_TEST_LICENSES")); }
    }
}
