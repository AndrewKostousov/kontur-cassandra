namespace SKBKontur.Catalogue.TeamCity
{
    public interface ITeamCityLogger
    {
        void BeginMessageBlock(string blockName);
        void EndMessageBlock();

        void ReportActivity(string activityName);
        void BeginActivity(string activityName);
        void EndActivity();

        void WriteBuildProblem(string description, string identity = null);
        void WriteMessage(TeamCityMessageSeverity severity, string text, string errorDetails = null);
        void WriteMessageFormat(TeamCityMessageSeverity severity, string text, params object[] parameters);
        void SetBuildStatus(string buildStatus, string buildStatusText);
        void PublishArtifact(string path);
        void ReportStatisticsValue(string name, string value);
    }
}