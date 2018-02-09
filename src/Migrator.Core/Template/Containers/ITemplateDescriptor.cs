using System;

namespace Migrator.Core
{
    public interface ITemplateDescriptor
    {
        Guid Guid { get; set; }

        Guid SiteGuid { get; set; }
    }
}