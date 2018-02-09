using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Migrator.DataAccess
{
    public class SqlQueryFailedException : Exception
    {
        public SqlQueryFailedException()
        {
        }

        public SqlQueryFailedException(string message) : base(message)
        {
        }

        public SqlQueryFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SqlQueryFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
