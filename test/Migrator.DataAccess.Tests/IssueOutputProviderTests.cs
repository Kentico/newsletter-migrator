using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using Migrator.Core;

using NUnit.Framework;
using NSubstitute;

namespace Migrator.DataAccess.Tests
{
    public class IssueOutputProviderTests
    {
        [TestFixture]
        public class SaveIssuesTests : TestWithRestoredDb
        {
            private const int SITE_ID = 1;
            private ILogService _logService = null;
            private readonly IEnumerable<Guid> issuesGuids = new[]
            {
                new Guid("7e79b639-fe12-410b-a208-25cfa284618a"),
                new Guid("eab5fbd1-802e-4a28-8230-47895079c420")
            };

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
            public void SaveIssues_IssuesSavedInDatabase()
            {
                var issueOutputProvider = new IssueOutputProvider(ConnectionString, LogService);

                var issues = GetOutputIssues();
                issueOutputProvider.SaveIssues(issues);

                var actualIssues = issueOutputProvider.GetAllIssues().Where(i => issuesGuids.Contains(i.IssueGuid));

                Assert.Multiple(() =>
                {
                    Assert.That(actualIssues.Count(), Is.EqualTo(2));
                    Assert.That(actualIssues.Select(t => t.Widgets), Is.EquivalentTo(new[] { "code1", "code2" }));
                });
            }

            private IEnumerable<IssueOutput> GetOutputIssues()
            {
                return new[]
                {
                    new IssueOutput
                    {
                        IssueGuid = issuesGuids.ElementAt(0),
                        Widgets = "code1",
                    },
                    new IssueOutput
                    {
                        IssueGuid = issuesGuids.ElementAt(1),
                        Widgets = "code2",
                    },
                };
            }
        }
    }
}
