using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Liquid
{
    public abstract partial class BaseTreeViewControl : ContentControl
    {
        #region Visual Elements

        /// <summary> 
        /// Root element template.
        /// </summary>
        public FrameworkElement ElementRoot { get; set; }
        internal const string ElementRootName = "RootElement";

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the opacity for certain control elements when disabled
        /// </summary>
        public static double DisabledOpacity { get; set; }

        /// <summary> 
        /// Gets or sets the ID
        /// </summary> 
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the Liquid Tag object
        /// </summary>
        public object LiquidTag { get; set; }

        #endregion

        #region Constructor

        public BaseTreeViewControl()
        {
            IsEnabledChanged += new DependencyPropertyChangedEventHandler(OnEnabledChanged);
            IsEnabled = true;
            DefaultStyleKey = this.GetType();
            DisabledOpacity = 0.7;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Is called when the visuals need to be updated
        /// </summary>
        protected virtual void UpdateVisualState()
        {
            if (ElementRoot != null)
            {
                ElementRoot.Width = Width;
                ElementRoot.Height = Height;
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// This is called when the template has been bound to the control
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ElementRoot = (FrameworkElement)GetTemplateChild(ElementRootName);
        }

        /// <summary>
        /// This event is called when the control enabled state changes
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void OnEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            IsTabStop = (bool)e.NewValue;
            UpdateVisualState();
        }

        #endregion
    }
}
