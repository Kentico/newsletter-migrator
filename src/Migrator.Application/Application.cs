using System;
using System.Linq;

using Migrator.Core;
using Migrator.DataAccess;

namespace Migrator.Application
{
    public class Application
    {
        public static void MigrateAllNewsletters(string sourceConnectionString, string targetConnectionString, ILogService logService, string issuesFilter = null, string templatesFilter = null)
        {
            try
            {
                // Obtain templates
                var templateProvider = new TemplateSourceProvider(sourceConnectionString, logService);
                var templates = templateProvider.GetEmailTemplates(templatesFilter);

                // Migrate templates
                var templateMigrator = new TemplateMigrator();
                var migratedTemplates = templateMigrator.MigrateTemplates(templates);

                // --> Save templates
                var templateOutputProvider = new TemplateOutputProvider(targetConnectionString, logService);
                templateOutputProvider.SaveTemplates(migratedTemplates);

                // Get sites from templates
                var siteGuids = templates.Select(t => t.SiteGuid).Distinct();

                // Migrate issues per site because widgets are site related
                foreach (var siteGuid in siteGuids)
                {
                    // Prepare helper widget in the new database
                    var widgetSettings = new WidgetCreationSettings
                    {
                        PropertyName = "Code",
                        TypeIdentifier = Guid.NewGuid(),
                        SiteGuid = siteGuid,
                    };

                    // Group templates by Site
                    var siteTemplates = templates.Where(t => t.SiteGuid == siteGuid);

                    // Create temporary widget and assign it to templates
                    var widgetCreator = new WidgetProvider(targetConnectionString, logService);
                    widgetCreator.CreateWidgetForTemplates(widgetSettings, siteTemplates);

                    // Get issues
                    var issueSourceProvider = new IssueSourceProvider(sourceConnectionString, logService);
                    var issues = issueSourceProvider.GetIssuesInSite(siteGuid, issuesFilter);

                    // Migrate issues
                    var issueMigrator = new IssueMigrator(widgetSettings, logService);
                    var migratedIssues = issueMigrator.MigrateIssues(issues);

                    // Save issues
                    var issueProvider = new IssueOutputProvider(targetConnectionString, logService);
                    issueProvider.SaveIssues(migratedIssues);
                }

                logService.LogMessage("Migration finished successfully.");
            }
            catch (SqlQueryFailedException exception)
            {
                logService.LogMessage(exception.Message);
                logService.LogMessage("Migration failed.");
            }
        }
    }
}
