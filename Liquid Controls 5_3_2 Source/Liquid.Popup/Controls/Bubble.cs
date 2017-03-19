using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System;

namespace Liquid
{
    public enum BubbleLip
    {
        None,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    /// A popup Bubble control
    /// </summary>
    public partial class Bubble : PopupBase
    {
        #region Visual Elements

        /// <summary> 
        /// Background element template.
        /// </summary>
        public Polygon ElementBackground { get; set; }
        internal const string ElementBackgroundName = "ElementBackground";

        #endregion

        #region Private Properties

        private BubbleLip _lip = BubbleLip.TopLeft;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the amount of roundedness
        /// </summary>
        public double CornerRadius { get; set; }

        /// <summary>
        /// Gets or sets where the lip should be rendered
        /// </summary>
        public BubbleLip Lip
        {
            get { return _lip; }
            set { _lip = value; UpdateVisualState(); }
        }

        #endregion

        #region Constructor

        public Bubble()
        {
            Width = 64;
            Height = 32;
            CornerRadius = 8;
            Lip = BubbleLip.TopLeft;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the bubble to the correct position
        /// </summary>
        private void SetShape()
        {
            Point[] pointsBL = null;
            Point position = new Point(HorizontalOffset, VerticalOffset);
            double halfway = CornerRadius * 0.40f;
            Point inner = new Point(Width - (CornerRadius * 2), Height - (CornerRadius * 2));
            double mainHeight = Height - 10;

            switch (Lip)
            {
                case BubbleLip.BottomLeft:
                    pointsBL = new Point[16] { new Point(CornerRadius, 0), new Point(Width - CornerRadius, 0),
                        new Point(Width - halfway,halfway), new Point(Width,CornerRadius),
						new Point(Width,mainHeight -CornerRadius), new Point(Width - halfway,mainHeight - halfway),
                        new Point(Width - CornerRadius,mainHeight), new Point(CornerRadius + 12,mainHeight),
                        new Point(CornerRadius + 12,Height), new Point(CornerRadius + 2,mainHeight),
                        new Point(CornerRadius,mainHeight), new Point(3,mainHeight - halfway), new Point(0,mainHeight - CornerRadius),
                        new Point(0,CornerRadius), new Point(halfway,halfway), new Point(CornerRadius,0) };
                    break;
                case BubbleLip.BottomRight:
                    pointsBL = new Point[16] { new Point(CornerRadius, 0), new Point(inner.X + CornerRadius, 0),
                        new Point(Width - halfway,halfway), new Point(Width,CornerRadius),
						new Point(Width,mainHeight - CornerRadius), new Point(Width - halfway,mainHeight - halfway),
                        new Point(inner.X + CornerRadius,mainHeight), new Point((Width - CornerRadius) - 2,mainHeight),
                        new Point((Width - CornerRadius) - 2,Height), new Point((Width - CornerRadius) - 12,mainHeight),
						new Point(CornerRadius,mainHeight), new Point(halfway,mainHeight - halfway),
                        new Point(0,mainHeight - CornerRadius), new Point(0,CornerRadius), new Point(halfway,halfway),
                        new Point(CornerRadius,0) };
                    break;
                case BubbleLip.TopRight:
                    pointsBL = new Point[16] { new Point(CornerRadius, 10), new Point((Width - CornerRadius) - 12, 10),
                        new Point((Width - CornerRadius) - 2,0), new Point((Width - CornerRadius) - 2,10),
						new Point(Width - CornerRadius, 10), new Point(Width - halfway,10 + halfway),
                        new Point(Width,10 + CornerRadius), new Point(Width,Height - CornerRadius),
                        new Point(Width - halfway,Height - halfway), new Point(Width - CornerRadius,Height),
						new Point(CornerRadius,Height), new Point(halfway,Height - halfway), new Point(0,Height - CornerRadius),
                        new Point(0,10 + CornerRadius), new Point(halfway,10 + halfway), new Point(CornerRadius,10) };
                    break;
                case BubbleLip.TopLeft:
                    pointsBL = new Point[16] { new Point(CornerRadius, 10), new Point(CornerRadius + 2, 10),
                        new Point(CornerRadius + 2,0), new Point(CornerRadius + 12,10), new Point(Width - CornerRadius, 10),
                        new Point(Width - halfway,10 + halfway), new Point(Width,10 + CornerRadius),
                        new Point(Width,Height - CornerRadius), new Point(Width - halfway,Height - halfway),
                        new Point(Width - CornerRadius,Height), new Point(CornerRadius,Height), new Point(halfway,Height - halfway),
                        new Point(0,Height - CornerRadius), new Point(0,10 + CornerRadius), new Point(halfway,10 + halfway),
                        new Point(CornerRadius,10) };
                    break;
                default:
                    pointsBL = new Point[] { new Point(CornerRadius, 0), new Point(CornerRadius + 2, 0),
                        new Point(Width - CornerRadius, 0),
                        new Point(Width - halfway,halfway), new Point(Width,CornerRadius),
                        new Point(Width,Height - CornerRadius), new Point(Width - halfway,Height - halfway),
                        new Point(Width - CornerRadius,Height), new Point(CornerRadius,Height), new Point(halfway,Height - halfway),
                        new Point(0,Height - CornerRadius), new Point(0,CornerRadius), new Point(halfway,halfway),
                        new Point(CornerRadius,0) };
                    break;
            }

            if (pointsBL != null)
            {
                ElementBackground.Points.Clear();

                foreach (Point p in pointsBL)
                {
                    ElementBackground.Points.Add(p);
                }
            }
        }

        protected override void UpdateVisualState()
        {
            base.UpdateVisualState();

            if (ElementRoot != null)
            {
                SetShape();
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// This is called when the template has been bound to the control
        /// </summary>
        public override void OnApplyTemplate()
        {
            ElementBackground = (Polygon)GetTemplateChild(ElementBackgroundName);

            base.OnApplyTemplate();
            this.SizeChanged += new SizeChangedEventHandler(Bubble_SizeChanged);
        }

        private void Bubble_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualState();
        }

        #endregion
    }
}
