using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Migrator.Core;

namespace Migrator.DataAccess
{
    public class TemplateSourceProvider
    {
        private readonly string _connectionString;
        private readonly ILogService _logService;

        public TemplateSourceProvider(string connectionString, ILogService logService)
        {
            _connectionString = connectionString;
            _logService = logService;
        }

        /// <summary>
        /// Retrieves templates being used for marketing emails (not (un)subscription or double opt-in templates) from the database.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        /// <returns>Collection of templates</returns>
        /// <exception cref="SqlQueryFailedException"></exception>
        public IReadOnlyCollection<TemplateSource> GetEmailTemplates(string whereCondition = null)
        {
            const string TEMPLATE_TYPE_CONDITION = "[TemplateType] = N'I'";
            string completeWhereCondition = String.IsNullOrWhiteSpace(whereCondition) ? TEMPLATE_TYPE_CONDITION : $"{TEMPLATE_TYPE_CONDITION} AND ({whereCondition})";

            return GetAllTemplates(completeWhereCondition);
        }


        /// <summary>
        /// Retrieves ALL templates regardless thier type from the database .
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        /// <returns>Collection of templates</returns>
        /// <exception cref="SqlQueryFailedException"></exception>
        internal IReadOnlyCollection<TemplateSource> GetAllTemplates(string whereCondition)
        {
            var query = new StringBuilder("SELECT [TemplateGUID], [TemplateBody], [TemplateHeader], [TemplateFooter], [TemplateStylesheetText], [SiteGUID] FROM [Newsletter_EmailTemplate] INNER JOIN [CMS_Site] ON [SiteID] = [TemplateSiteID]");

            if (!String.IsNullOrWhiteSpace(whereCondition))
            {
                query.Append(" WHERE ").Append(whereCondition);
            }

            var dbm = new DatabaseManager(_connectionString, _logService);
            var templates = dbm.ExecuteQuery(query.ToString());

            return templates.Select(dr => new TemplateSource
            {
                Body = dr.Field<string>("TemplateBody"),
                Footer = dr.Field<string>("TemplateFooter"),
                Guid = dr.Field<Guid>("TemplateGuid"),
                Header = dr.Field<string>("TemplateHeader"),
                StylesheetText = dr.Field<string>("TemplateStylesheetText"),
                SiteGuid = dr.Field<Guid>("SiteGUID"),
            }).ToList().AsReadOnly();
        }
    }
}
