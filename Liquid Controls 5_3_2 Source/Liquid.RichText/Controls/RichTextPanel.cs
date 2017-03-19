using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Liquid
{
    #region Delegates

    public delegate void RichTextPanelEventHandler(object sender, RichTextPanelEventArgs e);

    #endregion

    public partial class RichTextPanel : Canvas
    {
        #region Private Properties

        private Canvas _contentChildren = null;
        private bool _disableUpdates = false;
        private List<RichTextPanelRow> _rows = new List<RichTextPanelRow>();
        private List<RichTextPanelRow> _tempRows = new List<RichTextPanelRow>();
        private int _removed = 0;
        private double _lastUpdatedSize = 0;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the element row list
        /// </summary>
        public List<RichTextPanelRow> Rows
        {
            get { return _rows; }
        }

        /// <summary>
        /// Gets or sets whether updates to the children collection updates the layout immediately
        /// </summary>
        public bool DisableUpdates
        {
            get { return _disableUpdates; }
            set
            {
                _disableUpdates = value;
                if (!_disableUpdates)
                {
                    Update();
                }
            }
        }

        /// <summary>
        /// Gets the content child collection
        /// </summary>
        public UIElementCollection ContentChildren
        {
            get { return _contentChildren.Children; }
        }

        #endregion

        #region Public Events

        public event RichTextPanelEventHandler TextBlockSplit;
        public event RichTextPanelEventHandler TextBlockMerge;
        public event RichTextPanelEventHandler ApplyFormatting;
        public event RichTextPanelEventHandler Updated;

        #endregion

        #region Constructor

        public RichTextPanel()
        {
            _contentChildren = new Canvas();
            _contentChildren.Tag = this;

            Children.Insert(0, _contentChildren);

            this.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Compacts all textblock elements with the same style ID
        /// </summary>
        internal void CompactTextElements()
        {
            int i;
            TextBlockPlus current;
            TextBlockPlus next;
            RichTextTag currentTag;
            RichTextTag nextTag;

            for (i = 0; i < ContentChildren.Count - 1; i++)
            {
                if (ContentChildren[i] is TextBlockPlus)
                {
                    current = (TextBlockPlus)ContentChildren[i];

                    if (current.Text.Length == 0 && i > 0)
                    {
                        if (ContentChildren[i - 1] is TextBlockPlus || ContentChildren[i + 1] is TextBlockPlus)
                        {
                            ContentChildren.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }

                    if (i + 1 < ContentChildren.Count)
                    {
                        if (ContentChildren[i + 1] is TextBlockPlus)
                        {
                            next = (TextBlockPlus)ContentChildren[i + 1];
                            currentTag = (RichTextTag)current.Tag;
                            nextTag = (RichTextTag)next.Tag;

                            if (currentTag.StyleID == nextTag.StyleID && currentTag.Metadata == nextTag.Metadata)
                            {
                                current.Text += next.Text;
                                ContentChildren.RemoveAt(i + 1);
                                i--;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Re-positions all the elements, wrapping as necessary
        /// </summary>
        internal void Update()
        {
            _rows.Clear();

            if (ContentChildren.Count > 0)
            {
                CompactTextElements();
                Update(ContentChildren[0], true);
            }
        }

        internal void Update(UIElement element, bool updateAll)
        {
            int i = ContentChildren.IndexOf(element) - 1;

            if (i < 0)
            {
                i = 0;
            }

            Update(GetRowForElement(ContentChildren[i]), updateAll, null);
        }

        internal void Update(RichTextPanelRow topRow, bool updateAll, UIElement ignore)
        {
            int rowIndex = _rows.IndexOf(topRow);
            int startIndex = 0;
            int rowEndIndex = 0;
            Point pos = new Point();
            double thisWidth = 0;
            List<string> splitText;
            TextBlockPlus currentTextBlock;
            TextBlockPlus next;
            RichTextPanelRow temp;
            RichTextPanelEventArgs args;
            FrameworkElement current;
            Table table;
            bool updateNextLine = false;
            bool newlineStarted = false;
            bool createBlankTextBlock;
            int startRow = 0;
            int lineOut = 1;
            int endRow;
            double offset;
            int i;
            int a;
            int b;

            if (ActualWidth == 0)
            {
                return;
            }

            if (rowIndex >= 0)
            {
                startIndex = ContentChildren.IndexOf(_rows[rowIndex].Start);
                rowEndIndex = ContentChildren.IndexOf(_rows[rowIndex].End);
            }
            else
            {
                rowIndex = 0;
            }

            startRow = rowIndex;
            _tempRows.Clear();

            if (rowIndex < _rows.Count)
            {
                pos.Y = _rows[rowIndex].Position.Y;
            }

            if (startIndex < 0)
            {
                return;
            }

            for (i = startIndex; i < ContentChildren.Count; i++)
            {
                newlineStarted = false;
                current = (FrameworkElement)ContentChildren[i];

                current.SetValue(Canvas.LeftProperty, pos.X);
                current.SetValue(Canvas.TopProperty, pos.Y);

                if (!(current is TextBlockPlus))
                {
                    if (current is Table)
                    {
                        table = (Table)current;
                        if (table.AutoWidth)
                        {
                            table.Width = (double.IsNaN(Width) ? ActualWidth : Width) - pos.X;
                        }
                        thisWidth = table.Width;
                    }
                    else
                    {
                        thisWidth = (double.IsNaN(current.Width) ? current.ActualWidth : current.Width);
                    }
                }
                else
                {
                    currentTextBlock = (TextBlockPlus)current;
                    thisWidth = currentTextBlock.ContentWidth;

                    if (pos.X + thisWidth > ActualWidth)
                    {
                        splitText = SplitText(currentTextBlock, currentTextBlock.Text, pos.X, ActualWidth);

                        if (splitText.Count > 1)
                        {
                            b = 0;
                            args = new RichTextPanelEventArgs(currentTextBlock) { Created = null, Index = b };
                            RaiseEvent(TextBlockSplit, this, args);

                            if (!args.Cancel)
                            {
                                currentTextBlock.Text = splitText[0];

                                args = new RichTextPanelEventArgs(currentTextBlock) { Created = currentTextBlock, Index = b };
                                RaiseEvent(TextBlockSplit, this, args);

                                for (a = 1; a < splitText.Count; a++)
                                {
                                    next = CreateTextBlock(splitText[a]);
                                    args = new RichTextPanelEventArgs(currentTextBlock) { Created = next, Index = b };
                                    RaiseEvent(TextBlockSplit, this, args);

                                    if (!args.Cancel)
                                    {
                                        ContentChildren.Insert(i + a, next);
                                        b += splitText[a].Length;
                                    }
                                }
                            }
                            if (currentTextBlock.Text.Length == 0)
                            {
                                pos.X = 99999;
                            }
                        }
                        updateNextLine = true;
                        lineOut = 1;
                    }
                    thisWidth = currentTextBlock.ContentWidth;
                }

                pos.X += thisWidth;

                if (current is Newline)
                {
                    createBlankTextBlock = (i== 0);
                    if (i > 0)
                    {
                        if (ContentChildren[i - 1] is TextBlockPlus)
                        {
                            currentTextBlock = (TextBlockPlus)ContentChildren[i - 1];
                            if (current.Tag != currentTextBlock.Tag)
                            {
                                RaiseEvent(ApplyFormatting, this, new RichTextPanelEventArgs(currentTextBlock) { Created = current });
                            }
                        }
                        else
                        {
                            createBlankTextBlock = true;
                        }
                    }

                    if (createBlankTextBlock)
                    {
                        next = CreateTextBlock("");
                        args = new RichTextPanelEventArgs(current) { Created = next };
                        RaiseEvent(TextBlockSplit, this, args);

                        if (!args.Cancel)
                        {
                            next.SetValue(Canvas.LeftProperty, pos.X - thisWidth);
                            next.SetValue(Canvas.TopProperty, pos.Y);
                            ContentChildren.Insert(i, next);
                            i++;
                        }
                    }
                    newlineStarted = true;
                    pos.Y += UpdateRowContent(startIndex, i, ignore);
                    i -= _removed;
                    pos.X = 0;
                    startIndex = i + 1;
                }
                else
                {
                    if (pos.X > ActualWidth)
                    {
                        newlineStarted = true;
                        pos.Y += UpdateRowContent(startIndex, i - 1, ignore);
                        i -= _removed;
                        if (i < 0)
                        {
                            break;
                        }
                        pos.X = 0;

                        current.SetValue(Canvas.LeftProperty, pos.X);
                        current.SetValue(Canvas.TopProperty, pos.Y);

                        pos.X = thisWidth;
                        startIndex = i;
                    }
                }

                if (newlineStarted && i > rowEndIndex)
                {
                    if (!updateAll && !updateNextLine)
                    {
                        if (lineOut <= 0)
                        {
                            break;
                        }
                        lineOut--;
                    }

                    updateNextLine = false;
                }
                else if (i == ContentChildren.Count - 1 && startIndex < i)
                {
                    UpdateRowContent(startIndex, i, ignore);
                    i -= _removed;
                }
            }

            temp = GetRowForElementIndex(i);
            endRow = (i == ContentChildren.Count && _rows.Count > 0 ? _rows.Count - 1 : _rows.IndexOf(temp));

            if (endRow >= 0)
            {
                _rows.RemoveRange(startRow, (endRow - startRow) + 1);
            }

            _rows.InsertRange(startRow, _tempRows);
            a = startRow + _tempRows.Count;

            if (endRow >= 0 && endRow < _rows.Count - 1 && a < _rows.Count)
            {
                offset = (_rows[a - 1].Position.Y + _rows[a - 1].Dimensions.Y) - _rows[a].Position.Y;
                startIndex = ContentChildren.IndexOf(_rows[a].Start);

                if (offset != 0)
                {
                    // Reposition following rows if the size of the modified rows has changed
                    for (i = a; i < _rows.Count; i++)
                    {
                        _rows[i].Position = new Point(_rows[i].Position.X, _rows[i].Position.Y + offset);
                    }
                    for (i = startIndex; i < ContentChildren.Count; i++)
                    {
                        ContentChildren[i].SetValue(Canvas.TopProperty, (double)ContentChildren[i].GetValue(Canvas.TopProperty) + offset);
                    }
                }
            }

            if (Rows.Count > 0)
            {
                CalculateRowDimensions(Rows[Rows.Count - 1], false);
                SetElementAlignment(Rows[Rows.Count - 1]);
            }

            _lastUpdatedSize = ActualWidth;
            UpdateCanvasSize();
            RaiseEvent(Updated, this, null);
        }

        #endregion

        #region Public Methods

        public void Insert(int index, UIElement value)
        {
            RichTextPanelRow insertRow;
            UIElement currentElement;

            if (_disableUpdates)
            {
                ContentChildren.Insert(index, value);
            }
            else
            {
                insertRow = GetRowForElementIndex(index);
                if (insertRow != null)
                {
                    currentElement = ContentChildren[index];
                    ContentChildren.Insert(index, value);
                    if (currentElement == insertRow.Start)
                    {
                        insertRow.Start = value;
                    }
                }
                else if (index >= 0)
                {
                    ContentChildren.Insert(index, value);
                }
            }
        }

        /// <summary>
        /// Removes the provided element from the panel
        /// </summary>
        /// <param name="element">Element to remove</param>
        public UIElement Remove(UIElement element)
        {
            RichTextPanelRow insertRow = GetRowForElement(element);

            if (insertRow == null)
            {
                return null;
            }

            List<UIElement> children = insertRow.GetChildren(_contentChildren.Children);

            if (children.Count > 1)
            {
                if (children[0] == element)
                {
                    insertRow.Start = children[1];
                }
            }

            ContentChildren.Remove(element);
            return insertRow.Start;
        }

        public void Clear()
        {
            for (int i = ContentChildren.Count - 1; i >= 0; i--)
            {
                ContentChildren.RemoveAt(i);
            }
        }

        /// <summary>
        /// Gets the element at the provided position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Element at the position or null</returns>
        public UIElement GetRowElementAtPosition(Point position, RichTextPanelRow row)
        {
            UIElement result = null;
            int startIndex = ContentChildren.IndexOf(row.Start);
            int endIndex = ContentChildren.IndexOf(row.End);
            double x;
            double width;
            int i;

            if (startIndex >= 0)
            {
                for (i = startIndex; i <= endIndex; i++)
                {
                    x = (double)ContentChildren[i].GetValue(Canvas.LeftProperty);
                    width = (double)ContentChildren[i].GetValue(Canvas.WidthProperty);

                    if (ContentChildren[i] is TextBlockPlus)
                    {
                        width = ((TextBlockPlus)ContentChildren[i]).ContentWidth;
                    }
                    else
                    {
                        width = (double)ContentChildren[i].GetValue(Canvas.ActualWidthProperty);
                    }

                    if (position.X >= x && position.X <= x + width)
                    {
                        result = ContentChildren[i];
                        break;
                    }
                }

                if (result == null)
                {
                    x = (double)ContentChildren[startIndex].GetValue(Canvas.LeftProperty);
                    result = (position.X < x ? ContentChildren[startIndex] : ContentChildren[endIndex]);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the row at the provided position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Element at the position or null</returns>
        public RichTextPanelRow GetRowAtPosition(Point position)
        {
            foreach (RichTextPanelRow row in _rows)
            {
                if ((position.Y >= row.Position.Y) && (position.Y < (row.Position.Y + row.Dimensions.Y)))
                {
                    return row;
                }
            }
            return null;

        }

        public int GetRowIndexForElement(UIElement element)
        {
            RichTextPanelRow row = GetRowForElement(element);

            return _rows.IndexOf(row);
        }

        /// <summary>
        /// Gets the row for a given element
        /// </summary>
        /// <param name="element">Element</param>
        /// <returns>RichTextPanelRow</returns>
        public RichTextPanelRow GetRowForElement(UIElement element)
        {
            int index = ContentChildren.IndexOf(element);

            return GetRowForElementIndex(index);
        }

        /// <summary>
        /// Compacts the text elements of a row
        /// </summary>
        /// <param name="element">Element whose row should be compacted</param>
        public void CompactRow(UIElement element)
        {
            CompactRow(GetRowForElement(element), null);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Merges TextBlock elements with the same style name (Tag property)
        /// </summary>
        /// <param name="row">Row to compact</param>
        /// <param name="ignore">Element to ignore when compacting the row</param>
        private void CompactRow(RichTextPanelRow row, UIElement ignore)
        {
            _removed = 0;

            if (row == null)
            {
                return;
            }

            int startIndex = ContentChildren.IndexOf(row.Start);
            int endIndex = ContentChildren.IndexOf(row.End);
            RichTextPanelEventArgs args;
            TextBlockPlus current;
            TextBlockPlus next;
            bool tagMatched = false;
            int i;

            if (startIndex < 0 || endIndex < 0)
            {
                return;
            }

            for (i = endIndex - 1; i >= startIndex; i--)
            {
                if (ContentChildren[i] is TextBlockPlus && ContentChildren[i] != ignore)
                {
                    current = (TextBlockPlus)ContentChildren[i];
                    if (ContentChildren[i + 1] is TextBlockPlus && ContentChildren[i + 1] != ignore)
                    {
                        next = (TextBlockPlus)ContentChildren[i + 1];
                        tagMatched = false;

                        if (current.Tag is RichTextTag && next.Tag is RichTextTag)
                        {
                            if (((RichTextTag)current.Tag).CompareTo(next.Tag) == 0)
                            {
                                if (current.Text.Length > 0)
                                {
                                    args = new RichTextPanelEventArgs(current) { Created = next, Index = current.Text.Length };

                                    current.Text += next.Text;
                                    RaiseEvent(TextBlockMerge, this, args);

                                    if (!args.Cancel)
                                    {
                                        ContentChildren.Remove(next);
                                        if (row.End == next)
                                        {
                                            row.End = current;
                                        }
                                        _removed++;
                                    }
                                }
                                else
                                {
                                    ContentChildren.Remove(current);
                                    if (row.End == current)
                                    {
                                        row.End = next;
                                    }
                                    if (row.Start == current)
                                    {
                                        row.Start = next;
                                    }
                                    _removed++;
                                }

                                tagMatched = true;
                            }
                        }

                        if (!tagMatched && current.Text.Length == 0)
                        {
                            ContentChildren.Remove(current);
                            if (row.Start == current)
                            {
                                row.Start = next;
                            }
                            _removed++;
                        }
                    }
                }
            }
        }

        private double UpdateRowContent(int startIndex, int endIndex, UIElement ignore)
        {
            if (startIndex >= 0 && endIndex >= 0)
            {
                _tempRows.Add(new RichTextPanelRow(ContentChildren[startIndex], ContentChildren[endIndex]));
                CompactRow(_tempRows[_tempRows.Count - 1], ignore);
                CalculateRowDimensions(_tempRows[_tempRows.Count - 1], true);

                return SetElementAlignment(_tempRows[_tempRows.Count - 1]);
            }

            return 0;
        }

        /// <summary>
        /// Gets the row the element is located in using the 'Quick Search' method
        /// </summary>
        /// <param name="index">Row index</param>
        /// <returns>RichTextPanelRow object</returns>
        private RichTextPanelRow GetRowForElementIndex(int index)
        {
            foreach (RichTextPanelRow row in _rows)
            {
                int startIndex = ContentChildren.IndexOf(row.Start);
                int endIndex = ContentChildren.IndexOf(row.End);
                if ((index >= startIndex) && (index <= endIndex))
                {
                    return row;
                }
            }
            return null;
        }

        /// <summary>
        /// Positions each row element based on it's alignment properties
        /// </summary>
        /// <param name="row">Row index</param>
        /// <returns>Height of the row</returns>
        private double SetElementAlignment(RichTextPanelRow row)
        {
            Point currentDims = new Point();
            FrameworkElement current;
            Point position = new Point();
            int startIndex = ContentChildren.IndexOf(row.Start);
            int endIndex = ContentChildren.IndexOf(row.End);
            double baseX = 0;
            int i;

            if (startIndex < 0)
            {
                return 0;
            }

            for (i = startIndex; i <= endIndex; i++)
            {
                current = (FrameworkElement)ContentChildren[i];
                currentDims.X = (current is TextBlockPlus ? ((TextBlockPlus)current).ContentWidth : (double.IsNaN(current.Width) ? current.ActualWidth : current.Width));
                currentDims.Y = (current is TextBlockPlus ? ((TextBlockPlus)current).ContentHeight : (double.IsNaN(current.Height) ? current.ActualHeight : current.Height));

                if (current is Bullet)
                {
                    currentDims.X = ((Bullet)current).IndentWidth;
                }

                switch (current.VerticalAlignment)
                {
                    case VerticalAlignment.Top:
                        position.Y = row.Position.Y;
                        break;
                    case VerticalAlignment.Center:
                        position.Y = row.Position.Y + ((row.Dimensions.Y - (currentDims.Y + row.Margin.Top + row.Margin.Bottom)) * 0.5);
                        break;
                    default:
                        position.Y = ((row.Position.Y + (row.Dimensions.Y - row.Margin.Bottom)) - currentDims.Y) - row.Margin.Top;
                        break;
                }

                if (row.Margin.Top != current.Margin.Top)
                {
                    position.Y -= current.Margin.Top;
                }

                switch (((FrameworkElement)ContentChildren[startIndex]).HorizontalAlignment)
                {
                    case HorizontalAlignment.Center:
                        baseX = (ActualWidth - row.Dimensions.X) * 0.5;
                        break;
                    case HorizontalAlignment.Right:
                        baseX = ActualWidth - row.Dimensions.X;
                        break;
                    default:
                        baseX = 0;
                        break;
                }

                current.SetValue(Canvas.LeftProperty, baseX + position.X);
                position.X += currentDims.X;

                current.SetValue(Canvas.TopProperty, position.Y);
            }

            row.Position = new Point(baseX, row.Position.Y);

            return row.Dimensions.Y;
        }

        private TextBlockPlus CreateTextBlock(string text)
        {
            TextBlockPlus tb = new TextBlockPlus();

            tb.VerticalAlignment = VerticalAlignment.Bottom;
            tb.Text = text;
            tb.Tag = new RichTextTag(RichTextBlock.DefaultStyle);

            return tb;
        }

        /// <summary>
        /// Splits a provided text string and returns a collection
        /// </summary>
        /// <param name="template">A textblock template used to determine the dimensions of the characters</param>
        /// <param name="text">The text string to split</param>
        /// <param name="x">A starting x position</param>
        /// <param name="width">A width to split the text on</param>
        /// <returns>A list of text strings</returns>
        private List<string> SplitText(TextBlockPlus template, string text, double x, double width)
        {
            List<string> results = new List<string>();
            string temp = template.Text;
            double currentX = x;
            int lineIndex = 0;
            int indexOfLastWord = 0;
            int charsRemaining = text.Length;
            int i = 0;
            char c;

            if (text.Length < 2 || width < 20)
            {
                results.Add(text);
                return results;
            }

            while (i < text.Length)
            {
                c = text[i];

                template.Text = c.ToString();
                currentX += template.ContentWidth;

                if (currentX > width)
                {
                    if (indexOfLastWord == lineIndex)
                    {
                        if (indexOfLastWord == 0 && x > 1)
                        {
                            indexOfLastWord = text.Length;
                            results.Add("");
                        }
                        else
                        {
                            indexOfLastWord = i - 1;
                        }
                    }

                    results.Add(text.Substring(lineIndex, indexOfLastWord - lineIndex));
                    charsRemaining -= indexOfLastWord - lineIndex;

                    if (i != indexOfLastWord)
                    {
                        i = indexOfLastWord;
                    }
                    lineIndex = i;
                    currentX = 0;
                    continue;
                }

                if (c == ' ' || c == '-')
                {
                    indexOfLastWord = i + 1;
                }
                i++;
            }

            if (charsRemaining > 0)
            {
                results.Add(text.Substring(text.Length - charsRemaining, charsRemaining));
            }

            template.Text = temp;
            /*
            if (results.Count > 1)
            {
                if (results[0].Length == 0)
                {
                    results.RemoveAt(0);
                }
            }*/

            return results;
        }

        /// <summary>
        /// Recalculates the size of the canvas
        /// </summary>
        private void UpdateCanvasSize()
        {
            double newHeight = (_rows.Count > 0 ? _rows[_rows.Count - 1].Position.Y + _rows[_rows.Count - 1].Dimensions.Y : 0);

            if (newHeight > 0)
            {
                Height = newHeight;
            }
        }

        /// <summary>
        /// Calculates the size of a given row
        /// </summary>
        /// <param name="row">Row index</param>
        private double CalculateRowDimensions(RichTextPanelRow row, bool updatePosition)
        {
            int startIndex = ContentChildren.IndexOf(row.Start);
            int endIndex = ContentChildren.IndexOf(row.End);
            int rowIndex = Rows.IndexOf(row);
            double tempY;
            double tempX;
            Point dims = new Point();
            double topY = 0;
            FrameworkElement element;
            TextBlockPlus currentTextBlock;
            double marginTop = 0;
            double marginBottom = 0;
            int i;

            if (startIndex < 0)
            {
                return 0;
            }

            for (i = startIndex; i <= endIndex; i++)
            {
                element = (FrameworkElement)ContentChildren[i];
                if (element is TextBlockPlus)
                {
                    currentTextBlock = (TextBlockPlus)element;

                    if (currentTextBlock.Text.Length == 0)
                    {
                        currentTextBlock.Text = "I";
                        tempY = currentTextBlock.ContentHeight;
                        currentTextBlock.Text = "";
                    }
                    else
                    {
                        tempY = currentTextBlock.ContentHeight;
                    }
                    tempY *= currentTextBlock.LineHeightMultiplier;
                    tempX = currentTextBlock.ContentWidth;
                }
                else
                {
                    tempY = (double.IsNaN(element.Height) ? element.ActualHeight : element.Height);
                    tempX = (double.IsNaN(element.Width) ? element.ActualWidth : element.Width);

                    if (element is Bullet)
                    {
                        tempX = ((Bullet)element).IndentWidth;
                    }
                }

                if (element.Margin.Top > marginTop)
                {
                    marginTop = element.Margin.Top;
                }
                if (element.Margin.Bottom > marginBottom)
                {
                    marginBottom = element.Margin.Bottom;
                }

                if (tempY > dims.Y)
                {
                    dims.Y = tempY;
                }
                dims.X += tempX;

                tempY = (double)element.GetValue(Canvas.TopProperty);
                if (tempY > topY)
                {
                    topY = tempY;
                }
            }

            if (!(row.End is Newline) || (rowIndex >= 0 && rowIndex == Rows.Count - 1))
            {
                marginBottom = 0;
            }
            if (startIndex > 0)
            {
                if (!(ContentChildren[startIndex - 1] is Newline))
                {
                    marginTop = 0;
                }
            }

            row.Margin = new Thickness(0, marginTop, 0, marginBottom);
            row.Dimensions = new Point(dims.X, dims.Y + marginTop + marginBottom);

            if (updatePosition)
            {
                row.Position = new Point(0, topY);
            }

            return row.Dimensions.Y;
        }

        /// <summary>
        /// Updates the size of the control
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != _lastUpdatedSize)
            {
                _contentChildren.Width = e.NewSize.Width;
                _contentChildren.Height = e.NewSize.Height;

                Update();
            }
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates the specified event
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected virtual void RaiseEvent(RichTextPanelEventHandler handler, object sender, RichTextPanelEventArgs args)
        {
            if (handler != null)
            {
                handler(sender, args);
            }
        }

        #endregion
    }
}
