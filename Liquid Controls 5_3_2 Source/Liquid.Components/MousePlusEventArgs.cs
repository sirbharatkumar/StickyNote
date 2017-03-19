using System;
using System.Windows;

namespace Liquid.Components
{
    /// <summary>
    /// Event arguments for use with extended mouse related events
    /// </summary>
    public partial class MousePlusEventArgs : EventArgs
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the position of the mouse
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// This can be used to cancel an operation
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets the element that is the source of the event
        /// </summary>
        public UIElement Source { get; set; }

        #endregion

        #region Constructor

        public MousePlusEventArgs()
        {
            Position = new Point();
            Cancel = false;
            Source = null;
        }

        #endregion
    }
}
