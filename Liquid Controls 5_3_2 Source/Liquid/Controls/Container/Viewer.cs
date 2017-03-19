using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Liquid
{
    /// <summary>
    /// This allows content to be scrolled and zoomed
    /// </summary>
    public partial class Viewer : LiquidControl
    {
        #region Visual Elements

        /// <summary> 
        /// Fade In template.
        /// </summary>
        internal Storyboard ElementFadeIn { get; set; }
        internal const string ElementFadeInName = "ElementFadeIn";

        /// <summary> 
        /// Fade Out template.
        /// </summary>
        internal Storyboard ElementFadeOut { get; set; }
        internal const string ElementFadeOutName = "ElementFadeOut";

        /// <summary> 
        /// Children template.
        /// </summary>
        internal Canvas ElementChildren { get; set; }
        internal const string ElementChildrenName = "ElementChildren";

        /// <summary> 
        /// Container template.
        /// </summary>
        internal ScrollViewer ElementContainer { get; set; }
        internal const string ElementContainerName = "ElementContainer";

        /// <summary> 
        /// Content template.
        /// </summary>
        internal ContentPresenter ElementContent { get; set; }
        internal const string ElementContentName = "ElementContent";

        /// <summary> 
        /// Scale template.
        /// </summary>
        internal ScaleTransform ElementScale { get; set; }
        internal const string ElementScaleName = "ElementScale";

        /// <summary> 
        /// Slider Grid template.
        /// </summary>
        internal Grid ElementSliderGrid { get; set; }
        internal const string ElementSliderGridName = "ElementSliderGrid";

        /// <summary> 
        /// Slider X template.
        /// </summary>
        internal Slider ElementSliderX { get; set; }
        internal const string ElementSliderXName = "ElementSliderX";

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty SliderBackgroundProperty = DependencyProperty.Register("SliderBackground", typeof(Brush), typeof(Viewer), null);
        public Brush SliderBackground
        {
            get { return (Brush)this.GetValue(SliderBackgroundProperty); }
            set { base.SetValue(SliderBackgroundProperty, (DependencyObject)value); }
        }

        public static readonly DependencyProperty SliderBorderBrushProperty = DependencyProperty.Register("SliderBorderBrush", typeof(Brush), typeof(Viewer), null);
        public Brush SliderBorderBrush
        {
            get { return (Brush)this.GetValue(SliderBorderBrushProperty); }
            set { base.SetValue(SliderBorderBrushProperty, (DependencyObject)value); }
        }

        public static readonly DependencyProperty SliderBorderThicknessProperty = DependencyProperty.Register("SliderBorderThickness", typeof(Thickness), typeof(Viewer), null);
        public Thickness SliderBorderThickness
        {
            get { return (Thickness)this.GetValue(SliderBorderThicknessProperty); }
            set { base.SetValue(SliderBorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty SliderCornerRadiusProperty = DependencyProperty.Register("SliderCornerRadius", typeof(double), typeof(Viewer), null);
        public double SliderCornerRadius
        {
            get { return (double)this.GetValue(SliderCornerRadiusProperty); }
            set { base.SetValue(SliderCornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(Viewer), null);
        public double Zoom
        {
            get { return (double)this.GetValue(ZoomProperty); }
            set
            {
                base.SetValue(ZoomProperty, value);
                if (ElementRoot != null)
                {
                    ElementSliderX.Value = value;
                }
            }
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(Viewer), null);
        public double Minimum
        {
            get { return (double)this.GetValue(MinimumProperty); }
            set { base.SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(Viewer), null);
        public double Maximum
        {
            get { return (double)this.GetValue(MaximumProperty); }
            set { base.SetValue(MaximumProperty, value); }
        }

        #endregion

        #region Private Properties

        private bool _leftButtonDown = false;
        private Point _lastMouseDownPosition = new Point();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the speed when dragging content
        /// </summary>
        public double DragSpeed { get; set; }

        /// <summary>
        /// Gets or sets whether the zoom slider is visible or not
        /// </summary>
        public Visibility ZoomSliderVisibility { get; set; }

        #endregion

        #region Constructor

        public Viewer()
        {
            Zoom = 1;
            DragSpeed = 1;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Scrolls the viewer to a position where the specified element is in view
        /// </summary>
        /// <param name="element">Element to view</param>
        public void ScrollIntoPosition(UIElement element)
        {
            Point p = element.TransformToVisual(ElementChildren).Transform(new Point());
            Size size = new Size((double)element.GetValue(WidthProperty), (double)element.GetValue(HeightProperty));

            if (double.IsNaN(size.Width) || double.IsNaN(size.Height))
            {
                size = new Size((double)element.GetValue(ActualWidthProperty), (double)element.GetValue(ActualHeightProperty));
            }

            ScrollIntoPosition(p, size);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Scrolls into position
        /// </summary>
        /// <param name="position">Position to be in view</param>
        /// <param name="size">Size that should be in view</param>
        private void ScrollIntoPosition(Point position, Size size)
        {
            if ((position.Y + size.Height) * Zoom >= ElementContainer.VerticalOffset + ElementContainer.ViewportHeight)
            {
                ElementContainer.ScrollToVerticalOffset(((position.Y + size.Height) * Zoom) - ElementContainer.ViewportHeight);
            }
            else if (position.Y * Zoom < ElementContainer.VerticalOffset)
            {
                ElementContainer.ScrollToVerticalOffset(position.Y * Zoom);
            }

            if ((position.X + size.Width * Zoom) >= ElementContainer.HorizontalOffset + ElementContainer.ViewportWidth)
            {
                ElementContainer.ScrollToHorizontalOffset(((position.X + size.Width) * Zoom) - ElementContainer.ViewportWidth);
            }
            else if (position.X * Zoom < ElementContainer.HorizontalOffset)
            {
                ElementContainer.ScrollToHorizontalOffset(position.X * Zoom);
            }
        }

        protected override void UpdateVisualState()
        {
            if (ElementRoot != null && RenderSize.Width > 0 && RenderSize.Height > 0)
            {
                ElementContainer.Width = RenderSize.Width;
                ElementContainer.Height = RenderSize.Height;
                ElementChildren.Width = ElementContent.RenderSize.Width* Zoom;
                ElementChildren.Height = ElementContent.RenderSize.Height* Zoom;
                ElementScale.ScaleX = Zoom;
                ElementScale.ScaleY = Zoom;
                ElementSliderGrid.Width = RenderSize.Width * 0.8;
                ElementSliderGrid.SetValue(Canvas.TopProperty, RenderSize.Height - 60);
                ElementSliderGrid.SetValue(Canvas.LeftProperty, (RenderSize.Width - ElementSliderGrid.Width) * 0.5);
                ElementChildren.HorizontalAlignment = HorizontalContentAlignment;
                ElementChildren.VerticalAlignment = VerticalContentAlignment;
                ElementSliderGrid.Visibility = ZoomSliderVisibility;
            }

            base.UpdateVisualState();
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// This is called when the template has been bound to the control
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ElementFadeIn = (Storyboard)GetTemplateChild(ElementFadeInName);
            ElementFadeOut = (Storyboard)GetTemplateChild(ElementFadeOutName);
            ElementContainer = (ScrollViewer)GetTemplateChild(ElementContainerName);
            ElementChildren = (Canvas)GetTemplateChild(ElementChildrenName);
            ElementContent = (ContentPresenter)GetTemplateChild(ElementContentName);
            ElementScale = (ScaleTransform)GetTemplateChild(ElementScaleName);
            ElementSliderGrid = (Grid)GetTemplateChild(ElementSliderGridName);
            ElementSliderX = (Slider)GetTemplateChild(ElementSliderXName);

            ElementContent.MouseLeftButtonDown += new MouseButtonEventHandler(ElementContent_MouseLeftButtonDown);
            ElementContent.MouseLeftButtonUp += new MouseButtonEventHandler(ElementContent_MouseLeftButtonUp);
            ElementContent.MouseMove += new MouseEventHandler(ElementContent_MouseMove);
            ElementContent.SizeChanged += new SizeChangedEventHandler(ElementContent_SizeChanged);
            ElementSliderX.Value = Zoom;
            ElementSliderX.ValueChanged += new RoutedPropertyChangedEventHandler<double>(ElementSliderX_ValueChanged);
            ElementSliderGrid.MouseEnter += new MouseEventHandler(ElementSliderX_MouseEnter);
            ElementSliderGrid.MouseLeave += new MouseEventHandler(ElementSliderX_MouseLeave);

            UpdateVisualState();

            this.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
        }

        private void ElementSliderX_MouseLeave(object sender, MouseEventArgs e)
        {
            ElementFadeOut.Begin();
            ElementFadeIn.Stop();
        }

        private void ElementSliderX_MouseEnter(object sender, MouseEventArgs e)
        {
            ElementFadeIn.Begin();
            ElementFadeOut.Stop();
        }

        private void ElementSliderX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Point p = new Point(ElementContainer.HorizontalOffset, ElementContainer.VerticalOffset);

            if (e.NewValue > e.OldValue)
            {
                p.X = ElementContainer.HorizontalOffset / ElementContainer.ScrollableWidth;
                p.Y = ElementContainer.VerticalOffset / ElementContainer.ScrollableHeight;
            }
            Zoom = e.NewValue;
            UpdateVisualState();
        }

        private void ElementContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualState();
        }

        private void ElementContent_MouseMove(object sender, MouseEventArgs e)
        {
            Point p;

            if (IsEnabled && _leftButtonDown)
            {
                p = e.GetPosition(ElementRoot);
                ElementContainer.ScrollToHorizontalOffset(ElementContainer.HorizontalOffset - ((p.X - _lastMouseDownPosition.X) * Zoom * DragSpeed));
                ElementContainer.ScrollToVerticalOffset(ElementContainer.VerticalOffset - ((p.Y - _lastMouseDownPosition.Y) * Zoom * DragSpeed));

                _lastMouseDownPosition = p;
            }
        }

        private void ElementContent_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled)
            {
                _leftButtonDown = false;
                ElementContent.ReleaseMouseCapture();
            }
        }

        private void ElementContent_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled)
            {
                IsTabStop = true;
                Focus();
                _leftButtonDown = true;
                ElementContent.CaptureMouse();
                _lastMouseDownPosition = e.GetPosition(ElementRoot);
            }
        }

        /// <summary>
        /// Updates the size of the control
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualState();
        }

        #endregion
    }
}
