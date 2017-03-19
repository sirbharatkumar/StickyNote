using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Liquid
{
    #region Enums

    public enum DialogStartPosition
    {
        Manual,
        Default,
        CenterParent
    }

    #endregion

    #region Delegates

    public delegate void DialogEventHandler(object sender, DialogEventArgs e);

    #endregion

    /// <summary>
    /// The base class for all popup controls
    /// </summary>
    public abstract partial class PopupBase : ContentControl
    {
        #region Visual Elements

        /// <summary> 
        /// Root element template.
        /// </summary>
        public FrameworkElement ElementRoot { get; set; }
        internal const string ElementRootName = "RootElement";

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

        #endregion

        #region Private Properties

        private DispatcherTimer _closeTimer = new DispatcherTimer();
        private DispatcherTimer _timer = new DispatcherTimer();
        private bool _isOpen = false;
        private Size _maxSize = new Size();
        private object _defaultFocus = null;
        private bool _autoFocusOnOpen = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the opacity for certain control elements when disabled
        /// </summary>
        public double DisabledOpacity { get; set; }

        /// <summary>
        /// Gets or sets the horizontal offset of the dialog
        /// </summary>
        public double HorizontalOffset
        {
            get
            {
                return Parent is Canvas ? (double)GetValue(Canvas.LeftProperty) : Margin.Left;
            }
            set
            {
                if (Parent is Canvas)
                {
                    SetValue(Canvas.LeftProperty, Math.Round(value));
                }
                else
                {
                    Margin = new Thickness(Math.Round(value), Margin.Top, Margin.Right, Margin.Bottom);
                }
            }
        }

        /// <summary>
        /// Gets or sets the vertical offset of the dialog
        /// </summary>
        public double VerticalOffset
        {
            get
            {
                return Parent is Canvas ? (double)GetValue(Canvas.TopProperty) : Margin.Top;
            }
            set
            {
                if (Parent is Canvas)
                {
                    SetValue(Canvas.TopProperty, Math.Round(value));
                }
                else
                {
                    Margin = new Thickness(Margin.Left, Math.Round(value), Margin.Right, Margin.Bottom);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the dialog is opened
        /// </summary>
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (value)
                {
                    this.Visibility = Visibility.Visible;
                    this.IsEnabled = true;

                    if (DefaultFocus != null)
                    {
                        _autoFocusOnOpen = true;
                    }

                    if (ElementRoot != null)
                    {
                        SetOpeningPosition();
                        ElementFadeOut.Stop();
                        ElementFadeIn.Begin();

                        RaiseOpened(this, new DialogEventArgs());
                    }

                    if (IsTimerEnabled)
                    {
                        _closeTimer.Interval = TimeUntilClose;
                        _closeTimer.Start();
                    }
                }
                else
                {
                    _autoFocusOnOpen = false;
                    if (ElementRoot != null)
                    {
                        ElementFadeIn.Stop();
                        ElementFadeOut.Begin();
                    }
                    else
                    {
                        this.IsEnabled = false;
                    }

                    if (_closeTimer.IsEnabled)
                    {
                        _closeTimer.Stop();
                    }
                }

                _isOpen = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the dialog will close after a specified time
        /// </summary>
        public bool IsTimerEnabled { get; set; }

        /// <summary>
        /// Gets or sets the time until the dialog should automatically close
        /// </summary>
        public TimeSpan TimeUntilClose { get; set; }

        /// <summary>
        /// Gets or sets the minimum size of the popup when its resizable
        /// </summary>
        public Size MinSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum size when its resizble
        /// </summary>
        public Size MaxSize
        {
            get
            {
                FrameworkElement parent;

                if (_maxSize.Width == 0 && _maxSize.Height == 0 && Parent is FrameworkElement)
                {
                    parent = (FrameworkElement)Parent;

                    if (parent is Canvas)
                    {
                        return new Size(parent.ActualWidth, parent.ActualHeight);
                    }
                    else
                    {
                        return parent.RenderSize;
                    }
                }
                else return _maxSize;
            }
            set
            {
                _maxSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the start position of the dialog
        /// </summary>
        public DialogStartPosition StartPosition { get; set; }

        /// <summary>
        /// Gets or sets the default control to give focus to
        /// </summary>
        public object DefaultFocus
        {
            get { return _defaultFocus; }
            set { _defaultFocus = value; _autoFocusOnOpen = true; }
        }

        #endregion

        #region Public Events

        public event DialogEventHandler Closed;
        public event DialogEventHandler Opened;
        public event DialogEventHandler CloseCompleted;

        #endregion

        #region Constructor

        public PopupBase()
        {
            DefaultStyleKey = this.GetType();
            Visibility = Visibility.Collapsed;
            StartPosition = DialogStartPosition.Manual;
            DisabledOpacity = 0.7;

            _closeTimer.Tick += new EventHandler(TimerEnd);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method displays the dialog
        /// </summary>
        public virtual void Show()
        {
            IsOpen = true;
        }

        /// <summary>
        /// This method closes the dialog
        /// </summary>
        public virtual void Close()
        {
            IsOpen = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Positions the popup at the correct startup position
        /// </summary>
        protected void SetOpeningPosition()
        {
            Size size = MaxSize;

            switch (StartPosition)
            {
                case DialogStartPosition.CenterParent:
                    HorizontalOffset = (size.Width - Width) * 0.5;
                    VerticalOffset = (size.Height - Height) * 0.5;
                    if (HorizontalOffset < 0)
                    {
                        HorizontalOffset = 0;
                    }
                    if (VerticalOffset < 0)
                    {
                        VerticalOffset = 0;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Is called when the visuals need to be updated
        /// </summary>
        protected virtual void UpdateVisualState()
        {
            if (ElementRoot != null)
            {
                ElementRoot.Width = Width;
                ElementRoot.Height = Height;
            }
        }

        /// <summary>
        /// This is called when the dialog has closed
        /// </summary>
        protected virtual void FadedOut()
        {
            this.IsEnabled = false;
            this.Visibility = Visibility.Collapsed;
            RaiseCloseCompleted(this, null);
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// This is called when the template has been bound to the control
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ElementRoot = (FrameworkElement)GetTemplateChild(ElementRootName);
            ElementFadeIn = (Storyboard)GetTemplateChild(ElementFadeInName);
            ElementFadeOut = (Storyboard)GetTemplateChild(ElementFadeOutName);

            ElementFadeOut.Completed += new EventHandler(FadeOutStoryboard_Completed);

            if (IsOpen)
            {
                IsOpen = _isOpen;
            }

            _timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            _timer.Tick += new EventHandler(Tick);
            _timer.Start();
        }

        /// <summary>
        /// This event is called when the the fade out animation ends
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void FadeOutStoryboard_Completed(object sender, EventArgs e)
        {
            FadedOut();
        }

        /// <summary>
        /// This event is called when the dialog should close
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected void TimerEnd(object sender, EventArgs e)
        {
            DialogEventArgs args = new DialogEventArgs();

            RaiseClosed(this, args);

            if (!args.Cancel)
            {
                Close();
            }
        }

        /// <summary>
        /// This event is called periodically
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected virtual void Tick(object sender, EventArgs e)
        {
            if (DefaultFocus != null && _autoFocusOnOpen)
            {
                if (DefaultFocus is Control)
                {
                    if (((Control)DefaultFocus).Focus())
                    {
                        _autoFocusOnOpen = false;
                        if (DefaultFocus is TextBox)
                        {
                            ((TextBox)DefaultFocus).SelectAll();
                        }
                    }
                }
            }
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a Closed event when the dialog is closed
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected void RaiseClosed(object sender, DialogEventArgs args)
        {
            if (Closed != null)
            {
                Closed(sender, args);
            }
        }

        /// <summary>
        /// Generates a CloseCompleted event when the dialog has finished closing
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected void RaiseCloseCompleted(object sender, DialogEventArgs args)
        {
            if (CloseCompleted != null)
            {
                CloseCompleted(sender, args);
            }
        }

        /// <summary>
        /// Generates a Opened event when the dialog is opened
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected void RaiseOpened(object sender, DialogEventArgs args)
        {
            if (Opened != null)
            {
                Opened(sender, args);
            }
        }

        #endregion
    }

    #region DialogEventArgs
    /// <summary>
    /// Event arguments for use with dialog related events
    /// </summary>
    public partial class DialogEventArgs : EventArgs
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets a generic tag for the event
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets or sets a generic parameter
        /// </summary>
        public object Parameter { get; set; }

        /// <summary>
        /// Gets or sets whether to cancel an operation
        /// </summary>
        public bool Cancel { get; set; }

        #endregion

        #region Constructor

        public DialogEventArgs()
        {
            Tag = string.Empty;
        }

        #endregion
    }

    #endregion
}
