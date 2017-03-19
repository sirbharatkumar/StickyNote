using System;
using System.Windows;

namespace Liquid
{
    /// <summary>
    /// Event arguments for use with rich text flow panel related events
    /// </summary>
    public partial class RichTextPanelEventArgs : EventArgs
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the element that generated the event
        /// </summary>
        public UIElement Source { get; set; }

        /// <summary>
        /// Gets or sets the element that will be created after the event
        /// </summary>
        public UIElement Created { get; set; }

        /// <summary>
        /// Gets or sets the index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets whether to cancel an operation
        /// </summary>
        public bool Cancel { get; set; }

        #endregion

        #region Constructor

        public RichTextPanelEventArgs()
        {
        }

        public RichTextPanelEventArgs(UIElement source)
        {
            Source = source;
        }

        #endregion
    }
}
