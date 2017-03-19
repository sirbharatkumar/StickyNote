using System.Windows.Controls;

namespace Liquid
{
    public partial class TextRollerBlind : RollerBlind
    {
        #region Visual Elements

        /// <summary> 
        /// Top cover text template.
        /// </summary>
        internal TextBlock ElementTopText { get; set; }
        internal const string ElementTopTextName = "ElementTopText";

        /// <summary> 
        /// Bottom cover text template.
        /// </summary>
        internal TextBlock ElementBottomText { get; set; }
        internal const string ElementBottomTextName = "ElementBottomText";

        /// <summary> 
        /// Top Content template.
        /// </summary>
        internal TextBlock ElementBottomSelectedText { get; set; }
        internal const string ElementBottomSelectedTextName = "ElementBottomSelectedText";

        #endregion

        #region Static Properties

        public static string DefaultValue = "...";

        #endregion

        #region Private Properties

        private string _topText = string.Empty;
        private string _bottomText = string.Empty;
        private string _value = TextRollerBlind.DefaultValue;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the top cover text value
        /// </summary>
        public string TopText
        {
            get { return _topText; }
            set { _topText = value; UpdateCoverText(); }
        }

        /// <summary>
        /// Gets or sets the bottom cover text value
        /// </summary>
        public string BottomText
        {
            get { return _bottomText; }
            set { _bottomText = value; UpdateCoverText(); }
        }

        /// <summary>
        /// Gets or sets the value text, this is displayed on the bottom cover in Bold
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; UpdateCoverText(); }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method updates the cover text
        /// </summary>
        private void UpdateCoverText()
        {
            if (ElementRoot != null)
            {
                ElementTopText.Text = _topText;
                ElementBottomText.Text = _bottomText;
                ElementBottomSelectedText.Text = _value;
            }
        }

        protected override void UpdateVisualState()
        {
            base.UpdateVisualState();

            if (ElementRoot != null)
            {
                ElementTopText.Width = Width;
                ElementBottomText.Width = Width;
                ElementBottomSelectedText.Width = Width;
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// This is called when the template has been bound to the control
        /// </summary>
        public override void OnApplyTemplate()
        {
            ElementTopText = (TextBlock)GetTemplateChild(ElementTopTextName);
            ElementBottomText = (TextBlock)GetTemplateChild(ElementBottomTextName);
            ElementBottomSelectedText = (TextBlock)GetTemplateChild(ElementBottomSelectedTextName);

            base.OnApplyTemplate();
            UpdateCoverText();
        }

        #endregion
    }
}
