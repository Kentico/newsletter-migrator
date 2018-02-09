using System;

namespace Migrator.Core
{
    public class TemplateOutput : ITemplateDescriptor
    {
        public Guid Guid { get; set; }

        public string TemplateCode { get; set; }

        public Guid SiteGuid { get; set; }
    }
}