using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Migrator.Core
{
    public class TemplateMigrator
    {
        private readonly Regex regionNameRegex = new Regex("\\$\\$([^$]+)\\$\\$", RegexOptions.Compiled);

        internal TemplateOutput MigrateTemplate(TemplateSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var templateBodyBuilder = new StringBuilder();

            if (!String.IsNullOrWhiteSpace(source.Header))
            {
                templateBodyBuilder.AppendLine(source.Header);
            }

            if (!String.IsNullOrWhiteSpace(source.Body))
            {
                string fixedBody = FixZonePlaceholders(source.Body);
                templateBodyBuilder.AppendLine(fixedBody);
            }

            if (!String.IsNullOrWhiteSpace(source.Footer))
            {
                templateBodyBuilder.Append(source.Footer); 
            }

            templateBodyBuilder.Replace("</head>", $"<style type=\"text/css\">{Environment.NewLine}{source.StylesheetText}{Environment.NewLine}</style>{Environment.NewLine}\t</head>");

            return new TemplateOutput { Guid = source.Guid, TemplateCode = templateBodyBuilder.ToString(), SiteGuid = source.SiteGuid };
        }

        public IEnumerable<TemplateOutput> MigrateTemplates(IEnumerable<TemplateSource> sourceTemplates)
        {
            return sourceTemplates.Select(MigrateTemplate);
        }

        internal string FixZonePlaceholders(string code)
        {
            if (String.IsNullOrWhiteSpace(code))
            {
                return code;
            }

            return regionNameRegex.Replace(code, m =>
            {
                string zoneName = ParseRegionName(m.Groups[1].Value);
                return $"$${zoneName}$$";
            });
        }
        
        internal string ParseRegionName(string regionExpression)
        {
            if (String.IsNullOrWhiteSpace(regionExpression))
            {
                return null;
            }

            int colonIndex = regionExpression.IndexOf(':');
            return (colonIndex > 0) ? regionExpression.Substring(0, colonIndex).Trim() : regionExpression.Trim();
        }
    }
}
