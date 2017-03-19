using System.Windows;
using System.Windows.Controls;

namespace Liquid
{
    /// <summary>
    /// Displays a simple text message in a popup Dialog
    /// </summary>
    public partial class MessageBox : DialogBase
    {
        #region Visual Elements

        /// <summary> 
        /// Grid template.
        /// </summary>
        internal Grid ElementGrid { get; set; }
        internal const string ElementGridName = "ElementGrid";

        /// <summary> 
        /// Text template.
        /// </summary>
        internal TextBlock ElementText { get; set; }
        internal const string ElementTextName = "ElementText";

        #endregion

        #region Private Properties

        private string _text = string.Empty;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the dialog text
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; UpdateVisualState(); }
        }

        #endregion

        #region Constructor

        public MessageBox()
        {
            IsResizable = false;
            IsMaximizeEnabled = false;
            IsMinimizeEnabled = false;
            Buttons = DialogButtons.OK;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method displays the simple text dialog
        /// <param name="message">Text to display</param>
        /// </summary>
        public void Show(string message)
        {
            Text = message;

            base.Show();
        }

        /// <summary>
        /// This method displays the simple text dialog
        /// <param name="message">Text to display</param>
        /// <param name="title">Text to display in the dialog title bar</param>
        /// </summary>
        public void Show(string message, string title)
        {
            Title = title;

            Show(message);
        }

        /// <summary>
        /// This method shows the dialog in modal format
        /// <param name="message">Text to display</param>
        /// </summary>
        public void ShowAsModal(string message)
        {
            Text = message;
            ShowAsModal();
        }

        /// <summary>
        /// This method shows the dialog in modal format
        /// <param name="message">Text to display</param>
        /// <param name="title">Text to display in the dialog title bar</param>
        /// </summary>
        public void ShowAsModal(string message, string title)
        {
            Title = title;
            ShowAsModal(message);
        }

        #endregion

        #region Private Methods

        protected override void UpdateVisualState()
        {
            base.UpdateVisualState();

            if (ElementRoot != null)
            {
                ElementGrid.Width = ElementWhole.Width;
                ElementGrid.Height = ElementWhole.Height - (Buttons == DialogButtons.None ? 0 : 48);

                ElementText.HorizontalAlignment = HorizontalContentAlignment;
                ElementText.VerticalAlignment = VerticalContentAlignment;
                ElementText.Text = _text;
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// This is called when the template has been bound to the control
        /// </summary>
        public override void OnApplyTemplate()
        {
            ElementGrid = (Grid)GetTemplateChild(ElementGridName);
            ElementText = (TextBlock)GetTemplateChild(ElementTextName);

            base.OnApplyTemplate();
        }

        #endregion
    }
}
