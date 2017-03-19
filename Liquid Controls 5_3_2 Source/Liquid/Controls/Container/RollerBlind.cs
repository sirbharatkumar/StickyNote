using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Liquid
{
    public enum RollerOpenType
    {
        Scroll,
        RotateX,
        RotateY
    };

    public partial class RollerBlind : LiquidControl
    {
        #region Visual Elements

        /// <summary> 
        /// Open template.
        /// </summary>
        internal Storyboard ElementOpen { get; set; }
        internal const string ElementOpenName = "ElementOpen";

        /// <summary> 
        /// Close template.
        /// </summary>
        internal Storyboard ElementClose { get; set; }
        internal const string ElementCloseName = "ElementClose";

        /// <summary> 
        /// Open Rotate template.
        /// </summary>
        internal Storyboard ElementRotateOpen { get; set; }
        internal const string ElementRotateOpenName = "ElementRotateOpen";

        /// <summary> 
        /// Close Rotate template.
        /// </summary>
        internal Storyboard ElementRotateClose { get; set; }
        internal const string ElementRotateCloseName = "ElementRotateClose";

        /// <summary> 
        /// Top Content template.
        /// </summary>
        internal ContentPresenter ElementTop { get; set; }
        internal const string ElementTopName = "ElementTop";

        /// <summary> 
        /// Bottom Content template.
        /// </summary>
        internal ContentPresenter ElementBottom { get; set; }
        internal const string ElementBottomName = "ElementBottom";

        /// <summary> 
        /// Clipping template.
        /// </summary>
        internal RectangleGeometry ElementClipping { get; set; }
        internal const string ElementClippingName = "ElementClipping";

        /// <summary> 
        /// Border template.
        /// </summary>
        internal Border ElementBorder { get; set; }
        internal const string ElementBorderName = "ElementBorder";

        /// <summary> 
        /// Content Projection template.
        /// </summary>
        internal Projection ElementContentProjection { get; set; }
        internal const string ElementContentProjectionName = "ElementContentProjection";

        /// <summary> 
        /// Top Projection template.
        /// </summary>
        internal Projection ElementTopProjection { get; set; }
        internal const string ElementTopProjectionName = "ElementTopProjection";

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty CoverTopProperty = DependencyProperty.Register("CoverTop", typeof(object), typeof(RollerBlind), null);
        public object CoverTop
        {
            get { return (object)this.GetValue(CoverTopProperty); }
            set { base.SetValue(CoverTopProperty, (DependencyObject)value); }
        }

        public static readonly DependencyProperty CoverBottomProperty = DependencyProperty.Register("CoverBottom", typeof(object), typeof(RollerBlind), null);
        public object CoverBottom
        {
            get { return (object)this.GetValue(CoverBottomProperty); }
            set { base.SetValue(CoverBottomProperty, (DependencyObject)value); }
        }

        #endregion

        #region Private Properties

        private DispatcherTimer _timer = new DispatcherTimer();
        private TimeSpan _openSpeed = new TimeSpan(0, 0, 0, 0, 300);
        private TimeSpan _closeSpeed = new TimeSpan(0, 0, 0, 0, 150);
        private bool _waitClose = false;
        private bool _isOpen = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the time before the control will automatically close
        /// </summary>
        public TimeSpan CloseWait
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        /// <summary>
        /// Gets or sets the open speed
        /// </summary>
        public TimeSpan OpenSpeed
        {
            get { return _openSpeed; }
            set { _openSpeed = value; SetSpeeds(); }
        }

        /// <summary>
        /// Gets or sets the close speed
        /// </summary>
        public TimeSpan CloseSpeed
        {
            get { return _closeSpeed; }
            set { _closeSpeed = value; SetSpeeds(); }
        }

        /// <summary>
        /// Gets or sets how the content is switched
        /// </summary>
        public RollerOpenType OpenType { get; set; }

        /// <summary>
        /// Gets or sets whether it will open with a mouse click
        /// </summary>
        public bool OpenOnClick { get; set; }

        /// <summary>
        /// Gets whether the blind is open
        /// </summary>
        public bool IsOpen { get { return _isOpen; } }

        #endregion

        #region Constructor

        public RollerBlind()
        {
            this.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
            this.MouseEnter += new MouseEventHandler(Blind_MouseEnter);
            this.MouseLeave += new MouseEventHandler(Blind_MouseLeave);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(Blind_MouseLeftButtonDown);

            CloseWait = new TimeSpan(0, 0, 0, 0, 100);
            _timer.Tick += new EventHandler(Tick);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the blinds to reveal the content
        /// </summary>
        public void Open()
        {
            if (ElementRoot != null)
            {
                ((FrameworkElement)Content).Visibility = Visibility.Visible;

                if (OpenType == RollerOpenType.Scroll)
                {
                    ElementOpen.Begin();
                    ElementClose.Stop();
                }
                else
                {
                    PlaneProjection p = (PlaneProjection)ElementContentProjection;

                    if (OpenType == RollerOpenType.RotateX)
                    {
                        p.RotationX = 270;
                    }
                    else
                    {
                        p.RotationY = 270;
                    }

                    ElementRotateOpen.Begin();
                    ElementRotateClose.Stop();
                }

                _isOpen = true;
            }
        }

        /// <summary>
        /// Closes the blinds to hide the content
        /// </summary>
        public void Close()
        {
            if (ElementRoot != null)
            {
                ElementTop.Visibility = Visibility.Visible;
                if (OpenType == RollerOpenType.Scroll)
                {
                    if (ElementBottom != null)
                    {
                        ElementBottom.Visibility = Visibility.Visible;
                    }

                    ElementClose.Begin();
                    ElementOpen.Stop();
                }
                else
                {
                    PlaneProjection p = (PlaneProjection)ElementTopProjection;

                    if (OpenType == RollerOpenType.RotateX)
                    {
                        p.RotationX = 270;
                    }
                    else
                    {
                        p.RotationY = 270;
                    }

                    ElementRotateClose.Begin();
                    ElementRotateOpen.Stop();
                }

                _isOpen = false;
            }
        }

        #endregion

        #region Private Methods

        private void SetSpeeds()
        {
            if (ElementRoot != null)
            {
                if (OpenType == RollerOpenType.Scroll)
                {
                    ((DoubleAnimation)ElementOpen.Children[0]).Duration = _openSpeed;
                    ((DoubleAnimation)ElementOpen.Children[1]).Duration = _openSpeed;
                    ((DoubleAnimation)ElementClose.Children[0]).Duration = _closeSpeed;
                    ((DoubleAnimation)ElementClose.Children[1]).Duration = _closeSpeed;
                }
                else
                {
                    ((DoubleAnimation)ElementRotateOpen.Children[0]).Duration = _openSpeed;
                    ((DoubleAnimation)ElementRotateOpen.Children[1]).Duration = _openSpeed;
                    ((DoubleAnimation)ElementRotateOpen.Children[1]).BeginTime = _openSpeed;

                    ((DoubleAnimation)ElementRotateClose.Children[0]).Duration = _closeSpeed;
                    ((DoubleAnimation)ElementRotateClose.Children[0]).BeginTime = _closeSpeed;
                    ((DoubleAnimation)ElementRotateClose.Children[1]).Duration = _closeSpeed;
                }
            }
        }

        protected override void UpdateVisualState()
        {
            FrameworkElement mainElement;
            double width = (ActualWidth > 2 ? ActualWidth - 0 : 2);
            double height = (ActualHeight > 2 ? ActualHeight - 0 : 2);
            double halfHeight;

            if (ElementRoot != null)
            {
                ElementBorder.Width = width;
                ElementBorder.Height = height;

                halfHeight = height * 0.5;

                mainElement = (FrameworkElement)Content;
                mainElement.Width = width;
                mainElement.Height = height;
                ElementClipping.Rect = new Rect(0, 0, width, height);

                ElementTop.Width = width;

                if (OpenType == RollerOpenType.Scroll)
                {
                    ElementTop.Height = halfHeight;
                    ElementBottom.Height = halfHeight;
                    ElementBottom.Width = Width;

                    if (ElementTop.Content != null)
                    {
                        ((FrameworkElement)ElementTop.Content).Width = Width;
                        ((FrameworkElement)ElementTop.Content).Height = halfHeight;
                    }

                    if (ElementBottom.Content != null)
                    {
                        ElementBottom.SetValue(Canvas.TopProperty, halfHeight);
                        ((FrameworkElement)ElementBottom.Content).Width = Width;
                        ((FrameworkElement)ElementBottom.Content).Height = halfHeight;
                    }
                }
                else
                {
                    ElementTop.Height = height;
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

            ElementOpen = (Storyboard)GetTemplateChild(ElementOpenName);
            ElementClose = (Storyboard)GetTemplateChild(ElementCloseName);
            ElementRotateOpen = (Storyboard)GetTemplateChild(ElementRotateOpenName);
            ElementRotateClose = (Storyboard)GetTemplateChild(ElementRotateCloseName);
            ElementTop = (ContentPresenter)GetTemplateChild(ElementTopName);
            ElementBottom = (ContentPresenter)GetTemplateChild(ElementBottomName);
            ElementClipping = (RectangleGeometry)GetTemplateChild(ElementClippingName);
            ElementBorder = (Border)GetTemplateChild(ElementBorderName);
            ElementTopProjection = (Projection)GetTemplateChild(ElementTopProjectionName);
            ElementContentProjection = (Projection)GetTemplateChild(ElementContentProjectionName);

            ElementClose.Completed += new EventHandler(ElementClose_Completed);
            ElementOpen.Completed += new EventHandler(ElementOpen_Completed);

            ((FrameworkElement)Content).Visibility = Visibility.Collapsed;

            UpdateVisualState();
            SetSpeeds();

            if (OpenType == RollerOpenType.RotateX)
            {
                Storyboard.SetTargetProperty(ElementRotateOpen.Children[0], new PropertyPath("RotationX"));
                Storyboard.SetTargetProperty(ElementRotateOpen.Children[1], new PropertyPath("RotationX"));
                Storyboard.SetTargetProperty(ElementRotateClose.Children[0], new PropertyPath("RotationX"));
                Storyboard.SetTargetProperty(ElementRotateClose.Children[1], new PropertyPath("RotationX"));
            }
            else if (OpenType == RollerOpenType.RotateY)
            {
                Storyboard.SetTargetProperty(ElementRotateOpen.Children[0], new PropertyPath("RotationY"));
                Storyboard.SetTargetProperty(ElementRotateOpen.Children[1], new PropertyPath("RotationY"));
                Storyboard.SetTargetProperty(ElementRotateClose.Children[0], new PropertyPath("RotationY"));
                Storyboard.SetTargetProperty(ElementRotateClose.Children[1], new PropertyPath("RotationY"));
            }
        }

        private void ElementOpen_Completed(object sender, EventArgs e)
        {
            ElementTop.Visibility = Visibility.Collapsed;
            if (ElementBottom != null)
            {
                ElementBottom.Visibility = Visibility.Collapsed;
            }
        }

        private void ElementClose_Completed(object sender, EventArgs e)
        {
            ((FrameworkElement)Content).Visibility = Visibility.Collapsed;
            ElementTop.Visibility = Visibility.Visible;
        }

        private void Blind_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (OpenOnClick && !_isOpen)
            {
                Open();
            }
        }

        private void Blind_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!OpenOnClick)
            {
                _waitClose = true;
                _timer.Start();
            }
        }

        private void Blind_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!OpenOnClick)
            {
                Open();
            }
        }

        /// <summary>
        /// Updates the size of the control
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double halfHeight;

            UpdateVisualState();

            if (ElementRoot != null)
            {
                halfHeight = ActualHeight * 0.5;

                ((DoubleAnimation)ElementOpen.Children[0]).From = 0;
                ((DoubleAnimation)ElementOpen.Children[0]).To = -halfHeight;
                ((DoubleAnimation)ElementOpen.Children[1]).From = halfHeight;
                ((DoubleAnimation)ElementOpen.Children[1]).To = ActualHeight;

                ((DoubleAnimation)ElementClose.Children[0]).From = -halfHeight;
                ((DoubleAnimation)ElementClose.Children[0]).To = 0;
                ((DoubleAnimation)ElementClose.Children[1]).From = ActualHeight;
                ((DoubleAnimation)ElementClose.Children[1]).To = halfHeight;
            }
        }

        /// <summary>
        /// This event is called periodically to expand a node when dragging and hovering above it
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected void Tick(object sender, EventArgs e)
        {
            if (ElementRoot != null)
            {
                if (_waitClose)
                {
                    Close();
                }

                _waitClose = false;

                _timer.Stop();
            }
        }

        #endregion
    }
}
