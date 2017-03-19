using System.Windows.Controls;

namespace Liquid
{
    public abstract partial class LiquidTextControl : LiquidControl
    {
        #region Visual Elements

        /// <summary> 
        /// Text template.
        /// </summary>
        internal TextBlock ElementText { get; set; }
        internal const string ElementTextName = "ElementText";

        #endregion

        #region Event Handling

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ElementText = (TextBlock)GetTemplateChild(ElementTextName);
        }

        #endregion
    }
}
