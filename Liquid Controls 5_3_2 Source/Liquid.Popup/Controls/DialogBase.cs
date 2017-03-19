using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace Liquid
{
    #region Enums

    public enum DialogButtons
    {
        None = 0,
        OK = 1,
        Yes = 2,
        No = 4,
        Cancel = 8,
        Apply = 16,
        Close = 32,
        Custom = 64
    }

    public enum DialogSizeState
    {
        Normal,
        Maximized,
        Minimized,
        Restored
    }

    #endregion

    public abstract partial class DialogBase : PopupBase
    {
        #region Visual Elements

        /// <summary> 
        /// Scale template.
        /// </summary>
        internal Storyboard ElementScale { get; set; }
        internal const string ElementScaleName = "ElementScale";

        /// <summary> 
        /// Content template.
        /// </summary>
        internal ContentPresenter ElementContent { get; set; }
        internal const string ElementContentName = "ElementContent";

        /// <summary> 
        /// Main Clipping template.
        /// </summary>
        internal RectangleGeometry ElementClipping { get; set; }
        internal const string ElementClippingName = "ElementClipping";

        /// <summary> 
        /// Background template.
        /// </summary>
        internal Rectangle ElementBackground { get; set; }
        internal const string ElementBackgroundName = "ElementBackground";

        /// <summary> 
        /// Top Bar template.
        /// </summary>
        internal Rectangle ElementTopBar { get; set; }
        internal const string ElementTopBarName = "ElementTopBar";

        /// <summary> 
        /// Title template.
        /// </summary>
        internal TextBlock ElementTitle { get; set; }
        internal const string ElementTitleName = "ElementTitle";

        /// <summary> 
        /// Top Buttons template.
        /// </summary>
        internal StackPanel ElementTopButtons { get; set; }
        internal const string ElementTopButtonsName = "ElementTopButtons";

        /// <summary> 
        /// Minimize template.
        /// </summary>
        internal Button ElementMin { get; set; }
        internal const string ElementMinName = "ElementMin";

        /// <summary> 
        /// Restore template.
        /// </summary>
        internal Button ElementRestore { get; set; }
        internal const string ElementRestoreName = "ElementRestore";

        /// <summary> 
        /// Maximize template.
        /// </summary>
        internal Button ElementMax { get; set; }
        internal const string ElementMaxName = "ElementMax";

        /// <summary> 
        /// Close template.
        /// </summary>
        internal Button ElementCross { get; set; }
        internal const string ElementCrossName = "ElementCross";

        /// <summary> 
        /// Whole template.
        /// </summary>
        internal Canvas ElementWhole { get; set; }
        internal const string ElementWholeName = "ElementWhole";

        /// <summary> 
        /// Buttons panel template.
        /// </summary>
        internal StackPanel ElementButtons { get; set; }
        internal const string ElementButtonsName = "ElementButtons";

        /// <summary> 
        /// Cursor template.
        /// </summary>
        internal CursorPlus ElementCursor { get; set; }
        internal const string ElementCursorName = "ElementCursor";

        /// <summary> 
        /// Disable template.
        /// </summary>
        internal Rectangle ElementDisable { get; set; }
        internal const string ElementDisableName = "ElementDisable";

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty DisabledBrushProperty = DependencyProperty.Register("DisabledBrush", typeof(Brush), typeof(DialogBase), null);
        public Brush DisabledBrush
        {
            get { return (Brush)this.GetValue(DisabledBrushProperty); }
            set { base.SetValue(DisabledBrushProperty, (DependencyObject)value); }
        }

        public static readonly DependencyProperty ShadowBrushProperty = DependencyProperty.Register("ShadowBrush", typeof(Brush), typeof(DialogBase), null);
        public Brush ShadowBrush
        {
            get { return (Brush)this.GetValue(ShadowBrushProperty); }
            set { base.SetValue(ShadowBrushProperty, (DependencyObject)value); }
        }

        public static readonly DependencyProperty TitleBarBackgroundProperty = DependencyProperty.Register("TitleBarBackground", typeof(Brush), typeof(DialogBase), null);
        public Brush TitleBarBackground
        {
            get { return (Brush)this.GetValue(TitleBarBackgroundProperty); }
            set { base.SetValue(TitleBarBackgroundProperty, (DependencyObject)value); }
        }

        public static readonly DependencyProperty TitleBarBorderBrushProperty = DependencyProperty.Register("TitleBarBorderBrush", typeof(Brush), typeof(DialogBase), null);
        public Brush TitleBarBorderBrush
        {
            get { return (Brush)this.GetValue(TitleBarBorderBrushProperty); }
            set { base.SetValue(TitleBarBorderBrushProperty, (DependencyObject)value); }
        }

        public static readonly DependencyProperty TitleBarBorderThicknessProperty = DependencyProperty.Register("TitleBarBorderThickness", typeof(Thickness), typeof(DialogBase), null);
        public Thickness TitleBarBorderThickness
        {
            get { return (Thickness)this.GetValue(TitleBarBorderThicknessProperty); }
            set { base.SetValue(TitleBarBorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(double), typeof(DialogBase), null);
        public double CornerRadius
        {
            get { return (double)this.GetValue(CornerRadiusProperty); }
            set { base.SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register("TitleForeground", typeof(Brush), typeof(DialogBase), null);
        public Brush TitleForeground
        {
            get { return (Brush)this.GetValue(TitleForegroundProperty); }
            set { base.SetValue(TitleForegroundProperty, (DependencyObject)value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(DialogBase), new PropertyMetadata(string.Empty, OnTitlePropertyChanged));
        public string Title
        {
            get { return this.GetValue(TitleProperty).ToString(); }
            set { base.SetValue(TitleProperty, value); }
        }

        private static void OnTitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DialogBase db = d as DialogBase;

            db.SetTitle(e.NewValue.ToString());
        }

        public static readonly DependencyProperty OkButtonContentProperty = DependencyProperty.Register("OkButtonContent", typeof(object), typeof(DialogBase), new PropertyMetadata(string.Empty, OnOkButtonContentPropertyChanged));
        public object OkButtonContent
        {
            get { return this.GetValue(OkButtonContentProperty); }
            set { base.SetValue(OkButtonContentProperty, value); }
        }

        private static void OnOkButtonContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DialogBase db = d as DialogBase;

            db._okContent = e.NewValue;
            db.CreateButtons();
        }

        public static readonly DependencyProperty CancelButtonContentProperty = DependencyProperty.Register("CancelButtonContent", typeof(object), typeof(DialogBase), new PropertyMetadata(string.Empty, OnCancelButtonContentPropertyChanged));
        public object CancelButtonContent
        {
            get { return this.GetValue(CancelButtonContentProperty); }
            set { base.SetValue(CancelButtonContentProperty, value); }
        }

        private static void OnCancelButtonContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DialogBase db = d as DialogBase;

            db._cancelContent = e.NewValue;
            db.CreateButtons();
        }

        public static readonly DependencyProperty YesButtonContentProperty = DependencyProperty.Register("YesButtonContent", typeof(object), typeof(DialogBase), new PropertyMetadata(string.Empty, OnYesButtonContentPropertyChanged));
        public object YesButtonContent
        {
            get { return this.GetValue(YesButtonContentProperty); }
            set { base.SetValue(YesButtonContentProperty, value); }
        }

        private static void OnYesButtonContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DialogBase db = d as DialogBase;

            db._yesContent = e.NewValue;
            db.CreateButtons();
        }

        public static readonly DependencyProperty NoButtonContentProperty = DependencyProperty.Register("NoButtonContent", typeof(object), typeof(DialogBase), new PropertyMetadata(string.Empty, OnNoButtonContentPropertyChanged));
        public object NoButtonContent
        {
            get { return this.GetValue(NoButtonContentProperty); }
            set { base.SetValue(NoButtonContentProperty, value); }
        }

        private static void OnNoButtonContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DialogBase db = d as DialogBase;

            db._noContent = e.NewValue;
            db.CreateButtons();
        }

        public static readonly DependencyProperty ApplyButtonContentProperty = DependencyProperty.Register("ApplyButtonContent", typeof(object), typeof(DialogBase), new PropertyMetadata(string.Empty, OnApplyButtonContentPropertyChanged));
        public object ApplyButtonContent
        {
            get { return this.GetValue(ApplyButtonContentProperty); }
            set { base.SetValue(ApplyButtonContentProperty, value); }
        }

        private static void OnApplyButtonContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DialogBase db = d as DialogBase;

            db._applyContent = e.NewValue;
            db.CreateButtons();
        }

        public static readonly DependencyProperty CloseButtonContentProperty = DependencyProperty.Register("CloseButtonContent", typeof(object), typeof(DialogBase), new PropertyMetadata(string.Empty, OnCloseButtonContentPropertyChanged));
        public object CloseButtonContent
        {
            get { return this.GetValue(CloseButtonContentProperty); }
            set { base.SetValue(CloseButtonContentProperty, value); }
        }

        private static void OnCloseButtonContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DialogBase db = d as DialogBase;

            db._closeContent = e.NewValue;
            db.CreateButtons();
        }

        #endregion

        #region Private Static Properties

        private static List<DialogBase> _dialogList = new List<DialogBase>();

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets or sets whether drop shadows should be rendered when dragging
        /// </summary>
        public static bool ShowShadowWhenDragging = true;

        /// <summary>
        /// Gets the number of currently open dialogs
        /// </summary>
        public static int OpenDialogCount
        {
            get
            {
                int result = 0;

                foreach (DialogBase d in _dialogList)
                {
                    if (d.IsOpen && d.SizeState != DialogSizeState.Minimized)
                    {
                        result++;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Determines whether the rendering order of dialogs should be managed automatically
        /// </summary>
        public static bool EnableAutoArrange = true;

        /// <summary>
        /// This property determines the starting z-index value of all dialogs
        /// </summary>
        public static int StartZIndex = 100;

        /// <summary>
        /// Gets or sets the width of a minimized dialog
        /// </summary>
        public static double MinimizedWidth = 150;

        /// <summary>
        /// Gets or sets the offset to render the shadow
        /// </summary>
        public static double ShadowOffset { get; set; }

        #endregion

        #region Private Properties

        private DialogButtons _buttons = DialogButtons.Cancel | DialogButtons.OK;
        private bool _closeEnabled = true;
        private bool _minimizeEnabled = true;
        private bool _maximizeEnabled = true;

        private bool _dragging = false;
        private Point _dragStart = new Point();

        private bool _hoverLeftBorder = false;
        private bool _hoverRightBorder = false;
        private bool _hoverTopBorder = false;
        private bool _hoverBottomBorder = false;
        private bool _borderResizing = false;
        private Point _restorePoint = new Point();
        private Size _restoreSize = new Size();

        private bool _resizable = false;
        private bool _isModal = false;

        internal object _okContent = "OK";
        internal object _cancelContent = "Cancel";
        internal object _closeContent = "Close";
        internal object _applyContent = "Apply";
        internal object _noContent = "No";
        internal object _yesContent = "Yes";

        private List<Button> _pending = new List<Button>();
        private Size _buttonsSize = new Size();
        internal Size _topButtonsSize = new Size();

        private DialogSizeState _sizeState = DialogSizeState.Normal;
        private int _ticksSinceLastMouseDown = 0;

        private Point _scaleTarget = new Point();
        private Size _scaleTargetSize = new Size();
        private Point _bottomRight = new Point();
        private bool _disableOnLoad = false;
        private Style _buttonStyle = null;
        private Visibility _buttonsVisibility = Visibility.Visible;
        private Visibility _topButtonsVisibility = Visibility.Visible;

        private Effect _originalEffect = null;

        #endregion

        #region Protected Properties

        protected double OriginalWidth = 0;
        protected double OriginalHeight = 0;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets whether the dialog can be dragged
        /// </summary>
        public bool IsDraggable { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the button panel
        /// </summary>
        public Visibility ButtonsVisibility
        {
            get { return _buttonsVisibility; }
            set
            {
                if (ElementButtons != null)
                {
                    ElementButtons.Visibility = value;
                }

                _buttonsVisibility = value;
            }
        }

        /// <summary>
        /// Gets or sets the visibility of the top button panel
        /// </summary>
        public Visibility TopButtonsVisibility
        {
            get { return _topButtonsVisibility; }
            set
            {
                if (ElementTopButtons != null)
                {
                    ElementTopButtons.Visibility = value;
                }

                _topButtonsVisibility = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the dialog is modal
        /// </summary>
        public bool IsModal
        {
            get { return _isModal; }
            set
            {
                _isModal = value;
                IsResizable = !value;

                if (value)
                {
                    DisableParent();
                }
                else
                {
                    EnableParent();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the dialog is resizable
        /// </summary>
        public bool IsResizable
        {
            get { return _resizable; }
            set { _resizable = value; }
        }

        /// <summary>
        /// Gets or sets whether the Dialog is minimized/maximized/normal
        /// </summary>
        public DialogSizeState SizeState
        {
            get { return _sizeState; }
            set { ChangeSizeState(value); }
        }

        /// <summary>
        /// Gets or sets the upper-left most point the dialog can be dragged to
        /// </summary>
        public Point TopLeft { get; set; }

        /// <summary>
        /// Gets or sets the lower-right most point the dialog can be dragged to
        /// </summary>
        public Point BottomRight
        {
            get
            {
                FrameworkElement parent;

                if (_bottomRight.X == 0 && _bottomRight.Y == 0 && Parent is FrameworkElement)
                {
                    parent = Parent as FrameworkElement;

                    if (parent is Canvas)
                    {
                        return new Point(parent.ActualWidth, parent.ActualHeight);
                    }
                    else
                    {
                        return new Point(parent.RenderSize.Width, parent.RenderSize.Height);
                    }
                }
                else return _bottomRight;
            }
            set
            {
                _bottomRight = value;
            }
        }

        /// <summary>
        /// Gets or sets the buttons for this dialog
        /// </summary>
        public DialogButtons Buttons
        {
            get { return _buttons; }
            set { _buttons = value; CreateButtons(); }
        }

        /// <summary>
        /// Gets or sets the button the button that was clicked to close the dialog
        /// </summary>
        public DialogButtons Result { get; set; }

        /// <summary>
        /// Gets or sets whether the close button is enabled
        /// </summary>
        public bool IsCloseEnabled
        {
            get { return _closeEnabled; }
            set { _closeEnabled = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets or sets whether the minimize button is enabled
        /// </summary>
        public bool IsMinimizeEnabled
        {
            get { return _minimizeEnabled; }
            set { _minimizeEnabled = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets or sets whether the maximize button is enabled
        /// </summary>
        public bool IsMaximizeEnabled
        {
            get { return _maximizeEnabled; }
            set { _maximizeEnabled = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets or sets the amount of pixels on each size that can be used for dragging
        /// </summary>
        public double GripSize { get; set; }

        /// <summary>
        /// Gets or sets the button margin
        /// </summary>
        public double ButtonMargin { get; set; }

        /// <summary>
        /// Gets the size of the dialog when minimized
        /// </summary>
        public Size MinimizedSize
        {
            get
            {
                if (ElementRoot != null)
                {
                    return new Size(MinimizedWidth, ElementTopBar.Height + ((ElementTopBar.Margin.Top + ElementWhole.Margin.Top) * 2));
                }
                else
                {
                    return new Size();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the button style
        /// </summary>
        public Style ButtonStyle
        {
            get { return _buttonStyle; }
            set
            {
                _buttonStyle = value;
                if (ElementRoot != null)
                {
                    foreach (Button b in ElementButtons.Children)
                    {
                        b.Style = value;
                    }

                }
            }
        }

        /// <summary>
        /// Gets or sets whether the background is overlayed when IsModal=true
        /// </summary>
        public bool EnableBackgroundCover { get; set; }

        #endregion

        #region Public Events

        public event DialogEventHandler Apply;
        public event DialogEventHandler Resized;
        public event DialogEventHandler HasFocus;
        public event DialogEventHandler Maximizing;
        public event DialogEventHandler Maximized;
        public event DialogEventHandler Minimizing;
        public event DialogEventHandler Minimized;
        public event DialogEventHandler Restoring;
        public event DialogEventHandler Restored;
        public event DialogEventHandler DisableBackground;
        public event DialogEventHandler EnableBackground;
        public event DialogEventHandler DialogTitleMouseDown;
        public event DialogEventHandler DialogTitleMouseMove;
        public event DialogEventHandler DialogTitleMouseUp;

        #endregion

        #region Constructor

        public DialogBase()
        {
            this.Visibility = Visibility.Collapsed;
            StartPosition = DialogStartPosition.CenterParent;

            Result = DialogButtons.None;
            GripSize = 4;
            ButtonMargin = 8;
            MinSize = new Size(100, 100);
            TopLeft = new Point();
            IsDraggable = true;

            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            EnableBackgroundCover = true;

            CreateButtons();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the element sizes after a change to the size of the dialog
        /// </summary>
        public void UpdateSize()
        {
            if (ElementRoot != null)
            {
                OriginalWidth = Width;
                OriginalHeight = Height;
                _restoreSize = new Size(Width, Height);

                UpdateButtonPositions();
                UpdateVisualState();
            }
        }

        /// <summary>
        /// This method displays the dialog
        /// </summary>
        public override void Show()
        {
            base.Show();

            if (!IsOpen)
            {
                Result = DialogButtons.None;

                if (_isModal)
                {
                    DisableParent();
                }
                else
                {
                    EnableParent();
                }
            }

            if (_sizeState == DialogSizeState.Minimized)
            {
                Restore();
            }
        }

        /// <summary>
        /// This method shows the dialog in modal format
        /// </summary>
        public void ShowAsModal()
        {
            if (!IsOpen)
            {
                IsModal = true;
                Show();
            }
        }

        /// <summary>
        /// This method closes the dialog
        /// </summary>
        public override void Close()
        {
            base.Close();

            EnableParent();
        }

        /// <summary>
        /// This method closes the dialog by simulating a button click
        /// </summary>
        /// <param name="button">A button type to close the dialog using</param>
        public virtual void Close(DialogButtons button)
        {
            AttemptClose(button, button.ToString().ToLower());
        }

        /// <summary>
        /// Brings the dialog to the front of all other dialogs
        /// </summary>
        public void BringToFront()
        {
            int i;

            if (EnableAutoArrange)
            {
                _dialogList.Remove(this);
                _dialogList.Add(this);

                for (i = 0; i < _dialogList.Count; i++)
                {
                    _dialogList[i].SetValue(Canvas.ZIndexProperty, StartZIndex + i);
                }
            }
        }

        /// <summary>
        /// Positions the dialog behind all other dialogs
        /// </summary>
        public void SendToBack()
        {
            int i;

            if (EnableAutoArrange)
            {
                _dialogList.Remove(this);
                _dialogList.Insert(0, this);

                for (i = 0; i < _dialogList.Count; i++)
                {
                    _dialogList[i].SetValue(Canvas.ZIndexProperty, StartZIndex + i);
                }
            }
        }

        /// <summary>
        /// Deletes all buttons
        /// </summary>
        public void DeleteButtons()
        {
            if (ElementRoot != null)
            {
                ElementButtons.Children.Clear();
            }

            _pending.Clear();
        }

        /// <summary>
        /// Creates a button to be placed at the bottom of the dialog
        /// </summary>
        /// <param name="button">Button type</param>
        /// <param name="content">Content</param>
        /// <param name="tag">Indentity tag</param>
        public Button CreateButton(DialogButtons button, object content, string tag)
        {
            return CreateButton(null, button, content, tag, false);
        }

        /// <summary>
        /// Creates a button to be placed at the bottom of the dialog
        /// </summary>
        /// <param name="button">Button type</param>
        /// <param name="content">Content</param>
        /// <param name="tag">Indentity tag</param>
        /// <param name="insertBefore">Indicates whether the button should appear before or after the existing buttons</param>
        public Button CreateButton(DialogButtons button, object content, string tag, bool insertBefore)
        {
            return CreateButton(null, button, content, tag, insertBefore);
        }

        /// <summary>
        /// Maximizes the dialog to dimensions set in MaxSize
        /// </summary>
        public virtual void Maximize()
        {
            DialogEventArgs args = new DialogEventArgs();

            if (_maximizeEnabled)
            {
                RaiseMaximizing(this, args);
                if (!args.Cancel)
                {
                    ChangeSizeState(DialogSizeState.Maximized);
                }
            }
        }

        /// <summary>
        /// Restores the dialog to its size before it was maximized
        /// </summary>
        public virtual void Restore()
        {
            DialogEventArgs args = new DialogEventArgs();

            RaiseRestoring(this, args);
            if (!args.Cancel)
            {
                ChangeSizeState(DialogSizeState.Restored);
                BringToFront();
            }
        }

        /// <summary>
        /// Minimizes the dialog
        /// </summary>
        public virtual void Minimize()
        {
            DialogEventArgs args = new DialogEventArgs();
            Point target = new Point();

            if (_minimizeEnabled)
            {
                RaiseMinimizing(this, args);
                if (!args.Cancel)
                {
                    if (args.Parameter is Point)
                    {
                        target = (Point)args.Parameter;
                    }

                    ChangeSizeState(DialogSizeState.Minimized, target);
                }
            }
        }

        /// <summary>
        /// Removes any static references to this object
        /// </summary>
        public void Dispose()
        {
            _dialogList.Remove(this);
        }

        /// <summary>
        /// Gets the Button object for the specified button ID
        /// </summary>
        /// <param name="button">Button ID</param>
        /// <returns>Button Control</returns>
        public Button GetButton(string button)
        {
            Button result = null;

            if (ElementButtons != null && _pending.Count == 0)
            {
                foreach (Button b in ElementButtons.Children)
                {
                    if (b.Tag.ToString() == button.ToString().ToLower())
                    {
                        result = b;
                    }
                }
            }
            else
            {
                foreach (Button b in _pending)
                {
                    if (b.Tag.ToString() == button.ToString().ToLower())
                    {
                        result = b;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Enables all buttons
        /// </summary>
        public void EnableAllButtons()
        {
            SetButtonEnabledState(true);
        }

        /// <summary>
        /// Disables all buttons
        /// </summary>
        public void DisableAllButtons()
        {
            SetButtonEnabledState(false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the IsEnabled property for all buttons
        /// </summary>
        /// <param name="enabled">The new value for all buttons</param>
        private void SetButtonEnabledState(bool enabled)
        {
            if (ElementCross != null)
            {
                ElementCross.IsEnabled = enabled;
            }

            if (ElementButtons != null && _pending.Count == 0)
            {
                foreach (Button b in ElementButtons.Children)
                {
                    b.IsEnabled = enabled;
                }
            }
            else
            {
                foreach (Button b in _pending)
                {
                    b.IsEnabled = enabled;
                }
            }
        }

        /// <summary>
        /// Starts the animation to change the dialog size
        /// </summary>
        /// <param name="newState">New dialog size state</param>
        private void ChangeSizeState(DialogSizeState newState)
        {
            ChangeSizeState(newState, new Point());
        }

        /// <summary>
        /// Starts the animation to change the dialog size
        /// </summary>
        /// <param name="newState">New dialog size state</param>
        /// <param name="target">Position to place the dialog</param>
        private void ChangeSizeState(DialogSizeState newState, Point target)
        {
            if (ElementRoot != null)
            {
                if (_originalEffect == null)
                {
                    _originalEffect = ElementBackground.Effect;
                }

                if (newState != DialogSizeState.Normal)
                {
                    ElementBackground.Effect = null;
                }
                else
                {
                    ElementBackground.Effect = _originalEffect;
                }

                switch (newState)
                {
                    case DialogSizeState.Minimized:
                        _restorePoint = new Point(HorizontalOffset, VerticalOffset);
                        _restoreSize = new Size(Width, Height);

                        ElementContent.Visibility = Visibility.Collapsed;
                        SetSizeChangeStoryboard(_restorePoint, _restoreSize, target, MinimizedSize);
                        break;
                    case DialogSizeState.Maximized:
                        _restorePoint = new Point(HorizontalOffset, VerticalOffset);
                        _restoreSize = new Size(Width, Height);

                        SetSizeChangeStoryboard(_restorePoint, _restoreSize, new Point(0, 0), MaxSize);
                        break;
                    case DialogSizeState.Restored:
                        if (_sizeState == DialogSizeState.Minimized)
                        {
                            ElementTopBar.Visibility = Visibility.Collapsed;
                        }

                        SetSizeChangeStoryboard(new Point(HorizontalOffset, VerticalOffset),
                            new Size(Width, Height), _restorePoint, _restoreSize);
                        break;
                    default:
                        ElementContent.Visibility = Visibility.Visible;
                        break;
                }
            }
            _sizeState = newState;
            if (!(Parent is Canvas) && _sizeState != DialogSizeState.Normal)
            {
                ElementScale_Completed(this, null);
            }
        }

        /// <summary>
        /// Sets the scaling animation info
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="startSize">Start size</param>
        /// <param name="target">Target position</param>
        /// <param name="targetSize">Target size</param>
        private void SetSizeChangeStoryboard(Point start, Size startSize, Point target, Size targetSize)
        {
            double t = targetSize.Width - (startSize.Width - ElementTopBar.Width);

            // Scaling
            ((DoubleAnimation)ElementScale.Children[0]).From = startSize.Width;
            ((DoubleAnimation)ElementScale.Children[0]).To = targetSize.Width;
            ((DoubleAnimation)ElementScale.Children[1]).From = startSize.Height;
            ((DoubleAnimation)ElementScale.Children[1]).To = targetSize.Height;
            ((DoubleAnimation)ElementScale.Children[2]).From = ElementTopBar.Width;
            ((DoubleAnimation)ElementScale.Children[2]).To = t;
            // Positioning
            ((DoubleAnimation)ElementScale.Children[3]).From = start.X;
            ((DoubleAnimation)ElementScale.Children[3]).To = target.X;
            ((DoubleAnimation)ElementScale.Children[4]).From = start.Y;
            ((DoubleAnimation)ElementScale.Children[4]).To = target.Y;

            _scaleTarget = target;
            _scaleTargetSize = targetSize;
            SetClippingArea(targetSize);

            if (Parent is Canvas)
            {
                ElementScale.Begin();
            }
        }

        /// <summary>
        /// Disables the parent element by placing a transparent rectangle over the surface
        /// </summary>
        private void DisableParent()
        {
            FrameworkElement parent;
            Grid grid;
            DialogEventArgs args;

            if (ElementRoot != null && Parent is Panel && ElementDisable != null && EnableBackgroundCover)
            {
                args = new DialogEventArgs();

                if (DisableBackground != null)
                {
                    DisableBackground(this, args);
                }

                if (!args.Cancel)
                {
                    parent = (FrameworkElement)Parent;

                    ElementDisable.Width = parent.ActualWidth;
                    ElementDisable.Height = parent.ActualHeight;

                    if (parent is Grid)
                    {
                        grid = (Grid)parent;
                        if (grid.RowDefinitions.Count > 0)
                        {
                            ElementDisable.SetValue(Grid.RowSpanProperty, grid.RowDefinitions.Count);
                        }
                        if (grid.ColumnDefinitions.Count > 0)
                        {
                            ElementDisable.SetValue(Grid.ColumnSpanProperty, grid.ColumnDefinitions.Count);
                        }
                    }
                    else
                    {
                        ElementDisable.SetValue(Canvas.ZIndexProperty, StartZIndex - 1);
                    }

                    if (!((Panel)Parent).Children.Contains(ElementDisable))
                    {
                        ((Panel)Parent).Children.Add(ElementDisable);
                    }
                }
            }
            else
            {
                _disableOnLoad = true;
            }
        }

        /// <summary>
        /// Enables the parent element by removing the transparent rectangle
        /// </summary>
        private void EnableParent()
        {
            UIElement parent;
            DialogEventArgs args;

            if (Parent is Panel && EnableBackgroundCover && ElementDisable != null)
            {
                args = new DialogEventArgs();

                if (EnableBackground != null)
                {
                    EnableBackground(this, args);
                }

                if (!args.Cancel)
                {
                    parent = (UIElement)Parent;

                    ((Panel)Parent).Children.Remove(ElementDisable);
                }
            }
        }

        /// <summary>
        /// Creates the default buttons
        /// </summary>
        internal void CreateButtons()
        {
            if (ElementRoot != null)
            {
                ElementButtons.Children.Clear();
            }

            _pending.Clear();

            CreateButton(GetButton("apply"), DialogButtons.Apply, _applyContent, "apply", false);
            CreateButton(GetButton("close"), DialogButtons.Close, _closeContent, "close", false);
            CreateButton(GetButton("cancel"), DialogButtons.Cancel, _cancelContent, "cancel", false);
            CreateButton(GetButton("no"), DialogButtons.No, _noContent, "no", false);
            CreateButton(GetButton("ok"), DialogButtons.OK, _okContent, "ok", false);
            CreateButton(GetButton("yes"), DialogButtons.Yes, _yesContent, "yes", false);

            UpdateVisualState();
        }

        private Button CreateButton(Button buttonControl, DialogButtons button, object content, string tag, bool before)
        {
            Button result;

            if (content is string)
            {
                content = new TextBlock()
                {
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    FontStyle = FontStyle,
                    Text = content.ToString()
                };
            }

            result = (buttonControl != null ? buttonControl : new Button());
            result.Content = content;
            result.Tag = tag;
            result.Margin = new Thickness(ButtonMargin, 0, 0, 0);
            result.Padding = new Thickness(8, 4, 8, 4);
            result.Click += new RoutedEventHandler(Button_Clicked);

            if ((_buttons & button) != 0 || button == DialogButtons.Custom)
            {
                if (ElementRoot != null)
                {
                    if (before)
                    {
                        ElementButtons.Children.Insert(0, result);
                    }
                    else
                    {
                        ElementButtons.Children.Add(result);
                    }
                }
                else
                {
                    if (before)
                    {
                        _pending.Insert(0, result);
                    }
                    else
                    {
                        _pending.Add(result);
                    }
                }
            }

            return result;
        }

        protected void UpdateButtonPositions()
        {
            Point position = new Point(Width - (_buttonsSize.Width + ElementButtons.Margin.Right), Height - (_buttonsSize.Height + (ElementButtons.Margin.Bottom + ElementButtons.Margin.Top)));

            ElementButtons.SetValue(Canvas.TopProperty, position.Y);
            ElementButtons.SetValue(Canvas.LeftProperty, position.X);

            ElementTopButtons.SetValue(Canvas.LeftProperty, Width - _topButtonsSize.Width);
            Title = this.GetValue(DialogBase.TitleProperty).ToString();
        }

        private bool IsSizeWithinRange(double newWidth, double newHeight)
        {
            return (newWidth >= MinSize.Width && newWidth <= MaxSize.Width) && (newHeight >= MinSize.Height && newHeight <= MaxSize.Height);
        }

        internal void SetTitle(string title)
        {
            if (ElementRoot != null)
            {
                ElementTitle.Text = TrimTextToWidth(ElementTitle, title, ElementTopBar.Width - _topButtonsSize.Width);
            }
        }

        internal string TrimTextToWidth(TextBlock textBlock, string text, double width)
        {
            int len = text.Length;

            textBlock.Text = text;

            while (textBlock.ActualWidth >= width && len >= 0)
            {
                textBlock.Text = text.Substring(0, len) + "...";
                len--;
            }

            return textBlock.Text;
        }

        /// <summary>
        /// Sets the correct mouse cursor
        /// </summary>
        /// <param name="p">Position</param>
        private void UpdateMouseCursor(Point p)
        {
            if (!_borderResizing)
            {	// Were just hovering over the border at the moment
                _hoverLeftBorder = (p.X < GripSize);
                _hoverRightBorder = (p.X > Width - GripSize);
                _hoverTopBorder = (p.Y < GripSize);
                _hoverBottomBorder = (p.Y > Height - GripSize);
                this.Cursor = Cursors.Arrow;
            }

            if (_hoverLeftBorder | _hoverRightBorder && _hoverTopBorder | _hoverBottomBorder)
            {
                this.Cursor = Cursors.None;
                if ((_hoverLeftBorder & _hoverTopBorder) || (_hoverRightBorder & _hoverBottomBorder))
                {
                    ElementCursor.Show(MoreCursors.SizeNWSE, p);
                }
                else
                {
                    ElementCursor.Show(MoreCursors.SizeNESW, p);
                }
            }
            else
            {
                ElementCursor.Hide();

                if (_hoverLeftBorder | _hoverRightBorder)
                {
                    this.Cursor = Cursors.SizeWE;
                }
                if (_hoverTopBorder | _hoverBottomBorder)
                {
                    this.Cursor = Cursors.SizeNS;
                }
            }
        }

        private void SetClippingArea(Size size)
        {
            ElementWhole.Width = size.Width - (ElementWhole.Margin.Left + ElementWhole.Margin.Right);
            ElementWhole.Height = size.Height - (ElementWhole.Margin.Top + ElementWhole.Margin.Bottom);

            ElementClipping.Rect = new Rect(0, 0, ElementWhole.Width, ElementWhole.Height);
        }

        private void AttemptClose(DialogButtons button, object tag)
        {
            DialogEventArgs args = new DialogEventArgs();

            Result = button;

            args.Tag = tag;
            RaiseClosed(this, args);

            if (!args.Cancel)
            {
                if (tag.ToString() != "apply")
                {
                    Close();
                }
            }
        }

        protected override void UpdateVisualState()
        {
            FrameworkElement content;
            double buttonsHeight = 0;
            double innerWidth;

            if (ElementRoot != null)
            {
                innerWidth = (ElementWhole.Margin.Left + ElementWhole.Margin.Right + ElementTopBar.Margin.Left + ElementTopBar.Margin.Right);

                ElementWhole.Width = Width - (ElementWhole.Margin.Left + ElementWhole.Margin.Right);
                ElementWhole.Height = Height - (ElementWhole.Margin.Top + ElementWhole.Margin.Bottom);

                ElementClipping.Rect = new Rect(0, 0, ElementWhole.Width, ElementWhole.Height);

                if (Content is FrameworkElement && _sizeState != DialogSizeState.Minimized)
                {
                    buttonsHeight = (ElementButtons.Children.Count > 0 ? _buttonsSize.Height + ElementButtons.Margin.Bottom + ElementButtons.Margin.Top : 6);
                    content = (FrameworkElement)Content;
                    content.Width = (_resizable ? Width : OriginalWidth) - innerWidth - (content.Margin.Left + content.Margin.Right);
                    content.Height = (_resizable ? Height : OriginalHeight) - (ElementTopBar.Height + buttonsHeight + content.Margin.Top + content.Margin.Bottom);
                }

                ElementBackground.Width = Width;
                ElementBackground.Height = Height;
                ElementBackground.StrokeThickness = BorderThickness.Left;
                ElementTopBar.Width = (Width - innerWidth) - 1;

                ElementCross.IsEnabled = _closeEnabled;
                if ((_restoreSize.Width == Width && _restoreSize.Height == Height) || _isModal || !_resizable)
                {
                    ElementMax.Visibility = Visibility.Visible;
                    ElementRestore.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ElementMax.Visibility = Visibility.Collapsed;
                    ElementRestore.Visibility = Visibility.Visible;
                }
                if (!_minimizeEnabled)
                {
                    ElementMin.Visibility = Visibility.Collapsed;
                }
                if (!_maximizeEnabled)
                {
                    ElementMax.Visibility = Visibility.Collapsed;
                }
                if (_sizeState == DialogSizeState.Minimized)
                {
                    ElementRestore.Visibility = Visibility.Visible;
                }
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

            ElementScale = (Storyboard)GetTemplateChild(ElementScaleName);
            ElementContent = (ContentPresenter)GetTemplateChild(ElementContentName);
            ElementClipping = (RectangleGeometry)GetTemplateChild(ElementClippingName);
            ElementBackground = (Rectangle)GetTemplateChild(ElementBackgroundName);
            ElementTopBar = (Rectangle)GetTemplateChild(ElementTopBarName);
            ElementTitle = (TextBlock)GetTemplateChild(ElementTitleName);
            ElementTopButtons = (StackPanel)GetTemplateChild(ElementTopButtonsName);
            ElementMin = (Button)GetTemplateChild(ElementMinName);
            ElementRestore = (Button)GetTemplateChild(ElementRestoreName);
            ElementMax = (Button)GetTemplateChild(ElementMaxName);
            ElementCross = (Button)GetTemplateChild(ElementCrossName);
            ElementWhole = (Canvas)GetTemplateChild(ElementWholeName);
            ElementButtons = (StackPanel)GetTemplateChild(ElementButtonsName);
            ElementCursor = (CursorPlus)GetTemplateChild(ElementCursorName);
            ElementDisable = (Rectangle)GetTemplateChild(ElementDisableName);

            ((Canvas)ElementRoot).Children.Remove(ElementDisable);

            Storyboard.SetTarget(ElementScale.Children[3], this);
            Storyboard.SetTarget(ElementScale.Children[4], this);

            this.MouseLeftButtonDown += new MouseButtonEventHandler(DialogBase_MouseLeftButtonDown);

            ElementCross.Click += new RoutedEventHandler(Cross_Clicked);
            ElementMin.Click += new RoutedEventHandler(Min_Click);
            ElementMax.Click += new RoutedEventHandler(Max_Click);
            ElementRestore.Click += new RoutedEventHandler(Restore_Click);
            ElementButtons.SizeChanged += new SizeChangedEventHandler(ElementButtons_SizeChanged);
            ElementTopButtons.SizeChanged += new SizeChangedEventHandler(ElementTopButtons_SizeChanged);

            ElementScale.Completed += new EventHandler(ElementScale_Completed);
            ElementBackground.MouseLeftButtonDown += new MouseButtonEventHandler(ElementBorder_MouseLeftButtonDown);
            ElementBackground.MouseLeftButtonUp += new MouseButtonEventHandler(ElementBorder_MouseLeftButtonUp);
            ElementBackground.MouseMove += new MouseEventHandler(ElementBorder_MouseMove);
            ElementBackground.MouseLeave += new MouseEventHandler(ElementBorder_MouseLeave);
            ElementTopBar.MouseLeftButtonDown += new MouseButtonEventHandler(TitleMouseDown);
            ElementTopBar.MouseLeftButtonUp += new MouseButtonEventHandler(TitleMouseUp);
            ElementTopBar.MouseMove += new MouseEventHandler(TitleMouseMove);
            ElementFadeIn.Completed += new EventHandler(ElementFadeIn_Completed);

            OriginalWidth = Width;
            OriginalHeight = Height;
            _restoreSize = new Size(Width, Height);

            foreach (Button b in _pending)
            {
                ElementButtons.Children.Add(b);
            }
            _pending.Clear();
            ChangeSizeState(_sizeState);

            if (EnableAutoArrange)
            {
                BringToFront();
            }

            ButtonStyle = _buttonStyle;
            UpdateVisualState();

            ((FrameworkElement)Parent).SizeChanged += ParentDialogBase_SizeChanged;
            SetTitle(this.GetValue(DialogBase.TitleProperty).ToString());

            TopButtonsVisibility = _topButtonsVisibility;
            ButtonsVisibility = _buttonsVisibility;
        }

        private void ParentDialogBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement parent;

            if (ElementRoot != null && Parent is Panel && ElementDisable != null && EnableBackgroundCover)
            {
                parent = (FrameworkElement)Parent;

                ElementDisable.Width = e.NewSize.Width;
                ElementDisable.Height = e.NewSize.Height;
                SetOpeningPosition();
            }
        }

        private void ElementFadeIn_Completed(object sender, EventArgs e)
        {
            if (_disableOnLoad)
            {
                DisableParent();
            }
        }

        private void ElementScale_Completed(object sender, EventArgs e)
        {
            DialogEventArgs args = new DialogEventArgs();

            HorizontalOffset = _scaleTarget.X;
            VerticalOffset = _scaleTarget.Y;
            Width = _scaleTargetSize.Width;
            Height = _scaleTargetSize.Height;

            switch (_sizeState)
            {
                case DialogSizeState.Minimized:
                    ElementButtons.Visibility = Visibility.Collapsed;
                    ElementMin.Visibility = Visibility.Collapsed;
                    ElementMax.Visibility = Visibility.Collapsed;
                    ElementRestore.Visibility = Visibility.Visible;
                    if (Parent is Grid)
                    {
                        HorizontalOffset = 0;
                        VerticalOffset = 0;
                    }

                    RaiseMinimized(this, args);
                    break;
                case DialogSizeState.Maximized:
                    ElementBackground.Effect = _originalEffect;
                    ElementMin.Visibility = Visibility.Visible;
                    //ElementMax.Visibility = Visibility.Collapsed;
                    //ElementRestore.Visibility = Visibility.Visible;
                    RaiseMaximized(this, args);
                    break;
                case DialogSizeState.Restored:
                    ElementBackground.Effect = _originalEffect;
                    ElementTopBar.Visibility = Visibility.Visible;
                    ElementContent.Visibility = Visibility.Visible;
                    ElementButtons.Visibility = ButtonsVisibility;
                    ElementMin.Visibility = Visibility.Visible;
                    HorizontalOffset = _restorePoint.X;
                    VerticalOffset = _restorePoint.Y;
                    RaiseRestored(this, args);
                    break;
                default:
                    break;
            }
            UpdateButtonPositions();
            UpdateVisualState();
        }

        /// <summary>
        /// This event is called periodically as a ticker
        /// </summary>
        protected override void Tick(object sender, EventArgs e)
        {
            base.Tick(sender, e);

            _ticksSinceLastMouseDown++;
            if (_ticksSinceLastMouseDown > 1024)
            {
                _ticksSinceLastMouseDown = 512;
            }
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            Restore();
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            Maximize();
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            Minimize();
        }

        private void ElementTopButtons_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _topButtonsSize = e.NewSize;
            UpdateButtonPositions();
        }

        private void ElementButtons_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _buttonsSize = e.NewSize;
            UpdateButtonPositions();
            UpdateVisualState();
        }

        private void ElementBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p;

            if (_resizable)
            {
                p = e.GetPosition(ElementBackground);

                _borderResizing = false;
                ElementBackground.ReleaseMouseCapture();
                UpdateMouseCursor(p);
                ElementCursor.Hide();
            }
        }

        private void ElementBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_resizable)
            {
                _borderResizing = true;
                ElementBackground.CaptureMouse();
            }
        }

        private void ElementBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_resizable && !_borderResizing)
            {
                this.Cursor = Cursors.Arrow;
                ElementCursor.Hide();
            }
        }

        private void ElementBorder_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(ElementBackground);
            Point originalPosition = new Point(HorizontalOffset, VerticalOffset);

            if (_resizable && _sizeState != DialogSizeState.Minimized && _sizeState != DialogSizeState.Maximized)
            {
                UpdateMouseCursor(p);

                if (_borderResizing)
                {	// Resizing in progress
                    if (_hoverLeftBorder)
                    {
                        if (IsSizeWithinRange(Width + -p.X, Height))
                        {
                            HorizontalOffset += p.X;
                            Width = Width + -p.X;
                            RaiseResized(this, new DialogEventArgs());
                        }
                    }
                    if (_hoverTopBorder)
                    {
                        if (IsSizeWithinRange(Width, Height + -p.Y))
                        {
                            VerticalOffset += p.Y;
                            Height = Height + -p.Y;
                            RaiseResized(this, new DialogEventArgs());
                        }
                    }
                    if (_hoverRightBorder)
                    {
                        if (IsSizeWithinRange(p.X, Height))
                        {
                            Width = p.X;
                        }
                    }
                    if (_hoverBottomBorder)
                    {
                        if (IsSizeWithinRange(Width, p.Y))
                        {
                            Height = p.Y;
                        }
                    }

                    _restoreSize = new Size(Width, Height);
                    UpdateVisualState();
                    UpdateButtonPositions();
                }
            }
        }

        /// <summary>
        /// This event is called when the dialog is closed
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void Cross_Clicked(object sender, RoutedEventArgs e)
        {
            AttemptClose(DialogButtons.Close, "close");
        }

        /// <summary>
        /// This event is called when a button is clicked
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void Button_Clicked(object sender, RoutedEventArgs e)
        {
            object tag = ((Button)sender).Tag;

            try
            {
                Result = (DialogButtons)Enum.Parse(typeof(DialogButtons), tag.ToString(), true);
            }
            catch
            {
                Result = DialogButtons.Custom;
            }

            AttemptClose(Result, tag);
        }

        /// <summary>
        /// This event is called when the mouse button is clicked for the title bar
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected void TitleMouseDown(object sender, MouseEventArgs e)
        {
            DialogEventArgs args = new DialogEventArgs();

            if (IsEnabled && IsDraggable)
            {
                if (DialogTitleMouseDown != null)
                {
                    DialogTitleMouseDown(this, args);
                }

                if (!args.Cancel)
                {
                    if (_ticksSinceLastMouseDown < 10)
                    {   // double-click
                        if ((_sizeState == DialogSizeState.Restored || _sizeState == DialogSizeState.Normal) && _maximizeEnabled)
                        {
                            Maximize();
                        }
                        else if (_sizeState == DialogSizeState.Maximized || _sizeState == DialogSizeState.Minimized)
                        {
                            Restore();
                        }
                    }
                    else
                    {
                        RaiseHasFocus(this, args);

                        if (!args.Cancel)
                        {
                            _dragging = true;
                            _dragStart = e.GetPosition(Parent as UIElement);

                            if (!ShowShadowWhenDragging)
                            {
                                ElementBackground.Effect = null;
                            }
                            ((UIElement)sender).CaptureMouse();

                            BringToFront();
                        }
                    }
                    _ticksSinceLastMouseDown = 0;
                }
            }
        }

        /// <summary>
        /// This event is called when the mouse button is released for the title bar
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected void TitleMouseUp(object sender, MouseEventArgs e)
        {
            if (IsEnabled && _dragging)
            {
                if (DialogTitleMouseDown != null)
                {
                    DialogTitleMouseDown(this, new DialogEventArgs());
                }

                _dragging = false;

                if (!ShowShadowWhenDragging)
                {
                    ElementBackground.Effect = _originalEffect;
                }
                ((UIElement)sender).ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// This event is called when the mouse moves over the button
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected void TitleMouseMove(object sender, MouseEventArgs e)
        {
            Point current;
            Point newPos = new Point();
            Point move = new Point();
            DialogEventArgs args = new DialogEventArgs();

            if (IsEnabled && _dragging)
            {
                if (DialogTitleMouseMove != null)
                {
                    DialogTitleMouseMove(this, args);
                }

                if (!args.Cancel)
                {
                    current = e.GetPosition(Parent as UIElement);

                    move.X = current.X - _dragStart.X;
                    newPos.X = HorizontalOffset + move.X;

                    move.Y = current.Y - _dragStart.Y;
                    newPos.Y = VerticalOffset + move.Y;

                    if (newPos.X < TopLeft.X)
                    {
                        newPos.X = TopLeft.X;
                    }
                    if (newPos.Y < TopLeft.Y)
                    {
                        newPos.Y = TopLeft.Y;
                    }

                    if (newPos.X + Width > BottomRight.X)
                    {
                        newPos.X = BottomRight.X - Width;
                    }
                    if (newPos.Y + Height > BottomRight.Y)
                    {
                        newPos.Y = BottomRight.Y - Height;
                    }

                    if (current.X >= TopLeft.X && current.Y >= TopLeft.Y && current.X <= BottomRight.X && current.Y <= BottomRight.Y)
                    {
                        HorizontalOffset = newPos.X;
                        VerticalOffset = newPos.Y;

                        _dragStart.X = current.X;
                        _dragStart.Y = current.Y;
                    }
                    else
                    {
                        TitleMouseUp(sender, e);
                    }
                }
            }
        }

        /// <summary>
        /// This event is called when the mouse is down over the dialog
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void DialogBase_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BringToFront();
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a Maximizing event when the dialog is going to be maximized
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseMaximizing(object sender, DialogEventArgs args)
        {
            if (Maximizing != null)
            {
                Maximizing(sender, args);
            }
        }

        /// <summary>
        /// Generates a Maximized event when the dialog is maximized
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseMaximized(object sender, DialogEventArgs args)
        {
            if (Maximized != null)
            {
                Maximized(sender, args);
            }
        }

        /// <summary>
        /// Generates a Minimizing event when the dialog is going to be minimized
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseMinimizing(object sender, DialogEventArgs args)
        {
            if (Minimizing != null)
            {
                Minimizing(sender, args);
            }
        }

        /// <summary>
        /// Generates a Minimized event when the dialog is minimized
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseMinimized(object sender, DialogEventArgs args)
        {
            if (Minimized != null)
            {
                Minimized(sender, args);
            }
        }

        /// <summary>
        /// Generates a Restoring event when the dialog is going to be restored
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseRestoring(object sender, DialogEventArgs args)
        {
            if (Restoring != null)
            {
                Restoring(sender, args);
            }
        }

        /// <summary>
        /// Generates a Restored event when the dialog is restored
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseRestored(object sender, DialogEventArgs args)
        {
            if (Restored != null)
            {
                Restored(sender, args);
            }
        }

        /// <summary>
        /// Generates a Apply event when the dialog apply button is clicked
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseApply(object sender, DialogEventArgs args)
        {
            if (Apply != null)
            {
                Apply(sender, args);
            }
        }

        /// <summary>
        /// Generates a Resized event when the dialog is resized
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseResized(object sender, DialogEventArgs args)
        {
            if (Resized != null)
            {
                Resized(sender, args);
            }
        }

        /// <summary>
        /// Generates a HasFocus event when the dialog receives focus (The title bar is clicked)
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseHasFocus(object sender, DialogEventArgs args)
        {
            if (HasFocus != null)
            {
                HasFocus(sender, args);
            }
        }

        #endregion
    }
}
