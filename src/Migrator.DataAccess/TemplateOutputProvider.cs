using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using Migrator.Core;

namespace Migrator.DataAccess
{
    public class TemplateOutputProvider
    {
        private readonly string _connectionString;
        private readonly ILogService _logService;

        public TemplateOutputProvider(string connectionString, ILogService logService)
        {
            _connectionString = connectionString;
            _logService = logService;
        }

        /// <exception cref="SqlQueryFailedException">Data update failed</exception>
        public void SaveTemplates(IEnumerable<TemplateOutput> templates)
        {
            var dbm = new DatabaseManager(_connectionString, _logService);
            dbm.ExecuteMultipleNonQueryCommands(GetTemplateUpdateCommands(templates));
        }

        private IEnumerable<SqlCommand> GetTemplateUpdateCommands(IEnumerable<TemplateOutput> templates)
        {
            const string QUERY_TEXT = "UPDATE [Newsletter_EmailTemplate] SET [TemplateCode] = @templateCode WHERE [TemplateGuid] = @templateGuid";

            foreach (var template in templates)
            {
                var command = new SqlCommand(QUERY_TEXT);
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@templateCode", template.TemplateCode);
                command.Parameters.AddWithValue("@templateGuid", template.Guid);

                yield return command;
            }
        }

        internal IEnumerable<TemplateOutput> GetAllTemplates()
        {
            const string QUERY_TEXT = "SELECT [TemplateCode], [TemplateSiteID], [TemplateGUID], [SiteGUID] FROM [Newsletter_EmailTemplate] INNER JOIN [CMS_Site] ON [SiteID] = [TemplateSiteID]";

            var dbm = new DatabaseManager(_connectionString, _logService);
            var queryResults = dbm.ExecuteQuery(QUERY_TEXT);

            return queryResults.Select(dr =>
                new TemplateOutput
                {
                    Guid = dr.Field<Guid>("TemplateGUID"),
                    TemplateCode = dr.Field<string>("TemplateCode"),
                    SiteGuid = dr.Field<Guid>("SiteGUID"),
                }
            );
        }
    }
}
