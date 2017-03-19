using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Liquid
{
    public partial class RichTextPanelRow
    {
        public UIElement Start { get; set; }

        public UIElement End { get; set; }

        public Point Dimensions { get; set; }

        public Point Position { get; set; }

        public Thickness Margin { get; set; }

        public RichTextPanelRow()
        {
            Start = null;
            End = null;
            Dimensions = new Point();
            Position = new Point();
            Margin = new Thickness();
        }

        public RichTextPanelRow(UIElement start, UIElement end)
        {
            Start = start;
            End = end;
            Dimensions = new Point();
            Position = new Point();
            Margin = new Thickness();
        }

        public RichTextPanelRow(RichTextPanelRow row)
        {
            Start = row.Start;
            End = row.End;
            Dimensions = new Point(Dimensions.X, Dimensions.Y);
            Position = new Point(Position.X, Position.Y);
            Margin = new Thickness(row.Margin.Left, row.Margin.Top, row.Margin.Right, row.Margin.Bottom);
        }

        #region Public Methods

        public List<UIElement> GetChildren(UIElementCollection collection)
        {
            List<UIElement> children = new List<UIElement>();
            int start;
            int end;

            if (Start != null && End != null)
            {
                start = collection.IndexOf(Start);
                end = collection.IndexOf(End);

                for (int i = start; i <= end; i++)
                {
                    children.Add(collection[i]);
                }
            }

            return children;
        }

        #endregion
    }
}
