using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Liquid
{
    /// <summary>
    /// This is a collapsable fieldset control which can contain child controls
    /// </summary>
    public partial class FieldSet : BaseTreeViewControl
    {
        #region Visual Elements

        /// <summary> 
        /// Panel template.
        /// </summary>
        internal StackPanel ElementPanel { get; set; }
        internal const string ElementPanelName = "ElementPanel";

        /// <summary> 
        /// Background template.
        /// </summary>
        internal Rectangle ElementBackground { get; set; }
        internal const string ElementBackgroundName = "ElementBackground";

        /// <summary> 
        /// Expand template.
        /// </summary>
        internal Expand ElementExpand { get; set; }
        internal const string ElementExpandName = "ElementExpand";

        /// <summary> 
        /// Checkbox template.
        /// </summary>
        internal CheckBox ElementCheckBox { get; set; }
        internal const string ElementCheckBoxName = "ElementCheckBox";

        /// <summary> 
        /// Text template.
        /// </summary>
        internal TextBlock ElementText { get; set; }
        internal const string ElementTextName = "ElementText";

        /// <summary> 
        /// Clipping template.
        /// </summary>
        internal RectangleGeometry ElementClipping { get; set; }
        internal const string ElementClippingName = "ElementClipping";

        /// <summary> 
        /// Content template.
        /// </summary>
        internal ContentPresenter ElementContent { get; set; }
        internal const string ElementContentName = "ElementContent";

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(double), typeof(FieldSet), null);
        public double CornerRadius
        {
            get { return (double)this.GetValue(CornerRadiusProperty); }
            set { base.SetValue(CornerRadiusProperty, value); }
        }

        #endregion

        #region Private Properties

        private bool _expanded = false;
        private string _text = string.Empty;
        private double _expandedHeight = 200;
        private double _expandDirection = 0.0;
        private bool _enableCheckbox = false;
        private bool? _checked = false;
        private Size _panelSize = new Size();
        private Size _contentSize = new Size();
        private bool _setExpandedHeightAsDefault = false;
        private DispatcherTimer _timer = new DispatcherTimer();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets whether the fieldset is collapsed or not
        /// </summary>
        public bool IsExpanded
        {
            get { return _expanded; }
            set
            {
                _expandDirection = (_expanded ? CollapseSpeed : ExpandSpeed);
                _expanded = value;
                if (ElementRoot != null)
                {
                    ElementExpand.IsExpanded = _expanded;
                }
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Gets or sets the label text for the fieldset
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets or sets whether to display a checkbox
        /// </summary>
        public bool EnableCheckbox
        {
            get { return _enableCheckbox; }
            set { _enableCheckbox = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets or sets the checked state of the checkbox if enabled
        /// </summary>
        public bool? IsChecked
        {
            get { return _checked; }
            set { _checked = value; UpdateVisualState(); RaiseCheckChanged(this, EventArgs.Empty); }
        }

        /// <summary>
        /// Gets or sets the rate the fieldset expands
        /// </summary>
        public double ExpandSpeed { get; set; }

        /// <summary>
        /// Gets or sets the rate the fieldset collapses
        /// </summary>
        public double CollapseSpeed { get; set; }

        /// <summary>
        /// Gets or sets the Roundness of the expand icons
        /// </summary>
        public double ExpandRoundness { get; set; }

        #endregion

        #region Public Events

        public event EventHandler CheckChanged;

        #endregion

        #region Constructor

        public FieldSet()
        {
            Width = 100;
            IsExpanded = false;
            ExpandSpeed = 20;
            CollapseSpeed = -30;
            ExpandRoundness = 5;
            IsTabStop = false;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the height of the control when expanded
        /// </summary>
        /// <returns>Height</returns>
        private double GetExpandedHeight()
        {
            if (ElementRoot != null)
            {
                return (double)ElementContent.GetValue(Canvas.TopProperty) + (_contentSize.Height + ElementContent.Margin.Top + ElementContent.Margin.Bottom);
            }
            else
            {
                return 0;
            }
        }

        private void UpdateExpandedHeight()
        {
            if (_setExpandedHeightAsDefault && _panelSize.Height > 0 && _contentSize.Height > 0)
            {
                Height = GetExpandedHeight();
                UpdateVisualState();
                _setExpandedHeightAsDefault = false;
            }
        }

        protected void UpdateVisualState()
        {
            Rect clipping = new Rect(0, 0, Width, _panelSize.Height);
            double opacity = (IsEnabled ? 1 : 0.8);

            if (ElementRoot != null)
            {
                ElementBackground.SetValue(Canvas.TopProperty, _panelSize.Height);
                ElementBackground.SetValue(Canvas.LeftProperty, 0d);
                ElementContent.SetValue(Canvas.TopProperty, _panelSize.Height);

                ElementText.Opacity = opacity;
                ElementCheckBox.Opacity = opacity;
                ElementExpand.IsEnabled = IsEnabled;

                ElementExpand.Cursor = (IsEnabled ? Cursors.Hand : Cursors.Arrow);
                ElementText.Cursor = ElementExpand.Cursor;

                ElementBackground.Width = Width;
                ElementBackground.Height = Height - _panelSize.Height;

                clipping = new Rect(0, 0, Width, double.IsNaN(Height) ? _panelSize.Height : Height);
                if (_expanded)
                {
                    ElementBackground.Visibility = Visibility.Visible;
                }
                else if (_expandDirection == 0.0)
                {
                    ElementBackground.Visibility = Visibility.Collapsed;
                }

                ElementClipping.SetValue(RectangleGeometry.RectProperty, clipping);
                ElementText.Text = _text;

                if (_enableCheckbox)
                {
                    ElementCheckBox.IsChecked = _checked;
                    ElementCheckBox.Visibility = Visibility.Visible;
                }
                else
                {
                    ElementCheckBox.Visibility = Visibility.Collapsed;
                }
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

            ElementPanel = (StackPanel)GetTemplateChild(ElementPanelName);
            ElementBackground = (Rectangle)GetTemplateChild(ElementBackgroundName);
            ElementExpand = (Expand)GetTemplateChild(ElementExpandName);
            ElementCheckBox = (CheckBox)GetTemplateChild(ElementCheckBoxName);
            ElementText = (TextBlock)GetTemplateChild(ElementTextName);
            ElementClipping = (RectangleGeometry)GetTemplateChild(ElementClippingName);
            ElementContent = (ContentPresenter)GetTemplateChild(ElementContentName);

            ElementExpand.IsExpanded = false;
            ElementExpand.Click += new EventHandler(Expand_Click);
            ElementText.MouseLeftButtonDown -= new MouseButtonEventHandler(Text_MouseLeftButtonDown);
            ElementText.MouseLeftButtonDown += new MouseButtonEventHandler(Text_MouseLeftButtonDown);
            ElementCheckBox.Checked += new RoutedEventHandler(ElementCheckBox_CheckChanged);
            ElementCheckBox.Unchecked += new RoutedEventHandler(ElementCheckBox_CheckChanged);
            ElementPanel.SizeChanged += new SizeChangedEventHandler(ElementPanel_SizeChanged);
            ElementContent.SizeChanged += new SizeChangedEventHandler(ElementContent_SizeChanged);
            ElementExpand.CornerRadius = ExpandRoundness;

            if (_expanded)
            {
                ElementExpand.IsExpanded = true;
                _setExpandedHeightAsDefault = true;
                UpdateExpandedHeight();
            }

            UpdateVisualState();

            _timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            _timer.Tick += new EventHandler(Tick);
            _timer.Start();
        }

        private void ElementContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _contentSize = e.NewSize;
            UpdateVisualState();
            UpdateExpandedHeight();
        }

        private void ElementPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _panelSize = e.NewSize;

            if (double.IsNaN(Height))
            {
                Height = _panelSize.Height;
            }
            UpdateVisualState();
            UpdateExpandedHeight();
        }

        /// <summary>
        /// This event is called when the checkbox checked state changes
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void ElementCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (IsEnabled)
            {
                IsChecked = ElementCheckBox.IsChecked;
            }
        }

        /// <summary>
        /// This event is called periodically to expand the fieldset
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected void Tick(object sender, EventArgs e)
        {
            double newHeight = Height;

            if (IsEnabled)
            {
                _expandedHeight = GetExpandedHeight();

                if (_expandDirection != 0.0)
                {
                    newHeight += _expandDirection;
                    if (newHeight > _expandedHeight)
                    {
                        newHeight = _expandedHeight;
                        _expandDirection = 0.0;
                    }
                    else if (newHeight < _panelSize.Height)
                    {
                        newHeight = _panelSize.Height;
                        _expandDirection = 0.0;
                    }

                    Height = newHeight;
                    UpdateVisualState();
                }
            }
        }

        /// <summary>
        /// This event is called when the expand button is clicked
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void Expand_Click(object sender, EventArgs e)
        {
            if (IsEnabled)
            {
                IsExpanded ^= true;
            }
        }

        /// <summary>
        /// This event is called when the title text is clicked
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void Text_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled)
            {
                Expand_Click(sender, e);
            }
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a CheckChanged event to indicate the checkbox has been changed
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseCheckChanged(object sender, EventArgs args)
        {
            if (CheckChanged != null)
            {
                CheckChanged(sender, args);
            }
        }

        #endregion
    }
}
