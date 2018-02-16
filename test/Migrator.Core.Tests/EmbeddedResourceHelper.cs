using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Migrator.Core.Tests
{
    internal class EmbeddedResourceHelper
    {
        public static string ReadTextFromResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string result = null;

            using (Stream stream = assembly.GetManifestResourceStream(name))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }
    }
}
