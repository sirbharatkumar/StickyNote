using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Liquid
{
    #region Delegates

    public delegate void ItemViewerEventHandler(object sender, ItemViewerEventArgs e);

    #endregion

    /// <summary>
    /// This control displays a list of items flowing from left-to-right
    /// </summary>
    public partial class ItemViewer : LiquidTimerControl
    {
        #region Visual Elements

        /// <summary> 
        /// Container template.
        /// </summary>
        internal ScrollViewer ElementContainer { get; set; }
        internal const string ElementContainerName = "ElementContainer";

        /// <summary> 
        /// Children template.
        /// </summary>
        internal FlowPanel ElementChildren { get; set; }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the visibility of the vertical scrollbar
        /// </summary>
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(ItemViewer), null);
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)this.GetValue(VerticalScrollBarVisibilityProperty); }
            set { base.SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Gets or sets the visibility of the vertical scrollbar
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(ItemViewer), null);
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)this.GetValue(HorizontalScrollBarVisibilityProperty); }
            set { base.SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        #endregion

        #region Private Properties

        private double _wrapWidth = 180;
        private ItemViewerItem _selected = null;
        private int _ticksSinceLastClick = -1;

        #endregion

        #region Public Properties

        public UIElementCollection Items
        {
            get { return ElementChildren.Children; }
        }

        /// <summary>
        /// Gets or sets the selected item
        /// </summary>
        public ItemViewerItem Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != null)
                {
                    _selected.IsSelected = false;
                }
                _selected = value;
                if (_selected != null)
                {
                    _selected.IsSelected = true;
                }
            }
        }

        public bool DisableUpdates
        {
            get { return ElementChildren.DisableUpdates; }
            set { ElementChildren.DisableUpdates = value; }
        }

        public bool UpdateOnLoad { get; set; }

        #endregion

        #region Public Events

        public event ItemViewerEventHandler ItemSelected;
        public event ItemViewerEventHandler DoubleClick;
        public event ItemViewerEventHandler SlowDoubleClick;
        public event ItemViewerEventHandler EditingFinished;

        #endregion

        #region Constructor

        public ItemViewer()
        {
            ElementChildren = new FlowPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            IsTabStop = true;
            TabNavigation = KeyboardNavigationMode.Once;
            UpdateOnLoad = true;

            this.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a single ItemViewerItem object
        /// </summary>
        /// <param name="item">Item object</param>
        public void Add(ItemViewerItem item)
        {
            SetChildEvents(item);

            ElementChildren.Add(item);
        }

        /// <summary>
        /// Adds a collection of ItemViewerItem objects to the Items collection
        /// </summary>
        /// <param name="items"></param>
        public void Add(IEnumerable<ItemViewerItem> items)
        {
            List<UIElement> temp = new List<UIElement>();

            foreach (ItemViewerItem item in items)
            {
                SetChildEvents(item);
                Items.Add(item);
                temp.Add(item);
            }
        }

        /// <summary>
        /// Selects an item
        /// </summary>
        /// <param name="name">Item text</param>
        public void SetSelected(string name)
        {
            foreach (ItemViewerItem item in Items)
            {
                if (item.Text == name)
                {
                    Selected = item;
                    RaiseItemSelected(this, new ItemViewerEventArgs());
                    break;
                }
            }
        }

        /// <summary>
        /// Clears all the child items
        /// </summary>
        public void Clear()
        {
            if (ElementChildren != null)
            {
                ElementChildren.Children.Clear();
                ElementChildren.Update();
            }
            Items.Clear();
        }

        private void SetChildEvents(ItemViewerItem item)
        {
            item.Loaded -= new RoutedEventHandler(Element_Loaded);
            item.ItemSelected -= new ItemViewerEventHandler(ItemViewer_ItemSelected);
            item.EditingFinished -= new ItemViewerEventHandler(RaiseEditingFinished);

            item.Loaded += new RoutedEventHandler(Element_Loaded);
            item.ItemSelected += new ItemViewerEventHandler(ItemViewer_ItemSelected);
            item.EditingFinished += new ItemViewerEventHandler(RaiseEditingFinished);
        }

        /// <summary>
        /// Updates the size of the control
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualState();
        }

        /// <summary>
        /// Updates the visual state of the control
        /// </summary>
        protected override void UpdateVisualState()
        {
            if (ElementContainer != null)
            {
                ElementContainer.Width = ActualWidth;

                if (VerticalAlignment != VerticalAlignment.Stretch)
                {
                    ElementChildren.Height = ActualHeight;
                    ElementChildren.Width = ElementContainer.Width;
                }
                else
                {
                    if (ElementContainer.Width - 24 >= 1)
                    {
                        ElementChildren.Width = ElementContainer.Width - 24;
                    }
                    else
                    {
                        ElementChildren.Width = 1;
                    }
                }
                _wrapWidth = ElementChildren.Width;
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

            ElementContainer = (ScrollViewer)GetTemplateChild(ElementContainerName);
            ElementContainer.Content = ElementChildren;

            ElementChildren.Updated += new EventHandler(ElementChildren_Updated);

            UpdateVisualState();
            ElementChildren.Update();

            this.MouseWheel += new MouseWheelEventHandler(OnMouseWheel);

            foreach (ItemViewerItem item in ElementChildren.Children)
            {
                SetChildEvents(item);
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ElementContainer != null)
            {
                ElementContainer.ScrollToVerticalOffset(ElementContainer.VerticalOffset + -e.Delta);
                e.Handled = true;
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (Parent is DropDown)
            {
                ((DropDown)Parent).IsOpen = false;
            }
            base.OnLostFocus(e);
        }

        private void ElementChildren_Updated(object sender, EventArgs e)
        {
            double viewportWidth;
            double widthWithScroll;

            if (ElementContainer != null)
            {
                if (VerticalAlignment != VerticalAlignment.Stretch)
                {
                    ElementContainer.Height = ElementChildren.Height + 4;
                }
                else
                {
                    viewportWidth = RenderSize.Width - (ElementContainer.BorderThickness.Left + ElementContainer.BorderThickness.Right + ElementContainer.Padding.Left + ElementContainer.Padding.Right);
                    widthWithScroll = viewportWidth - 17;

                    if (ElementChildren.Height > ElementContainer.ViewportHeight && _wrapWidth > widthWithScroll)
                    {
                        _wrapWidth = (RenderSize.Width >= 74 ? widthWithScroll : 50);
                        ElementChildren.Width = _wrapWidth;
                    }
                    else if (ElementChildren.Height < ElementContainer.ViewportHeight && _wrapWidth < viewportWidth)
                    {
                        _wrapWidth = viewportWidth;
                        ElementChildren.Width = _wrapWidth;
                    }
                }
            }
        }

        /// <summary>
        /// This event handler is called when an element is loaded
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void Element_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is ColorItem))
            {
                ElementChildren.Update();
            }
        }
        
        /// <summary>
        /// This event handler is called when an item is selected
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void ItemViewer_ItemSelected(object sender, ItemViewerEventArgs e)
        {
            if (Selected != null && Selected != sender)
            {
                _ticksSinceLastClick = 0;
                if (Selected.IsEditable)
                {
                    Selected.IsEditable = false;
                }
            }

            Selected = (ItemViewerItem)sender;

            if (_ticksSinceLastClick > 0 && _ticksSinceLastClick < 15 && sender == Selected)
            {	// Double-click
                RaiseDoubleClick(this, e);
            }
            else if (_ticksSinceLastClick > 0 && _ticksSinceLastClick < 40 && sender == Selected)
            {	// Slow double-click
                RaiseSlowDoubleClick(this, e);
            }
            else
            {
                RaiseItemSelected(this, e);
            }

            _ticksSinceLastClick = 0;
        }

        /// <summary>
        /// This is called frequently by a timer to keep tabs on double-clicks
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected override void Tick(object sender, EventArgs e)
        {
            if (_ticksSinceLastClick >= 0)
            {
                _ticksSinceLastClick++;
                if (_ticksSinceLastClick > 1024)
                {
                    _ticksSinceLastClick = 512;
                }
            }

            base.Tick(sender, e);
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
        /// Generates a SlowDoubleClick event to indicate the item has been slow double-clicked
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected virtual void RaiseSlowDoubleClick(object sender, ItemViewerEventArgs args)
        {
            if (SlowDoubleClick != null)
            {
                SlowDoubleClick(this, args);
            }
        }

        /// <summary>
        /// Generates a DoubleClick event to indicate the item has been double-clicked
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected virtual void RaiseDoubleClick(object sender, ItemViewerEventArgs args)
        {
            if (DoubleClick != null)
            {
                DoubleClick(this, args);
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

    #region ItemViewerEventArgs
    /// <summary>
    /// Event arguments for use with ItemViewer related events
    /// </summary>
    public partial class ItemViewerEventArgs : LiquidEventArgs
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the item title text
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the new item title text
        /// </summary>
        public string NewTitle { get; set; }

        #endregion

        #region Constructor

        public ItemViewerEventArgs()
        {
        }

        #endregion
    }

    #endregion
}
