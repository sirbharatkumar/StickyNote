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

namespace Liquid
{
    public partial class CellReference
    {
        public int Row { get; set; }

        public int Column { get; set; }

        public UIElement Element { get; set; }

        public CellReference(int row, int column, UIElement element)
        {
            Row = row;
            Column = column;
            Element = element;
        }
    }
}
