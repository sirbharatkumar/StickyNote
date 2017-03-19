using System;
using System.Collections.Generic;

namespace Liquid
{
    public partial class RichTextBoxHtmlEventArgs : EventArgs
    {
        #region Public Properties

        public string ID { get; set; }

        public Dictionary<string, string> Styles { get; set; }

        /// <summary>
        /// Gets or sets whether to cancel an operation
        /// </summary>
        public bool Cancel { get; set; }

        #endregion

        #region Constructor

        public RichTextBoxHtmlEventArgs()
        {
        }

        #endregion
    }
}
