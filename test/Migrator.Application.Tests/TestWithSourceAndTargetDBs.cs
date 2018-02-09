using System;
using System.Configuration;
using System.IO;
using System.Reflection;

using Migrator.Core;
using Migrator.DataAccess;

using NUnit.Framework;

namespace Migrator.Application.Tests
{
    [TestFixture]
    public class TestWithSourceAndTargetDBs
    {
        // Connection string just to connect to database server; doesn't include Initial Catalog property.
        private string _commonConnectionString = null;
        private string sourceDbName = null;
        private string targetDbName = null;
        private DatabaseManager dbManager = null;
        protected bool dropDatabases = true;

        public string CurrentDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        protected virtual ILogService LogService
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected string SourceDbConnectionString
        {
            get
            {
                return DatabaseManager.AddDatabaseNameToConnectionString(sourceDbName, _commonConnectionString, LogService);
            }
        }

        protected string TargetDbConnectionString
        {
            get
            {
                return DatabaseManager.AddDatabaseNameToConnectionString(targetDbName, _commonConnectionString, LogService);
            }
        }

        public virtual string GetSourceDbBackupPath()
        {
            throw new NotImplementedException();
        }

        public virtual string GetTargetDbBackupPath()
        {
            throw new NotImplementedException();
        }

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            _commonConnectionString = ConfigurationManager.ConnectionStrings["Main"].ConnectionString;
            string uniquePrefix = Guid.NewGuid().ToString();
            sourceDbName = $"NewsletterMigratorDB_{uniquePrefix}_Source";
            targetDbName = $"NewsletterMigratorDB_{uniquePrefix}_Target";

            // Restore source database
            dbManager = new DatabaseManager(_commonConnectionString, LogService);
            dbManager.RestoreDbFromBackup(sourceDbName, GetSourceDbBackupPath(), CurrentDirectory);

            // Restore target database
            dbManager.RestoreDbFromBackup(targetDbName, GetTargetDbBackupPath(), CurrentDirectory);
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            if (dropDatabases)
            {
                dbManager?.DropDatabase(sourceDbName);
                dbManager?.DropDatabase(targetDbName);
            }
        }
    }
}
