using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Liquid
{
    public partial class RichTextState
    {
        public List<UIElement> Elements { get; set; }

        public string Text { get; set; }

        public int SelectionStart { get; set; }

        public int Length { get; set; }

        public Table Table { get; set; }

        public ColumnDefinition Column { get; set; }

        public RowDefinition Row { get; set; }

        public int Index { get; set; }

        public int Count { get; set; }

        public RichTextState()
        {
            Elements = new List<UIElement>();
        }

        public RichTextState(int selectionStart, int length)
        {
            Elements = new List<UIElement>();
            SelectionStart = selectionStart;
            Length = length;
        }

        public RichTextState(string text, int selectionStart, int length)
        {
            Text = text;
            SelectionStart = selectionStart;
            Length = length;
        }

        public RichTextState(List<UIElement> elements, int selectionStart, int length)
        {
            Elements = elements;
            SelectionStart = selectionStart;
            Length = length;
        }
    }
}
