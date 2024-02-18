using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Html_Serializer
{
    public class Selector
    {
        public string TagName { get; set; }
        
        public string Id { get; set; }
        
        public List<string> Classes { get; set; }
        
        public Selector Parent { get; private set; }
        
        public Selector Child { get; private set; }

        public Selector()
        {
            Classes = new List<string>();
        }

        public static Selector FromQueryString(string queryString)
        {
            string[] levels = queryString.Split(' ');

            Selector rootSelector = new Selector();
            Selector currentSelector = rootSelector;

            foreach (string level in levels)
            {
                string[] parts = Regex.Split(level, @"(?=[#.\s])");
                
                foreach (string part in parts)
                {
                    if (!string.IsNullOrWhiteSpace(part))
                    {
                        if (part.StartsWith("#"))
                        {
                            currentSelector.Id = part.Substring(1);
                        }
                        else if (part.StartsWith("."))
                        {
                            currentSelector.Classes.Add(part.Substring(1));
                        }
                        else
                        {
                            bool flag = IsValidTagName(part);
                            if (flag && currentSelector != null && string.IsNullOrEmpty(currentSelector.TagName))
                            {
                                currentSelector.TagName = part;
                            }
                            else if (flag)
                            {
                                Selector newSelector = new Selector();
                                newSelector.Parent = currentSelector;
                                currentSelector.Child = newSelector;
                                newSelector.TagName = part;
                                currentSelector = newSelector;
                            }
                        }
                    }
                }
            }
            return rootSelector;
        }

        private static bool IsValidTagName(string tagName)
        {
            return !string.IsNullOrWhiteSpace(tagName) && HtmlHelper.Instance.AllHtmlTags.Contains(tagName);
        }

        public override string ToString()
        {
            return $"TagName: {TagName}\nId: {Id}\nClasses: {string.Join(", ", Classes)}";
        }

        public static void PrintSelectorTree(Selector root, string indent = "")
        {
            if (root != null)
            {
                Console.WriteLine($"{indent}{root}");
                if (root.Child != null)
                {
                    PrintSelectorTree(root.Child, indent + "  ");
                }
            }
        }
    }
}

