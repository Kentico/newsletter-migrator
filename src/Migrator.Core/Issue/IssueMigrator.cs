using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

using Migrator.Core.Issue.ViewModels;
using Migrator.Core.Services;

namespace Migrator.Core
{
    public class IssueMigrator
    {
        private readonly WidgetCreationSettings _widgetSettings = null;
        private readonly ILogService _logService;

        public IssueMigrator(WidgetCreationSettings widgetSettings, ILogService logService)
        {
            _widgetSettings = widgetSettings;
            _logService = logService;
        }

        /// <summary>
        /// Parses the issue text of <paramref name="issue"/> and returns a collection of regions with their content.
        /// </summary>
        /// <param name="issue">Issue source</param>
        /// <returns>Collection of regions with their content</returns>
        /// <exception cref="ArgumentNullException"><paramref name="issue"/> is null</exception>
        internal IEnumerable<IssueRegionContent> GetRegions(IssueSource issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            var doc = new XmlDocument();

            try
            {
                doc.LoadXml(issue.IssueText);
            }
            catch (XmlException exception)
            {
                _logService.LogMessage($"Error during loading XML from issue: [{exception.GetType()}] {exception.Message}");
                return Enumerable.Empty<IssueRegionContent>();
            }

            var regions = new List<IssueRegionContent>();

            foreach (XmlNode regionNode in doc.SelectNodes("//region"))
            {
                var region = new IssueRegionContent
                {
                    RegionName = regionNode.Attributes["id"].Value,
                    Content = regionNode.InnerText
                };

                regions.Add(region);
            }

            return regions;
        }

        public IEnumerable<IssueOutput> MigrateIssues(IEnumerable<IssueSource> sourceIssues)
        {
            var guidGenerator = new GuidGenerator();

            return sourceIssues.Select(i => MigrateIssue(i, guidGenerator)).ToList();
        }

        public IssueOutput MigrateIssue(IssueSource source, IGuidGenerator guidGenerator)
        {
            // For each region in the original issue corresponds to single zone with single widget in the migrated issue.

            var zoneConfiguration = new ZonesConfiguration();

            foreach (var region in GetRegions(source))
            {
                var zone = new Zone(region.RegionName);
                zoneConfiguration.Zones.Add(zone);
                var widget = CreateWidgetWithSingleProperty(_widgetSettings.PropertyName, region.Content, guidGenerator.GetGuid(), _widgetSettings.TypeIdentifier);
                zone.Widgets.Add(widget);
            }

            return new IssueOutput
            {
                IssueGuid = source.IssueGuid,
                Widgets = SerializeZoneConfiguration(zoneConfiguration),
            };
        }

        private string SerializeZoneConfiguration(ZonesConfiguration zoneConfiguration)
        {
            if (zoneConfiguration == null)
            {
                return null;
            }

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter))
            {
                var serializer = new DataContractSerializer(typeof(ZonesConfiguration));
                serializer.WriteObject(xmlWriter, zoneConfiguration);

                xmlWriter.Flush();
                return stringWriter.ToString();
            }
        }

        internal Widget CreateWidgetWithSingleProperty(string propertyName, string propertyValue, Guid widgetGuid, Guid typeIdentifier)
        {
            var widget = new Widget(widgetGuid, typeIdentifier);
            var properties = widget.Properties ?? new List<WidgetProperty>();
            properties.Add(new WidgetProperty(propertyName, propertyValue));

            return widget;
        }
    }
}
