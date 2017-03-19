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
using System.Windows.Controls.Primitives;

namespace Liquid
{
    public class Magnifier : LiquidControl
    {
        #region Visual Elements

        /// <summary> 
        /// Whole Content template.
        /// </summary>
        internal ContentControl ElementWhole { get; set; }
        internal const string ElementWholeName = "ElementWhole";

        /// <summary> 
        /// Magnifier template.
        /// </summary>
        internal Popup ElementMagnifier { get; set; }
        internal const string ElementMagnifierName = "ElementMagnifier";

        /// <summary> 
        /// Scale template.
        /// </summary>
        internal ScaleTransform ElementScale { get; set; }
        internal const string ElementScaleName = "ElementScale";

        /// <summary> 
        /// Clipping template.
        /// </summary>
        internal RectangleGeometry ElementClipGeometry { get; set; }
        internal const string ElementClipGeometryName = "ElementClipGeometry";

        /// <summary> 
        /// Cover template.
        /// </summary>
        internal ContentControl ElementCover { get; set; }
        internal const string ElementCoverName = "ElementCover";

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty MagnifierContentProperty = DependencyProperty.Register("MagnifierContent", typeof(object), typeof(Magnifier), null);
        public object MagnifierContent
        {
            get { return (object)this.GetValue(MagnifierContentProperty); }
            set { base.SetValue(MagnifierContentProperty, (DependencyObject)value); }
        }

        public static readonly DependencyProperty CoverContentProperty = DependencyProperty.Register("CoverContent", typeof(object), typeof(Magnifier), null);
        public object CoverContent
        {
            get { return (object)this.GetValue(CoverContentProperty); }
            set { base.SetValue(CoverContentProperty, (DependencyObject)value); }
        }

        #endregion

        #region Private Properties

        private bool _leftButtonDown = false;
        private double _zoom = 4;
        private double _cornerRadius = 40;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the amount of zoom when applied
        /// </summary>
        public double Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                if (ElementRoot != null)
                {
                    ElementScale.ScaleX = value;
                    ElementScale.ScaleY = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the corner radius of the magnified area
        /// </summary>
        public double CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                _cornerRadius = value;
                if (ElementRoot != null)
                {
                    ElementClipGeometry.RadiusX = _cornerRadius;
                    ElementClipGeometry.RadiusY = _cornerRadius;
                }
            }
        }

        /// <summary>
        /// Gets or sets the widht/height of the magnified area
        /// </summary>
        public double Size { get; set; }

        #endregion

        #region Constructor

        public Magnifier()
        {
            Zoom = 4;
            CornerRadius = 40;
            Size = 40;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the magnifier clipping position
        /// </summary>
        /// <param name="position">Mouse cursor position</param>
        private void UpdateMagnifierPosition(Point position)
        {
            FrameworkElement content = CoverContent as FrameworkElement;
            
            ElementClipGeometry.Rect = new Rect(position.X - (Size * 0.5), position.Y - (Size * 0.5), Size, Size);
            ElementMagnifier.HorizontalOffset = (position.X * -(Zoom-1));
            ElementMagnifier.VerticalOffset = (position.Y * -(Zoom - 1));

            if (content != null)
            {
                ElementCover.Width = Size * Zoom;
                ElementCover.Height = Size * Zoom;
                content.Width = ElementCover.Width;
                content.Height = ElementCover.Height;

                ElementCover.SetValue(Canvas.LeftProperty, (position.X - (Size * 0.5)) * Zoom);
                ElementCover.SetValue(Canvas.TopProperty, (position.Y - (Size * 0.5)) * Zoom);
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

            ElementWhole = (ContentControl)GetTemplateChild(ElementWholeName);
            ElementMagnifier = (Popup)GetTemplateChild(ElementMagnifierName);
            ElementScale = (ScaleTransform)GetTemplateChild(ElementScaleName);
            ElementClipGeometry = (RectangleGeometry)GetTemplateChild(ElementClipGeometryName);
            ElementCover = (ContentControl)GetTemplateChild(ElementCoverName);

            ElementWhole.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            ElementWhole.MouseLeftButtonUp += new MouseButtonEventHandler(OnMouseLeftButtonUp);
            ElementWhole.MouseMove += new MouseEventHandler(OnMouseMove);

            Zoom = _zoom;
            CornerRadius = _cornerRadius;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ElementRoot != null)
            {
                ElementMagnifier.IsOpen = false;
                _leftButtonDown = false;
                ElementWhole.ReleaseMouseCapture();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (IsEnabled && _leftButtonDown)
            {
                UpdateMagnifierPosition(e.GetPosition(ElementWhole));
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ElementRoot != null)
            {
                ElementMagnifier.IsOpen = true;
                UpdateMagnifierPosition(e.GetPosition(ElementWhole));
                _leftButtonDown = true;
                ElementWhole.CaptureMouse();
            }
        }

        #endregion
    }
}
