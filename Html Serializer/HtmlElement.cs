using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Html_Serializer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class HtmlElement
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<string> Attributes { get; set; } = new List<string>();

        public List<string> Classes { get; set; } = new List<string>();

        public string InnerHtml { get; set; } = "";

        public HtmlElement Parent { get; set; }

        public List<HtmlElement> Children { get; set; } = new List<HtmlElement>();
       
        public HtmlElement(string name)
        {
            Name = name;
        }

        public static HtmlElement BuildTreeFromHtml(List<string> htmlStrings)
        {
            HtmlElement root = new HtmlElement("");
            HtmlElement currentElement = root;

            foreach (string htmlString in htmlStrings)
            {
                string firstWord = GetFirstWord(htmlString);

                if (firstWord.StartsWith("html/"))
                {
                    break;
                }

                if (firstWord.StartsWith("/"))
                {
                    if (currentElement.Parent != null) 
                    {
                        currentElement = currentElement.Parent; 
                    }
                }
                else if (HtmlHelper.Instance.AllHtmlTags.Contains(firstWord))
                {
                    HtmlElement newElement = new HtmlElement(firstWord);
                    var restOfString = htmlString.Remove(0, firstWord.Length);
                    var attributes = Regex.Matches(restOfString, "([a-zA-Z]+)=\\\"([^\\\"]*)\\\"")
                        .Cast<Match>()
                        .Select(m => $"{m.Groups[1].Value}=\"{m.Groups[2].Value}\"")
                        .ToList();

                    if (attributes.Any(a => a.StartsWith("class")))
                    {
                        var classAttr = attributes.First(a => a.StartsWith("class"));
                        var classes = classAttr.Split('=')[1].Trim('"').Split(' ');
                        newElement.Classes.AddRange(classes);
                    }

                    if (attributes.Any(a => a.StartsWith("id")))
                    {
                        var classAttr = attributes.First(a => a.StartsWith("id"));
                        var classes = classAttr.Split('=')[1].Trim('"').Split(' ');
                        newElement.Id = classes[0];
                        attributes.RemoveAt(0);
                    }

                    if (attributes.Count > 0)
                    {
                        newElement.Attributes = attributes;

                    }
                    newElement.Parent = currentElement;
                    currentElement.Children.Add(newElement);

                    if (htmlString.EndsWith("/") || HtmlHelper.Instance.SelfClosingTags.Contains(firstWord))
                    {
                        currentElement = newElement.Parent;
                    }

                    else
                    {
                        currentElement = newElement;
                    }
                }
                else
                {
                    currentElement.InnerHtml = htmlString;
                }
            }
            return root;
        }

        private static string GetFirstWord(string input)
        {
            return input.Split(' ').First().Trim();
        }

        public void PrintTree(int depth = 0, int maxDepth = 5)
        {
            if (depth > maxDepth)
            {
               /* Console.WriteLine("...");*/ 
                return;
            }
            string indent = new string(' ', depth * 2);
            Console.WriteLine($" {indent}Name: {Name}");
            Console.WriteLine($"{indent}  Id: {Id}");
            Console.WriteLine($"{indent}  Attributes:");
            foreach (var attribute in Attributes)
            {
                Console.WriteLine($"{indent}    {attribute}");
            }
            Console.WriteLine($"{indent}  Classes: {string.Join(", ", Classes)}");
            Console.WriteLine($"{indent}  InnerHtml: {InnerHtml}");

            foreach (var child in Children)
            {
                child.PrintTree(depth + 1, maxDepth);
            }

            //Console.WriteLine($" {indent}/{Name}");
        }

        public IEnumerable<HtmlElement> Descendants()
        {
            Queue<HtmlElement> queue = new Queue<HtmlElement>();
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                HtmlElement current = queue.Dequeue();
                yield return current;
                if (current.Children != null)
                {
                    foreach (var child in current.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
        }

        public IEnumerable<HtmlElement> Ancestors()
        {
            HtmlElement current = this.Parent; 

            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }

        public List<HtmlElement> FindElements(Selector selector)
        {
            List<HtmlElement> elements = new List<HtmlElement>();
            HashSet<HtmlElement> elementshSet = new HashSet<HtmlElement>();
            FindElementsRecursive(this, selector, elements, elementshSet);
            return elements;
        }

        private void FindElementsRecursive(HtmlElement current, Selector selector, List<HtmlElement> elements, HashSet<HtmlElement> elementshSet)
        {
            foreach (var children in current.Descendants())
            {
                if (MatchesSelector(children, selector) && elementshSet.Add(children))
                {
                    if (selector.Child == null)
                    {
                        elements.Add(children);
                    }
                    else
                    {
                        FindElementsRecursive(children, selector.Child, elements, elementshSet);
                    }
                }

            }
        }

        private bool MatchesSelector(HtmlElement element, Selector selector)
        {
            if (element == null || selector == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(selector.TagName) && element.Name != selector.TagName)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(selector.Id) && element.Id != selector.Id)
            {
                return false;
            }

            foreach (var className in selector.Classes)
            {
                if (!element.Classes.Contains(className, StringComparer.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}




