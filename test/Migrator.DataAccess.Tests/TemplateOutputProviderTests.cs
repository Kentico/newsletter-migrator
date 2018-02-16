using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using Migrator.Core;

using NUnit.Framework;
using NSubstitute;

namespace Migrator.DataAccess.Tests
{
    public class TemplateOutputProviderTests
    {
        [TestFixture]
        public class SaveTemplates : TestWithRestoredDb
        {
            private const int SITE_ID = 1;
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
            public void SaveTemplates_AllStoredInDatabase()
            {
                var tsp = new TemplateOutputProvider(ConnectionString, LogService);

                var templates = GetOutputTemplates();
                tsp.SaveTemplates(templates);

                var actualTemplates = tsp.GetAllTemplates();

                Assert.Multiple(() =>
                {
                    Assert.That(actualTemplates.Count(), Is.EqualTo(4));
                    Assert.That(actualTemplates.Select(t => t.TemplateCode), Is.EquivalentTo(new[] { "code1", "code2", "code3", "code4" }));
                });
            }

            private IEnumerable<TemplateOutput> GetOutputTemplates()
            {
                return new[]
                {
                    new TemplateOutput
                    {
                        Guid = new Guid("0B8739EC-7A44-4E92-A212-A6AAD04DF3BE"),
                        TemplateCode = "code1"
                    },
                    new TemplateOutput
                    {
                        Guid = new Guid("B51E07ED-E45A-4B58-AB5F-6CCF2B5714CC"),
                        TemplateCode = "code2"
                    },
                    new TemplateOutput
                    {
                        Guid = new Guid("B2B2D5E8-C669-4621-BAA9-CC48C8309779"),
                        TemplateCode = "code3"
                    },
                    new TemplateOutput
                    {
                        Guid = new Guid("A3D46EDF-02A7-4C4E-B78C-F02826F496F9"),
                        TemplateCode = "code4"
                    },
                };
            }
        }
    }
}
