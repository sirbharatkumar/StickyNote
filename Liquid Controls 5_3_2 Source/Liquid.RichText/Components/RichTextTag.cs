using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace Liquid
{
    public partial class RichTextTag : IComparable
    {
        public string StyleID { get; set; }

        public object Tag { get; set; }

        public ContentMetadata Metadata { get; set; } 

        public RichTextTag()
        {
            StyleID = string.Empty;
            Tag = null;
            Metadata = null;
        }

        public RichTextTag(string styleID)
        {
            StyleID = styleID;
            Tag = null;
            Metadata = null;
        }

        public RichTextTag(string styleID, object tag)
        {
            StyleID = styleID;
            Tag = tag;
            Metadata = null;
        }

        public RichTextTag(XmlReader reader)
        {
            Load(reader);
        }

        public void Load(XmlReader reader)
        {
            Metadata = new ContentMetadata();

            StyleID = reader.GetAttribute("Style");

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Meta")
                {
                    Metadata.Add(reader.GetAttribute("Key"), reader.GetAttribute("Value"));
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Tag")
                {
                    reader.Read();
                    break;
                }
            }
        }

        /// <summary>
        /// Serializes the Tag to XML
        /// </summary>
        /// <returns></returns>
        public string ToXML()
        {
            StringBuilder result = new StringBuilder();

            result.Append("<Tag");
            if (StyleID != null && StyleID != string.Empty)
            {
                result.Append(" Style=\"" + StyleID + "\"");
            }
            result.Append(">\r");

            if (Metadata != null)
            {
                foreach (KeyValuePair<string, string> pair in Metadata)
                {
                    result.Append("<Meta Key=\"" + pair.Key + "\" Value=\"" + pair.Value + "\" />\r");
                }
            }

            result.Append("</Tag>\r");

            return result.ToString();
        }

        public int CompareTo(object obj)
        {
            RichTextTag p = obj as RichTextTag;

            if (Metadata == p.Metadata)
            {
                return string.Compare(StyleID, p.StyleID);
            }
            else
            {
                return 1;
            }
        }

        public override string ToString()
        {
            return StyleID;
        }
    }
}
