using System;
using System.Configuration;
using System.Linq;

using Migrator.Core;

using NSubstitute;
using NUnit.Framework;

namespace Migrator.DataAccess.Tests
{
    public class IssueSourceProviderTests
    {
        [TestFixture]
        public class GetIssuesTests : TestWithRestoredDb
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

            [Test]
            public void GetIssues_ReturnsIssuesFromDatabase()
            {
                var issueSourceProvider = new IssueSourceProvider(ConnectionString, LogService);

                var sourceIssues = issueSourceProvider.GetAllIssues();

                Assert.Multiple(() =>
                {
                    Assert.That(sourceIssues.Count(), Is.EqualTo(6));
                    Assert.That(sourceIssues.Select(i => i.IssueText), Is.All.Not.Empty);
                    Assert.That(sourceIssues.Select(i => i.IssueGuid), Is.All.Not.EqualTo(Guid.Empty));
                });
            }
        }
    }
}
