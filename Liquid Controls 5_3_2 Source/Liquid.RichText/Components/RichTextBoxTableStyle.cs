using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;

using Liquid.Components;
using Liquid.Components.Internal;

namespace Liquid
{
    public partial class RichTextBoxTableStyle
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the table style ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the background brush
        /// </summary>
        public Brush Background { get; set; }

        /// <summary>
        /// Gets or sets the cell fill brush
        /// </summary>
        public Brush CellFill { get; set; }

        /// <summary>
        /// Gets or sets the header fill brush
        /// </summary>
        public Brush HeaderFill { get; set; }

        /// <summary>
        /// Gets or sets the overall border brush
        /// </summary>
        public Brush BorderBrush { get; set; }

        /// <summary>
        /// Gets or sets the overall border thickness
        /// </summary>
        public Thickness BorderThickness { get; set; }

        /// <summary>
        /// Gets or sets the cell border brush
        /// </summary>
        public Brush CellBorderBrush { get; set; }

        /// <summary>
        /// Gets or sets the cell border thickness
        /// </summary>
        public Thickness CellBorderThickness { get; set; }

        /// <summary>
        /// Gets or sets the cell padding
        /// </summary>
        public double CellPadding { get; set; }

        #endregion

        #region Constructor

        public RichTextBoxTableStyle()
        {
            ID = string.Empty;
        }

        public RichTextBoxTableStyle(string id, Brush background, Brush cellFill, Brush headerFill, Brush borderBrush, Thickness borderThickness, Brush cellBorderBrush, Thickness cellBorderThickness, double cellPadding)
        {
            ID = id;
            Background = background;
            CellFill = cellFill;
            HeaderFill = headerFill;
            BorderBrush = borderBrush;
            BorderThickness = borderThickness;
            CellBorderBrush = cellBorderBrush;
            CellBorderThickness = cellBorderThickness;
            CellPadding = cellPadding;
        }

        public RichTextBoxTableStyle(XmlReader reader)
        {
            string temp;

            SetProperty("ID", reader.GetAttribute("ID"));
            SetProperty("BorderBrush", reader.GetAttribute("BorderBrush"));
            SetProperty("BorderThickness", reader.GetAttribute("BorderThickness"));
            SetProperty("CellBorderBrush", reader.GetAttribute("CellBorderBrush"));
            SetProperty("CellBorderThickness", reader.GetAttribute("CellBorderThickness"));
            SetProperty("CellPadding", reader.GetAttribute("CellPadding"));

            while (reader.Read())
            {
                temp = reader.Name.ToLower();

                if (temp == RichTextBox.TableStyleElement && reader.NodeType == XmlNodeType.EndElement)
                {
                    reader.Read();
                    break;
                }

                switch (temp)
                {
                    case "background":
                        Background = (Brush)Utility.CreateFromXaml(reader.ReadInnerXml().Trim());
                        break;
                    case "headerfill":
                        HeaderFill = (Brush)Utility.CreateFromXaml(reader.ReadInnerXml().Trim());
                        break;
                    case "cellfill":
                        CellFill = (Brush)Utility.CreateFromXaml(reader.ReadInnerXml().Trim());
                        break;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies the style to a provided Table object
        /// </summary>
        /// <param name="table">A Table object</param>
        public void ApplyToTable(Table table)
        {
            table.Tag = ID;
            table.Background = Background;
            table.CellFill = CellFill;
            table.HeaderFill = HeaderFill;
            table.BorderBrush = BorderBrush;
            table.BorderThickness = BorderThickness;
            table.CellBorderBrush = CellBorderBrush;
            table.CellBorderThickness = CellBorderThickness;
            table.CellPadding = new Thickness(CellPadding);
        }

        /// <summary>
        /// Sets the specified property to a value
        /// </summary>
        /// <param name="key">Property name</param>
        /// <param name="value">Property value</param>
        public void SetProperty(string key, string value)
        {
            if (value != null)
            {
                switch (key)
                {
                    case "ID":
                        ID = value;
                        break;
                    case "Background":
                        Background = Utility.GetBrush(value);
                        break;
                    case "CellFill":
                        CellFill = Utility.GetBrush(value);
                        break;
                    case "HeaderFill":
                        HeaderFill = Utility.GetBrush(value);
                        break;
                    case "BorderBrush":
                        BorderBrush = Utility.GetBrush(value);
                        break;
                    case "BorderThickness":
                        BorderThickness = Utility.ParseThickness(value, true);
                        break;
                    case "CellBorderBrush":
                        CellBorderBrush = Utility.GetBrush(value);
                        break;
                    case "CellBorderThickness":
                        CellBorderThickness = Utility.ParseThickness(value, true);
                        break;
                    case "CellPadding":
                        CellPadding = double.Parse(value, CultureInfo.InvariantCulture);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the style as an HTML style
        /// </summary>
        /// <returns>HTML CSS style tags</returns>
        public Dictionary<string, Dictionary<string, string>> ToHTML()
        {
            Dictionary<string, Dictionary<string, string>> results = new Dictionary<string, Dictionary<string, string>>();
            string htmlColor;
            string id = ID;

            // The main table style
            results.Add(id, new Dictionary<string, string>());
            htmlColor = Serialize.BrushHex6(Background);
            if (htmlColor.Length > 0)
            {
                results[id].Add("background", htmlColor);
            }
            results[id].Add("border-collapse", "collapse");
            results[id].Add("border", BorderThickness.Left.ToString() + "px solid " + Serialize.BrushHex6(BorderBrush));

            // The header style
            id = ID + " th";
            results.Add(id, new Dictionary<string, string>());
            results[id].Add("padding", CellPadding.ToString(CultureInfo.InvariantCulture) + "px");
            results[id].Add("vertical-align", "top");
            results[id].Add("text-align", "left");
            htmlColor = Serialize.BrushHex6(HeaderFill);
            if (htmlColor.Length > 0)
            {
                results[id].Add("background", htmlColor);
            }
            results[id].Add("border", CellBorderThickness.Left.ToString() + "px solid " + Serialize.BrushHex6(CellBorderBrush));

            // The normal cell style
            id = ID + " td";
            results.Add(id, new Dictionary<string, string>());
            results[id].Add("padding", CellPadding.ToString(CultureInfo.InvariantCulture) + "px");
            results[id].Add("vertical-align", "top");
            results[id].Add("text-align", "left");

            htmlColor = Serialize.BrushHex6(CellFill);
            if (htmlColor.Length > 0)
            {
                results[id].Add("background", htmlColor);
            }
            results[id].Add("border", CellBorderThickness.Left.ToString() + "px solid " + Serialize.BrushHex6(CellBorderBrush));

            return results;
        }

        /// <summary>
        /// Creates XML for a given style
        /// </summary>
        /// <returns>Style in XML format</returns>
        public string ToXML()
        {
            StringBuilder result = new StringBuilder();

            result.Append("<TableStyle ID=\"" + ID + "\"");
            result.Append(Serialize.Brush(BorderBrush, " BorderBrush", false));
            result.Append(" BorderThickness=\"" + BorderThickness.ToString() + "\"");
            result.Append(Serialize.Brush(CellBorderBrush, " CellBorderBrush", false));
            result.Append(" CellBorderThickness=\"" + CellBorderThickness.ToString() + "\"");
            result.Append(" CellPadding=\"" + CellPadding.ToString(CultureInfo.InvariantCulture) + "\"");
            result.Append(">\r");
            result.Append(Serialize.Brush(Background, "Background", true));
            result.Append(Serialize.Brush(CellFill, "CellFill", true));
            result.Append(Serialize.Brush(HeaderFill, "HeaderFill", true));
            result.Append("</TableStyle>\r");

            return result.ToString();
        }

        /// <summary>
        /// Converts a CSS style to a RichText style
        /// </summary>
        /// <param name="id">Style ID</param>
        /// <param name="style">CSS style</param>
        /// <returns>RichText styles</returns>
        public void FromHTML(string id, string style)
        {
            string[] split2;
            string[] split3;
            string tag = string.Empty;

            split2 = style.Trim().Split('{');
            if (split2.Length > 1)
            {
                if (split2[0].Contains(id + " th"))
                {
                    tag = "th";
                }
                else if (split2[0].Contains(id + " td"))
                {
                    tag = "td";
                }

                ID = id;
                split3 = split2[1].Split(';');
                foreach (string a in split3)
                {
                    switch (tag)
                    {
                        case "td":
                            SetProperty(a, "CellFill", "CellBorderBrush", "CellBorderThickness");
                            break;
                        case "th":
                            SetProperty(a, "HeaderFill", "CellBorderBrush", "CellBorderThickness");
                            break;
                        default:
                            SetProperty(a, "Background", "BorderBrush", "BorderThickness");
                            break;
                    }
                }
            }

            //BorderThickness = new Thickness(BorderThickness.Left + 1);
        }

        #endregion

        #region Private Methods

        private void SetProperty(string property, string backgroundProperty, string borderProperty, string borderThicknessProperty)
        {
            string[] split = property.Trim().Split(':');
            string[] split2;

            if (split.Length > 1)
            {
                switch (split[0].ToLower())
                {
                    case "background":
                        SetProperty(backgroundProperty, split[1]);
                        break;
                    case "border":
                        split2 = split[1].Split(' ');
                        SetProperty(borderThicknessProperty, Regex.Replace(split2[0], "px", "", RegexOptions.IgnoreCase));
                        SetProperty(borderProperty, split2[2]);
                        break;
                    case "padding":
                        SetProperty("CellPadding", Regex.Replace(split[1], "px", "", RegexOptions.IgnoreCase));
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
