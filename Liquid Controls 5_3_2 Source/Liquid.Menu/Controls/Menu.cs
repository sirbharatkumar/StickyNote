using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Liquid
{
    /// <summary>
    /// A popup Menu control
    /// </summary>
    public partial class Menu : ItemsControl
    {
        #region Visual Elements

        /// <summary>
        /// Root template.
        /// </summary>
        internal Canvas ElementChildren { get; set; }
        internal const string ElementChildrenName = "ElementChildren";

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty LeftBrushProperty = DependencyProperty.Register("LeftBrush", typeof(Brush), typeof(Menu), null);
        public Brush LeftBrush
        {
            get { return (Brush)this.GetValue(LeftBrushProperty); }
            set { base.SetValue(LeftBrushProperty, value); }
        }

        public static readonly DependencyProperty LeftBorderBrushProperty = DependencyProperty.Register("LeftBorderBrush", typeof(Brush), typeof(Menu), null);
        public Brush LeftBorderBrush
        {
            get { return (Brush)this.GetValue(LeftBorderBrushProperty); }
            set { base.SetValue(LeftBorderBrushProperty, (DependencyObject)value); }
        }

        public static readonly DependencyProperty LeftBorderThicknessProperty = DependencyProperty.Register("LeftBorderThickness", typeof(double), typeof(Menu), null);
        public double LeftBorderThickness
        {
            get { return (double)this.GetValue(LeftBorderThicknessProperty); }
            set { base.SetValue(LeftBorderThicknessProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether this menu should be hidden when it loses focus
        /// </summary>
        public static readonly DependencyProperty CloseWhenOutOfFocusProperty = DependencyProperty.Register("CloseWhenOutOfFocus", typeof(bool), typeof(Menu), null);
        public bool CloseWhenOutOfFocus
        {
            get { return (bool)this.GetValue(CloseWhenOutOfFocusProperty); }
            set { base.SetValue(CloseWhenOutOfFocusProperty, value); }
        }

        #endregion

        #region Internal Properties

        internal bool ShowChild = false;

        #endregion

        #region Private Properties

        private Menu _current = null;
        private MenuItem _hilighted = null;

        #endregion

        #region Public Events

        public event MenuEventHandler ItemSelected;
        public event MenuEventHandler Closing;
        public event MenuEventHandler Closed;

        #endregion

        #region Public Properties

        #endregion

        #region Constructor

        public Menu()
        {
            DefaultStyleKey = this.GetType();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the top most Menu object in the menu structure
        /// </summary>
        /// <returns>A MenuList object</returns>
        internal Menu GetTop()
        {
            DependencyObject element = (FrameworkElement)this.Parent;
            Menu top = this;

            while (element != null && (element is Menu || element is MenuItem))
            {
                if (element is Menu)
                {
                    top = (Menu)element;
                }
                element = ((FrameworkElement)element).Parent;
            }

            return top;
        }

        /// <summary>
        /// Opens a child menu item
        /// </summary>
        /// <param name="item">The item to open</param>
        /// <param name="openUnderneath">Indicates whether to display below or to the right</param>
        internal void OpenChild(MenuItem item, bool openUnderneath)
        {
            Menu content = (Menu)item.Content;
            Point p;

            if (_current != null)
            {
                _current.Hide();
                _current = null;
            }

            if (content != null)
            {
                p = item.TransformToVisual(ElementChildren).Transform((openUnderneath ? new Point(0, item.RenderSize.Height - 1) : new Point(item.RenderSize.Width - 1, 0)));
                content.SetValue(Canvas.LeftProperty, p.X);
                content.SetValue(Canvas.TopProperty, p.Y);

                if (!ElementChildren.Children.Contains(content))
                {
                    ElementChildren.Children.Add(content);
                }
                content.ClearSelection();
                content.CloseChild();
                content.Show();
                _current = content;
            }
        }

        /// <summary>
        /// Closes the currently open child menu
        /// </summary>
        internal void CloseChild()
        {
            if (_current != null)
            {
                _current.Hide();
            }
        }

        /// <summary>
        /// Clears the currently hilighted item and sets the provided item
        /// </summary>
        /// <param name="item">New menu item to hilight</param>
        internal void SetHilightChild(MenuItem item)
        {
            ClearSelection();
            
            if (item != null)
            {
                _hilighted = item;
                _hilighted.IsHighlighted = true;
            }

            EnsureParentHilighted();
        }

        internal void ClearSelection()
        {
            if (_hilighted != null)
            {
                _hilighted.IsHighlighted = false;
                _hilighted = null;
            }
        }

        internal void EnsureParentHilighted()
        {
            DependencyObject element = (FrameworkElement)this.Parent;

            while (element is MainMenuItem)
            {
                ((MainMenuItem)element).ParentMenu.SetHilightChild((MainMenuItem)element);
                element = ((FrameworkElement)element).Parent;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows the menu
        /// </summary>
        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the menu
        /// </summary>
        public void Hide()
        {
            ClearSelection();
            this.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Gets the menu item with the provided ID
        /// </summary>
        /// <param name="ID">Menu ID to look for</param>
        /// <returns>MenuItem with the matching ID or null if not found</returns>
        public BaseMenuControl Get(string ID)
        {
            BaseMenuControl result = null;
            MenuItem item;

            foreach (BaseMenuControl e in Items)
            {
                if (e.ID == ID)
                {
                    result = e;
                    break;
                }

                if (e is MenuItem)
                {
                    item = (MenuItem)e;
                    if (item.Content is Menu)
                    {
                        result = ((Menu)item.Content).Get(ID);
                        if (result != null)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the enabled state of a menu item with the provided ID
        /// </summary>
        /// <param name="ID">Menu item ID</param>
        /// <param name="enabledState">Enabled state</param>
        public void SetEnabledStatus(string ID, bool enabledState)
        {
            BaseMenuControl item = Get(ID);

            if (item != null)
            {
                item.IsEnabled = enabledState;
            }
        }

        /// <summary>
        /// Gets the number of visible child items
        /// </summary>
        /// <returns>Number of items</returns>
        public int GetNumberOfVisibleItems()
        {
            int result = 0;

            foreach (BaseMenuControl e in Items)
            {
                if (e.Visibility == Visibility.Visible)
                {
                    result++;
                }
            }

            return result;
        }

        #endregion

        #region Private Methods

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            MenuEventArgs args = new MenuEventArgs();

            if (FocusManager.GetFocusedElement() is CheckBox)
            {
                return;
            }

            if (Closing != null)
            {
                Closing(this, args);
            }

            if (!args.Cancel)
            {
                ShowChild = false;
                CloseChild();

                if (CloseWhenOutOfFocus)
                {
                    Hide();
                }
            }

            if (Closed != null)
            {
                Closed(this, args);
            }

            base.OnLostFocus(e);
        }

        private void UpdateVisualState()
        {
            if (ElementChildren != null)
            {
                this.Opacity = (IsEnabled ? 1.0 : BaseMenuControl.DisabledOpacity);
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

            ElementChildren = (Canvas)GetTemplateChild(ElementChildrenName);

            this.MouseLeave += new System.Windows.Input.MouseEventHandler(Menu_MouseLeave);

            UpdateVisualState();
        }

        private void Menu_MouseLeave(object sender, MouseEventArgs e)
        {
            ClearSelection();
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a ItemSelected event to indicate an item has been clicked
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        internal void RaiseItemSelected(object sender, MenuEventArgs args)
        {
            if (ItemSelected != null)
            {
                ItemSelected(this, args);
            }
        }

        #endregion
    }
}
