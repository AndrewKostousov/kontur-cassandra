using System;

namespace SKBKontur.Catalogue.TeamCity
{
    public class TeamCityEnvironment
    {
        public static bool IsExecutionViaTeamCity
        {
            get
            {
                if(isExecutionViaTeamCity == null)
                    isExecutionViaTeamCity = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION"));
                return isExecutionViaTeamCity.Value;
            }
        }

        private static bool? isExecutionViaTeamCity;
    }
}