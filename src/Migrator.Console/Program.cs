using System;
using System.Configuration;

using Migrator.Application;

namespace NewsletterMigrator
{
    class Program
    {
        const string ARGUMENT_OLD_DB = "v10connection";
        const string ARGUMENT_NEW_DB = "v11connection";
        const string ARGUMENT_ISSUES_FILTER = "issuesFilter";
        const string ARGUMENT_TEMPLATES_FILTER = "templatesFilter";


        static void Main(string[] args)
        {
            var configuration = new ProgramConfiguration
            {
                OldDbConnectionString = ConfigurationManager.ConnectionStrings[ARGUMENT_OLD_DB].ConnectionString,
                NewDbConnectionString = ConfigurationManager.ConnectionStrings[ARGUMENT_NEW_DB].ConnectionString,
                IssuesFilter = ConfigurationManager.AppSettings[ARGUMENT_ISSUES_FILTER],
                TemplatesFilter = ConfigurationManager.AppSettings[ARGUMENT_TEMPLATES_FILTER]
            };

            ValidateConfiguration(configuration);

            Application.MigrateAllNewsletters(configuration.OldDbConnectionString, configuration.NewDbConnectionString, new LogService(), configuration.IssuesFilter, configuration.TemplatesFilter);
        }

        private static void ValidateConfiguration(ProgramConfiguration configuration)
        {
            if (String.IsNullOrEmpty(configuration.OldDbConnectionString))
            {
                throw new InvalidOperationException("Old database connection string not defined");
            }

            if (String.IsNullOrEmpty(configuration.NewDbConnectionString))
            {
                throw new InvalidOperationException("New database connection string not defined");
            }
        }
    }
}
