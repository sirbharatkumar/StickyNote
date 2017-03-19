
namespace Liquid
{
    public partial class RichTextHilight
    {
        public TextBlockPlus Element { get; set; }
        public TextBlockPlusHilight Hilight { get; set; }
        public RichTextBoxPosition Start { get; set; }
        public RichTextBoxPosition End { get; set; }
        public string Text { get; set; }
        public int LastTextLength { get; set; }

        public RichTextHilight()
        {
            Element = null;
            Hilight = null;
            Start = null;
            End = null;
            Text = string.Empty;
            LastTextLength = 0;
        }

        public RichTextHilight(TextBlockPlus element, TextBlockPlusHilight hilight, RichTextBoxPosition start, RichTextBoxPosition end, string text)
        {
            Element = element;
            Hilight = hilight;
            Start = start;
            End = end;
            Text = text;
            LastTextLength = 0;
        }
    }
}
