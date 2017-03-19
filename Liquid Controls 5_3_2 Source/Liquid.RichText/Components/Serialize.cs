using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text;

namespace Liquid.Components.Internal
{
    internal static partial class Serialize
    {
        /// <summary>
        /// Gets the RichTextBlock and its content as XML
        /// </summary>
        /// <returns>XML string</returns>
        internal static string RichTextBlock(RichTextBlock rtb)
        {
            StringBuilder result = new StringBuilder();

            result.Append("<liquid:RichTextBlock VerticalAlignment=\"Top\" HorizontalAlignment=\"Stretch\" Margin=\"" + rtb.Margin.ToString() + "\" IsReadOnly=\"False\">\r");
            result.Append("<liquid:RichTextBlock.DirectRichText>\r");
            result.Append("<TextBlock>\r");

            result.Append(Utility.XmlEncode(rtb.RichText));

            result.Append("</TextBlock>\r");
            result.Append("</liquid:RichTextBlock.DirectRichText>\r");
            result.Append("</liquid:RichTextBlock>\r");

            return result.ToString();
        }

        /// <summary>
        /// Builds an XMLrepresentation of a PlaceHolder control
        /// </summary>
        /// <param name="placeholder">A PlaceHolder control</param>
        /// <returns>An XML string</returns>
        internal static string PlaceHolderAsXML(PlaceHolder placeholder)
        {
            StringBuilder result = new StringBuilder();

            result.Append("<Xaml>\r");
            result.Append("<liquid:PlaceHolder");

            if (placeholder.Content is string)
            {
                result.Append(" Content=\"" + placeholder.Content.ToString() + "\"");
            }

            result.Append(" Value=\"" + Utility.XmlEncode(placeholder.Value) + "\"");
            result.Append(" />\r");
            result.Append("</Xaml>\r");

            return result.ToString();
        }

        /// <summary>
        /// Builds an XML representation of the Table and its contents
        /// </summary>
        /// <returns>RichText XML</returns>
        internal static string TableAsXML(Table table, RichTextSaveOptions options)
        {
            StringBuilder result = new StringBuilder();
            RichTextBlock richTextBlock;
            Border border;
            int column;
            int row;
            int columnSpan;
            int rowSpan;

            result.Append("<Xaml>\r");
            result.Append("<liquid:Table ");

            if (!double.IsNaN(table.Width))
            {
                result.Append(" Width=\"" + table.Width.ToString() + "\"");
            }
            if (!double.IsNaN(table.Height))
            {
                result.Append(" Height=\"" + table.Height.ToString() + "\"");
            }
            if (table.HorizontalAlignment != HorizontalAlignment.Left)
            {
                result.Append(" HorizontalAlignment=\"" + table.HorizontalAlignment.ToString() + "\"");
            }
            result.Append(" CellPadding=\"" + table.CellPadding.ToString() + "\"");
            result.Append(" HeaderRows=\"" + table.HeaderRows.ToString() + "\"");
            result.Append(" HeaderColumns=\"" + table.HeaderColumns.ToString() + "\"");
            result.Append(" AutoWidth=\"" + table.AutoWidth.ToString() + "\"");
            result.Append(" Tag=\"" + table.Tag.ToString() + "\"");

            result.Append(">\r");

            if (table.ColumnDefinitions.Count > 0)
            {
                result.Append("<Grid.ColumnDefinitions>\r");
                foreach (ColumnDefinition c in table.ColumnDefinitions)
                {
                    result.Append("<ColumnDefinition Width=\"" + c.Width.ToString() + "\" />\r");
                }
                result.Append("</Grid.ColumnDefinitions>\r");
            }

            if (table.RowDefinitions.Count > 0)
            {
                result.Append("<Grid.RowDefinitions>\r");
                foreach (RowDefinition c in table.RowDefinitions)
                {
                    result.Append("<RowDefinition Height=\"" + c.Height.ToString() + "\"/>\r");
                }
                result.Append("</Grid.RowDefinitions>\r");
            }

            foreach (UIElement e in table.Children)
            {
                if (e is Border)
                {
                    border = (Border)e;
                    row = (int)border.GetValue(Grid.RowProperty);
                    column = (int)border.GetValue(Grid.ColumnProperty);
                    rowSpan = (int)border.GetValue(Grid.RowSpanProperty);
                    columnSpan = (int)border.GetValue(Grid.ColumnSpanProperty);

                    result.Append("<Border Grid.Column=\"" + column.ToString() + "\" Grid.Row=\"" + row.ToString() + "\">\r");
                    if (border.Child is RichTextBlock)
                    {
                        richTextBlock = (RichTextBlock)border.Child;
                        result.Append(Serialize.RichTextBlock(richTextBlock));
                    }
                    result.Append("</Border>\r");
                }
            }

            result.Append("</liquid:Table>\r");
            result.Append("</Xaml>\r");

            return result.ToString();
        }

        /// <summary>
        /// Builds an HTML representation of the Table and its contents
        /// </summary>
        /// <returns>HTML</returns>
        internal static string TableAsHTML(Table table, RichTextSaveOptions options)
        {
            StringBuilder result = new StringBuilder();
            RichTextBlock richTextBlock;
            string tag = string.Empty;
            string width;
            string valign = "top";
            Border border;
            string temp;
            int column;
            int row;
            int index;

            result.Append("<table");

            if (table.Tag != null)
            {
                result.Append(" class=\"" + table.Tag.ToString() + "\"");
            }

            if (!double.IsNaN(table.Width))
            {
                result.Append(" width=\"" + table.Width.ToString() + "\"");
            }
            if (!double.IsNaN(table.Height))
            {
                result.Append(" height=\"" + table.Height.ToString() + "\"");
            }
            if (table.HorizontalAlignment != HorizontalAlignment.Left && table.HorizontalAlignment != HorizontalAlignment.Stretch)
            {
                result.Append(" style='text-align:" + table.HorizontalAlignment.ToString() + "'");
            }

            result.Append(">\r");

            index = 0;
            for (row = 0; row < table.RowDefinitions.Count; row++)
            {
                result.Append("<tr>\r");
                for (column = 0; column < table.ColumnDefinitions.Count; column++, index++)
                {
                    if (row < table.HeaderRows || column < table.HeaderColumns)
                    {
                        tag = "th";
                    }
                    else
                    {
                        tag = "td";
                    }
                    border = (Border)table.Children[index];
                    table.SetColumnsToPixelWidth();

                    if (table.ColumnDefinitions[column].Width.GridUnitType == GridUnitType.Pixel)
                    {
                        width = table.ColumnDefinitions[column].Width.Value.ToString() + "px";
                    }
                    else
                    {
                        width = Math.Round((double)100 / table.ColumnDefinitions.Count).ToString() + "%";
                    }

                    switch ((VerticalAlignment)border.Child.GetValue(FrameworkElement.VerticalAlignmentProperty))
                    {
                        case VerticalAlignment.Top:
                            valign = "top";
                            break;
                        case VerticalAlignment.Center:
                            valign = "middle";
                            break;
                        case VerticalAlignment.Bottom:
                            valign = "bottom";
                            break;
                        default:
                            valign = "top";
                            break;
                    }

                    result.Append("<" + tag + " style=\"width:" + width + "; vertical-align:" + valign + ";\">");

                    if (border.Child is RichTextBlock)
                    {
                        richTextBlock = (RichTextBlock)border.Child;
                        temp = richTextBlock.Save(Format.HTML, options).Trim();
                        if (temp.Length == 0)
                        {
                            temp = "<p>&nbsp;</p>";
                        }
                        result.Append(temp);
                    }
                    result.Append("</" + tag + ">\r");
                }
                result.Append("</tr>\r");
            }

            result.Append("</table>\r");

            return result.ToString();
        }

        /// <summary>
        /// Gets the color of the brush as a 6-digit hex number preceeded by a #
        /// </summary>
        /// <param name="brush">A Brush object</param>
        /// <returns>6-digit hex value</returns>
        internal static string BrushHex6(Brush brush)
        {
            LinearGradientBrush gradientBrush;
            Color color = new Color();

            if (brush is SolidColorBrush)
            {
                color = ((SolidColorBrush)brush).Color;
            }
            else if (brush is LinearGradientBrush)
            {
                gradientBrush = (LinearGradientBrush)brush;
                if (gradientBrush.GradientStops.Count > 0)
                {
                    color = gradientBrush.GradientStops[gradientBrush.GradientStops.Count - 1].Color;
                }
            }

            if (color.A == 0)
            {
                return string.Empty;
            }
            else
            {
                return "#" + color.ToString().Substring(3);
            }
        }

        /// <summary>
        /// Builds a XAML representation of the provided brush object
        /// </summary>
        /// <param name="brush">A brush object</param>
        /// <param name="label">The brush property name</param>
        /// <returns>The brush object in XAML form</returns>
        internal static string Brush(Brush brush, string label, bool fullTag)
        {
            if (brush != null)
            {
                if (fullTag)
                {
                    return "<" + label + ">\r" + Brush(brush, fullTag) + "</" + label + ">\r";
                }
                else
                {
                    return label + "=\"" + Brush(brush, fullTag) + "\"";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Builds a XAML representation of the provided brush object
        /// </summary>
        /// <param name="brush">A brush object</param>
        /// <returns>The brush object in XAML form</returns>
        internal static string Brush(Brush brush, bool fullTag)
        {
            StringBuilder result = new StringBuilder();
            LinearGradientBrush gb;

            if (brush is SolidColorBrush)
            {
                if (fullTag)
                {
                    result.Append("<SolidColorBrush Color=\"" + ((SolidColorBrush)brush).Color.ToString() + "\" />\r");
                }
                else
                {
                    result.Append(((SolidColorBrush)brush).Color.ToString());
                }
            }
            else if (brush is LinearGradientBrush)
            {
                gb = (LinearGradientBrush)brush;
                result.Append("<LinearGradientBrush StartPoint=\"" + gb.StartPoint.ToString(CultureInfo.InvariantCulture) + "\" EndPoint=\"" + gb.EndPoint.ToString(CultureInfo.InvariantCulture) + "\">\r");
                foreach (GradientStop stop in gb.GradientStops)
                {
                    result.Append("<GradientStop Color=\"" + stop.Color.ToString(CultureInfo.InvariantCulture) + "\" Offset=\"" + Math.Round(stop.Offset, 2).ToString(CultureInfo.InvariantCulture) + "\" />\r");
                }
                result.Append("</LinearGradientBrush>");
            }

            return result.ToString();
        }
    }
}
