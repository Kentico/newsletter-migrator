using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Migrator.Core;

namespace Migrator.DataAccess
{
    public class IssueSourceProvider
    {
        private readonly string _connectionString;
        private readonly ILogService _logService;

        public IssueSourceProvider(string connectionString, ILogService logService)
        {
            _connectionString = connectionString;
            _logService = logService;
        }

        public IReadOnlyCollection<IssueSource> GetIssues(string whereCondition = null)
        {
            var queryBuilder = new StringBuilder("SELECT [IssueGUID], [IssueText] FROM [Newsletter_NewsletterIssue]");

            if (!String.IsNullOrWhiteSpace(whereCondition))
            {
                queryBuilder.Append(" WHERE ").Append(whereCondition);
            }

            var dbm = new DatabaseManager(_connectionString, _logService);
            var sourceIssues = dbm.ExecuteQuery(queryBuilder.ToString());

            return sourceIssues.Select(dr => new IssueSource
            {
                IssueGuid = dr.Field<Guid>("IssueGUID"),
                IssueText = dr.Field<string>("IssueText"),
            }).ToList().AsReadOnly();
        }
    }
}
