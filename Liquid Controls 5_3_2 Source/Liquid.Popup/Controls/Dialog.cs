using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Liquid
{
    #region Enums

    public enum DialogCanvas
    {
        Main,
        Right
    }

    #endregion

    /// <summary>
    /// An expandable popup Dialog control
    /// </summary>
    public partial class Dialog : DialogBase
    {
        #region Visual Elements

        /// <summary> 
        /// Right Expand template.
        /// </summary>
        internal Storyboard ElementToggleExpandRight { get; set; }
        internal const string ElementToggleExpandRightName = "ElementToggleExpandRight";

        /// <summary> 
        /// Right template.
        /// </summary>
        internal Canvas ElementRight { get; set; }
        internal const string ElementRightName = "ElementRight";

        /// <summary> 
        /// Right Clipping template.
        /// </summary>
        internal RectangleGeometry ElementRightClipping { get; set; }
        internal const string ElementRightClippingName = "ElementRightClipping";

        internal const string ElementRightContentName = "ElementRightContent";

        /// <summary> 
        /// Bottom Expand template.
        /// </summary>
        internal Storyboard ElementToggleExpandBottom { get; set; }
        internal const string ElementToggleExpandBottomName = "ElementToggleExpandBottom";

        /// <summary> 
        /// Bottom template.
        /// </summary>
        internal Canvas ElementBottom { get; set; }
        internal const string ElementBottomName = "ElementBottom";

        /// <summary> 
        /// Right Clipping template.
        /// </summary>
        internal RectangleGeometry ElementBottomClipping { get; set; }
        internal const string ElementBottomClippingName = "ElementBottomClipping";

        internal const string ElementBottomContentName = "ElementBottomContent";

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty ContentRightProperty = DependencyProperty.Register("ContentRight", typeof(object), typeof(Dialog), null);
        public object ContentRight
        {
            get { return (object)GetValue(ContentRightProperty); }
            set { SetValue(ContentRightProperty, value); }
        }

        public static readonly DependencyProperty ContentBottomProperty = DependencyProperty.Register("ContentBottom", typeof(object), typeof(Dialog), null);
        public object ContentBottom
        {
            get { return (object)GetValue(ContentBottomProperty); }
            set { SetValue(ContentBottomProperty, value); }
        }

        #endregion

        #region Public Enums

        public enum State { Normal, Expanding, Expanded, Collapsing }

        #endregion

        #region Private Properities

        private State _stateRight = State.Normal;
        private State _stateBottom = State.Normal;
        private double _expandedWidth = 0;
        private double _expandedHeight = 0;
        private Duration _expandDuration = new Duration(new TimeSpan(0, 0, 0, 0, 200));

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the state of the right expanded state of the dialog
        /// </summary>
        public State RightExpandedState
        {
            get { return _stateRight; }
        }

        /// <summary>
        /// Gets the state of the right expanded state of the dialog
        /// </summary>
        public State BottomExpandedState
        {
            get { return _stateBottom; }
        }

        /// <summary>
        /// Gets or sets the duration of the expand/collapse operations
        /// </summary>
        public Duration ExpandDuration
        {
            get { return _expandDuration; }
            set { _expandDuration = value; }
        }

        /// <summary>
        /// Gets or sets the width of the dialog when expanded
        /// </summary>
        public double ExpandedWidth
        {
            get { return _expandedWidth; }
            set { _expandedWidth = value; }
        }

        /// <summary>
        /// Gets or sets the height of the dialog when expanded
        /// </summary>
        public double ExpandedHeight
        {
            get { return _expandedHeight; }
            set { _expandedHeight = value; }
        }

        #endregion

        #region Constructor

        public Dialog()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method expands out to the right the dialog
        /// </summary>
        public void Expand()
        {
            ExpandRight();
        }

        /// <summary>
        /// This method expands out to the right the dialog
        /// </summary>
        public void ExpandRight()
        {
            DoubleAnimation temp;
            double offsetToMove = _expandedWidth - Width;

            if (ElementRoot != null && _stateRight == State.Normal || _stateRight == State.Expanded)
            {
                if (_stateRight == State.Expanded)
                {
                    offsetToMove = -(Width - OriginalWidth);
                }

                for (int i = 0; i < ElementToggleExpandRight.Children.Count; i++)
                {
                    if (ElementToggleExpandRight.Children[i] is DoubleAnimation)
                    {
                        temp = (DoubleAnimation)ElementToggleExpandRight.Children[i];

                        switch (temp.GetValue(Storyboard.TargetNameProperty).ToString())
                        {
                            case ElementTopButtonsName:
                                temp.From = (double)ElementTopButtons.GetValue(Canvas.LeftProperty);
                                temp.To = temp.From + offsetToMove;
                                break;
                            case ElementButtonsName:
                                temp.From = (double)ElementButtons.GetValue(Canvas.LeftProperty);
                                temp.To = temp.From + offsetToMove;
                                break;
                            case ElementRightContentName:
                                if (_stateRight == State.Expanded)
                                {
                                    temp.From = 0;
                                    temp.To = offsetToMove;
                                }
                                else
                                {
                                    temp.From = -offsetToMove;
                                    temp.To = 0;
                                }
                                break;
                            default:
                                temp.From = Width;
                                temp.To = Width + offsetToMove;
                                break;
                        }
                        temp.Duration = _expandDuration;
                    }
                }

                _stateRight = (_stateRight == State.Normal ? State.Expanding : State.Collapsing);

                if (_stateRight == State.Expanding)
                {
                    ElementWhole.Width = _expandedWidth;
                    ElementWhole.Height = (_expandedHeight > Height ? _expandedHeight : Height);
                    ElementClipping.Rect = new Rect(0, 0, ElementWhole.Width, ElementWhole.Height);
                }

                ElementToggleExpandRight.Begin();
            }
        }

        /// <summary>
        /// This method expands out to the right the dialog
        /// </summary>
        public void ExpandBottom()
        {
            DoubleAnimation temp;
            double offsetToMove = _expandedHeight - Height;

            if (ElementRoot != null && _stateBottom == State.Normal || _stateBottom == State.Expanded)
            {
                if (_stateBottom == State.Expanded)
                {
                    offsetToMove = -(Height - OriginalHeight);
                }

                for (int i = 0; i < ElementToggleExpandBottom.Children.Count; i++)
                {
                    if (ElementToggleExpandBottom.Children[i] is DoubleAnimation)
                    {
                        temp = (DoubleAnimation)ElementToggleExpandBottom.Children[i];

                        switch (temp.GetValue(Storyboard.TargetNameProperty).ToString())
                        {
                            case ElementButtonsName:
                                temp.From = (double)ElementButtons.GetValue(Canvas.TopProperty);
                                temp.To = temp.From + offsetToMove;
                                break;
                            case ElementBottomContentName:
                                if (_stateBottom == State.Expanded)
                                {
                                    temp.From = 0;
                                    temp.To = offsetToMove;
                                }
                                else
                                {
                                    temp.From = -offsetToMove;
                                    temp.To = 0;
                                }
                                break;
                            default:
                                temp.From = Height;
                                temp.To = Height + offsetToMove;
                                break;
                        }
                        temp.Duration = _expandDuration;
                    }
                }

                _stateBottom = (_stateBottom == State.Normal ? State.Expanding : State.Collapsing);

                if (_stateBottom == State.Expanding)
                {
                    ElementWhole.Width = _expandedWidth;
                    ElementWhole.Height = (_expandedHeight > Height ? _expandedHeight : Height);
                    ElementClipping.Rect = new Rect(0, 0, ElementWhole.Width, ElementWhole.Height);
                }

                ElementToggleExpandBottom.Begin();
            }
        }

        #endregion

        #region Private Methods

        protected override void UpdateVisualState()
        {
            FrameworkElement right;

            base.UpdateVisualState();

            if (ElementRoot != null)
            {
                ElementRight.SetValue(Canvas.LeftProperty, OriginalWidth);
                ElementBottom.SetValue(Canvas.TopProperty, (double)ElementButtons.GetValue(Canvas.TopProperty));

                if (_expandedWidth >= OriginalWidth)
                {
                    ElementRight.Width = _expandedWidth - OriginalWidth;
                }
                else
                {
                    ElementRight.Width = 0;
                }

                ElementRight.Height = Height;

                if (ElementRight.Width < 0d)
                {
                    ElementRight.Width = 0d;
                }

                if (_expandedHeight >= OriginalHeight)
                {
                    ElementBottom.Height = _expandedHeight - OriginalHeight;
                }
                else
                {
                    ElementBottom.Height = 0;
                }

                ElementBottom.Width = Width;

                if (ElementBottom.Height < 0d)
                {
                    ElementBottom.Height = 0d;
                }

                if (ContentRight != null)
                {
                    right = (FrameworkElement)ContentRight;

                    if (_stateRight == State.Normal)
                    {
                        right.SetValue(Canvas.LeftProperty, -ElementRight.Width);
                    }
                    else
                    {
                        right.SetValue(Canvas.LeftProperty, 0d);
                    }

                    right.Width = ElementRight.Width;
                    right.Height = Height;
                }

                if (ContentBottom != null)
                {
                    right = (FrameworkElement)ContentBottom;

                    if (_stateBottom == State.Normal)
                    {
                        right.SetValue(Canvas.TopProperty, -ElementBottom.Height);
                    }
                    else
                    {
                        right.SetValue(Canvas.TopProperty, 0d);
                    }

                    right.Width = ElementBottom.Width;
                    right.Height = Height;
                }

                if (_stateRight == State.Expanded)
                {
                    ElementRightClipping.Rect = new Rect(0d, 0d, ElementRight.Width, ElementRight.Height);
                }
                if (_stateBottom == State.Expanded)
                {
                    ElementBottomClipping.Rect = new Rect(0d, 0d, ElementBottom.Width, ElementBottom.Height);
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
            ElementRight = (Canvas)GetTemplateChild(ElementRightName);
            ElementRightClipping = (RectangleGeometry)GetTemplateChild(ElementRightClippingName);
            ElementBottom = (Canvas)GetTemplateChild(ElementBottomName);
            ElementBottomClipping = (RectangleGeometry)GetTemplateChild(ElementBottomClippingName);

            ElementToggleExpandRight = (Storyboard)GetTemplateChild(ElementToggleExpandRightName);
            ElementToggleExpandRight.Completed += new EventHandler(ElementToggleExpandRight_Completed);
            ElementToggleExpandBottom = (Storyboard)GetTemplateChild(ElementToggleExpandBottomName);
            ElementToggleExpandBottom.Completed += new EventHandler(ElementToggleExpandBottom_Completed);

            base.OnApplyTemplate();
        }

        private void ElementToggleExpandRight_Completed(object sender, EventArgs e)
        {
            if (_stateRight == State.Expanding)
            {
                Width = _expandedWidth;
                _stateRight = State.Expanded;
            }
            else
            {
                Width = OriginalWidth;
                _stateRight = State.Normal;
            }

            UpdateButtonPositions();
            UpdateVisualState();
        }

        private void ElementToggleExpandBottom_Completed(object sender, EventArgs e)
        {
            if (_stateBottom == State.Expanding)
            {
                Height = _expandedHeight;
                _stateBottom = State.Expanded;
            }
            else
            {
                Height = OriginalHeight;
                _stateBottom = State.Normal;
            }

            UpdateButtonPositions();
            UpdateVisualState();
        }

        #endregion
    }
}
