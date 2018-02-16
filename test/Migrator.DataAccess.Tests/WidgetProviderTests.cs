using Migrator.Core;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrator.DataAccess.Tests
{
    public class WidgetProviderTests
    {
        [TestFixture]
        public class CreateWidgetForTemplatesTests : TestWithRestoredDb
        {
            private ILogService _logService = null;

            public override ILogService LogService
            {
                get
                {
                    if (_logService == null)
                    {
                        _logService = Substitute.For<ILogService>();
                        _logService.When
                        (
                            s => s.LogMessage(Arg.Any<string>())
                        )
                        .Do
                        (
                            callInfo => Console.WriteLine(callInfo.Arg<string>())
                        );
                    }

                    return _logService;
                }
            }

            protected override string GetBackupFile()
            {
                string backupFile = ConfigurationManager.AppSettings["NewVersionDatabaseBackupLocation"];
                return backupFile;
            }

            [Test]
            public void CreateWidgetForTemplates_WidgetSettingsIsNull_ThrowsException()
            {
                var widgetProvider = new WidgetProvider(ConnectionString, LogService);

                Assert.That(() => widgetProvider.CreateWidgetForTemplates(null, null), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("widgetSettings"));
            }

            [Test]
            public void CreateWidgetForTemplates_TemplateCollectionIsNull_ThrowsException()
            {
                var widgetSettings = new WidgetCreationSettings();
                var widgetProvider = new WidgetProvider(ConnectionString, LogService);

                Assert.That(() => widgetProvider.CreateWidgetForTemplates(widgetSettings, null), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("templates"));
            }

            [Test]
            public void CreateWidgetForTemplates_WidgetIsCreated()
            {
                var settings = new WidgetCreationSettings
                {
                    PropertyName = "Code",
                    SiteGuid = new Guid("C00E8ED8-8C18-45AC-9AE4-A863D10B1B6F"),
                    TypeIdentifier = Guid.NewGuid(),
                };

                var template = Substitute.For<ITemplateDescriptor>();
                template.Guid = new Guid("B51E07ED-E45A-4B58-AB5F-6CCF2B5714CC");

                var widgetProvider = new WidgetProvider(ConnectionString, LogService);
                widgetProvider.CreateWidgetForTemplates(settings, new[] { template });
            }
        }
    }
}
