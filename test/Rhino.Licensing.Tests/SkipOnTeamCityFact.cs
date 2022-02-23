using System;
using Xunit;

namespace Rhino.Licensing.Tests
{
    public sealed class SkipOnTeamCityFact : FactAttribute
    {
        public SkipOnTeamCityFact()
        {
            if (IsTeamCity) {
                Skip = "Ignore incompatible test on TeamCity";
            }
        }

        private static bool IsTeamCity { get => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME")); }
    }
}
