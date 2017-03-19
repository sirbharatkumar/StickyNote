using System.Windows;
using System.Windows.Controls;

namespace Liquid
{
    /// <summary>
    /// Represents a RichText Newline
    /// </summary>
    public partial class Newline : Canvas
    {
        #region Constructor

        public Newline()
        {
            Width = 4;
            Height = 8;
            IsHitTestVisible = false;
        }

        #endregion
    }
}
