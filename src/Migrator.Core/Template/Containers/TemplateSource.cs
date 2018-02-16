using System;

namespace Migrator.Core
{
    public class TemplateSource : ITemplateDescriptor
    {
        public Guid Guid { get; set; }

        public string Body { get; set; }

        public string Header { get; set; }

        public string Footer { get; set; }

        public string StylesheetText { get; set; }

        public Guid SiteGuid { get; set; }
    }
}