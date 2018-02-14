using System;
using System.Configuration;
using System.Linq;

using Migrator.Core;

using NUnit.Framework;
using NSubstitute;

namespace Migrator.DataAccess.Tests
{
    public class TemplateSourceProviderTests
    {
        [TestFixture]
        public class GetTemplates : TestWithRestoredDb
        {
            private ILogService _logService;

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
                string backupFile = ConfigurationManager.AppSettings["OldVersionDatabaseBackupLocation"];
                return backupFile;
            }

            [TestCase(null, 4)]
            [TestCase("TemplateName LIKE 'Sample%'", 3)]
            public void GetTemplates_ReturnsTemplateFromDatabase(string whereCondition, int expectedCount)
            {
                var logger = Substitute.For<ILogService>();
                var tsp = new TemplateSourceProvider(ConnectionString, logger);
                var actualTemplates = tsp.GetAllTemplates(whereCondition);

                Assert.That(actualTemplates.Count(), Is.EqualTo(expectedCount));
            }
        }
    }
}
