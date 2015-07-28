using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;

namespace hpMvc.Services.HtmlSanitizer
{
    public class HtmlSanitizer
    {
        public HashSet<string> BlackList = new HashSet<string>()
        {
                { "script" },
                { "iframe" },
                { "form" },
                { "object" },
                { "embed" },
                { "link" },                
                { "head" },
                { "meta" }
        };

        public HashSet<string> HrefWhiteList = new HashSet<string>()
        {
                { "" },
                {"#"}
        };

        public bool Sanitize(string html, out string message)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            if (!IsSanitizedHtmlNode(doc.DocumentNode, out message))
            {
                return false;
            }
            return true;
        }

        private bool IsSanitizedHtmlNode(HtmlNode node, out string message)
        {
            message = "";
            if (node.NodeType == HtmlNodeType.Element)
            {
                // check for blacklist items and remove
                if (BlackList.Contains(node.Name))
                {
                    message = node.Name + " is not allowed";
                    return false;
                }


            }

            // remove CSS Expressions and embedded script links
            if (node.Name == "style")
            {
                if (string.IsNullOrEmpty(node.InnerText))
                {
                    if (node.InnerHtml.Contains("expression") || node.InnerHtml.Contains("javascript:"))
                    {
                        message = node.Name + " cannot contain an expression or javascript";
                        return false;
                    }
                }
            }

            // remove script attributes
            if (node.HasAttributes)
            {
                for (int i = node.Attributes.Count - 1; i >= 0; i--)
                {
                    HtmlAttribute currentAttribute = node.Attributes[i];

                    var attr = currentAttribute.Name.ToLower();
                    var val = currentAttribute.Value.ToLower();

                    // remove event handlers
                    if (attr.StartsWith("on"))
                    {
                        message = attr + " cannot contain an event handler";
                        return false;
                    }

                    // remove script links
                    else if ((attr == "href" || attr == "src" || attr == "dynsrc" || attr == "lowsrc") && val.Contains("javascript:"))
                    {
                        message = attr + " cannot contain javascript";
                        return false;
                    }
                    // Remove CSS Expressions
                    else if (attr == "style" &&
                             val.Contains("expression") || val.Contains("javascript:") || val.Contains("vbscript:"))
                    {
                        message = attr + " cannot contain an expression";
                        return false;
                    }
                    if (attr == "href")
                    {
                        if (val.StartsWith("https://halfpintstudy.org"))
                            return true;

                        if (!HrefWhiteList.Contains(val))
                        {
                            message = "invalid href attribute";
                            return false;
                        }
                    }

                    if (attr == "src")
                    {
                        if (!val.StartsWith("content/images"))
                        {
                            message = "invalid src attribute";
                            return false;
                        }
                    }
                }


            }

            // Look through child nodes recursively
            if (node.HasChildNodes)
            {
                for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
                {
                    if (!IsSanitizedHtmlNode(node.ChildNodes[i], out message))
                        return false;
                }
            }


            return true;
        }
    }
}