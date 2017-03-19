using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Liquid
{
    public partial class Scroller : LiquidTimerControl
    {
        #region Visual Elements

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

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(Scroller), null);
        public double HorizontalOffset
        {
            get { return (double)this.GetValue(HorizontalOffsetProperty); }
            set { base.SetValue(HorizontalOffsetProperty, value); }
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(double), typeof(Scroller), null);
        public double VerticalOffset
        {
            get { return (double)this.GetValue(VerticalOffsetProperty); }
            set { base.SetValue(VerticalOffsetProperty, value); }
        }

        #endregion

        #region Private Properties

        private bool _leftButtonDown = false;
        private Point _lastMouseDownPosition = new Point();
        private bool _setPositionAsDefault = false;
        private int _ticksUntilRestart = 0;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the direction to scroll
        /// </summary>
        public Point Direction { get; set; }

        /// <summary>
        /// Gets or sets whether scrolling is active
        /// </summary>
        public bool IsScrolling { get; set; }

        /// <summary>
        /// Gets or sets whether the user can control the scrolling with the mouse
        /// </summary>
        public bool EnableUserControl { get; set; }

        /// <summary>
        /// Gets or sets the number of ticks after user intervention the scroller will restart
        /// </summary>
        public int RestartDelay { get; set; }

        public int InitialDelay { get; set; }

        public Point StartPositionOffset { get; set; }

        public bool UseOwnUpdateCall { get; set; }

        #endregion

        #region Constructor

        public Scroller()
        {
            UseOwnUpdateCall = false;
            IsScrolling = true;
            InitialDelay = 0;
            RestartDelay = 50;
            StartPositionOffset = new Point();
        }

        #endregion

        #region Public Methods

        public void Update()
        {
            if (UseOwnUpdateCall)
            {
                if (!_leftButtonDown && _ticksUntilRestart == 0 && IsScrolling && ElementRoot != null)
                {
                    Scroll(Direction, false);
                }
                if (_ticksUntilRestart > 0)
                {
                    _ticksUntilRestart--;
                }
            }
        }

        #endregion

        #region Private Methods

        protected override void UpdateVisualState()
        {
            Rect clipping;

            if (ElementRoot != null)
            {
                clipping = new Rect(0, 0, RenderSize.Width, RenderSize.Height);
                ElementClipping.SetValue(RectangleGeometry.RectProperty, clipping);
            }
        }

        /// <summary>
        /// Sets the start position of the scrolling content
        /// </summary>
        private void UpdateStartPosition()
        {
            if (_setPositionAsDefault && RenderSize.Width > 0 && RenderSize.Height > 0 && ElementContent.RenderSize.Width > 0 && ElementContent.RenderSize.Height > 0)
            {
                if (Direction.X < 0)
                {
                    HorizontalOffset = RenderSize.Width;
                }
                else if (Direction.X > 0)
                {
                    HorizontalOffset = -ElementContent.RenderSize.Width;
                }
                if (Direction.Y < 0)
                {
                    VerticalOffset = RenderSize.Height + StartPositionOffset.Y;
                }
                else if (Direction.Y > 0)
                {
                    VerticalOffset = -ElementContent.RenderSize.Height + StartPositionOffset.Y;
                }

                UpdateVisualState();
                _setPositionAsDefault = false;
            }
        }

        /// <summary>
        /// Scrolls the content in the specified direction
        /// </summary>
        /// <param name="direction">Direction to scroll</param>
        private void Scroll(Point direction, bool forced)
        {
            Point dir = new Point(direction.X, direction.Y);
            double mul;

            if (!forced && StartPositionOffset.Y < 0 && direction.Y > 0)
            {
                /*if (VerticalOffset >= -80 && VerticalOffset < 0)
                {
                    mul = (-VerticalOffset) * 0.5;
                }
                else
                {
                    mul = 32;
                }
                dir = new Point(direction.X * mul, direction.Y * mul);*/

                mul = 32;
                dir = new Point(direction.X * mul, direction.Y * mul);
            }

            HorizontalOffset += dir.X;
            VerticalOffset += dir.Y;

            if (StartPositionOffset.Y < 0)
            {
                if (VerticalOffset > 0 && direction.Y > 0)
                {
                    VerticalOffset = 0;
                    _ticksUntilRestart = InitialDelay;
                    if (!forced)
                    {
                        Direction = new Point(-direction.X, -direction.Y);
                    }
                    return;
                }
                else if (VerticalOffset < -(ElementContent.RenderSize.Height-RenderSize.Height) && direction.Y < 0)
                {
                    VerticalOffset = -(ElementContent.RenderSize.Height-RenderSize.Height);
                    _ticksUntilRestart = InitialDelay >> 4;
                    if (!forced)
                    {
                        Direction = new Point(-direction.X, -direction.Y);
                    }
                    return;
                }
            }

            if (HorizontalOffset + ElementContent.RenderSize.Width < 0)
            {
                HorizontalOffset = RenderSize.Width;
            }
            else if (HorizontalOffset > RenderSize.Width)
            {
                HorizontalOffset = -ElementContent.RenderSize.Width;
            }
            if (VerticalOffset + (ElementContent.RenderSize.Height) <= 0)
            {
                VerticalOffset = RenderSize.Height;
            }
            else if (VerticalOffset > (RenderSize.Height))
            {
                VerticalOffset = -ElementContent.RenderSize.Height;
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

            ElementClipping = (RectangleGeometry)GetTemplateChild(ElementClippingName);
            ElementContent = (ContentPresenter)GetTemplateChild(ElementContentName);

            ElementContent.SizeChanged += new SizeChangedEventHandler(ElementContent_SizeChanged);

            this.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
            this.MouseEnter += new MouseEventHandler(OnMouseEnter);
            this.MouseLeave += new MouseEventHandler(OnMouseLeave);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(OnMouseLeftButtonUp);
            this.MouseMove += new MouseEventHandler(OnMouseMove);

            _setPositionAsDefault = true;
            UpdateStartPosition();

            _ticksUntilRestart = InitialDelay;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (EnableUserControl)
            {
                _leftButtonDown = false;
                _ticksUntilRestart = RestartDelay;
                this.ReleaseMouseCapture();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(ElementRoot);
            Point offset = new Point();

            if (IsEnabled && EnableUserControl && _leftButtonDown)
            {
                if (IsEnabled)
                {
                    offset.X = p.X - _lastMouseDownPosition.X;
                    offset.Y = p.Y - _lastMouseDownPosition.Y;
                }

                if (Direction.X != 0 && Direction.Y != 0)
                {
                }
                else if (Direction.X != 0)
                {
                    offset.Y = 0;
                }
                else if (Direction.Y != 0)
                {
                    offset.X = 0;
                }
                Scroll(offset, true);
            }
            _lastMouseDownPosition = p;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (EnableUserControl)
            {
                _leftButtonDown = true;
                this.CaptureMouse();
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (EnableUserControl)
            {
                ElementContent.Cursor = Cursors.Arrow;
            }
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (EnableUserControl)
            {
                if (Direction.X != 0 && Direction.Y != 0)
                {
                    ElementContent.Cursor = Cursors.Hand;
                }
                else if (Direction.X != 0)
                {
                    ElementContent.Cursor = Cursors.SizeWE;
                }
                else if (Direction.Y != 0)
                {
                    ElementContent.Cursor = Cursors.SizeNS;
                }
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualState();
            UpdateStartPosition();
        }

        /// <summary>
        /// This event is called periodically to expand the fieldset
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected override void Tick(object sender, EventArgs e)
        {
            if (!UseOwnUpdateCall)
            {
                if (!_leftButtonDown && _ticksUntilRestart == 0 && IsScrolling && ElementRoot != null)
                {
                    Scroll(Direction, false);
                }
                if (_ticksUntilRestart > 0)
                {
                    _ticksUntilRestart--;
                }
            }

            base.Tick(sender, e);
        }

        private void ElementContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualState();
            UpdateStartPosition();
        }

        #endregion
    }
}
