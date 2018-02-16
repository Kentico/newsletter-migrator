using System;

namespace Migrator.Core.Services
{
    internal class GuidGenerator : IGuidGenerator
    {
        public Guid GetGuid()
        {
            return Guid.NewGuid();
        }
    }
}
