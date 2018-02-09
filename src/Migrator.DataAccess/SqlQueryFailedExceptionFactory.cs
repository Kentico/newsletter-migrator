using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrator.DataAccess
{
    internal class SqlQueryFailedExceptionFactory
    {
        private const string MESSAGE_FORMAT = "SQL query failed with the following error: [{0}] {1}. See InnerException for details.";

        public static SqlQueryFailedException Create(Exception innerException)
        {
            return new SqlQueryFailedException(String.Format(MESSAGE_FORMAT, innerException.GetType(), innerException.Message), innerException);
        }
    }
}
