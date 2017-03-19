using System.Windows;
using System.Windows.Controls;

namespace Liquid
{
    public partial class RichTextBoxPosition
    {
        #region Public Properties

        public UIElement Element { get; set; }

        public int Index { get; set; }

        public Point Position
        {
            get
            {
                Point result;
                TextBlockPlus tempTextBlock;
                string temp;

                if (Element is TextBlockPlus)
                {
                    tempTextBlock = (TextBlockPlus)Element;
                    temp = tempTextBlock.Text;
                    tempTextBlock.Text = temp.Substring(0, (Index > tempTextBlock.Text.Length ? tempTextBlock.Text.Length : Index));

                    result = new Point((double)Element.GetValue(Canvas.LeftProperty) + tempTextBlock.ContentWidth, (double)Element.GetValue(Canvas.TopProperty));

                    tempTextBlock.Text = temp;
                }
                else
                {
                    result = new Point((double)Element.GetValue(Canvas.LeftProperty), (double)Element.GetValue(Canvas.TopProperty));
                }

                return result;
            }
        }

        public int GlobalIndex { get; set; }

        #endregion

        #region Constructor

        public RichTextBoxPosition()
        {
            Element = null;
            Index = 0;
            GlobalIndex = 0;
        }

        public RichTextBoxPosition(int globalIndex)
        {
            Element = null;
            Index = 0;
            GlobalIndex = globalIndex;
        }

        public RichTextBoxPosition(UIElement element, int index)
        {
            Element = element;
            Index = index;
            GlobalIndex = 0;
        }

        public RichTextBoxPosition(RichTextBoxPosition position)
        {
            Element = position.Element;
            Index = position.Index;
            GlobalIndex = position.GlobalIndex;
        }

        #endregion

        #region Public Methods

        public int CompareTo(RichTextBoxPosition position)
        {
            return GlobalIndex.CompareTo(position.GlobalIndex);
        }

        public void CalculatePositionFromGlobalIndex(UIElementCollection elements)
        {
            int current = 0;
            int step;
            bool broken = false;
            int hitIndex;
            int i;

            if (elements.Count > 0)
            {
                Element = elements[0];

                for (i = 0; i < elements.Count; i++)
                {
                    step = (elements[i] is TextBlockPlus ? elements[i].ToString().Length : 1);

                    if (GlobalIndex >= current && GlobalIndex < current + step)
                    {
                        Element = elements[i];
                        Index = GlobalIndex - current;
                        broken = true;
                        break;
                    }

                    current += step;
                }

                if (!broken && GlobalIndex == current)
                {
                    Element = elements[elements.Count - 1];
                    Index = (Element is TextBlockPlus ? ((TextBlockPlus)Element).Text.Length : 0);
                    broken = true;
                }

                if (broken && Index == 0)
                {
                    hitIndex = elements.IndexOf(Element);
                    if (hitIndex > 0)
                    {
                        if (elements[hitIndex - 1] is TextBlockPlus)
                        {
                            Element = elements[hitIndex - 1];
                            Index = ((TextBlockPlus)Element).Text.Length;
                        }
                    }
                }
            }
        }

        public void CalculateGlobalIndex(UIElementCollection elements)
        {
            int i;
            int elementIndex = elements.IndexOf(Element);

            GlobalIndex = Index;

            for (i = 0; i < elementIndex; i++)
            {
                GlobalIndex += (elements[i] is TextBlockPlus ? ((TextBlockPlus)elements[i]).Text.Length : 1);
            }
        }

        #endregion
    }
}
