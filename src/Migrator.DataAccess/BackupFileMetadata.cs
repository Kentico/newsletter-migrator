using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrator.DataAccess
{
    internal class BackupFileMetadata
    {
        public string LogicalName { get; set; }

        public string Type { get; set; }

        public string Location { get; set; }
    }
}
