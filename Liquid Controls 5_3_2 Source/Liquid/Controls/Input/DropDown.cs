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
    public enum DropDownPosition
    {
        Left,
        Right
    };

    public partial class DropDown : LiquidControl
    {
        #region Visual Elements

        /// <summary> 
        /// Container template.
        /// </summary>
        internal Button ElementButton { get; set; }
        internal const string ElementButtonName = "ElementButton";

        /// <summary> 
        /// Popup template.
        /// </summary>
        internal Popup ElementPopup { get; set; }
        internal const string ElementPopupName = "ElementPopup";

        /// <summary> 
        /// Layout Grid template.
        /// </summary>
        internal Grid ElementLayoutGrid { get; set; }
        internal const string ElementLayoutGridName = "ElementLayoutGrid";

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the visibility of the vertical scrollbar
        /// </summary>
        public static readonly DependencyProperty SelectedItemContentProperty = DependencyProperty.Register("SelectedItemContent", typeof(object), typeof(DropDown), null);
        public object SelectedItemContent
        {
            get { return this.GetValue(SelectedItemContentProperty); }
            set { base.SetValue(SelectedItemContentProperty, value); }
        }

        #endregion

        #region Private Properties

        private bool _isOpen = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the open status of the Drop down
        /// </summary>
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                _isOpen = value;
                if (ElementPopup != null)
                {
                    ElementPopup.IsOpen = value;
                    UpdateVisualState();
                }
            }
        }

        /// <summary>
        /// Gets or sets the opening position
        /// </summary>
        public DropDownPosition OpenPosition { get; set; }

        #endregion

        #region Public Events

        public event LiquidEventHandler Opened;

        #endregion

        #region Constructor

        public DropDown()
        {
            OpenPosition = DropDownPosition.Left;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the visual state of the control
        /// </summary>
        protected override void UpdateVisualState()
        {
            if (ElementRoot != null && RenderSize.Width >= 6)
            {
                ElementLayoutGrid.Width = RenderSize.Width - 6;
                ElementLayoutGrid.Height = RenderSize.Height - 6;
                ElementPopup.Margin = new Thickness(0, RenderSize.Height, 0, 0);

                if (OpenPosition == DropDownPosition.Right && Content is FrameworkElement)
                {
                    ElementPopup.Margin = new Thickness(-((FrameworkElement)Content).Width+RenderSize.Width, RenderSize.Height, 0, 0);
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

            ElementButton = (Button)GetTemplateChild(ElementButtonName);
            ElementPopup = (Popup)GetTemplateChild(ElementPopupName);
            ElementLayoutGrid = (Grid)GetTemplateChild(ElementLayoutGridName);

            ElementButton.Click += new RoutedEventHandler(ElementButton_Click);
            SizeChanged += new SizeChangedEventHandler(OnSizeChanged);

            UpdateVisualState();
            IsOpen = _isOpen;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualState();
        }

        protected void ElementButton_Click(object sender, RoutedEventArgs e)
        {
            LiquidEventArgs args = new LiquidEventArgs();

            if (!IsOpen)
            {
                if (Opened != null)
                {
                    Opened(this, args);
                }
            }

            if (!args.Cancel)
            {
                IsOpen ^= true;

                if (Content is Control)
                {
                    ((Control)Content).Focus();
                }
            }
        }

        #endregion
    }
}
