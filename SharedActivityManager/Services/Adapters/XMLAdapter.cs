using System.Xml;
using System.Xml.Serialization;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Adapters
{
    /// <summary>
    /// Adapter pentru format XML
    /// </summary>
    public class XMLAdapter : BaseFileAdapter
    {
        public override string FormatName => "XML";
        public override string[] SupportedExtensions => new[] { ".xml" };

        public override async Task<List<Activity>> ImportAsync(string filePath)
        {
            if (!FileExists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var serializer = new XmlSerializer(typeof(ActivityList));

            using var reader = new StreamReader(filePath);
            var activityList = (ActivityList)serializer.Deserialize(reader);

            return activityList?.Activities ?? new List<Activity>();
        }

        public override async Task ExportAsync(string filePath, List<Activity> activities)
        {
            var activityList = new ActivityList
            {
                ExportDate = DateTime.Now,
                Activities = activities
            };

            var serializer = new XmlSerializer(typeof(ActivityList));

            using var writer = new StreamWriter(filePath);
            serializer.Serialize(writer, activityList);

            await Task.CompletedTask;
        }

        [XmlRoot("Activities")]
        public class ActivityList
        {
            [XmlAttribute("ExportDate")]
            public DateTime ExportDate { get; set; }

            [XmlElement("Activity")]
            public List<Activity> Activities { get; set; }
        }
    }
}