using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace SKBKontur.Catalogue.TeamCity
{
    public class TeamCityLogger : ITeamCityLogger
    {
        public TeamCityLogger(TextWriter writer)
        {
            this.writer = writer;
        }

        public void BeginMessageBlock(string blockName)
        {
            messageBlocks.Push(blockName);
            WriteMessageRaw("blockOpened", new {name = blockName});
        }

        public void EndMessageBlock()
        {
            WriteMessageRaw("blockClosed", new {name = messageBlocks.Pop()});
        }

        public void WriteBuildProblem(string description, string identity = null)
        {
            WriteMessageRaw("buildProblem", new {description, identity});
        }

        public void WriteMessage(TeamCityMessageSeverity severity, string text, string errorDetails = null)
        {
            WriteMessageRaw("message", new {text, errorDetails, status = severity.ToString().ToUpper()});
        }

        public void WriteMessageFormat(TeamCityMessageSeverity severity, string text, params object[] parameters)
        {
            WriteMessage(severity, string.Format(text, parameters));
        }

        public void ReportActivity(string activityName)
        {
            WriteSimpleMessage("progressMessage", activityName);
        }

        public void BeginActivity(string activityName)
        {
            activityBlocks.Push(activityName);
            WriteSimpleMessage("progressStart", activityName);
        }

        public void EndActivity()
        {
            WriteSimpleMessage("progressFinish", activityBlocks.Pop());
        }

        public void SetBuildStatus(string buildStatus, string buildStatusText)
        {
            WriteMessageRaw("buildStatus", new {status = buildStatus, text = buildStatusText});
        }

        public void PublishArtifact(string path)
        {
            WriteSimpleMessage("publishArtifacts", path);
        }

        public void ReportStatisticsValue(string name, string value)
        {
            WriteMessageRaw("buildStatisticValue", new {key = name, value});
        }

        public string FormatMessage(string messageName, object values)
        {
            return FormatMessageFromDictionary(messageName, MakeDictionary(values));
        }

        public string FormatSimpleMessage(string messageName, string value)
        {
            var result = new StringBuilder();
            result.Append("##teamcity[");
            result.Append(messageName);
            result.Append(" ");
            result.Append("'");
            result.Append(ServiceMessageReplacements.Encode(value));
            result.Append("'");
            result.Append("]");
            return result.ToString();
        }

        public void WriteMessageRaw(string messageName, object values)
        {
            writer.WriteLine(FormatMessage(messageName, values));
        }

        private void WriteSimpleMessage(string messageName, string value)
        {
            writer.WriteLine(FormatSimpleMessage(messageName, value));
        }

        private string FormatMessageFromDictionary(string messageName, IEnumerable<KeyValuePair<string, object>> properties)
        {
            var result = new StringBuilder();
            result.Append("##teamcity[");
            result.Append(messageName);
            foreach(var property in properties)
            {
                if(property.Value == null)
                    continue;
                result.Append(" ");
                result.Append(property.Key);
                result.Append("=");
                result.Append("'");
                result.Append(ServiceMessageReplacements.Encode(property.Value.ToString()));
                result.Append("'");
            }
            result.Append("]");
            return result.ToString();
        }

        private IDictionary<string, object> MakeDictionary(object withProperties)
        {
            return TypeDescriptor
                .GetProperties(withProperties)
                .Cast<PropertyDescriptor>()
                .ToDictionary(
                    property => property.Name,
                    property => property.GetValue(withProperties));
        }

        private readonly TextWriter writer;
        private readonly Stack<string> messageBlocks = new Stack<string>();
        private readonly Stack<string> activityBlocks = new Stack<string>();
    }
}