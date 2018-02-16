using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Migrator.Core;
using System.Data;

namespace Migrator.DataAccess
{
    public class IssueOutputProvider
    {
        private readonly string _connectionString;
        private readonly ILogService _logService;

        public IssueOutputProvider(string connectionString, ILogService logService)
        {
            _connectionString = connectionString;
            _logService = logService;
        }

        /// <summary>
        /// Saves the given <see cref="issues"/> in database.
        /// </summary>
        /// <param name="issues">Collection of issues</param>
        /// <exception cref="SqlQueryFailedException">Data update failed</exception>
        public void SaveIssues(IEnumerable<IssueOutput> issues)
        {
            var dbm = new DatabaseManager(_connectionString, _logService);
            dbm.ExecuteMultipleNonQueryCommands(GetIssueUpdateCommands(issues));
        }

        private IEnumerable<SqlCommand> GetIssueUpdateCommands(IEnumerable<IssueOutput> issues)
        {
            const string UPDATE_COMMAND = "UPDATE [Newsletter_NewsletterIssue] SET [IssueWidgets] = @widgets WHERE [IssueGUID] = @issueGuid";

            foreach (var issue in issues)
            {
                var command = new SqlCommand(UPDATE_COMMAND);
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@issueGuid", issue.IssueGuid);
                command.Parameters.AddWithValue("@widgets", issue.Widgets);

                yield return command;
            }
        }
        internal IEnumerable<IssueOutput> GetAllIssues()
        {
            const string QUERY_TEXT = @"SELECT [IssueGUID], [IssueWidgets], [TemplateGUID] 
FROM [Newsletter_NewsletterIssue] INNER JOIN [Newsletter_EmailTemplate] ON [IssueTemplateID] = [TemplateID]";

            var dbm = new DatabaseManager(_connectionString, _logService);
            var queryResults = dbm.ExecuteQuery(QUERY_TEXT);

            return queryResults.Select(dr =>
                new IssueOutput
                {
                    IssueGuid = dr.Field<Guid>("IssueGUID"),
                    TemplateGuid = dr.Field<Guid>("TemplateGUID"),
                    Widgets = dr.Field<string>("IssueWidgets"),
                }
            );
        }
    }
}
