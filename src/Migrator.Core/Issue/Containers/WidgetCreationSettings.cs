using System;

namespace Migrator.Core
{
    public class WidgetCreationSettings
    {
        public string PropertyName { get; set; }

        public Guid TypeIdentifier { get; set; }

        public Guid SiteGuid { get; set; }
    }
}