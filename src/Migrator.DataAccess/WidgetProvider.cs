using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using Migrator.Core;

namespace Migrator.DataAccess
{
    public class WidgetProvider
    {
        private readonly string _connectionString = null;
        private readonly ILogService _logService;

        public WidgetProvider(string connectionString, ILogService logService)
        {
            _connectionString = connectionString;
            _logService = logService;
        }

        private void InsertWidget(WidgetCreationSettings settings)
        {
            const string WIDGET_PROPERTIES = @"<form version=""2""><field allowempty=""true"" column=""{1}"" columntype=""longtext"" guid=""{0}"" publicfield=""false"" visible=""true""><properties><fieldcaption>{1}</fieldcaption></properties><settings><controlname>LargeTextArea</controlname></settings></field></form>";
            const string INSERT_QUERY = @"
DECLARE @siteID int = (SELECT TOP 1 [SiteID] FROM [CMS_Site] WHERE [SiteGUID] = @siteGUID)
IF @siteID IS NOT NULL
BEGIN
INSERT INTO [Newsletter_EmailWidget] ([EmailWidgetGuid], [EmailWidgetLastModified], [EmailWidgetDisplayName], [EmailWidgetName], [EmailWidgetDescription], [EmailWidgetCode], [EmailWidgetSiteID], [EmailWidgetIconCssClass], [EmailWidgetProperties]) VALUES (@guid, @lastModified, @displayName, @codeName, @description, @code, @siteID, @cssClass, @properties)
END";

            if (settings == null)
            {
                return;
            }

            var parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@guid", settings.TypeIdentifier));
            parameters.Add(new SqlParameter("@lastModified", DateTime.Now));
            parameters.Add(new SqlParameter("@displayName", "Static HTML (generated)"));
            parameters.Add(new SqlParameter("@codeName", "StaticHtml"));
            parameters.Add(new SqlParameter("@description", "Widget automatically created by migration tool."));
            parameters.Add(new SqlParameter("@code", $"{{% {settings.PropertyName} %}}"));
            parameters.Add(new SqlParameter("@siteGUID", settings.SiteGuid));
            parameters.Add(new SqlParameter("@cssClass", "icon-cogwheel-square"));
            parameters.Add(new SqlParameter("@properties", String.Format(WIDGET_PROPERTIES, Guid.NewGuid(), settings.PropertyName)));

            var dbm = new DatabaseManager(_connectionString, _logService);
            dbm.ExecuteNonQuery(INSERT_QUERY, parameters);
        }

        private void AssignToTemplate(WidgetCreationSettings widgetSettings, ITemplateDescriptor template)
        {
            const string insertBindingQuery = "INSERT INTO [Newsletter_EmailWidgetTemplate] ([EmailWidgetID], [TemplateID]) SELECT (SELECT TOP 1 [EmailWidgetID] FROM [Newsletter_EmailWidget] WHERE [EmailWidgetGuid] = @widgetGuid) AS [EmailWidgetID], (SELECT TOP 1 [TemplateID] FROM [Newsletter_EmailTemplate] WHERE [TemplateGUID] = @templateGuid) AS [TemplateID]";

            var dbm = new DatabaseManager(_connectionString, _logService);
            var parameters = new List<SqlParameter>(2);
            parameters.Add(new SqlParameter("@widgetGuid", widgetSettings.TypeIdentifier));
            parameters.Add(new SqlParameter("@templateGuid", template.Guid));

            dbm.ExecuteNonQuery(insertBindingQuery, parameters);
        }

        /// <summary>
        /// Creates a temporary widget definition for each site and assign it to all <paramref name="templates"/>.
        /// </summary>
        /// <param name="widgetSettings">Widget settings</param>
        /// <param name="templates">Collection of templates</param>
        /// <exception cref="ArgumentNullException"><paramref name="templates">Templates</paramref> or <paramref name="widgetSettings"/> is null</exception>
        /// <exception cref="SqlQueryFailedException"></exception>
        public void CreateWidgetForTemplates(WidgetCreationSettings widgetSettings, IEnumerable<ITemplateDescriptor> templates)
        {
            if (widgetSettings == null)
            {
                throw new ArgumentNullException(nameof(widgetSettings));
            }

            if (templates == null)
            {
                throw new ArgumentNullException(nameof(templates));
            }

            // Create widget for the site specified in widgetSettings
            InsertWidget(widgetSettings);

            // Assign widget to templates
            foreach (var template in templates)
            {
                AssignToTemplate(widgetSettings, template);
            }
        }
    }
}
