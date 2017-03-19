using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


namespace Liquid
{
    /// <summary>
    /// A Progress Bar control
    /// </summary>
    public partial class ProgressBarPlus : LiquidTimerControl
    {
        #region Visual Elements

        /// <summary> 
        /// Fader template.
        /// </summary>
        internal Storyboard ElementFader { get; set; }
        internal const string ElementFaderName = "ElementFader";

        /// <summary> 
        /// Background template.
        /// </summary>
        internal Rectangle ElementBackground { get; set; }
        internal const string ElementBackgroundName = "ElementBackground";

        /// <summary>
        /// Filling template.
        /// </summary>
        internal Rectangle ElementFilling { get; set; }
        internal const string ElementFillingName = "ElementFilling";

        /// <summary>
        /// Overlay template.
        /// </summary>
        internal Rectangle ElementOverlay { get; set; }
        internal const string ElementOverlayName = "ElementOverlay";

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty FillBrushProperty = DependencyProperty.Register("FillBrush", typeof(Brush), typeof(ProgressBarPlus), null);
        public Brush FillBrush
        {
            get { return (Brush)this.GetValue(FillBrushProperty); }
            set { base.SetValue(FillBrushProperty, (DependencyObject)value); }
        }

        public static readonly DependencyProperty OverlayBrushProperty = DependencyProperty.Register("OverlayBrush", typeof(Brush), typeof(ProgressBarPlus), null);
        public Brush OverlayBrush
        {
            get { return (Brush)this.GetValue(OverlayBrushProperty); }
            set { base.SetValue(OverlayBrushProperty, (DependencyObject)value); }
        }

        public static readonly DependencyProperty FillStrokeProperty = DependencyProperty.Register("FillStroke", typeof(Brush), typeof(ProgressBarPlus), null);
        public Brush FillStroke
        {
            get { return (Brush)this.GetValue(FillStrokeProperty); }
            set { base.SetValue(FillStrokeProperty, (DependencyObject)value); }
        }

        public static readonly DependencyProperty FillStrokeThicknessProperty = DependencyProperty.Register("FillStrokeThickness", typeof(Thickness), typeof(ProgressBarPlus), null);
        public Thickness FillStrokeThickness
        {
            get { return (Thickness)this.GetValue(FillStrokeThicknessProperty); }
            set { base.SetValue(FillStrokeThicknessProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ProgressBarPlus), null);
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { base.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(double), typeof(ProgressBarPlus), null);
        public double CornerRadius
        {
            get { return (double)this.GetValue(CornerRadiusProperty); }
            set { base.SetValue(CornerRadiusProperty, value); }
        }

        #endregion

        #region Private Properties

        private double _complete = 0;
        private double _target = 0;
        private bool _useTarget = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the complete vale.  This is a % and values should be between 1-100.
        /// </summary>
        public double Complete
        {
            get { return _complete; }
            set { _complete = value; UpdateProgress(); }
        }

        /// <summary>
        /// Gets or sets whether the Target completion value is to be used
        /// </summary>
        public bool UseTarget
        {
            get { return _useTarget; }
            set { _useTarget = value; }
        }

        /// <summary>
        /// Gets or sets the completion target value
        /// </summary>
        public double Target
        {
            get { return _target; }
            set { _target = value; }
        }

        #endregion

        #region Constructor

        public ProgressBarPlus()
        {
            Width = 200;
            Height = 22;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This updates the bar with the current Complete value
        /// </summary>
        private void UpdateProgress()
        {
            double width;

            if (_complete > 100)
            {
                _complete = 100;
            }

            if (ElementRoot != null)
            {
                width = ((ActualWidth - (ElementFilling.Margin.Left + ElementFilling.Margin.Right)) / 100) * _complete;
                ElementFilling.Width = width;
                ElementOverlay.Width = width;
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

            ElementFader = (Storyboard)GetTemplateChild(ElementFaderName);
            ElementBackground = (Rectangle)GetTemplateChild(ElementBackgroundName);
            ElementFilling = (Rectangle)GetTemplateChild(ElementFillingName);
            ElementOverlay = (Rectangle)GetTemplateChild(ElementOverlayName);

            UpdateVisualState();
        }

        /// <summary>
        /// This event is called when a tick occurs
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected override void Tick(object sender, EventArgs e)
        {
            if (_useTarget)
            {
                if (_complete < _target)
                {
                    Complete++;
                    if (_complete > _target)
                    {
                        Complete = _target;
                    }
                }
                else if (_target < _complete)
                {
                    Complete--;
                    if (_complete < _target)
                    {
                        Complete = _target;
                    }
                }
            }

            base.Tick(sender, e);
        }

        #endregion
    }
}
