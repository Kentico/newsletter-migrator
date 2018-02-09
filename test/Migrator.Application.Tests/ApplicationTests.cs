using System;
using System.Configuration;

using Migrator.Core;

using NUnit.Framework;
using NSubstitute;

namespace Migrator.Application.Tests
{
    [TestFixture]
    public class ApplicationTests : TestWithSourceAndTargetDBs
    {
        private ILogService _logService;

        protected override ILogService LogService
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

        public override string GetSourceDbBackupPath()
        {
            return ConfigurationManager.AppSettings["OldVersionDatabaseBackupLocation"];
        }

        public override string GetTargetDbBackupPath()
        {
            return ConfigurationManager.AppSettings["NewVersionDatabaseBackupLocation"];
        }

        [Test]
        public void MigrateAllNewsletters()
        {
            Assert.That(() => Application.MigrateAllNewsletters(SourceDbConnectionString, TargetDbConnectionString, LogService), Throws.Nothing);
        }
    }
}
