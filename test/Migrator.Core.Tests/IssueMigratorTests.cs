using System;
using System.Collections.Generic;
using System.Linq;

using Migrator.Core.Issue.ViewModels;
using Migrator.Core.Services;

using NSubstitute;
using NUnit.Framework;

namespace Migrator.Core.Tests
{
    public class IssueMigratorTests
    {
        [TestFixture]
        public class MigrateIssue
        {
            private const string SOURCE = "Source";
            private const string RESULT = "Result";

            [TestCase("Empty1")]
            [TestCase("Empty2")]
            [TestCase("ValidIssue1")]
            [TestCase("ValidIssue2")]
            [TestCase("Invalid")]
            public void MigrateIssue_MultipleRegions_MigratesCorrectly(string testFileName)
            {
                Guid issueGuid = new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
                Guid typeGuid = new Guid(2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2);
                var logService = Substitute.For<ILogService>();

                string issueContent = GetFileContent(testFileName, SOURCE);

                var migrator = new IssueMigrator(new WidgetCreationSettings
                {
                    PropertyName = "Content",
                    TypeIdentifier = typeGuid
                }, logService);

                var guidGenerator = Substitute.For<IGuidGenerator>();
                guidGenerator.GetGuid().Returns(new Guid(3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3));

                var actualResult = migrator.MigrateIssue(new IssueSource
                {
                    IssueGuid = issueGuid,
                    IssueText = issueContent,
                }, guidGenerator);

                string expectedContent = GetFileContent(testFileName, RESULT);

                Assert.Multiple(() => 
                {
                    Assert.That(actualResult.IssueGuid, Is.EqualTo(issueGuid));
                    Assert.That(actualResult.Widgets, Is.EqualTo(expectedContent));
                });
            }

            private string GetFileContent(string fileName, string folder)
            {
                return EmbeddedResourceHelper.ReadTextFromResource($"{typeof(MigrateIssue).Namespace}.Data.IssueMigration.{folder}.{fileName}.xml");
            }
        }

        [TestFixture]
        public class GetRegions
        {
            [Test]
            public void GetRegions_ArgumentIsNull_ThrowsException()
            {
                var logService = Substitute.For<ILogService>();
                var issueMigrator = new IssueMigrator(new WidgetCreationSettings(), logService);
                Assert.That(() => issueMigrator.GetRegions(null), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("issue"));
            }

            [Test]
            public void GetRegions_InvalidXml_DoesNotThrowException()
            {
                string message = null;
                var logService = Substitute.For<ILogService>();
                logService.LogMessage(Arg.Do<string>(x => message = x));

                var issueMigrator = new IssueMigrator(new WidgetCreationSettings(), logService);
                var issueSource = new IssueSource
                {
                    IssueGuid = Guid.NewGuid(),
                    IssueText = "xxx",
                    TemplateGuid = Guid.NewGuid()
                };

                Assert.Multiple(() =>
                {
                    Assert.That(() => issueMigrator.GetRegions(issueSource), Throws.Nothing);
                    Assert.That(message, Is.Not.Empty.Or.Null);
                });
            }

            [Test]
            public void GetRegions_ReturnsCorrectRegionNamesAndCount()
            {
                var logService = Substitute.For<ILogService>();
                string issueContent = EmbeddedResourceHelper.ReadTextFromResource($"{typeof(GetRegions).Namespace}.Data.{nameof(GetRegions)}.Issue1.xml");
                var migrator = new IssueMigrator(new WidgetCreationSettings(), logService);

                var actualRegions = migrator.GetRegions(new IssueSource
                {
                    IssueGuid = Guid.NewGuid(),
                    IssueText = issueContent,
                });

                Assert.Multiple(() =>
                {
                    Assert.That(actualRegions.Count(), Is.EqualTo(4));
                    Assert.That(actualRegions.Select(r => r.RegionName), Is.EquivalentTo(new[] { "title", "picture", "teaser", "text" }));
                });
            }
        }

        [TestFixture]
        public class CreateWidgetWithSinglePropertyTests
        {
            [Test]
            public void CreateWidgetWithSingleProperty_CreatedWidgetReflectsConfiguration()
            {
                var logService = Substitute.For<ILogService>();
                var issueMigrator = new IssueMigrator(new WidgetCreationSettings(), logService);
                Guid widgetGuid = new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
                Guid typeGuid = new Guid(2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2);
                var actualResult = issueMigrator.CreateWidgetWithSingleProperty("MyProperty", "MyValue", widgetGuid, typeGuid);

                Assert.Multiple(() =>
                {
                    Assert.That(actualResult.Identifier, Is.EqualTo(widgetGuid));

                    var properties = actualResult.Properties;
                    Assert.That(properties, Is.Not.Null);

                    WidgetProperty property = null;

                    Assert.That(() =>
                    {
                        property = actualResult.Properties.Single(p => p.Name.Equals("MyProperty", StringComparison.CurrentCulture));
                    }, Throws.Nothing);

                    Assert.That(property, Is.Not.Null);
                    Assert.That(property.Value.Equals("MyValue", StringComparison.CurrentCulture));
                });
            }
        }
    }
}
