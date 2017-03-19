using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Liquid
{
    /// <summary>
    /// The expand control used in the TreeView Node and Fieldset controls
    /// </summary>
    public partial class Expand : BaseTreeViewControl
    {
        #region Visual Elements

        /// <summary> 
        /// MouseHover template.
        /// </summary>
        internal Storyboard ElementMouseHover { get; set; }
        internal const string ElementMouseHoverName = "ElementMouseHover";

        /// <summary> 
        /// MouseLeave template.
        /// </summary>
        internal Storyboard ElementMouseLeave { get; set; }
        internal const string ElementMouseLeaveName = "ElementMouseLeave";

        /// <summary> 
        /// Rotate template.
        /// </summary>
        internal Storyboard ElementRotate { get; set; }
        internal const string ElementRotateName = "ElementRotate";

        /// <summary> 
        /// Background template.
        /// </summary>
        internal Rectangle ElementBackground { get; set; }
        internal const string ElementBackgroundName = "ElementBackground";

        /// <summary> 
        /// Plus template.
        /// </summary>
        internal UIElement ElementPlus { get; set; }
        internal const string ElementPlusName = "ElementPlus";

        /// <summary> 
        /// Minus template.
        /// </summary>
        internal UIElement ElementMinus { get; set; }
        internal const string ElementMinusName = "ElementMinus";

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(double), typeof(Expand), null);
        public double CornerRadius
        {
            get { return (double)this.GetValue(CornerRadiusProperty); }
            set { base.SetValue(CornerRadiusProperty, value); }
        }

        #endregion

        #region Private Properties

        private bool _expanded = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates whether the control is expanded or collapsed
        /// </summary>
        public bool IsExpanded
        {
            get { return _expanded; }
            set { _expanded = value; UpdateVisualState(); }
        }

        #endregion

        #region Public Events

        public event EventHandler Click;

        #endregion

        #region Constructor

        public Expand()
        {
            Width = 11;
            Height = 11;

            this.MouseEnter += new MouseEventHandler(Expand_MouseEnter);
            this.MouseLeave += new MouseEventHandler(Expand_MouseLeave);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(Expand_MouseDown);
        }

        #endregion

        #region Public Methods

        public void BeginRotate()
        {
            if (ElementRotate != null)
            {
                ElementRotate.Begin();
            }
        }

        public void EndRotate()
        {
            if (ElementRotate != null)
            {
                ElementRotate.Stop();
            }
        }

        #endregion

        #region Private Methods

        protected void UpdateVisualState()
        {
            double opacity = (IsEnabled ? 1 : 0.8);

            if (ElementRoot != null)
            {
                ElementPlus.Visibility = (_expanded ? Visibility.Collapsed : Visibility.Visible);
                ElementMinus.Visibility = (!_expanded ? Visibility.Collapsed : Visibility.Visible);

                ElementBackground.Opacity = opacity;
                ElementPlus.Opacity = opacity;
                ElementMinus.Opacity = opacity;
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

            ElementMouseHover = (Storyboard)GetTemplateChild(ElementMouseHoverName);
            ElementMouseLeave = (Storyboard)GetTemplateChild(ElementMouseLeaveName);
            ElementRotate = (Storyboard)GetTemplateChild(ElementRotateName);
            ElementBackground = (Rectangle)GetTemplateChild(ElementBackgroundName);
            ElementPlus = (UIElement)GetTemplateChild(ElementPlusName);
            ElementMinus = (UIElement)GetTemplateChild(ElementMinusName);

            UpdateVisualState();
        }

        private void Expand_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ElementRoot != null && IsEnabled)
            {
                ElementMouseLeave.Begin();
                ElementMouseHover.Stop();
            }
        }

        private void Expand_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ElementRoot != null && IsEnabled)
            {
                ElementMouseHover.Begin();
                ElementMouseLeave.Stop();
            }
        }

        /// <summary>
        /// Is called when the mouse button is pressed
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void Expand_MouseDown(object sender, MouseButtonEventArgs args)
        {
            if (IsEnabled)
            {
                IsExpanded ^= true;
                RaiseClick(this, null);
            }
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a Click event a click has occured
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseClick(object sender, EventArgs args)
        {
            if (Click != null)
            {
                Click(sender, args);
            }
        }

        #endregion
    }
}
