using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrator.Core
{
    public class IssueSource
    {
        public Guid IssueGuid { get; set; }
        public Guid TemplateGuid { get; set; }
        public string IssueText { get; set; }
    }
}
