using System;

using NUnit.Framework;

namespace Migrator.Core.Tests
{
    public class TemplateMigratorTests
    {
        [TestFixture]
        public class ParseRegionName
        {
            [TestCase("", null)]
            [TestCase(null, null)]
            [TestCase(" ", null)]
            [TestCase("region1", "region1")]
            [TestCase("region1:400:300", "region1")]
            [TestCase(" region1", "region1")]
            [TestCase(" region1 :400:300", "region1")]
            public void ParseRegionName_ReturnsCorrectRegionNames(string regionName, string expectedResult)
            {
                var migrator = new TemplateMigrator();
                string actualResult = migrator.ParseRegionName(regionName);

                Assert.That(actualResult, Is.EqualTo(expectedResult));
            }
        }

        [TestFixture]
        public class MigrateTemplate
        {
            [Test]
            public void MigrateTemplate_SourceIsNull_ExceptionThrown()
            {
                var migrator = new TemplateMigrator();

                Assert.That(() => migrator.MigrateTemplate(null), Throws.ArgumentNullException);
            }

            [Test]
            public void MigrateTemplate_CommonTemplate_OutputIsCorrect([Values(TemplateTypeEnum.Valid, TemplateTypeEnum.Empty)] TemplateTypeEnum templateType)
            {
                var migrator = new TemplateMigrator();

                var testData = TemplateDataFactory.Generate(templateType);

                var outcomeTemplate = migrator.MigrateTemplate(testData.Source);

                Assert.That(outcomeTemplate.TemplateCode, Is.EqualTo(testData.Output.TemplateCode));
            }
        }

        [TestFixture]
        public class FixZonePlaceholdersTests
        {
            [TestCase(null, null)]
            [TestCase("", "")]
            [TestCase("<div>$$region:400px$$</div>", "<div>$$region$$</div>")]
            [TestCase("<div>$$region:400px:300px$$</div>", "<div>$$region$$</div>")]
            public void FixZonePlaceholders_DoesReplacement(string sourceCode, string expectedResult)
            {
                var migrator = new TemplateMigrator();
                string actualResult = migrator.FixZonePlaceholders(sourceCode);
                Assert.That(actualResult, Is.EqualTo(expectedResult));
            }
        }
    }
}
