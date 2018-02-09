using System;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;

namespace Migrator.Core.Tests
{
    internal class TemplateDataFactory
    {
        public static TemplateTestsData Generate(TemplateTypeEnum type)
        {
            switch (type)
            {
                case TemplateTypeEnum.Empty:
                    return CreateTestsData("empty");
                case TemplateTypeEnum.Valid:
                    return CreateTestsData("valid");
                default:
                    break;
            }

            return null;
        }

        private static TemplateTestsData CreateTestsData(string name)
        {
            return new TemplateTestsData
            {
                Source = CreateSourceTemplateFromJson(name),
                Output = CreateResultTemplateFromJson(name),
            };
        }

        private static TemplateSource CreateSourceTemplateFromJson(string name)
        {
            string resourceName = GetResourceName(name, "SourceTemplates");
            string templateJson = EmbeddedResourceHelper.ReadTextFromResource(resourceName);
            var serializer = new JavaScriptSerializer();

            return serializer.Deserialize<TemplateSource>(templateJson);
        }

        private static TemplateOutput CreateResultTemplateFromJson(string name)
        {
            string resourceName = GetResourceName(name, "ResultTemplates");
            string templateJson = EmbeddedResourceHelper.ReadTextFromResource(resourceName);
            var serializer = new JavaScriptSerializer();

            return serializer.Deserialize<TemplateOutput>(templateJson);
        }

        private static string GetResourceName(string fileName, string folder)
        {
            if (String.IsNullOrEmpty(fileName) || String.IsNullOrEmpty(folder))
            {
                return String.Empty;
            }

            return $"{typeof(TemplateDataFactory).Namespace}.Data.TemplateMigration.{folder}.{fileName}.json";
        }
    }
}
