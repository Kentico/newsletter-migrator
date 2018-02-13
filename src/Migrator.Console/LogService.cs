using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Migrator.Core;

namespace NewsletterMigrator
{
    public class LogService : ILogService
    {
        public void LogMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
