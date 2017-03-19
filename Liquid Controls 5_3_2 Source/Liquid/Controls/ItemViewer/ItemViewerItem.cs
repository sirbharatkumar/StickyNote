using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Liquid
{
    /// <summary>
    /// An Item used by the ItemViewer control
    /// </summary>
    public partial class ItemViewerItem : LiquidTextControl
    {
        #region Visual Elements

        /// <summary> 
        /// Icon template.
        /// </summary>
        internal Image ElementIcon { get; set; }
        internal const string ElementIconName = "ElementIcon";

        /// <summary> 
        /// Input template.
        /// </summary>
        internal TextBox ElementInput { get; set; }
        internal const string ElementInputName = "ElementInput";

        /// <summary> 
        /// Other text template.
        /// </summary>
        internal TextBlock ElementOtherText { get; set; }
        internal const string ElementOtherTextName = "ElementOtherText";

        /// <summary> 
        /// Hover template.
        /// </summary>
        internal Rectangle ElementHover { get; set; }
        internal const string ElementHoverName = "ElementHover";

        /// <summary> 
        /// Selected template.
        /// </summary>
        internal Rectangle ElementSelected { get; set; }
        internal const string ElementSelectedName = "ElementSelected";

        #endregion

        #region Private Properties

        private string _text = string.Empty;
        private string _otherText = string.Empty;
        private string _icon = string.Empty;
        private BitmapImage _iconSource = null;
        private bool _editable = false;
        private bool _hovering = false;
        private bool _selected = false;
        private BitmapCreateOptions _createOptions = BitmapCreateOptions.None;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the text
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets or sets the other text value
        /// </summary>
        public string OtherText
        {
            get { return _otherText; }
            set { _otherText = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets or sets whether the text can be edited
        /// </summary>
        public bool IsEditable
        {
            get { return _editable; }
            set
            {
                ItemViewerEventArgs args = new ItemViewerEventArgs();

                if (_editable)
                {
                    args.Title = _text;
                    args.NewTitle = ElementInput.Text;

                    RaiseEditingFinished(this, args);
                    if (!args.Cancel)
                    {
                        _text = args.NewTitle;
                    }
                }

                _editable = value;

                UpdateVisualState();
                if (value && ElementRoot != null)
                {
                    ElementInput.Focus();
                    ElementInput.SelectAll();
                }
            }
        }

        /// <summary>
        /// Gets or sets the icon URL
        /// </summary>
        public string Icon
        {
            get { return _icon; }
            set { _icon = value; UpdateIcon(); }
        }

        /// <summary>
        /// Gets or sets the image source for the icon
        /// </summary>
        public BitmapImage IconSource
        {
            get { return _iconSource; }
            set { _iconSource = value; UpdateIcon(); }
        }

        /// <summary>
        /// Gets or sets whether this item is selected
        /// </summary>
        public bool IsSelected
        {
            get { return _selected; }
            set { _selected = value; UpdateVisualState(); }
        }

        #endregion

        #region Public Events

        public event ItemViewerEventHandler ItemSelected;
        public event ItemViewerEventHandler EditingFinished;
        public event EventHandler IconLoadFail;

        #endregion

        #region Constructor

        public ItemViewerItem()
        {
            IsTabStop = true;
            TabNavigation = KeyboardNavigationMode.Once;

            this.MouseEnter += new MouseEventHandler(ItemViewerItem_MouseEnter);
            this.MouseLeave += new MouseEventHandler(ItemViewerItem_MouseLeave);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(ItemViewerItem_MouseLeftButtonDown);
        }

        #endregion

        #region Public Methods

        public void SetImageCreateOptions(BitmapCreateOptions createOptions)
        {
            if (ElementIcon != null && ElementIcon.Source != null)
            {
                _createOptions = createOptions;
                ((BitmapImage)ElementIcon.Source).CreateOptions = createOptions;
            }
            else
            {
                _createOptions = createOptions;
            }
        }

        #endregion

        #region Private Methods

        private void UpdateIcon()
        {
            if (ElementRoot != null)
            {
                if (_icon.Length > 0)
                {
                    BitmapImage image = new BitmapImage();
                    image.CreateOptions = _createOptions;
                    image.UriSource = new Uri(_icon, UriKind.RelativeOrAbsolute);

                    ElementIcon.Source = image;
                }
                else if (_iconSource != null)
                {
                    ElementIcon.Source = _iconSource;
                }
            }

            UpdateVisualState();
        }

        /// <summary>
        /// Updates the visual state of the control
        /// </summary>
        protected override void UpdateVisualState()
        {
            if (ElementText != null)
            {
                ElementText.Text = _text;
                ElementText.Visibility = (_editable ? Visibility.Collapsed : Visibility.Visible);
            }
            if (ElementOtherText != null)
            {
                ElementOtherText.Text = _otherText;
            }
            if (ElementInput != null)
            {
                ElementInput.Text = _text;
                ElementInput.Visibility = (_editable ? Visibility.Visible : Visibility.Collapsed);
            }
            if (ElementRoot != null)
            {
                ElementHover.Visibility = (_hovering ? Visibility.Visible : Visibility.Collapsed);
                ElementSelected.Visibility = (_selected ? Visibility.Visible : Visibility.Collapsed);
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

            ElementIcon = (Image)GetTemplateChild(ElementIconName);
            ElementInput = (TextBox)GetTemplateChild(ElementInputName);
            ElementOtherText = (TextBlock)GetTemplateChild(ElementOtherTextName);
            ElementHover = (Rectangle)GetTemplateChild(ElementHoverName);
            ElementSelected = (Rectangle)GetTemplateChild(ElementSelectedName);

            if (ElementIcon != null)
            {
                ElementIcon.ImageFailed += new EventHandler<ExceptionRoutedEventArgs>(ElementIcon_ImageFailed);
            }

            if (ElementInput != null)
            {
                ElementInput.KeyDown += new KeyEventHandler(ElementInput_KeyDown);
            }

            UpdateIcon();
            SetImageCreateOptions(_createOptions);
        }

        private void ElementIcon_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (IconLoadFail != null)
            {
                IconLoadFail(this, EventArgs.Empty);
            }
        }

        private void ElementInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IsEditable = false;
            }
        }

        protected void ItemViewerItem_MouseEnter(object sender, MouseEventArgs e)
        {
            _hovering = true;
            UpdateVisualState();
        }

        protected void ItemViewerItem_MouseLeave(object sender, MouseEventArgs e)
        {
            _hovering = false;
            UpdateVisualState();
        }

        protected void ItemViewerItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ItemViewerEventArgs args = new ItemViewerEventArgs() { Title = _text };

            _selected = true;
            _hovering = false;
            UpdateVisualState();

            RaiseItemSelected(this, args);
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a EditingFinished event to indicate the item has finished editing
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected virtual void RaiseEditingFinished(object sender, ItemViewerEventArgs args)
        {
            if (EditingFinished != null)
            {
                EditingFinished(this, args);
            }
        }

        /// <summary>
        /// Generates a ItemSelected event to indicate the item has been clicked
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected virtual void RaiseItemSelected(object sender, ItemViewerEventArgs args)
        {
            if (ItemSelected != null)
            {
                ItemSelected(this, args);
            }
        }

        #endregion
    }
}
