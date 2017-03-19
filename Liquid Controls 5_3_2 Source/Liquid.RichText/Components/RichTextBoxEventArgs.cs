using System;
using System.Windows;
using System.Windows.Controls;

namespace Liquid
{
    public partial class RichTextBoxEventArgs : EventArgs
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the content format for the associated event
        /// </summary>
        public Format Format { get; set; }

        /// <summary>
        /// Gets or sets the element that generated the event
        /// </summary>
        public UIElement Source { get; set; }

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        public string Name
        {
            get { return Source.GetValue(Canvas.NameProperty).ToString(); }
        }

        /// <summary>
        /// Gets or sets any hilight object
        /// </summary>
        public RichTextHilight Hilight { get; set; }

        /// <summary>
        /// Gets or sets the prefix of a custom UIElement during Serialization
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the content of a custom UIElement during Serialization
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets a generic parameter for the associated event
        /// </summary>
        public object Parameter { get; set; }

        /// <summary>
        /// Gets or sets whether to cancel an operation
        /// </summary>
        public bool Cancel { get; set; }

        #endregion

        #region Constructor

        public RichTextBoxEventArgs()
        {
            Format = Format.XML;
        }

        public RichTextBoxEventArgs(UIElement source)
        {
            Format = Format.XML;
            Source = source;
        }

        public RichTextBoxEventArgs(UIElement source, string parameter) :
            this(source)
        {
            Parameter = parameter;
        }

        #endregion

        #region Public Methods

        #endregion
    }
}
