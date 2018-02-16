using System;
using System.Configuration;
using System.IO;
using System.Reflection;

using Migrator.Core;

using NUnit.Framework;

namespace Migrator.DataAccess.Tests
{
    [TestFixture]
    public class TestWithRestoredDb
    {
        private DatabaseManager dbManager = null;
        private string dbName = null;
        private string connectionString = null;

        public virtual string CurrentDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        public virtual ILogService LogService
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected virtual string GetBackupFile()
        {
            throw new NotImplementedException();
        }

        protected virtual string ConnectionString
        {
            get
            {
                return $"{connectionString};Initial Catalog={dbName}";
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            dbName = $"NewsletterMigrationTemporaryDB_{Guid.NewGuid()}";
            connectionString = ConfigurationManager.ConnectionStrings["Main"].ConnectionString;
            dbManager = new DatabaseManager(connectionString, LogService);
            dbManager.RestoreDbFromBackup(dbName, GetBackupFile(), CurrentDirectory);
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            dbManager?.DropDatabase(dbName);
        }
    }
}
