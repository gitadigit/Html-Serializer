using System.IO;
using System.IO.Pipes;
using System.Text.Json;

namespace Html_Serializer
{
    public class HtmlHelper
    {
        public string[] AllHtmlTags { get;  set; }

        public string[] SelfClosingTags { get;  set; }

        private readonly static HtmlHelper _instance = new HtmlHelper();
        public static HtmlHelper Instance => _instance;
        private HtmlHelper() {
            AllHtmlTags = loadTagFromFile("File/HtmlTags.json");
            SelfClosingTags = loadTagFromFile("File/HtmlVoidTags.json");
        }

        private string[] loadTagFromFile(string file)
        {
            string allTagsJson = File.ReadAllText(file);
            return JsonSerializer.Deserialize<string[]>(allTagsJson);
        }
    }
}
