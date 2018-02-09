using System;
using System.Configuration;
using System.IO;
using System.Reflection;

using Migrator.Core;

using NSubstitute;
using NUnit.Framework;

namespace Migrator.DataAccess.Tests
{
    public class DatabaseManagerTests
    {
        public class DatabaseManagerTestsBase
        {
            private ILogService _logService;

            protected virtual ILogService LogService
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
        }

        [TestFixture]
        public class DatabaseManagerCtorTests : DatabaseManagerTestsBase
        {
            [Test]
            public void DatabaseManagerCtor_ConnectionStringIsNullOrEmpty_ThrowsException([Values(null, "")] string connectionString)
            {
                Assert.That(() => new DatabaseManager(connectionString, LogService), 
                    Throws.ArgumentException
                    .With
                    .Property("ParamName")
                    .EqualTo("connectionString")
                    .And
                    .Message
                    .StartsWith(DatabaseManager.EXCEPTIONRESOURCE_CONNECTIONSTRINGNOTVALID));
            }

            [Test]
            public void DatabaseManagerCtor_ConnectionStringIsInvalid_ThrowsException()
            {
                const string INVALID_CONNECTION_STRING = "adsjhpciv";
                Assert.That(() => new DatabaseManager(INVALID_CONNECTION_STRING, LogService), 
                    Throws.ArgumentException
                    .With
                    .Property("ParamName")
                    .EqualTo("connectionString")
                    .And
                    .Message
                    .StartsWith(DatabaseManager.EXCEPTIONRESOURCE_CONNECTIONSTRINGNOTVALID));
            }

            [Test]
            public void DatabaseManagerCtor_LogServiceIsNull_ThrowsException()
            {
                Assert.That(() => new DatabaseManager("Data Source=computername", null), 
                    Throws.ArgumentNullException
                        .With
                        .Property("ParamName")
                        .EqualTo("logService")
                        .And
                        .Message
                        .StartsWith(DatabaseManager.EXCEPTIONRESOURCE_LOGSERVICENULL));
            }
        }

        [TestFixture]
        public class RestoreDbFromBackup : DatabaseManagerTestsBase
        {
            private const string TEST_CONNECTION_STRING = "Data Source=computername";
            private bool dropDatabase;
            private DatabaseManager dbm;
            private string dbName;

            [OneTimeSetUp]
            public void FixtureSetup()
            {

            }

            [SetUp]
            public void SetUp()
            {
                dropDatabase = true;
            }

            [Test]
            public void RestoreDbFromBackup_DbNameNotDefined_ThrowsException([Values("", null)]string dbNameValue)
            {
                var dbm = new DatabaseManager(TEST_CONNECTION_STRING, LogService);

                Assert.That(() => dbm.RestoreDbFromBackup(dbNameValue, "test"), Throws.ArgumentException);
            }

            [Test]
            public void RestoreDbFromBackup_BackupFileNotDefined_ThrowsException([Values("", null)]string pathValue)
            {
                var dbm = new DatabaseManager(TEST_CONNECTION_STRING, LogService);

                Assert.That(() => dbm.RestoreDbFromBackup("test", pathValue), Throws.ArgumentException);
            }

            [Test]
            public void RestoreDbFromBackup_RestoreSuccessfully()
            {
                dropDatabase = true;
                string connectionString = ConfigurationManager.ConnectionStrings["Main"].ConnectionString;
                string backupPath = ConfigurationManager.AppSettings["OldVersionDatabaseBackupLocation"];
                string executionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string targetPath = Path.GetFullPath(Path.Combine(executionPath, @"..\..\Data\RestoredDBs\"));

                dbm = new DatabaseManager(connectionString, LogService);
                dbName = $"NewsletterMigrator_DatabaseManagerTest_DB_{Guid.NewGuid().ToString().Substring(0, 8)}";

                Assert.That(() => dbm.RestoreDbFromBackup(dbName, backupPath, targetPath), Throws.Nothing);
            }

            [TearDown]
            public void TearDown()
            {
                if (dropDatabase)
                {
                    dbm?.DropDatabase(dbName);
                }
            }
        }
    }
}
