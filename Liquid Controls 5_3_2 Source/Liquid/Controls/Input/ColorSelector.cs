using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Liquid
{
    public partial class ColorSelector : LiquidControl
    {
        #region Visual Elements

        /// <summary> 
        /// Viewer template.
        /// </summary>
        internal ItemViewer ElementViewer { get; set; }
        internal const string ElementViewerName = "ElementViewer";

        /// <summary> 
        /// Selected color template.
        /// </summary>
        internal ColorItem ElementSelectedColor { get; set; }
        internal const string ElementSelectedColorName = "ElementSelectedColor";

        /// <summary> 
        /// Custom colors template.
        /// </summary>
        internal StackPanel ElementCustom { get; set; }
        internal const string ElementCustomName = "ElementCustom";

        /// <summary> 
        /// More colors button template.
        /// </summary>
        internal Button ElementAdd { get; set; }
        internal const string ElementAddName = "ElementAdd";

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty SelectModeProperty = DependencyProperty.Register("Selected", typeof(Color), typeof(ColorSelector), new PropertyMetadata(Colors.Black, OnSelectedPropertyChanged));
        public Color Selected
        {
            get { return _selected; }
            set { base.SetValue(SelectModeProperty, value); }
        }

        private static void OnSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSelector selector = d as ColorSelector;
            Color newColor = (Color)e.NewValue;

            selector._selected = newColor;
            selector.SetSelected(selector._selected);
        }

        public static readonly DependencyProperty CustomVisibilityProperty = DependencyProperty.Register("CustomVisibility", typeof(Visibility), typeof(ColorSelector), null);
        public Visibility CustomVisibility
        {
            get { return (Visibility)base.GetValue(CustomVisibilityProperty); }
            set { base.SetValue(CustomVisibilityProperty, value); }
        }

        public static readonly DependencyProperty AddColorVisibilityProperty = DependencyProperty.Register("AddColorVisibility", typeof(Visibility), typeof(ColorSelector), null);
        public Visibility AddColorVisibility
        {
            get { return (Visibility)base.GetValue(AddColorVisibilityProperty); }
            set { base.SetValue(AddColorVisibilityProperty, value); }
        }
        
        #endregion

        #region Public Events

        public event EventHandler SelectionChanged;
        public event RoutedEventHandler CustomClick;

        #endregion

        #region Private Properties

        internal Color _selected = new Color();
        private List<ColorItem> _pending = new List<ColorItem>();
        private List<ColorItem> _pendingCustom = new List<ColorItem>();
        private static int _nextCustomSlot = 0;
        private DropDown _dropDown = null;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the opening position
        /// </summary>
        public DropDownPosition OpenPosition { get; set; }

        #endregion

        #region Public Static Properties

        public static List<uint> Custom { get; set; }

        public static int NextCustomSlot
        {
            get
            {
                int result = _nextCustomSlot;

                _nextCustomSlot++;
                if (_nextCustomSlot >= 8)
                {
                    _nextCustomSlot = 0;
                }

                return result;
            }
            set { _nextCustomSlot = value; }
        }

        #endregion

        #region Constructor

        static ColorSelector()
        {
            Custom = new List<uint>();

            Custom.Add(0xFFFFFFFF);
            Custom.Add(0xFFFFFFFF);
            Custom.Add(0xFFFFFFFF);
            Custom.Add(0xFFFFFFFF);
            Custom.Add(0xFFFFFFFF);
            Custom.Add(0xFFFFFFFF);
            Custom.Add(0xFFFFFFFF);
            Custom.Add(0xFFFFFFFF);
        }

        public ColorSelector()
        {
            SetupColors();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines whether a color exists in the standard color palette
        /// </summary>
        /// <param name="color">Color to test</param>
        /// <returns>True if the color exists, False if not</returns>
        public bool DoesColorExist(Color color)
        {
            bool result = false;

            if (ElementViewer != null)
            {
                foreach (ColorItem item in ElementViewer.Items)
                {
                    if (item.Color.A == color.A && item.Color.R == color.R && item.Color.G == color.G && item.Color.B == color.B)
                    {
                        result = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (ColorItem item in _pending)
                {
                    if (item.Color.A == color.A && item.Color.R == color.R && item.Color.G == color.G && item.Color.B == color.B)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Clears all colors from the selector
        /// </summary>
        public void Clear()
        {
            _pending.Clear();

            if (ElementViewer != null)
            {
                ElementViewer.Clear();
            }
        }

        /// <summary>
        /// Clears all colors from the selector
        /// </summary>
        public void ClearCustom()
        {
            _pendingCustom.Clear();

            if (ElementViewer != null)
            {
                ElementCustom.Children.Clear();
            }
        }

        /// <summary>
        /// Adds a color to the selector
        /// </summary>
        /// <param name="color">Color to add</param>
        public void Add(Color color)
        {
            Add(color, false);
        }

        /// <summary>
        /// Adds a color to the selector
        /// </summary>
        /// <param name="color">Color to add</param>
        public void Add(Color color, bool custom)
        {
            ColorItem item = new ColorItem();

            item.Background = new SolidColorBrush(color);
            item.Width = 18;
            item.Height = 18;

            if (custom)
            {
                AddCustomColor(item);
            }
            else
            {
                AddColor(item);
            }
        }

        /// <summary>
        /// Sets a bulk set of colors
        /// </summary>
        /// <param name="colors">An array containing the colors</param>
        public void Set(uint[] colors, bool custom)
        {
            uint a;
            uint r;
            uint g;
            uint b;

            if (custom)
            {
                ClearCustom();
            }
            else
            {
                Clear();
            }

            foreach (uint i in colors)
            {
                a = (i >> 24) & 255;
                r = (i >> 16) & 255;
                g = (i >> 8) & 255;
                b = (i >> 0) & 255;

                Add(Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b), custom);
            }
        }

        public void Select(Color color)
        {
            Selected = color;
            RaiseSelectionChanged(this, new EventArgs());
        }

        #endregion

        #region Private Methods

        private void AddColor(ColorItem item)
        {
            if (ElementViewer != null)
            {
                ElementViewer.Add(item);
            }
            else
            {
                _pending.Add(item);
            }
        }

        private void AddCustomColor(ColorItem item)
        {
            item.ItemSelected += new ItemViewerEventHandler(item_ItemSelected);

            if (ElementViewer != null)
            {
                ElementCustom.Children.Add(item);
            }
            else
            {
                _pendingCustom.Add(item);
            }
        }

        private void SetupColors()
        {
            _pending.Clear();

            AddColorRange(0, 256, 8, 1, 1, 1);
            AddColorRange(32, 256, 8, 1, 0, 0);
            AddColorRange(32, 256, 8, 0, 1, 0);
            AddColorRange(32, 256, 8, 0, 0, 1);
            AddColorRange(32, 256, 8, 1, 1, 0);
            AddColorRange(32, 256, 8, 0, 1, 1);
            AddColorRange(32, 256, 8, 1, 0, 1);
        }

        private void AddColorRange(int start, int end, int count, double r, double g, double b)
        {
            int i;
            int val;
            int step;
            ColorItem item;

            step = (end - start) / (count - 1);

            for (i = start; i <= end; i+= step)
            {
                val = i;
                if (val >= 256)
                {
                    val = 255;
                }
                item = new ColorItem();

                item.Width = 18;
                item.Height = 18;
                item.Background = new SolidColorBrush(Color.FromArgb(255, (byte)(val * r), (byte)(val * g), (byte)(val * b)));
                AddColor(item);
            }
        }

        private void SetSelected(Color color)
        {
            SolidColorBrush brush;

            if (ElementRoot != null)
            {
                foreach (ColorItem item in ElementViewer.Items)
                {
                    brush = (SolidColorBrush)item.Background;

                    if (brush.Color == color)
                    {
                        ElementSelectedColor.Background = new SolidColorBrush(color);
                        ElementViewer.Selected = item;
                        break;
                    }
                }

                foreach (ColorItem item in ElementCustom.Children)
                {
                    brush = (SolidColorBrush)item.Background;

                    if (brush.Color == color)
                    {
                        ElementSelectedColor.Background = new SolidColorBrush(color);
                        ElementViewer.Selected = item;
                        break;
                    }
                }
            }
        }

        private void ClearSelectedCustom()
        {
            if (ElementCustom != null)
            {
                foreach (ColorItem item in ElementCustom.Children)
                {
                    item.IsSelected = false;
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
            base.OnApplyTemplate();

            ElementViewer = (ItemViewer)GetTemplateChild(ElementViewerName);
            ElementSelectedColor = (ColorItem)GetTemplateChild(ElementSelectedColorName);
            ElementAdd = (Button)GetTemplateChild(ElementAddName);
            ElementCustom = (StackPanel)GetTemplateChild(ElementCustomName);

            ElementViewer.ItemSelected += new ItemViewerEventHandler(ElementViewer_ItemSelected);
            ElementAdd.Click += new RoutedEventHandler(ElementAdd_Click);

            foreach (ColorItem item in _pending)
            {
                ElementViewer.Add(item);
            }
            Set(Custom.ToArray(), true);
            _pending.Clear();

            _dropDown = (DropDown)ElementRoot;

            _dropDown.OpenPosition = OpenPosition;
            _dropDown.LostFocus += new RoutedEventHandler(ColorSelector_LostFocus);

            SetSelected(_selected);
        }

        private void ColorSelector_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FocusManager.GetFocusedElement() != ElementAdd)
            {
                _dropDown.IsOpen = false;
            }
        }

        private void ElementAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CustomClick != null)
            {
                CustomClick(this, e);
                _dropDown.IsOpen = false;
            }
        }

        private void item_ItemSelected(object sender, ItemViewerEventArgs e)
        {
            if (ElementCustom != null)
            {
                _selected = ((ColorItem)sender).Color;
                ElementSelectedColor.Background = new SolidColorBrush(_selected);

                foreach (ColorItem item in ElementCustom.Children)
                {
                    if (item.IsSelected && item != sender)
                    {
                        item.IsSelected = false;
                    }
                }
                
                ElementViewer.Selected = null;

                RaiseSelectionChanged(this, new EventArgs());
                _dropDown.IsOpen = false;
            }
        }

        private void ElementViewer_ItemSelected(object sender, ItemViewerEventArgs e)
        {
            _selected = ((ColorItem)ElementViewer.Selected).Color;
            ElementSelectedColor.Background = new SolidColorBrush(_selected);
            ClearSelectedCustom();
            
            RaiseSelectionChanged(this, new EventArgs());

            _dropDown.IsOpen = false;
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a SelectionChanged event to indicate the selected item has changed
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected virtual void RaiseSelectionChanged(object sender, EventArgs args)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, args);
            }
        }

        #endregion
    }
}
