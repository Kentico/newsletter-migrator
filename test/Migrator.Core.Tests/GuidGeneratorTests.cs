using System;

using Migrator.Core.Services;

using NUnit.Framework;

namespace Migrator.Core.Tests
{
    public class GuidGeneratorTests
    {
        [TestFixture]
        public class GetGuidTests
        {
            [Test]
            public void GetGuid_ReturnsNonEmptyGuid()
            {
                var generator = new GuidGenerator();
                Guid actualValue = generator.GetGuid();

                Assert.Multiple(() =>
                {
                    Assert.That(actualValue, Is.Not.Null);
                    Assert.That(actualValue, Is.InstanceOf<Guid>());
                    Assert.That(actualValue, Is.Not.EqualTo(Guid.Empty));
                });
            }
        }
    }
}
