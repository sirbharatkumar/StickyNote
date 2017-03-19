using System;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Liquid
{
    /// <summary>
    /// A Menu Item control, this is used in the Menu control
    /// </summary>
    public partial class MenuItem : BaseMenuControl
    {
        #region Visual Elements

        /// <summary> 
        /// Text template.
        /// </summary>
        internal TextBlock ElementText { get; set; }
        internal const string ElementTextName = "ElementText";

        /// <summary>
        /// Hover template.
        /// </summary>
        internal Rectangle ElementHover { get; set; }
        internal const string ElementHoverName = "ElementHover";

        /// <summary>
        /// Image template.
        /// </summary>
        internal Image ElementIcon { get; set; }
        internal const string ElementIconName = "ElementIcon";

        /// <summary>
        /// CheckBox template.
        /// </summary>
        internal CheckBox ElementCheckBox { get; set; }
        internal const string ElementCheckBoxName = "ElementCheckBox";

        /// <summary>
        /// Shortcut template.
        /// </summary>
        internal TextBlock ElementShortcut { get; set; }
        internal const string ElementShortcutName = "ElementShortcut";

        /// <summary>
        /// Child indicator template.
        /// </summary>
        internal FrameworkElement ElementChildren { get; set; }
        internal const string ElementChildrenName = "ElementChildren";

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty HoverBorderBrushProperty = DependencyProperty.Register("HoverBorderBrush", typeof(Brush), typeof(MenuItem), null);
        public Brush HoverBorderBrush
        {
            get { return (Brush)this.GetValue(HoverBorderBrushProperty); }
            set { base.SetValue(HoverBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty HoverBorderThicknessProperty = DependencyProperty.Register("HoverBorderThickness", typeof(double), typeof(MenuItem), null);
        public double HoverBorderThickness
        {
            get { return (double)this.GetValue(HoverBorderThicknessProperty); }
            set { base.SetValue(HoverBorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty HoverBrushProperty = DependencyProperty.Register("HoverBrush", typeof(Brush), typeof(MenuItem), null);
        public Brush HoverBrush
        {
            get { return (Brush)this.GetValue(HoverBrushProperty); }
            set { base.SetValue(HoverBrushProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MenuItem), null);
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { base.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty ShortcutProperty = DependencyProperty.Register("Shortcut", typeof(string), typeof(MenuItem), null);
        public string Shortcut
        {
            get { return (string)this.GetValue(ShortcutProperty); }
            set { base.SetValue(ShortcutProperty, value); }
        }

        public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register("ContentMargin", typeof(Thickness), typeof(MenuItem), null);
        public Thickness ContentMargin
        {
            get { return (Thickness)this.GetValue(ContentMarginProperty); }
            set { base.SetValue(ContentMarginProperty, value); }
        }

        public static readonly DependencyProperty CheckBoxVisibilityProperty = DependencyProperty.Register("CheckBoxVisibility", typeof(Visibility), typeof(MenuItem), null);
        public Visibility CheckBoxVisibility
        {
            get { return (Visibility)this.GetValue(CheckBoxVisibilityProperty); }
            set { base.SetValue(CheckBoxVisibilityProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(MenuItem), null);
        public bool IsChecked
        {
            get { return (bool)this.GetValue(IsCheckedProperty); }
            set { base.SetValue(IsCheckedProperty, value); }
        }

        #endregion

        #region Private Properties

        private bool _isHighlighted = false;
        private string _icon = string.Empty;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets whether this item is highlighted
        /// </summary>
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set { _isHighlighted = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets the parent menu
        /// </summary>
        public Menu ParentMenu
        {
            get { return (Menu)Parent; }
        }

        /// <summary>
        /// Gets or sets the icon URL
        /// </summary>
        public string Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;

                if (ElementRoot != null)
                {
                    if (_icon.Length > 0)
                    {
                        ElementIcon.Source = new BitmapImage() { UriSource = new Uri(_icon, UriKind.RelativeOrAbsolute) };
                        ElementIcon.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ElementIcon.Source = null;
                        ElementIcon.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the URL to navigate to when clicked
        /// </summary>
        public Uri URL { get; set; }

        /// <summary>
        /// Gets or sets the width of the icon column
        /// </summary>
        public double IconColumnWidth { get; set; }

        #endregion

        #region Public Events

        public event MenuEventHandler CheckChanged;

        #endregion

        #region Constructor

        public MenuItem()
        {
            this.MouseEnter += new MouseEventHandler(OnMouseEnter);
            this.MouseLeave += new MouseEventHandler(OnMouseLeave);
            this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(MenuItem_IsEnabledChanged);
        }

        public MenuItem(string text)
            : this()
        {
            Text = text;
        }

        public MenuItem(string text, string iconURL)
            : this(text)
        {
            _icon = iconURL;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Closes the top menu
        /// </summary>
        protected void CollapseUp()
        {
            Menu top = ParentMenu.GetTop();

            if (top != null)
            {
                top.CloseChild();
            }
        }

        /// <summary>
        /// Updates the visual state of the control
        /// </summary>
        protected override void UpdateVisualState()
        {
            if (ElementRoot != null)
            {
                ElementHover.Visibility = (_isHighlighted ? Visibility.Visible : Visibility.Collapsed);
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

            ElementText = (TextBlock)GetTemplateChild(ElementTextName);
            ElementHover = (Rectangle)GetTemplateChild(ElementHoverName);
            ElementIcon = (Image)GetTemplateChild(ElementIconName);
            ElementCheckBox = (CheckBox)GetTemplateChild(ElementCheckBoxName);
            ElementShortcut = (TextBlock)GetTemplateChild(ElementShortcutName);
            ElementChildren = (FrameworkElement)GetTemplateChild(ElementChildrenName);

            this.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);

            if (ElementCheckBox != null)
            {
                ElementCheckBox.Click += new RoutedEventHandler(ElementCheckBox_Click);
            }

            if (ElementChildren != null)
            {
                ElementChildren.Visibility = (Content != null ? Visibility.Visible : Visibility.Collapsed);
            }
            if (ElementIcon != null)
            {
                if (IconColumnWidth != 0)
                {
                    ((Grid)ElementRoot).ColumnDefinitions[0].Width = new GridLength(IconColumnWidth);
                }
                Icon = _icon;
            }
        }

        private void ElementCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (CheckChanged != null)
            {
                CheckChanged(this, new MenuEventArgs());
            }
        }

        private void MenuItem_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.Opacity = (IsEnabled ? 1.0 : BaseMenuControl.DisabledOpacity);
        }

        protected virtual void OnMouseLeave(object sender, MouseEventArgs e)
        {
            ParentMenu.SetHilightChild(null);
        }

        protected virtual void OnMouseEnter(object sender, MouseEventArgs e)
        {
            ParentMenu.SetHilightChild(this);
            ParentMenu.OpenChild(this, false);
        }

        protected virtual void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuEventArgs args = new MenuEventArgs();
            Menu top;

            if (IsEnabled)
            {
                if (Content != null)
                {
                }
                else
                {
                    top = ParentMenu.GetTop();
                    args.Tag = this.ID;
                    args.Parameter = this;

                    if (top != null)
                    {
                        top.RaiseItemSelected(this, args);
                    }
                    if (!args.Cancel)
                    {
                        top.CloseChild();
                        top.ShowChild = false;
                        if (URL != null)
                        {
                            if (URL.OriginalString.Length > 0)
                            {
                                HtmlPage.Window.Navigate(URL);
                            }
                        }
                    }

                    e.Handled = true;
                }
            }
        }

        #endregion
    }
}
