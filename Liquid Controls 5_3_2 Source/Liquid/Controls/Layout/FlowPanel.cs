using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Liquid
{
    /// <summary>
    /// A Panel whose content flows from left-to-right
    /// </summary>
    public partial class FlowPanel : Canvas
    {
        #region Private Properties

        private bool _disableUpdates = false;
        private bool _initialUpdate = false;

        #endregion

        #region Public Properties

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

        #endregion

        #region Public Events

        public event EventHandler Updated;

        #endregion

        #region Constructor

        public FlowPanel()
        {
            this.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Re-positions all the elements, wrapping as necessary
        /// </summary>
        internal void Update()
        {
            Point pos = new Point();
            double thisWidth = 0;
            double thisHeight = 0;
            FrameworkElement current;
            TextBlock currentTextBlock;
            double lineHeight = 0;
            double totalHeight = 0;
            int i;

            for (i = 0; i < Children.Count; i++)
            {
                current = (FrameworkElement)Children[i];
                if (current.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                current.SetValue(Canvas.LeftProperty, pos.X + current.Margin.Left);
                current.SetValue(Canvas.TopProperty, pos.Y + current.Margin.Top);

                if (!(current is TextBlock))
                {
                    thisWidth = (double.IsNaN(current.Width) ? current.ActualWidth : current.Width);
                    thisHeight = (double.IsNaN(current.Height) ? current.ActualHeight : current.Height);
                }
                else
                {
                    currentTextBlock = (TextBlock)current;
                    thisWidth = currentTextBlock.ActualWidth;
                    thisHeight = currentTextBlock.ActualHeight;
                }

                thisWidth += current.Margin.Left + current.Margin.Right;

                pos.X += thisWidth;

                if (pos.X > ActualWidth)
                {
                    totalHeight += lineHeight;
                    pos.Y += lineHeight;
                    pos.X = 0;
                    lineHeight = 0;

                    current.SetValue(Canvas.LeftProperty, pos.X + current.Margin.Left);
                    current.SetValue(Canvas.TopProperty, pos.Y + current.Margin.Top);

                    pos.X = thisWidth;
                }
                if (thisHeight > lineHeight)
                {
                    lineHeight = thisHeight;
                }
            }

            Height = totalHeight + lineHeight;
            RaiseUpdated(this, null);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds an element to the panel
        /// </summary>
        /// <param name="element">Element to add</param>
        public void Add(UIElement element)
        {
            Children.Add(element);
            if (!_disableUpdates)
            {
                Update();
            }
        }

        /// <summary>
        /// Adds a range of elements to the panel
        /// </summary>
        /// <param name="elements">Element range to add</param>
        public void Add(IEnumerable<UIElement> elements)
        {
            foreach (UIElement element in elements)
            {
                Children.Add(element);
            }

            if (!_disableUpdates)
            {
                Update();
            }
        }

        public void Insert(int index, UIElement value)
        {
            Children.Insert(index, value);
            if (!_disableUpdates)
            {
                Update();
            }
        }

        /// <summary>
        /// Removes the provided element from the panel
        /// </summary>
        /// <param name="element">Element to remove</param>
        public void Remove(UIElement element)
        {
            Children.RemoveAt(Children.IndexOf(element));
        }

        /// <summary>
        /// Removes the element at a given index
        /// </summary>
        /// <param name="index">Element index</param>
        public void RemoveAt(int index)
        {
            Children.RemoveAt(index);
            if (!_disableUpdates)
            {
                Update();
            }
        }

        /// <summary>
        /// Recalculates the child element positions
        /// </summary>
        public void Refresh()
        {
            Update();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the size of the control
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != e.PreviousSize.Width)
            {
                Update();
            }
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a Updated event to indicate the panel has been modified
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected virtual void RaiseUpdated(object sender, EventArgs args)
        {
            if (Updated != null)
            {
                Updated(sender, args);
            }
        }

        #endregion
    }
}
