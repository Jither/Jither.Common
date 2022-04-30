using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Jither.Build
{
    public class BumpVersion : Task
    {
        [Required]
        public ITaskItem[] VersionFiles { get; set; }

        public override bool Execute()
        {
            foreach (var versionFile in VersionFiles)
            {
                string path = versionFile.ItemSpec;
                Log.LogMessage($"Bumping {path}...");
                if (!File.Exists(path))
                {
                    Log.LogError($"File not found: {path}");
                }

                var doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
                var eleVersion = FindVersionElement(doc, path);
                if (eleVersion == null)
                {
                    return false;
                }

                string newVersion = Bump(eleVersion.Value);

                eleVersion.Value = newVersion;

                using (var writer = XmlWriter.Create(path, new XmlWriterSettings { OmitXmlDeclaration = true }))
                {
                    doc.Save(writer);
                    writer.Flush();
                }
            }
            return true;
        }

        private string Bump(string version)
        {
            var parts = version.Split('.');
            int major = GetVersionPart(parts, 0) ?? 0;
            int minor = GetVersionPart(parts, 1) ?? 0;
            int build = GetVersionPart(parts, 2) ?? 0;
            //int revision = GetVersionPart(parts, 3) ?? 0;

            Log.LogMessage(MessageImportance.Normal, $"Old version: {version}");
            build++;
            var now = DateTimeOffset.UtcNow;
            int revision = (now.Year % 100) * 1000 + now.DayOfYear;

            version = $"{major}.{minor}.{build}.{revision}";
            Log.LogMessage(MessageImportance.Normal, $"New version: {version}");
            return version;
        }

        private int? GetVersionPart(string[] parts, int index)
        {
            return parts.Length > index ? Convert.ToInt32(parts[index]) : (int?)null;
        }

        private XElement FindVersionElement(XDocument doc, string path)
        {
            var propGroups = doc.Element("Project")?.Elements("PropertyGroup");
            if (propGroups == null)
            {
                Log.LogError($"No <PropertyGroup>(s) in file: {path}");
                return null;
            }

            var elesVersion = propGroups.Elements("Version");
            if (!elesVersion.Any())
            {
                elesVersion = propGroups.Elements("VersionPrefix");
            }

            var result = elesVersion.First();
            if (result == null)
            {
                Log.LogError($"No <Version> or <VersionPrefix> in file: {path}");
                return null;
            }

            return result;
        }
    }
}
