using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

using Migrator.Core;

namespace Migrator.DataAccess
{
    public class DatabaseManager
    {
        internal const string EXCEPTIONRESOURCE_CONNECTIONSTRINGNOTVALID = "The parameter can't be null, empty or whitespace.";
        internal const string EXCEPTIONRESOURCE_LOGSERVICENULL = "The parameter is not allowed to be null.";
        private string _connectionString;
        private readonly ILogService _logService;

        /// <summary>
        /// Creates <see cref="DatabaseManager"/> instance. 
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="logService">Log service</param>
        /// <exception cref="ArgumentException">Connection string is null, empty string or whitespace</exception>
        /// <exception cref="ArgumentNullException">Log service is null</exception>
        public DatabaseManager(string connectionString, ILogService logService)
        {
            if (String.IsNullOrWhiteSpace(connectionString) || !IsConnectionStringValid(connectionString))
            {
                throw new ArgumentException(EXCEPTIONRESOURCE_CONNECTIONSTRINGNOTVALID, nameof(connectionString));
            }

            _connectionString = connectionString;
            _logService = logService ?? throw new ArgumentNullException(nameof(logService), EXCEPTIONRESOURCE_LOGSERVICENULL);
        }

        private bool IsConnectionStringValid(string connectionString)
        {
            try
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            }
            catch (Exception exception) when (exception is FormatException || exception is ArgumentException || exception is KeyNotFoundException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Restores a .bak file containing the database. Restored database must have different name than the database which the backup is created from, 
        /// in order to prevent overwritting the original database. If <paramref name="dbFilesLocation"/> is specified then both .mdf and .ldf files are created 
        /// in the specified directory.
        /// </summary>
        /// <param name="newDbName">Name of the new database</param>
        /// <param name="backupFilePath">Backup file path</param>
        /// <param name="dbFilesLocation">Target database files location</param>
        /// <exception cref="SqlQueryFailedException">SQL error occured during executing the RESTORE query</exception>
        public void RestoreDbFromBackup(string newDbName, string backupFilePath, string dbFilesLocation = null)
        {
            if (String.IsNullOrWhiteSpace(newDbName))
            {
                throw new ArgumentException("Parameter must be specified", nameof(newDbName));
            }

            if (String.IsNullOrWhiteSpace(backupFilePath))
            {
                throw new ArgumentException("Parameter must be specified", nameof(backupFilePath));
            }

            var backupFiles = GetBackupFiles(backupFilePath);

            var queryBuilder = new StringBuilder();
            queryBuilder.Append($@"RESTORE DATABASE [{newDbName}] FROM DISK='{backupFilePath}'");

            if (backupFiles.Any())
            {
                var dataFile = backupFiles.FirstOrDefault(meta => meta.Type.Equals("D", StringComparison.OrdinalIgnoreCase));
                var logFile = backupFiles.FirstOrDefault(meta => meta.Type.Equals("L", StringComparison.OrdinalIgnoreCase));

                queryBuilder
                    .Append(" WITH MOVE '")
                    .Append(dataFile.LogicalName)
                    .Append("' TO '")
                    .Append(Path.GetDirectoryName(dbFilesLocation ?? dataFile.Location))
                    .Append("\\")
                    .Append(newDbName)
                    .Append(".mdf',")
                    .Append("MOVE '")
                    .Append(logFile.LogicalName)
                    .Append("' TO '")
                    .Append(Path.GetDirectoryName(dbFilesLocation ?? logFile.Location))
                    .Append("\\")
                    .Append(newDbName)
                    .Append(".ldf'");
            }

            ExecuteNonQuery(queryBuilder.ToString());
        }

        internal IEnumerable<BackupFileMetadata> GetBackupFiles(string backupfileName)
        {
            string fileListQuery = $"RESTORE FILELISTONLY FROM DISK = '{backupfileName}'";
            var files = ExecuteQuery(fileListQuery);

            return files?.Select(dr => new BackupFileMetadata
            {
                Location = dr.Field<string>("PhysicalName"),
                LogicalName = dr.Field<string>("LogicalName"),
                Type = dr.Field<string>("Type")
            });
        }

        internal virtual int ExecuteNonQueryInternal(string query, IEnumerable<SqlParameter> parameters = null)
        {
            int rowsAffected = 0;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = query;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                        }

                        rowsAffected = command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

            }
            catch (SqlException exception)
            {
                LogException(exception);
                throw SqlQueryFailedExceptionFactory.Create(exception);
            }

            return rowsAffected;
        }

        /// <summary>
        /// Executes the given <paramref name="query"/> and returns number of affected rows.
        /// </summary>
        /// <param name="query">SQL query which doesn't load data</param>
        /// <param name="parameters">SQL query parameters</param>
        /// <returns>Number of affected rows</returns>
        /// <exception cref="InvalidOperationException">Cannot open a connection without specifying a data source or server; or the connection is already open.</exception>
        /// <exception cref="SqlException">A connection-level error occurred while opening the connection;</exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException"></exception>
        public int ExecuteNonQuery(string query, IEnumerable<SqlParameter> parameters = null)
        {
            return ExecuteNonQueryInternal(query, parameters);
        }

        internal virtual IEnumerable<DataRow> ExecuteQueryInternal(string query, IEnumerable<SqlParameter> parameters = null)
        {
            DataTable resultTable = new DataTable();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = query;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                        }

                        var adapter = new SqlDataAdapter(command);
                        adapter.Fill(resultTable);
                    }
                }
            }
            catch (SqlException exception)
            {
                LogException(exception);
                throw SqlQueryFailedExceptionFactory.Create(exception);
            }        

            return resultTable.AsEnumerable();
        }

        /// <summary>
        /// Executes the given <paramref name="query"/> and returns query result.
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="parameters">SQL query parameters</param>
        /// <returns>Collection of data rows</returns>
        public IEnumerable<DataRow> ExecuteQuery(string query, IEnumerable<SqlParameter> parameters = null)
        {
            return ExecuteQueryInternal(query, parameters);
        }

        /// <summary>
        /// Executes given set of <paramref name="commands"/> in single transaction. All commands must execute without error to commit the transaction; 
        /// otherwise, the transaction is roll-backed. Each command is disposed after it's executed.
        /// </summary>
        /// <param name="commands">Commands to execute</param>
        public void ExecuteMultipleNonQueryCommands(IEnumerable<SqlCommand> commands)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var command in commands)
                    {
                        command.Connection = connection;
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }

                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    transaction.Rollback();

                    if (exception is SqlException)
                    {
                        throw SqlQueryFailedExceptionFactory.Create(exception);
                    }

                    throw;
                }
            }
        }

        public void DropDatabase(string dbName)
        {
            try
            {
                string dropCommand = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{dbName}]";
                ExecuteNonQuery(dropCommand);
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
        }

        public static string AddDatabaseNameToConnectionString(string databaseName, string connectionString, ILogService logService)
        {
            if (String.IsNullOrWhiteSpace(databaseName))
            {
                return connectionString;
            }

            try
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = databaseName
                };

                return connectionStringBuilder.ToString();
            }
            catch (Exception e) when (e is KeyNotFoundException || e is ArgumentException || e is FormatException)
            {
                logService?.LogMessage($"SqlConnectionStringBuilder: {e.Message}");
            }

            return connectionString;
        }

        private void LogException(Exception exception)
        {
            _logService.LogMessage($"[{exception.GetType()}] {exception.Message}");
        }
    }
}
