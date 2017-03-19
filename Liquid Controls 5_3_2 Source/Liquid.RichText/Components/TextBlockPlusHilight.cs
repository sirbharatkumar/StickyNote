using System.Windows.Media;
using System.Windows.Shapes;

namespace Liquid
{
    public partial class TextBlockPlusHilight
    {
        public TextBlockPlusSelectionEffect Effect { get; set; }
        public Brush Brush { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public Rectangle Element { get; set; }

        public TextBlockPlusHilight()
        {
            Effect = TextBlockPlusSelectionEffect.DottedUnderline;
            Brush = null;
            Start = 0;
            Length = 0;
            Element = null;
        }

        public TextBlockPlusHilight(TextBlockPlusSelectionEffect effect, Brush brush, int start, int length)
        {
            Effect = effect;
            Brush = brush;
            Start = start;
            Length = length;
            Element = null;
        }
    }
}
