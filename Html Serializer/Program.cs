using Html_Serializer;
using System.Text.RegularExpressions;

var html = await Load("https://hebrewbooks.org/beis");
var cleanHtml = new Regex("\\r\\n|^$").Replace(html,"");
var htmlLines =new Regex("<(.*?)>").Split(cleanHtml).Where(s=>s.Length>0);
var htmlElemnet = "<div id=\"my-id\" class=\"my-class-1 my-class2\" width=\"100%\">text</div>";
var attributes = new Regex("([^\\s]*?)=\"(.*?)\"").Matches(htmlElemnet);
Console.ReadLine();

async Task<string> Load(string url)
{
    HttpClient client = new HttpClient();
    var response = await client.GetAsync(url);
    var html = await response.Content.ReadAsStringAsync();
    return html;
}

var root = HtmlElement.BuildTreeFromHtml(htmlLines.ToList());
root.PrintTree();

string queryString = "p";
Selector selector = Selector.FromQueryString(queryString);
Console.WriteLine("Full Selector Tree:");
Selector.PrintSelectorTree(selector);
List<HtmlElement> list = root.FindElements(selector);
await Console.Out.WriteLineAsync("match element: " + list.Count());

foreach (var element in list)
{
    Console.WriteLine(element.Name);
}