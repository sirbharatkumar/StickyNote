using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Liquid
{
    /// <summary>
    /// An control for editing RichText
    /// </summary>
    public class RichTextBox : RichTextBlock
    {
        #region Visual Elements

        /// <summary> 
        /// Container template.
        /// </summary>
        internal ScrollViewer ElementContainer { get; set; }
        internal const string ElementContainerName = "ElementContainer";

        /// <summary> 
        /// Border template.
        /// </summary>
        internal Border ElementGrid { get; set; }
        internal const string ElementGridName = "ElementGrid";

        /// <summary> 
        /// Scale template.
        /// </summary>
        internal ScaleTransform ElementScale { get; set; }
        internal const string ElementScaleName = "ElementScale";

        /// <summary> 
        /// Bubble Scale template.
        /// </summary>
        internal ScaleTransform ElementBubbleScale { get; set; }
        internal const string ElementBubbleScaleName = "ElementBubbleScale";

        /// <summary> 
        /// Context Scale template.
        /// </summary>
        internal ScaleTransform ElementContextScale { get; set; }
        internal const string ElementContextScaleName = "ElementContextScale";

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the visibility of the vertical scrollbar
        /// </summary>
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(RichTextBox), null);
        public new ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)this.GetValue(VerticalScrollBarVisibilityProperty); }
            set { base.SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Gets or sets the visibility of the vertical scrollbar
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(RichTextBox), null);
        public new ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)this.GetValue(HorizontalScrollBarVisibilityProperty); }
            set { base.SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        #endregion

        #region Private Properties

        private bool _autoWrap = true;
        private double _wrapWidth = 180;
        private bool _contentScrollable = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets whether text will wrap automatically with the width of the control or not
        /// </summary>
        public bool AutoWrap
        {
            get { return _autoWrap; }
            set { _autoWrap = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets or sets the text wrapping width
        /// </summary>
        public double WrapWidth
        {
            get { return _wrapWidth; }
            set { _wrapWidth = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets or sets the zoom level
        /// </summary>
        public double Zoom
        {
            get { return _zoom; }
            set
            {
                double d;

                _zoom = value;
                if (ElementChildren != null)
                {
                    d = 1 / _zoom;

                    ElementScale.ScaleX = _zoom;
                    ElementScale.ScaleY = _zoom;
                    ElementBubbleScale.ScaleX = d;
                    ElementBubbleScale.ScaleY = d;
                    ElementContextScale.ScaleX = d;
                    ElementContextScale.ScaleY = d;
                    ElementGrid.Width = ElementChildren.RenderSize.Width * _zoom;
                    ElementGrid.Height = ElementChildren.RenderSize.Height * _zoom;
                }
            }
        }

        #endregion

        #region Public Events

        public event RichTextBoxEventHandler ContentShowVerticalScrollBar;
        public event RichTextBoxEventHandler ContentHideVerticalScrollBar;

        #endregion

        #region Constructor

        public RichTextBox()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves the screen coordinates of the cursor
        /// </summary>
        /// <returns>Position of the cursor</returns>
        public override Point GetCursorCoordinates()
        {
            Point result = base.GetCursorCoordinates();

            if (ElementContainer != null)
            {
                result.X += ElementContainer.HorizontalOffset;
                result.Y += ElementContainer.VerticalOffset;
            }

            return result;
        }

        /// <summary>
        /// Gets the index of content at the provided pixel position
        /// </summary>
        /// <param name="position">Pixel position</param>
        /// <returns>Content index</returns>
        public override int GetContentIndexAtPosition(Point position)
        {
            if (ElementContainer != null)
            {
                return base.GetContentIndexAtPosition(new Point(position.X + -ElementContainer.Padding.Left + -ElementContainer.BorderThickness.Left + ElementContainer.HorizontalOffset, position.Y + -ElementContainer.Padding.Top + -ElementContainer.BorderThickness.Top + ElementContainer.VerticalOffset));
            }
            else
            {
                return -1;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the height available for displaying the popup spell checking bubble
        /// </summary>
        /// <returns>Available height</returns>
        protected override double GetAvailableHeight()
        {
            return (ElementChildren.Height > ElementContainer.RenderSize.Height ? ElementChildren.Height : ElementContainer.RenderSize.Height);
        }

        /// <summary>
        /// Scrolls the area control into position
        /// </summary>
        protected override void OnCursorMoved()
        {
            if (ElementContainer != null)
            {
                double lineHeight = ElementCursor.ActualHeight * _zoom;
                double cursorY = (double)ElementCursor.GetValue(Canvas.TopProperty) * _zoom;
                double lineWidth = 2 * _zoom;
                double cursorX = (double)ElementCursor.GetValue(Canvas.LeftProperty) * _zoom;

                ElementContainer.UpdateLayout();
                ScrollIntoPosition(cursorX, cursorY, lineWidth, lineHeight);
            }
        }

        /// <summary>
        /// Scrolls the area control into position
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Width of the area that must be in view</param>
        /// <param name="height">Height of the area that must be in view</param>
        private void ScrollIntoPosition(double x, double y, double width, double height)
        {
            if (y + height >= ElementContainer.VerticalOffset + ElementContainer.ViewportHeight)
            {
                ElementContainer.ScrollToVerticalOffset((y + height) - ElementContainer.ViewportHeight);
            }
            else if (y < ElementContainer.VerticalOffset)
            {
                ElementContainer.ScrollToVerticalOffset(y);
            }

            if (x + width >= ElementContainer.HorizontalOffset + ElementContainer.ViewportWidth)
            {
                ElementContainer.ScrollToHorizontalOffset((x + width) - ElementContainer.ViewportWidth);
            }
            else if (x < ElementContainer.HorizontalOffset)
            {
                ElementContainer.ScrollToHorizontalOffset(x);
            }
        }

        protected override void UpdateVisualState()
        {
            ElementChildren_Updated(this, null);
            if (ElementContainer != null)
            {
                _setWidth = false;
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

            ElementContainer = (ScrollViewer)GetTemplateChild(ElementContainerName);
            ElementGrid = (Border)GetTemplateChild(ElementGridName);
            ElementScale = (ScaleTransform)GetTemplateChild(ElementScaleName);
            ElementBubbleScale = (ScaleTransform)GetTemplateChild(ElementBubbleScaleName);
            ElementContextScale = (ScaleTransform)GetTemplateChild(ElementContextScaleName);

            Zoom = _zoom;

            ElementChildren.Updated += new RichTextPanelEventHandler(ElementChildren_Updated);
            this.MouseWheel += new MouseWheelEventHandler(OnMouseWheel);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ElementContainer != null)
            {
                ElementContainer.ScrollToVerticalOffset(ElementContainer.VerticalOffset + -e.Delta);
                e.Handled = true;
            }
        }

        private void ElementChildren_Updated(object sender, RichTextPanelEventArgs e)
        {
            double viewportWidth;
            double widthWithScroll;

            if (ElementContainer != null && AutoWrap)
            {
                viewportWidth = RenderSize.Width - (ElementContainer.BorderThickness.Left + ElementContainer.BorderThickness.Right + ElementContainer.Padding.Left + ElementContainer.Padding.Right);
                widthWithScroll = viewportWidth - 17;

                if (ElementChildren.Height > ElementContainer.ViewportHeight && _wrapWidth > widthWithScroll)
                {
                    _wrapWidth = (RenderSize.Width >= 74 ? widthWithScroll : 20);
                    if (VerticalScrollBarVisibility == ScrollBarVisibility.Disabled || VerticalScrollBarVisibility == ScrollBarVisibility.Hidden)
                    {
                        _wrapWidth = viewportWidth;
                    }
                    ElementChildren.Width = _wrapWidth;
                }
                else if (ElementChildren.Height < ElementContainer.ViewportHeight && _wrapWidth < viewportWidth)
                {
                    _wrapWidth = viewportWidth;
                    ElementChildren.Width = _wrapWidth;
                }
                else
                {
                    if (VerticalScrollBarVisibility == ScrollBarVisibility.Disabled || VerticalScrollBarVisibility == ScrollBarVisibility.Hidden)
                    {
                        _wrapWidth = viewportWidth;
                    }
                    ElementChildren.Width = _wrapWidth;
                }

                if (ElementChildren.Height < ElementContainer.ViewportHeight && _contentScrollable)
                {
                    _contentScrollable = false;
                    RaiseRichTextEvent(ContentHideVerticalScrollBar, this, new RichTextBoxEventArgs(), false);
                }
                else if (ElementChildren.Height >= ElementContainer.ViewportHeight && !_contentScrollable)
                {
                    _contentScrollable = true;
                    RaiseRichTextEvent(ContentShowVerticalScrollBar, this, new RichTextBoxEventArgs(), false);
                }

                ElementChildren.MinHeight = ElementContainer.ViewportHeight;
            }
        }

        protected override void Tick(object sender, System.EventArgs e)
        {
            double nodeMoveStep;
            double temp;

            base.Tick(sender, e);

            if (ElementChildren != null)
            {
                if (_mouseDown)
                {
                    nodeMoveStep = 10;

                    if (LastMousePosition.Y * _zoom < ElementContainer.VerticalOffset)
                    {
                        temp = ElementContainer.VerticalOffset - nodeMoveStep;
                        LastMousePosition = new Point(LastMousePosition.X, LastMousePosition.Y - nodeMoveStep);
                        if (temp < 0)
                        {
                            temp = 0;
                        }

                        ElementContainer.ScrollToVerticalOffset(temp);
                    }
                    else if (LastMousePosition.Y * _zoom > ElementContainer.VerticalOffset + ElementContainer.ViewportHeight)
                    {
                        temp = ElementContainer.VerticalOffset + nodeMoveStep;
                        LastMousePosition = new Point(LastMousePosition.X, LastMousePosition.Y + nodeMoveStep);
                        if (temp > ElementContainer.ScrollableHeight)
                        {
                            temp = ElementContainer.ScrollableHeight;
                        }

                        ElementContainer.ScrollToVerticalOffset(temp);
                    }
                }
            }
        }

        protected override void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double viewportWidth;

            if (e.NewSize.Width != e.PreviousSize.Width)
            {
                viewportWidth = e.NewSize.Width - (ElementContainer.BorderThickness.Left + ElementContainer.BorderThickness.Right + ElementContainer.Padding.Left + ElementContainer.Padding.Right);

                if (VerticalScrollBarVisibility == ScrollBarVisibility.Disabled || VerticalScrollBarVisibility == ScrollBarVisibility.Hidden)
                    _wrapWidth = viewportWidth;
                else _wrapWidth = viewportWidth - 18;

                if (_wrapWidth < 20)
                {
                    _wrapWidth = 20;
                }
                ElementChildren.Width = _wrapWidth;
            }
        }

        protected override void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(sender, e);
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(sender, e);
            e.Handled = true;
        }

        protected override void ElementChildren_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ElementGrid.Width = e.NewSize.Width * _zoom;
            ElementGrid.Height = e.NewSize.Height * _zoom;
            SizeUpdated(e.PreviousSize, e.NewSize);
        }

        #endregion
    }
}
