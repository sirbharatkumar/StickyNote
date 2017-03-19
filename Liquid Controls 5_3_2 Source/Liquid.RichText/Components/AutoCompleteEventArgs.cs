using System;

namespace Liquid.Components.Internal
{
    /// <summary>
    /// Event arguments for use with auto completion related events
    /// </summary>
    public partial class AutoCompleteEventArgs : EventArgs
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets whether to cancel an operation
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets the text portion
        /// </summary>
        public string Text { get; set; }

        #endregion

        #region Constructor

        public AutoCompleteEventArgs()
        {
            Text = string.Empty;
            Cancel = false;
        }

        #endregion
    }
}
