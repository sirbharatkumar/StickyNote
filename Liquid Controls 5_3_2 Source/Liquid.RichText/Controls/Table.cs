using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Shapes;

namespace Liquid
{
    #region Delegates

    public delegate void TableEventHandler(object sender, TableEventArgs e);

    #endregion

    /// <summary>
    /// A resizable Table control with automatic borders and headers
    /// </summary>
    public partial class Table : Grid
    {
        #region Dependency Properties

        public static readonly DependencyProperty CellFillProperty = DependencyProperty.Register("CellFill", typeof(Brush), typeof(Table), null);
        public Brush CellFill
        {
            get { return (Brush)this.GetValue(CellFillProperty); }
            set { base.SetValue(CellFillProperty, value); UpdateStyle(); }
        }

        public static readonly DependencyProperty HeaderFillProperty = DependencyProperty.Register("HeaderFill", typeof(Brush), typeof(Table), null);
        public Brush HeaderFill
        {
            get { return (Brush)this.GetValue(HeaderFillProperty); }
            set { base.SetValue(HeaderFillProperty, value); UpdateStyle(); }
        }

        public static readonly DependencyProperty CellBorderBrushProperty = DependencyProperty.Register("CellBorderBrush", typeof(Brush), typeof(Table), null);
        public Brush CellBorderBrush
        {
            get { return (Brush)this.GetValue(CellBorderBrushProperty); }
            set { base.SetValue(CellBorderBrushProperty, value); UpdateStyle(); }
        }

        public static readonly DependencyProperty CellBorderThicknessProperty = DependencyProperty.Register("CellBorderThickness", typeof(Thickness), typeof(Table), null);
        public Thickness CellBorderThickness
        {
            get { return (Thickness)this.GetValue(CellBorderThicknessProperty); }
            set { base.SetValue(CellBorderThicknessProperty, value); UpdateStyle(); }
        }

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(Table), null);
        public Brush BorderBrush
        {
            get { return (Brush)this.GetValue(BorderBrushProperty); }
            set { base.SetValue(BorderBrushProperty, value); UpdateStyle(); }
        }

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(Table), null);
        public Thickness BorderThickness
        {
            get { return (Thickness)this.GetValue(BorderThicknessProperty); }
            set { base.SetValue(BorderThicknessProperty, value); UpdateStyle(); }
        }

        public static readonly DependencyProperty CellPaddingProperty = DependencyProperty.Register("CellPadding", typeof(Thickness), typeof(Table), null);
        public Thickness CellPadding
        {
            get { return (Thickness)this.GetValue(CellPaddingProperty); }
            set { base.SetValue(CellPaddingProperty, value); UpdateStyle(); }
        }

        #endregion

        #region Private Properties

        private bool _leftButtonDown = false;
        private bool _leftButtonSelectDown = false;
        private bool _leftButtonSelectRow = false;
        private bool _leftButtonSelectColumn = false;
        private Point _lastMouseDownPosition = new Point();
        private int _draggingColumn = -1;
        private int _draggingRow = -1;
        private List<Border> _selected = new List<Border>();
        private int _selectedCellRow = 0;
        private int _selectedCellColumn = 0;
        private bool _ignoreSizeChange = false;
        private int _headerRows = 0;
        private int _headerColumns = 0;
        private int _startSelectRow = -1;
        private int _startSelectColumn = -1;
        private bool _ignoreFocusChangeForSelection = false;
        private SelectMode _selectMode = SelectMode.Edit;

        #endregion

        #region Static Properties

        public static double GripSize = 4;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the row index for the selected cell
        /// </summary>
        public int SelectedCellRow
        {
            get { return _selectedCellRow; }
        }

        /// <summary>
        /// Gets the column index for the selected cell
        /// </summary>
        public int SelectedCellColumn
        {
            get { return _selectedCellColumn; }
        }

        /// <summary>
        /// Gets the first selected row index
        /// </summary>
        public int SelectedRowStart { get; set; }

        /// <summary>
        /// Gets the first selected column index
        /// </summary>
        public int SelectedColumnStart { get; set; }

        /// <summary>
        /// Gets the number of rows selected
        /// </summary>
        public int SelectedRowCount { get; set; }

        /// <summary>
        /// Gets the number of columns selected
        /// </summary>
        public int SelectedColumnCount { get; set; }

        /// <summary>
        /// Gets or sets whether the table can be resized by the user
        /// </summary>
        public bool IsResizable { get; set; }

        /// <summary>
        /// Gets or sets the selection mode
        /// </summary>
        public SelectMode SelectMode
        {
            get { return _selectMode; }
            set
            {
                _selectMode = value;
                UpdateEditableState();
            }
        }

        /// <summary>
        /// Gets or sets the number of header rows
        /// </summary>
        public int HeaderRows
        {
            get { return _headerRows; }
            set { _headerRows = value; UpdateStyle(); }
        }

        /// <summary>
        /// Gets or sets the number of header columns
        /// </summary>
        public int HeaderColumns
        {
            get { return _headerColumns; }
            set { _headerColumns = value; UpdateStyle(); }
        }

        /// <summary>
        /// Gets or sets whether the table stretches to full width
        /// </summary>
        public bool AutoWidth { get; set; }

        /// <summary>
        /// Gets or sets the minimum height a row can be dragged
        /// </summary>
        public double MinEditableRowHeight { get; set; }

        /// <summary>
        /// Gets or sets the minimum height a columns can be dragged
        /// </summary>
        public double MinEditableColumnWidth { get; set; }

        /// <summary>
        /// Gets or sets whether the table should fade-out during editing
        /// </summary>
        public bool EnableEditingFadeout { get; set; }

        /// <summary>
        /// Gets or sets the table style ID
        /// </summary>
        public string StyleID
        {
            get { return Tag.ToString(); }
            set { Tag = value; }
        }

        /// <summary>
        /// Gets the selected table cells
        /// </summary>
        public List<CellReference> Selected
        {
            get
            {
                List<CellReference> results = new List<CellReference>();
                UIElement cell;

                foreach (Border b in _selected)
                {
                    cell = GetCellContent(Grid.GetRow(b), Grid.GetColumn(b));
                    results.Add(new CellReference(Grid.GetRow(b), Grid.GetColumn(b), cell));
                }

                return results;
            }
        }

        #endregion

        #region Public Events

        public event TableEventHandler Clicked;
        public event TableEventHandler SelectionChanged;
        public event TableEventHandler ContentChanged;

        #endregion

        #region Constructor

        public Table()
        {
            CellPadding = new Thickness(3);
            EnableEditingFadeout = false;
            IsResizable = true;
            Setup();
        }

        public Table(int columns, int rows)
        {
            List<UIElement> elements = new List<UIElement>();
            int cells = columns * rows;

            CellPadding = new Thickness(3);
            EnableEditingFadeout = false;
            IsResizable = true;

            for (int i = 0; i < cells; i++)
            {
                elements.Add(CreateRichTextBlock());
            }

            Build(columns, rows, elements);
            Setup();
        }

        public Table(int columns, int rows, int headerRows, int headerColumns, List<UIElement> elements)
        {
            EnableEditingFadeout = false;
            _headerRows = headerRows;
            _headerColumns = headerColumns;
            Build(columns, rows, elements);
            Setup();
            IsResizable = true;
        }

        #endregion

        #region Internal Methods

        internal bool IsStyleUsed(string styleID)
        {
            bool result = false;
            Border border;

            foreach (UIElement element in Children)
            {
                if (element is Border)
                {
                    border = (Border)element;
                    if (border.Child is RichTextBlock)
                    {
                        result = ((RichTextBlock)border.Child).IsStyleUsed(styleID);
                        if (result)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new row to the bottom of the table
        /// </summary>
        public void AddRow()
        {
            InsertRow(RowDefinitions.Count, 1);
        }

        /// <summary>
        /// Adds a new row to the bottom of the table containing the provided elements
        /// </summary>
        /// <param name="elements">Any element derived from UIElement</param>
        public void AddRow(List<UIElement> elements)
        {
            InsertRow(RowDefinitions.Count, null, elements);
        }

        /// <summary>
        /// Adds a new column to the right of the table
        /// </summary>
        public void AddColumn()
        {
            InsertColumn(ColumnDefinitions.Count, 1);
        }

        /// <summary>
        /// Adds a new column to the right of the table containing the provided elements
        /// </summary>
        /// <param name="elements">Any element derived from UIElement</param>
        public void AddColumn(List<UIElement> elements)
        {
            InsertColumn(ColumnDefinitions.Count, null, elements);
        }

        /// <summary>
        /// Deletes the specified Row including all elements that refer to it
        /// </summary>
        /// <param name="rowIndex">Row index to delete</param>
        public void DeleteRow(int rowIndex)
        {
            int r;

            if (RowDefinitions.Count == HeaderRows || rowIndex >= RowDefinitions.Count)
            {
                return;
            }

            // Delete the row cells and move existing content up a row
            for (int i = GetEndCellIndex(); i >= 0; i--)
            {
                r = (int)Children[i].GetValue(Grid.RowProperty);
                if (r == rowIndex)
                {
                    if (Children[i] is Border)
                    {
                        ((Border)Children[i]).Child = null;
                    }
                    Children.RemoveAt(i);
                }
                if (r > rowIndex)
                {
                    Children[i].SetValue(Grid.RowProperty, r - 1);
                }
            }

            RowDefinitions.RemoveAt(rowIndex);
            UpdateStyle();
            UpdateSelected(-1, -1, Visibility.Collapsed, true);
        }

        /// <summary>
        /// Gets a collection of elements with a matching row index
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        /// <returns>Element collection</returns>
        public List<UIElement> GetRowElements(int rowIndex)
        {
            List<UIElement> elements = new List<UIElement>();

            for (int i = 0; i <= GetEndCellIndex(); i++)
            {
                if ((int)Children[i].GetValue(Grid.RowProperty) == rowIndex)
                {
                    if (Children[i] is Border)
                    {
                        elements.Add(((Border)Children[i]).Child);
                    }
                    else
                    {
                        elements.Add(Children[i]);
                    }
                }
            }

            return elements;
        }

        /// <summary>
        /// Gets a collection of elements with a matching column index
        /// </summary>
        /// <param name="columnIndex">Column index</param>
        /// <returns>Element collection</returns>
        public List<UIElement> GetColumnElements(int columnIndex)
        {
            List<UIElement> elements = new List<UIElement>();

            for (int i = 0; i <= GetEndCellIndex(); i++)
            {
                if ((int)Children[i].GetValue(Grid.ColumnProperty) == columnIndex)
                {
                    if (Children[i] is Border)
                    {
                        elements.Add(((Border)Children[i]).Child);
                    }
                    else
                    {
                        elements.Add(Children[i]);
                    }
                }
            }

            return elements;
        }

        /// <summary>
        /// Deletes the specified Column including all elements that refer to it
        /// </summary>
        /// <param name="columnIndex">Column index to delete</param>
        public void DeleteColumn(int columnIndex)
        {
            int c;

            if (ColumnDefinitions.Count == 1 || columnIndex >= ColumnDefinitions.Count)
            {
                return;
            }

            SetColumnsToPixelWidth();

            // Delete the column cells and move existing content left a column
            for (int i = GetEndCellIndex(); i >= 0; i--)
            {
                c = (int)Children[i].GetValue(Grid.ColumnProperty);

                if (c == columnIndex)
                {
                    if (Children[i] is Border)
                    {
                        ((Border)Children[i]).Child = null;
                    }
                    Children.RemoveAt(i);
                }
                if (c > columnIndex)
                {
                    Children[i].SetValue(Grid.ColumnProperty, c - 1);
                }
            }

            ColumnDefinitions.RemoveAt(columnIndex);
            CalculateNewWidth();
            UpdateStyle();
        }

        /// <summary>
        /// Creates a new column with default richtextblock content
        /// </summary>
        /// <param name="columnIndex">The column index to insert the new column</param>
        public void InsertColumn(int columnIndex, int count)
        {
            List<UIElement> elements = new List<UIElement>();
            int c;
            int i;

            for (c = 0; c < count; c++)
            {
                elements.Clear();

                for (i = 0; i < RowDefinitions.Count; i++)
                {
                    elements.Add(CreateRichTextBlock());
                }
                InsertColumn(columnIndex + c, null, elements);
            }
        }

        /// <summary>
        /// Creates a new column and populates the cells with the provided elements
        /// </summary>
        /// <param name="columnIndex">The column index to insert the new column</param>
        /// <param name="width">Width of the column</param>
        /// <param name="elements">Any element derived from UIElement</param>
        public void InsertColumn(int columnIndex, GridLength? width, List<UIElement> elements)
        {
            Border border;
            GridLength length;
            ColumnDefinition newColumn = new ColumnDefinition();
            int insertIndex;
            int c;
            int i;

            if (width == null)
            {
                if (ColumnDefinitions.Count > 0)
                {
                    SetColumnsToPixelWidth();
                    for (i = ColumnDefinitions.Count - 1; i >= 0; i--)
                    {
                        if (ColumnDefinitions[i].Width.Value > MinEditableColumnWidth * 2)
                        {
                            length = new GridLength(ColumnDefinitions[i].Width.Value - MinEditableColumnWidth);
                            ColumnDefinitions[i].SetValue(ColumnDefinition.WidthProperty, length);
                            break;
                        }
                    }
                }

                length = new GridLength(MinEditableColumnWidth);
                newColumn.SetValue(ColumnDefinition.WidthProperty, length);
            }
            else
            {
                newColumn.SetValue(ColumnDefinition.WidthProperty, width);
            }

            // Change the column references for all content on or above the new column index
            for (i = 0; i <= GetEndCellIndex(); i++)
            {
                c = (int)Children[i].GetValue(Grid.ColumnProperty);
                if (c >= columnIndex)
                {
                    Children[i].SetValue(Grid.ColumnProperty, c + 1);
                }
            }

            ColumnDefinitions.Insert(columnIndex, newColumn);

            for (i = 0; i < RowDefinitions.Count; i++)
            {
                border = null;

                if (elements != null)
                {
                    if (i < elements.Count)
                    {
                        if (elements[i] is Border)
                        {
                            border = (Border)elements[i];
                        }
                        else
                        {
                            border = CreateBorder(i, columnIndex);
                            border.Child = elements[i];
                        }
                    }
                }

                if (border == null)
                {
                    border = CreateBorder(i, columnIndex);
                }

                if (ColumnDefinitions.Count - 1 == columnIndex)
                {
                    insertIndex = GetCellIndex(i + 1, 0);
                }
                else
                {
                    insertIndex = GetCellIndex(i, columnIndex + 1);
                }
                Children.Insert(insertIndex, border);
            }
            UpdateStyle();
            SetupHandlers();
        }

        /// <summary>
        /// Creates a new row with default richtextblock content
        /// </summary>
        /// <param name="rowIndex">The row index to insert the new row</param>
        /// <param name="count">Number of rows to insert</param>
        public void InsertRow(int rowIndex, int count)
        {
            List<UIElement> elements = new List<UIElement>();
            int r;
            int i;

            for (r = 0; r < count; r++)
            {
                elements.Clear();

                for (i = 0; i < ColumnDefinitions.Count; i++)
                {
                    elements.Add(CreateRichTextBlock());
                }
                InsertRow(rowIndex + r, null, elements);
            }
        }

        /// <summary>
        /// Creates a new row and populates the cells with the provided elements
        /// </summary>
        /// <param name="rowIndex">The row index to insert the new row</param>
        /// <param name="height">Row height</param>
        /// <param name="elements">Any element derived from UIElement</param>
        public void InsertRow(int rowIndex, GridLength? height, List<UIElement> elements)
        {
            Border border;
            int insertIndex = GetCellIndex(rowIndex, 0);
            RowDefinition newRow = new RowDefinition();
            int r;
            int i;

            if (rowIndex == RowDefinitions.Count)
            {
                insertIndex++;
            }

            // Move existing content down a row
            for (i = 0; i <= GetEndCellIndex(); i++)
            {
                r = (int)Children[i].GetValue(Grid.RowProperty);
                if (r >= rowIndex)
                {
                    Children[i].SetValue(Grid.RowProperty, r + 1);
                }
            }

            if (height != null)
            {
                newRow.SetValue(RowDefinition.HeightProperty, height);
            }

            RowDefinitions.Insert(rowIndex, newRow);

            for (i = 0; i < ColumnDefinitions.Count; i++)
            {
                border = null;

                if (elements != null)
                {
                    if (i < elements.Count)
                    {
                        if (elements[i] is Border)
                        {
                            border = (Border)elements[i];
                        }
                        else
                        {
                            border = CreateBorder(rowIndex, i);
                            border.Child = elements[i];
                        }
                    }
                }

                if (border == null)
                {
                    border = CreateBorder(rowIndex, i);
                }
                Children.Insert(insertIndex + i, border);
            }
            UpdateStyle();
            SetupHandlers();
        }

        /// <summary>
        /// Swaps two rows around
        /// </summary>
        /// <param name="rowA">The first row</param>
        /// <param name="rowB">The second row</param>
        public void SwapRows(int rowA, int rowB)
        {
            List<UIElement> newRowBChildren = new List<UIElement>();
            int current;

            foreach (UIElement child in Children)
            {
                current = (int)child.GetValue(Grid.RowProperty);
                if (current == rowA)
                {
                    newRowBChildren.Add(child);
                }
                else if (current == rowB)
                {
                    child.SetValue(Grid.RowProperty, rowA);
                }
            }

            foreach (UIElement child in newRowBChildren)
            {
                child.SetValue(Grid.RowProperty, rowB);
            }
        }

        /// <summary>
        /// Fixes all column widths to pixel widths
        /// </summary>
        public void SetColumnsToPixelWidth()
        {
            GridLength l;

            if (ColumnDefinitions.Count > 0)
            {
                foreach (ColumnDefinition c in ColumnDefinitions)
                {
                    if (c.Width.GridUnitType != GridUnitType.Pixel)
                    {
                        l = new GridLength(c.ActualWidth, GridUnitType.Pixel);
                        c.SetValue(ColumnDefinition.WidthProperty, l);
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the editable state of any child rich textboxes
        /// </summary>
        private void UpdateEditableState()
        {
            Border border;

            for (int i = 0; i <= GetEndCellIndex(); i++)
            {
                border = (Border)Children[i];
                if (border.Child is RichTextBlock)
                {
                    ((RichTextBlock)border.Child).SelectMode = _selectMode;
                }
            }
        }

        /// <summary>
        /// Gets the element at a given row/column
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="column">Column index</param>
        /// <returns>Element</returns>
        private UIElement GetCellContent(int row, int column)
        {
            int index = GetCellIndex(row, column);
            UIElement result = null;
            Border b;

            if (index >= 0)
            {
                b = Children[index] as Border;
                if (b != null)
                {
                    result = b.Child;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the element index of the Border for the provided row/column location
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="column">Column Index</param>
        /// <returns>Index of the Border element</returns>
        private int GetCellIndex(int row, int column)
        {
            int last = GetEndCellIndex();
            int result = last;
            int i;
            UIElement e;

            for (i = 0; i <= last; i++)
            {
                e = Children[i];
                if (e is Border && (int)e.GetValue(Grid.RowProperty) == row && (int)e.GetValue(Grid.ColumnProperty) == column)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the size of the parent control
        /// </summary>
        /// <returns>Size of the parent</returns>
        private Size GetParentSize()
        {
            Size result = new Size(2048, 2048);

            if (Parent is FrameworkElement)
            {
                result = ((FrameworkElement)Parent).RenderSize;
                if (result.Width == 0)
                {
                    result.Width = this.ActualWidth;
                    result.Height = this.ActualHeight;
                }
            }

            return result;
        }

        /// <summary>
        /// Hook onto some events
        /// </summary>
        private void Setup()
        {
            this.Loaded += new RoutedEventHandler(OnLoaded);
            this.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(OnMouseLeftButtonUp);
            this.MouseMove += new MouseEventHandler(OnMouseMove);

            AutoWidth = true;
            _selectMode = SelectMode.Edit;
            Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            MinEditableRowHeight = 16;
            MinEditableColumnWidth = 24;
        }

        private int GetEndCellIndex()
        {
            return (ColumnDefinitions.Count * RowDefinitions.Count) - 1;
        }

        /// <summary>
        /// Adds border controls to the grid content
        /// </summary>
        private void AddChildBorders()
        {
            List<UIElement> temp;
            int i;

            if (Children.Count == GetEndCellIndex() + 1)
            {
                BuildControlBorders();
            }

            if (Children.Count > 0)
            {
                if (Children[0] is Border)
                {
                    return;
                }
            }

            temp = new List<UIElement>();
            temp.AddRange(Children);
            Children.Clear();

            BuildBorders(ColumnDefinitions.Count, RowDefinitions.Count);
            foreach (UIElement element in temp)
            {
                if (!(element is Border))
                {
                    i = GetCellIndex((int)element.GetValue(Grid.RowProperty), (int)element.GetValue(Grid.ColumnProperty));
                    if (i >= 0)
                    {
                        ((Border)Children[i]).Child = element;
                    }
                }
            }
        }

        /// <summary>
        /// Constructs a table of the specified dimensions
        /// </summary>
        /// <param name="columns">Columns to create</param>
        /// <param name="rows">Rows to create</param>
        /// <param name="elements">Elements to populate the cells with</param>
        private void Build(int columns, int rows, List<UIElement> elements)
        {
            int i;

            for (i = 0; i < columns; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (i = 0; i < rows; i++)
            {
                RowDefinitions.Add(new RowDefinition());
            }

            BuildBorders(columns, rows);
            UpdateStyle();

            for (i = 0; i <= GetEndCellIndex(); i++)
            {
                ((Border)Children[i]).Child = elements[i];
            }
        }

        /// <summary>
        /// Applies borders to the table
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="rows">Rows</param>
        private void BuildBorders(int columns, int rows)
        {
            int r;
            int c;

            for (r = 0; r < rows; r++)
            {
                for (c = 0; c < columns; c++)
                {
                    Children.Add(CreateBorder(r, c));
                }
            }

            BuildControlBorders();
        }

        private void BuildControlBorders()
        {
            Border border;

            border = new Border();
            border.IsHitTestVisible = false;
            Children.Add(border);

            border = new Border();
            border.IsHitTestVisible = false;
            Children.Add(border);
        }

        /// <summary>
        /// Sets the border styles
        /// </summary>
        private void UpdateStyle()
        {
            Border border;
            Thickness t;
            int r;
            int c;
            int i = 0;
            int expected = (RowDefinitions.Count * ColumnDefinitions.Count) + 2;

            if (Children.Count >= expected)
            {
                for (r = 0; r < RowDefinitions.Count; r++)
                {
                    for (c = 0; c < ColumnDefinitions.Count; c++, i++)
                    {
                        t = new Thickness(CellBorderThickness.Left, CellBorderThickness.Top, CellBorderThickness.Right, CellBorderThickness.Bottom);

                        t.Left = 0;
                        if (c == ColumnDefinitions.Count - 1)
                        {
                            t.Right = 0;
                        }
                        t.Top = 0;
                        if (r == RowDefinitions.Count - 1)
                        {
                            t.Bottom = 0;
                        }
                        border = (Border)Children[i];
                        border.BorderBrush = CellBorderBrush;
                        if (HeaderFill != null && (r < HeaderRows || c < HeaderColumns))
                        {
                            border.Background = HeaderFill;
                        }
                        else
                        {
                            border.Background = CellFill;
                        }
                        border.Padding = CellPadding;
                        border.BorderThickness = t;
                    }
                }

                border = (Border)Children[i];
                if (ColumnDefinitions.Count > 0)
                {
                    border.SetValue(Grid.ColumnSpanProperty, ColumnDefinitions.Count);
                }
                if (RowDefinitions.Count > 0)
                {
                    border.SetValue(Grid.RowSpanProperty, RowDefinitions.Count);
                }
                border.BorderBrush = BorderBrush;
                border.BorderThickness = BorderThickness;
                i++;

                border = (Border)Children[i];
                border.Visibility = Visibility.Visible;
                border.BorderBrush = BorderBrush;
                border.BorderThickness = BorderThickness;

                if (HeaderRows > 0)
                {
                    border.SetValue(Grid.ColumnSpanProperty, ColumnDefinitions.Count);
                    border.SetValue(Grid.RowSpanProperty, HeaderRows);
                }
                else if (HeaderColumns > 0)
                {
                    border.SetValue(Grid.ColumnSpanProperty, HeaderColumns);
                    border.SetValue(Grid.RowSpanProperty, RowDefinitions.Count);
                }
                else
                {
                    border.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Updates the selected element
        /// </summary>
        /// <param name="row">The row</param>
        /// <param name="column">The column</param>
        /// <param name="visibility">Visibility state</param>
        /// <param name="clearAll">Indicates whether existing selection should be cleared</param>
        private void UpdateSelected(int row, int column, Visibility visibility, bool clearAll)
        {
            TableEventArgs args;
            int startRow = row;
            int startColumn = column;
            int width = 0;
            int height = 0;
            int r;
            int c;
            Border select;

            if (_leftButtonSelectRow)
            {
                startColumn = 0;
                width = ColumnDefinitions.Count;
            }
            else
            {
                if (_startSelectColumn < column)
                {
                    startColumn = _startSelectColumn;
                    width = column - _startSelectColumn;
                }
                else
                {
                    startColumn = column;
                    width = _startSelectColumn - column;
                }
            }

            if (_leftButtonSelectColumn)
            {
                startRow = 0;
                height = RowDefinitions.Count;
            }
            else
            {
                if (_startSelectRow < row)
                {
                    startRow = _startSelectRow;
                    height = row - _startSelectRow;
                }
                else
                {
                    startRow = row;
                    height = _startSelectRow - row;
                }
            }

            if (!EnableEditingFadeout)
            {
                visibility = Visibility.Collapsed;
            }

            if (_selected != null)
            {
                if (clearAll)
                {
                    foreach (UIElement e in _selected)
                    {
                        if (Children.Contains(e))
                        {
                            Children.Remove(e);
                        }
                    }
                    _selected.Clear();
                }

                if (visibility == Visibility.Visible)
                {
                    SelectedRowStart = startRow;
                    SelectedColumnStart = startColumn;
                    SelectedRowCount = height + 1;
                    SelectedColumnCount = width + 1;

                    for (r = startRow; r <= startRow + height; r++)
                    {
                        for (c = startColumn; c <= startColumn + width; c++)
                        {
                            select = new Border();
                            select.IsHitTestVisible = false;
                            select.BorderThickness = new Thickness(1);
                            select.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
                            select.SetValue(Grid.ColumnProperty, c);
                            select.SetValue(Grid.RowProperty, r);
                            select.Margin = new Thickness(0, 0, 1, 1);
                            select.Background = new SolidColorBrush(Color.FromArgb(32, 0, 0, 255));

                            _selected.Add(select);
                            Children.Add(select);
                        }
                    }
                }
            }

            if (SelectionChanged != null)
            {
                args = new TableEventArgs();
                SelectionChanged(this, args);
            }
        }

        /// <summary>
        /// Gets the columns index for the provided position
        /// </summary>
        /// <param name="p">Position</param>
        /// <returns>Column index or -1</returns>
        private int GetColumnIndex(Point p)
        {
            int i;
            double width = 0;

            for (i = 0; i < ColumnDefinitions.Count; i++)
            {
                width += ColumnDefinitions[i].ActualWidth;
                if (p.X > width - GripSize && p.X < width + GripSize)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the rows index for the provided position
        /// </summary>
        /// <param name="p">Position</param>
        /// <returns>Row index or -1</returns>
        private int GetRowIndex(Point p)
        {
            int i;
            double height = 0;

            for (i = 0; i < RowDefinitions.Count; i++)
            {
                height += RowDefinitions[i].ActualHeight;
                if (p.Y > height - GripSize && p.Y < height + GripSize)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the row index for a given point
        /// </summary>
        /// <param name="p">Point</param>
        /// <returns>Column index</returns>
        private int GetRowIndexContainingPoint(Point p)
        {
            int i;
            double height = 0;

            for (i = 0; i < RowDefinitions.Count; i++)
            {
                if (p.Y >= height && p.Y < height + RowDefinitions[i].ActualHeight)
                {
                    return i;
                }

                height += RowDefinitions[i].ActualHeight;
            }

            return -1;
        }

        /// <summary>
        /// Gets a column index for a given point
        /// </summary>
        /// <param name="p">Point</param>
        /// <returns>Column index</returns>
        private int GetColumnIndexContainingPoint(Point p)
        {
            int i;
            double width = 0;

            for (i = 0; i < ColumnDefinitions.Count; i++)
            {
                if (p.X >= width && p.X < width + ColumnDefinitions[i].ActualWidth)
                {
                    return i;
                }
                width += ColumnDefinitions[i].ActualWidth;
            }

            return -1;
        }

        private void SetupHandlers()
        {
            Border border;

            foreach (UIElement e in Children)
            {
                if (e is Border)
                {
                    border = (Border)e;
                    if (border.Child is RichTextBlock)
                    {
                        border.Child.LostFocus -= Child_LostFocus;
                        border.Child.LostFocus += Child_LostFocus;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new editable RichTextBlock with the correct properties set
        /// </summary>
        /// <returns>A new RichTextBlock</returns>
        private RichTextBlock CreateRichTextBlock()
        {
            RichTextBlock rtb = new RichTextBlock()
            {
                IsReadOnly = false,
                SelectMode = SelectMode.Edit,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0),
            };

            return rtb;
        }

        /// <summary>
        /// Creates a new Border element
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="column">Column index</param>
        /// <returns>New Border element</returns>
        private Border CreateBorder(int row, int column)
        {
            Border border = new Border();

            border.Padding = new Thickness(3);
            border.SetValue(Grid.RowProperty, row);
            border.SetValue(Grid.ColumnProperty, column);

            return border;
        }

        private double GetRemainingWidth(double maxWidth, int columnIndex, double newColumnWidth)
        {
            double total = 0;
            int i;

            for (i = 0; i < ColumnDefinitions.Count; i++)
            {
                if (i == columnIndex)
                {
                    total += newColumnWidth;
                }
                else
                {
                    total += ColumnDefinitions[i].Width.Value;
                }
            }

            if (total > maxWidth)
            {
                newColumnWidth = maxWidth - (total - newColumnWidth);
            }

            return newColumnWidth;
        }

        /// <summary>
        /// Updates the control width to the width of the combined columns
        /// </summary>
        private void CalculateNewWidth()
        {
            double width = 0;
            int i;

            for (i = 0; i < ColumnDefinitions.Count; i++)
            {
                width += ColumnDefinitions[i].Width.Value;
            }

            _ignoreSizeChange = true;
            AutoWidth = false;
            Width = width;
        }

        #endregion

        #region Event Handling

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetupHandlers();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_ignoreSizeChange)
            {
                AddChildBorders();
                UpdateStyle();
            }
            _ignoreSizeChange = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(this);
            int rowIndex;
            int columnIndex;
            GridLength length;
            double temp;
            Size original = GetParentSize();
            double adjustedWidth;

            if (!IsResizable)
            {
                return;
            }

            if (_selectMode == SelectMode.Edit)
            {
                if (_leftButtonSelectDown)
                {
                    rowIndex = GetRowIndexContainingPoint(p);
                    columnIndex = GetColumnIndexContainingPoint(p);

                    if (rowIndex >= 0 && columnIndex >= 0)
                    {
                        UpdateSelected(rowIndex, columnIndex, Visibility.Visible, true);
                    }
                }
                else
                {
                    if (!_leftButtonDown)
                    {
                        rowIndex = GetRowIndex(p);
                        columnIndex = GetColumnIndex(p);

                        if (rowIndex >= 0)
                        {
                            Cursor = Cursors.SizeNS;
                        }
                        else if (columnIndex >= 0)
                        {
                            Cursor = Cursors.SizeWE;
                        }
                        else
                        {
                            Cursor = Cursors.Arrow;
                        }
                    }
                    else
                    {
                        if (_draggingColumn >= 0)
                        {
                            SetColumnsToPixelWidth();
                            temp = ColumnDefinitions[_draggingColumn].Width.Value + (p.X - _lastMouseDownPosition.X);
                            if (temp > MinEditableColumnWidth)
                            {
                                if (_draggingColumn < ColumnDefinitions.Count - 1)
                                {
                                    adjustedWidth = ColumnDefinitions[_draggingColumn + 1].Width.Value - (p.X - _lastMouseDownPosition.X);
                                    if (adjustedWidth < MinEditableColumnWidth)
                                    {
                                        adjustedWidth = MinEditableColumnWidth;
                                    }
                                    length = new GridLength(adjustedWidth);
                                    ColumnDefinitions[_draggingColumn + 1].SetValue(ColumnDefinition.WidthProperty, length);
                                }
                                temp = GetRemainingWidth(original.Width, _draggingColumn, temp);
                                if (temp > 0)
                                {
                                    length = new GridLength(temp);
                                    ColumnDefinitions[_draggingColumn].SetValue(ColumnDefinition.WidthProperty, length);
                                }
                            }
                            if (_draggingColumn == ColumnDefinitions.Count - 1)
                            {
                                CalculateNewWidth();
                            }
                        }
                        else if (_draggingRow >= 0)
                        {
                            temp = RowDefinitions[_draggingRow].ActualHeight;
                            if (RowDefinitions[_draggingRow].Height.GridUnitType == GridUnitType.Pixel)
                            {
                                temp = RowDefinitions[_draggingRow].Height.Value;
                            }
                            temp += (p.Y - _lastMouseDownPosition.Y);
                            if (temp > MinEditableRowHeight)
                            {
                                length = new GridLength(temp);
                                RowDefinitions[_draggingRow].SetValue(RowDefinition.HeightProperty, length);
                            }
                        }

                        if (ContentChanged != null)
                        {
                            ContentChanged(this, new TableEventArgs());
                        }

                        _lastMouseDownPosition = p;
                    }
                }
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_selectMode == SelectMode.Edit)
            {
                if (_leftButtonSelectDown)
                {
                    _leftButtonSelectDown = false;
                    _leftButtonSelectRow = false;
                    _leftButtonSelectColumn = false;
                }
                if (_leftButtonDown)
                {
                    _leftButtonDown = false;
                    ReleaseMouseCapture();
                }

                if (Parent.GetValue(TagProperty) is RichTextPanel)
                {
                    e.Handled = true;
                }
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);
            TableEventArgs args = new TableEventArgs();
            Border border;
            RichTextBlock rtb;

            args.ColumnIndex = GetColumnIndex(p);
            args.RowIndex = GetRowIndex(p);

            if (_selectMode == SelectMode.Edit)
            {
                SelectedRowStart = 0;
                SelectedColumnStart = 0;
                SelectedRowCount = 0;
                SelectedColumnCount = 0;

                _draggingColumn = args.ColumnIndex;
                if (_draggingColumn == -1)
                {
                    _draggingRow = args.RowIndex;
                }

                if (_draggingColumn >= 0 || _draggingRow >= 0)
                {
                    _leftButtonDown = true;
                    CaptureMouse();
                    _lastMouseDownPosition = e.GetPosition(this);
                }
                if(_draggingColumn == -1)
                {
                    _selectedCellColumn = GetColumnIndexContainingPoint(p);
                }
                if (_draggingRow == -1)
                {
                    _selectedCellRow = GetRowIndexContainingPoint(p);
                }

                if (!_leftButtonDown)
                {
                    if (e.OriginalSource is RichTextPanel)
                    {
                        UpdateSelected(_selectedCellRow, _selectedCellColumn, Visibility.Collapsed, true);
                        e.Handled = true;
                    }
                    else if (e.OriginalSource is Border && args.ColumnIndex == -1 || args.RowIndex == -1)
                    {
                        border = e.OriginalSource as Border;
                        if (border != null && border.Child is RichTextBlock)
                        {
                            _ignoreFocusChangeForSelection = true;
                            rtb = (RichTextBlock)border.Child;
                            rtb.Root.ActiveTable = this;
                            rtb.Focus();
                            rtb.End();

                            _selectedCellRow = (int)border.GetValue(Grid.RowProperty);
                            _selectedCellColumn = (int)border.GetValue(Grid.ColumnProperty);
                            _startSelectRow = _selectedCellRow;
                            _startSelectColumn = _selectedCellColumn;

                            p = e.GetPosition(border);
                            if (p.X >= 0 && p.X < GripSize)
                            {
                                _leftButtonSelectRow = true;
                            }
                            else if (p.Y >= 0 && p.Y < GripSize)
                            {
                                _leftButtonSelectColumn = true;
                            }

                            UpdateSelected(_selectedCellRow, _selectedCellColumn, Visibility.Visible, true);
                            _leftButtonSelectDown = true;
                            CaptureMouse();
                        }
                        else if (border != null && border.Child is Control)
                        {
                            ((Control)border.Child).Focus();
                        }
                        e.Handled = true;
                    }
                }
            }

            RaiseClicked(this, args);
        }

        private void Child_LostFocus(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = ((FrameworkElement)sender).Parent;

            if (!_ignoreFocusChangeForSelection && parent != null && _selectMode == SelectMode.Edit)
            {
                if (FocusManager.GetFocusedElement() is RichTextBlock)
                {
                    UpdateSelected(0, 0, Visibility.Collapsed, true);
                }
            }

            _ignoreFocusChangeForSelection = false;
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a Clicked event to indicate table has been clicked
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseClicked(object sender, TableEventArgs args)
        {
            if (Clicked != null)
            {
                Clicked(sender, args);
            }
        }

        #endregion
    }
}
