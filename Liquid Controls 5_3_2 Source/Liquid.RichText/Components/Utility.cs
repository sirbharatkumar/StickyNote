using System;
using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Liquid.Components
{
    public partial class Utility
    {
        #region Static Methods

        /// <summary>
        /// Encodes an XML string
        /// </summary>
        /// <param name="s">String to encode</param>
        /// <returns>Encoded string</returns>
        public static string XmlEncode(string s)
        {
            s = s.Replace("&", "&amp;");
            s = s.Replace("<", "&lt;");
            s = s.Replace(">", "&gt;");
            s = s.Replace("\"", "&quot;");
            s = s.Replace("'", "&apos;");

            return s;
        }

        /// <summary>
        /// Decodes an XML string
        /// </summary>
        /// <param name="s">String to decode</param>
        /// <returns>Decoded string</returns>
        public static string XmlDecode(string s)
        {
            s = s.Replace("&apos;", "'");
            s = s.Replace("&quot;", "\"");
            s = s.Replace("&lt;", "<");
            s = s.Replace("&gt;", ">");
            s = s.Replace("&amp;", "&");

            return s;
        }

        /// <summary>
        /// Parses a string containing a Thickness value and returns a Thickness object
        /// </summary>
        /// <param name="value">String containing a Thickness value</param>
        /// <param name="xaml">Indicates whether it is a XAML Thickness object or not (CSS)</param>
        /// <returns>A Thickness object</returns>
        public static Thickness ParseThickness(string value, bool xaml)
        {
            Thickness result;
            string[] split = (xaml ? value.Split(',') : Regex.Replace(value, "px", "", RegexOptions.IgnoreCase).Split(' '));

            if (split.Length == 1)
            {
                result = new Thickness(double.Parse(split[0]));
            }
            else
            {
                if (xaml)
                {
                    result = new Thickness(double.Parse(split[0]), double.Parse(split[1]), double.Parse(split[2]), double.Parse(split[3]));
                }
                else
                {
                    result = new Thickness(double.Parse(split[3]), double.Parse(split[0]), double.Parse(split[1]), double.Parse(split[2]));
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a XAML Thickness object to a string for use in CSS margins and paddings (Top,Right,Bottom,Left)
        /// </summary>
        /// <param name="thickness">Thickness to convert</param>
        /// <returns>CSS Thickness string</returns>
        public static string ThicknessToCSS(Thickness thickness)
        {
            string result = thickness.Top.ToString(CultureInfo.InvariantCulture) + "px ";

            result += thickness.Right.ToString(CultureInfo.InvariantCulture) + "px ";
            result += thickness.Bottom.ToString(CultureInfo.InvariantCulture) + "px ";
            result += thickness.Left.ToString(CultureInfo.InvariantCulture) + "px";

            return result;
        }

        /// <summary>
        /// Determines whether a style ID is a HTML heading tag
        /// </summary>
        /// <param name="styleID">Style ID</param>
        /// <returns>True if the style is a heading, false if not</returns>
        public static bool IsStyleAHeading(string styleID)
        {
            string temp = styleID.ToLower();

            return (temp == "h1" || temp == "h2" || temp == "h3" || temp == "h4");
        }

        public static Brush GetBrush(string brush)
        {
            Brush result;
            Color tempColor;

            if (brush.StartsWith("#"))
            {
                if (brush.Length == 7)
                {
                    brush = brush.Replace("#", "#ff");
                }
                tempColor = RichTextBoxStyle.StringToColor(brush.TrimStart('#'));
                result = new SolidColorBrush(tempColor);
            }
            else
            {
                result = (Brush)CreateFromXaml(brush);
            }

            return result;
        }

        /// <summary>
        /// Creates an element from the supplied XAML string
        /// </summary>
        /// <param name="xaml">XAML string</param>
        /// <returns>Instantiated element</returns>
        public static object CreateFromXaml(string xaml)
        {
            if (xaml.IndexOf("xmlns=\"http://schemas.microsoft.com/client/2007\"") == -1)
            {
                int index = xaml.IndexOf('>');

                if (index > 0 && xaml[index - 1] == '/')
                {
                    index--;
                }

                xaml = xaml.Insert(index, " xmlns=\"http://schemas.microsoft.com/client/2007\"");
            }

            return XamlReader.Load(xaml);
        }

        #endregion
    }
}
