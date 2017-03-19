using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;

using Liquid.Components;
using Liquid.Components.Internal;
using System.Text.RegularExpressions;

namespace Liquid
{
    public partial class RichTextBoxStyle
    {
        public string ID { get; set; }
        public string Family { get; set; }
        public double? Size { get; set; }
        public Brush Background { get; set; }
        public Brush Foreground { get; set; }
        public Brush BorderBrush { get; set; }
        public Brush ShadowBrush { get; set; }
        public FontWeight? Weight { get; set; }
        public FontStyle? Style { get; set; }
        public TextDecorationCollection Decorations { get; set; }
        public HorizontalAlignment Alignment { get; set; }
        public RichTextSpecialFormatting Special { get; set; }
        public TextBlockPlusEffect Effect { get; set; }
        public BorderEffect BorderType { get; set; }
        public ShadowEffect Shadow { get; set; }
        public Thickness Margin { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }

        public RichTextBoxStyle()
        {
            ID = string.Empty;
            Family = string.Empty;
            Margin = new Thickness();
            VerticalAlignment = VerticalAlignment.Center;
            Weight = FontWeights.Normal;
        }

        public RichTextBoxStyle(string id, string family, double? size, Brush background, Brush foreground, Brush borderBrush, Brush shadowBrush, FontWeight? weight, FontStyle? style, TextDecorationCollection decorations, HorizontalAlignment alignment, RichTextSpecialFormatting special, TextBlockPlusEffect effect, BorderEffect border, ShadowEffect shadow, Thickness margin, VerticalAlignment verticalAlign)
        {
            ID = id;
            Family = family;
            Size = size;
            Background = background;
            Foreground = foreground;
            BorderBrush = borderBrush;
            ShadowBrush = shadowBrush;
            Weight = weight;
            Style = style;
            Decorations = decorations;
            Alignment = alignment;
            Special = special;
            Effect = effect;
            BorderType = border;
            Shadow = shadow;
            Margin = margin;
            VerticalAlignment = verticalAlign;
        }

        public RichTextBoxStyle(string id, string family, double? size, FontWeight? weight) :
            this(id, family, size, new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)), null, null, weight, FontStyles.Normal, null, HorizontalAlignment.Left, RichTextSpecialFormatting.None, TextBlockPlusEffect.None, BorderEffect.None, ShadowEffect.None, new Thickness(), VerticalAlignment.Center)
        {
        }

        public RichTextBoxStyle(RichTextBoxStyle style) :
            this(style.ID, style.Family, style.Size, style.Background, style.Foreground, style.BorderBrush, style.ShadowBrush, style.Weight, style.Style, style.Decorations, style.Alignment, style.Special, style.Effect, style.BorderType, style.Shadow, new Thickness(style.Margin.Left, style.Margin.Top, style.Margin.Right, style.Margin.Bottom), style.VerticalAlignment)
        {
        }

        public RichTextBoxStyle(XmlReader reader) :
            this("", "Arial", 14, FontWeights.Normal)
        {
            SetProperty("ID", reader.GetAttribute("ID"));
            SetProperty("FontFamily", reader.GetAttribute("FontFamily"));
            SetProperty("FontSize", reader.GetAttribute("FontSize"));
            SetProperty("FontStyle", reader.GetAttribute("FontStyle"));
            SetProperty("FontWeight", reader.GetAttribute("FontWeight"));
            SetProperty("Decoration", reader.GetAttribute("Decoration"));
            SetProperty("Alignment", reader.GetAttribute("Alignment"));
            SetProperty("Background", reader.GetAttribute("Background"));
            SetProperty("Foreground", reader.GetAttribute("Foreground"));
            SetProperty("BorderBrush", reader.GetAttribute("BorderBrush"));
            SetProperty("ShadowBrush", reader.GetAttribute("ShadowBrush"));
            SetProperty("Special", reader.GetAttribute("Special"));
            SetProperty("Effect", reader.GetAttribute("Effect"));
            SetProperty("Border", reader.GetAttribute("Border"));
            SetProperty("Shadow", reader.GetAttribute("Shadow"));
            SetProperty("Margin", reader.GetAttribute("Margin"));
            SetProperty("VerticalAlignment", reader.GetAttribute("VerticalAlignment"));
        }

        /// <summary>
        /// Sets the specified property to a value
        /// </summary>
        /// <param name="key">Property name</param>
        /// <param name="value">Property value</param>
        public void SetProperty(string key, string value)
        {
            SetProperty(key, value, true);
        }

        /// <summary>
        /// Sets the specified property to a value
        /// </summary>
        /// <param name="key">Property name</param>
        /// <param name="value">Property value</param>
        /// <param name="xaml">Indicates whether the source value is XAML</param>
        public void SetProperty(string key, string value, bool xaml)
        {
            Color tempColor;

            if (value != null)
            {
                switch (key)
                {
                    case "ID":
                        ID = value;
                        break;
                    case "FontFamily":
                        Family = value;
                        break;
                    case "FontSize":
                        Size = double.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "FontStyle":
                        switch (value.ToLower())
                        {
                            case "italic":
                                Style = FontStyles.Italic;
                                break;
                            default:
                                Style = FontStyles.Normal;
                                break;
                        }
                        break;
                    case "FontWeight":
                        switch (value.ToLower())
                        {
                            case "bold":
                                Weight = FontWeights.Bold;
                                break;
                            default:
                                Weight = FontWeights.Normal;
                                break;
                        }
                        break;
                    case "Decoration":
                        switch (value.ToLower())
                        {
                            case "underline":
                                Decorations = TextDecorations.Underline;
                                break;
                            case "line-through":
                                Effect = TextBlockPlusEffect.Strike;
                                break;
                            default:
                                Decorations = null;
                                break;
                        }
                        break;
                    case "Alignment":
                        try
                        {
                            Alignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), value, true);
                        }
                        catch (Exception ex)
                        {
                            Alignment = HorizontalAlignment.Left;
                        }
                        break;
                    case "Background":
                        tempColor = RichTextBoxStyle.StringToColor(value.TrimStart('#'));
                        Background = new SolidColorBrush(tempColor);
                        break;
                    case "Foreground":
                        tempColor = RichTextBoxStyle.StringToColor(value.TrimStart('#'));
                        Foreground = new SolidColorBrush(tempColor);
                        break;
                    case "BorderBrush":
                        tempColor = RichTextBoxStyle.StringToColor(value.TrimStart('#'));
                        BorderBrush = new SolidColorBrush(tempColor);
                        break;
                    case "ShadowBrush":
                        tempColor = RichTextBoxStyle.StringToColor(value.TrimStart('#'));
                        ShadowBrush = new SolidColorBrush(tempColor);
                        break;
                    case "Special":
                        Special = (RichTextSpecialFormatting)Enum.Parse(typeof(RichTextSpecialFormatting), value, true);
                        break;
                    case "Effect":
                        Effect = (TextBlockPlusEffect)Enum.Parse(typeof(TextBlockPlusEffect), value, true);
                        break;
                    case "Border":
                        BorderType = (BorderEffect)Enum.Parse(typeof(BorderEffect), value, true);
                        break;
                    case "Shadow":
                        Shadow = (ShadowEffect)Enum.Parse(typeof(ShadowEffect), value, true);
                        break;
                    case "Margin":
                        Margin = Utility.ParseThickness(value, xaml);
                        break;
                    case "VerticalAlignment":
                        VerticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), Regex.Replace(value, "middle", "Center", RegexOptions.IgnoreCase), true);
                        break;
                }
            }
        }

        public static Color StringToColor(string paramValue)
        {
            byte red;
            byte green;
            byte blue;
            byte alpha;

            paramValue = paramValue.TrimStart('#');

            alpha = (byte.Parse(paramValue.Substring(0, 2), NumberStyles.AllowHexSpecifier));
            red = (byte.Parse(paramValue.Substring(2, 2), NumberStyles.AllowHexSpecifier));
            green = (byte.Parse(paramValue.Substring(4, 2), NumberStyles.AllowHexSpecifier));
            blue = (byte.Parse(paramValue.Substring(6, 2), NumberStyles.AllowHexSpecifier));

            return Color.FromArgb(alpha, red, green, blue);
        }

        public string ToInlineCSS(RichTextBoxStyle currentStyle, UIElement element)
        {
            /*StringBuilder result = new StringBuilder();
            Dictionary<string, string> pairs = ToHTML();
            Dictionary<string, string> current = currentStyle.ToHTML();
            string value;

            foreach (KeyValuePair<string, string> pair in pairs)
            {
                if (current.TryGetValue(pair.Key, out value))
                {
                    if (value == pair.Value)
                    {
                        continue;
                    }
                }
                result.Append(pair.Key + ":" + pair.Value + ";");
            }

            return result.ToString();*/

            StringBuilder result = new StringBuilder();
            Dictionary<string, string> pairs;
            Dictionary<string, string> current;
            string value;

            pairs = ToHTML(element);
            current = currentStyle.ToHTML(element);

            foreach (KeyValuePair<string, string> pair in pairs)
            {
                if (current.TryGetValue(pair.Key, out value))
                {
                    current.Remove(pair.Key);

                    if (value == pair.Value)
                    {
                        continue;
                    }
                }
                result.Append(pair.Key + ":" + pair.Value + ";");
            }

            pairs = GetStyleDefaults(current);
            foreach (KeyValuePair<string, string> pair in pairs)
            {
                result.Append(pair.Key + ":" + pair.Value + ";");
            }

            return result.ToString();
        }

        public static Dictionary<string, string> GetStyleDefaults(Dictionary<string, string> styles)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            string newValue = string.Empty;

            foreach (KeyValuePair<string, string> pair in styles)
            {
                switch (pair.Key)
                {
                    case "font-family":
                        newValue = string.Empty;
                        break;
                    case "font-size":
                        newValue = string.Empty;
                        break;
                    case "font-weight":
                        newValue = "normal";
                        break;
                    case "font-style":
                        newValue = "normal";
                        break;
                    case "text-decoration":
                        newValue = "none";
                        break;
                    case "text-align":
                        newValue = "left";
                        break;
                    case "color":
                        newValue = string.Empty;
                        break;
                    case "background":
                        newValue = string.Empty;
                        break;
                    case "vertical-align":
                        newValue = "middle";
                        break;
                    case "margin":
                        newValue = "0px 0px 0px 0px";
                        break;
                    default:
                        newValue = string.Empty;
                        break;
                }

                if (newValue.Length > 0)
                {
                    results.Add(pair.Key, newValue);
                }
            }

            return results;
        }

        /// <summary>
        /// Gets the style as an HTML style
        /// </summary>
        /// <returns>HTML CSS style tags</returns>
        public Dictionary<string, string> ToHTML()
        {
            return ToHTML(null);
        }

        /// <summary>
        /// Gets the style as an HTML style
        /// </summary>
        /// <returns>HTML CSS style tags</returns>
        public Dictionary<string, string> ToHTML(UIElement element)
        {
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            string tag;
            bool isText = (element is TextBlockPlus) || element == null;

            tag = Utility.IsStyleAHeading(ID) ? string.Empty : ".";

            if (isText)
            {
                if (Family.Length > 0)
                {
                    keyValues.Add("font-family", Family);
                }
                if (Size.HasValue)
                {
                    keyValues.Add("font-size", Math.Round((double)Size, 2).ToString(CultureInfo.InvariantCulture) + "px");
                }

                if (Background != null)
                {
                    if (((SolidColorBrush)Background).Color.A > 0)
                    {
                        keyValues.Add("background", Serialize.BrushHex6(Background));
                    }
                }

                if (Foreground != null)
                {
                    if (((SolidColorBrush)Foreground).Color.A > 0)
                    {
                        keyValues.Add("color", Serialize.BrushHex6(Foreground));
                    }
                }

                if (Weight == FontWeights.Bold)
                {
                    keyValues.Add("font-weight", "bold");
                }
                if (Style == FontStyles.Italic)
                {
                    keyValues.Add("font-style", "italic");
                }
                if (Effect == TextBlockPlusEffect.Strike)
                {
                    keyValues.Add("text-decoration", "line-through");
                }
                if (Decorations == TextDecorations.Underline && !keyValues.ContainsKey("text-decoration"))
                {
                    keyValues.Add("text-decoration", "underline");
                }

                if (Special == RichTextSpecialFormatting.Subscript)
                {
                    keyValues["vertical-align"] = "sub";
                    keyValues["font-size"] = Math.Round((double)Size * 0.6, 2).ToString(CultureInfo.InvariantCulture) + "px";
                }
                else if (Special == RichTextSpecialFormatting.Superscript)
                {
                    keyValues["vertical-align"] = "super";
                    keyValues["font-size"] = Math.Round((double)Size * 0.6, 2).ToString(CultureInfo.InvariantCulture) + "px";
                }
            }

            if (!keyValues.ContainsKey("vertical-align"))
            {
                keyValues.Add("vertical-align", VerticalAlignment.ToString().Replace("Center", "middle"));
            }

            if (Alignment != HorizontalAlignment.Left)
            {
                keyValues.Add("text-align", Alignment.ToString());
            }

            if (Margin.Bottom != 0 || Margin.Top != 0 || Margin.Left != 0 || Margin.Right != 0)
            {
                keyValues.Add("margin", Utility.ThicknessToCSS(Margin));
            }


            return keyValues;
        }

        /// <summary>
        /// Creates XML for a given style
        /// </summary>
        /// <returns>Style in XML format</returns>
        public string ToXML()
        {
            StringBuilder result = new StringBuilder();

            result.Append("<Style ID=\"" + ID + "\" FontFamily=\"" + Family + "\" FontSize=\"" + Size.ToString() + "\"");
            if (Style == FontStyles.Italic)
            {
                result.Append(" FontStyle=\"Italic\"");
            }
            if (Weight == FontWeights.Bold)
            {
                result.Append(" FontWeight=\"Bold\"");
            }
            if (Decorations == TextDecorations.Underline)
            {
                result.Append(" Decoration=\"Underline\"");
            }

            result.Append(Serialize.Brush(Foreground, " Foreground", false));

            if (Background is SolidColorBrush)
            {
                if (((SolidColorBrush)Background).Color.A > 0)
                {
                    result.Append(Serialize.Brush(Background, " Background", false));
                }
            }

            result.Append(Serialize.Brush(BorderBrush, " BorderBrush", false));
            result.Append(Serialize.Brush(ShadowBrush, " ShadowBrush", false));

            result.Append(" Alignment=\"" + Alignment.ToString() + "\"");

            if (VerticalAlignment != VerticalAlignment.Bottom)
            {
                result.Append(" VerticalAlignment=\"" + VerticalAlignment.ToString() + "\"");
            }

            switch (Special)
            {
                case RichTextSpecialFormatting.Subscript:
                    result.Append(" Special=\"Subscript\"");
                    break;
                case RichTextSpecialFormatting.Superscript:
                    result.Append(" Special=\"Superscript\"");
                    break;
                default:
                    break;
            }

            switch (Effect)
            {
                case TextBlockPlusEffect.Strike:
                    result.Append(" Effect=\"Strike\"");
                    break;
                default:
                    break;
            }

            switch (BorderType)
            {
                case BorderEffect.Solid:
                    result.Append(" Border=\"Solid\"");
                    break;
                case BorderEffect.Dotted:
                    result.Append(" Border=\"Dotted\"");
                    break;
                case BorderEffect.Dashed:
                    result.Append(" Border=\"Dashed\"");
                    break;
                default:
                    break;
            }

            switch (Shadow)
            {
                case ShadowEffect.Slight:
                    result.Append(" Shadow=\"Slight\"");
                    break;
                case ShadowEffect.Normal:
                    result.Append(" Shadow=\"Normal\"");
                    break;
                default:
                    break;
            }

            if (Margin.Bottom != 0 || Margin.Left != 0 | Margin.Right != 0 || Margin.Top != 0)
            {
                result.Append(" Margin=\"" + Margin.ToString() + "\"");
            }

            result.Append(" />\r");

            return result.ToString();
        }

        /// <summary>
        /// Gets a dictionary of style key/values
        /// </summary>
        /// <param name="style">Text style</param>
        /// <returns>Dictionary of key/values</returns>
        public static Dictionary<string, string> GetDictionaryFromStyle(string style)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            string[] split3;
            string[] split4;

            if (style == null)
            {
                return results;
            }

            split3 = style.Split(';');
            foreach (string a in split3)
            {
                split4 = a.Trim().Split(':');

                if (split4.Length > 1)
                {
                    results.Add(split4[0].ToLower().Trim(), split4[1].Trim());
                }
            }

            return results;
        }

        public void FromInlineStyle(string style)
        {
            Dictionary<string, string> keyValues = GetDictionaryFromStyle(style);
            string temp;

            foreach (KeyValuePair<string, string> pair in keyValues)
            {
                switch (pair.Key)
                {
                    case "font-family":
                        SetProperty("FontFamily", pair.Value);
                        break;
                    case "font-size":
                        SetProperty("FontSize", pair.Value.Replace("px", ""));
                        break;
                    case "font-weight":
                        SetProperty("FontWeight", pair.Value);
                        break;
                    case "font-style":
                        SetProperty("FontStyle", pair.Value);
                        break;
                    case "text-decoration":
                        SetProperty("Decoration", pair.Value);
                        break;
                    case "text-align":
                        SetProperty("Alignment", pair.Value);
                        break;
                    case "color":
                        SetProperty("Foreground", pair.Value.Replace("#", "#FF"));
                        break;
                    case "background":
                        SetProperty("Background", pair.Value.Replace("#", "#FF"));
                        break;
                    case "vertical-align":
                        temp = pair.Value.ToLower();
                        if (temp == "super")
                        {
                            Special = RichTextSpecialFormatting.Superscript;
                            Size = Math.Round(Size.Value * 1.66);
                        }
                        else if (temp == "sub")
                        {
                            Special = RichTextSpecialFormatting.Subscript;
                            Size = Math.Round(Size.Value * 1.66);
                        }
                        else
                        {
                            SetProperty("VerticalAlignment", pair.Value);
                        }
                        break;
                    case "margin":
                        SetProperty("Margin", pair.Value, false);
                        break;
                    default:
                        break;
                }
            }

            if (Background == null)
            {
                SetProperty("Background", "#00ffffff");
            }
        }

        /// <summary>
        /// Converts a CSS style to a RichText style
        /// </summary>
        /// <param name="style">CSS style</param>
        /// <returns>RichText styles</returns>
        public void FromHTML(string style)
        {
            string[] split2;

            split2 = style.Trim().Split('{');
            if (split2.Length > 1)
            {
                ID = split2[0].Trim().Replace(".", "");
                FromInlineStyle(split2[1]);
            }
        }

        public override string ToString()
        {
            return ID;
        }
    }
}
