using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Liquid
{
    #region Delegates

    public delegate void TreeEventHandler(object sender, TreeEventArgs e);

    #endregion

    /// <summary>
    /// An expandable treeview control
    /// </summary>
    [ContentPropertyAttribute("Nodes")]
    public partial class Tree : Node
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
        internal Canvas ElementDraggingCanvas { get; set; }
        internal const string ElementDraggingCanvasName = "ElementDraggingCanvas";

        /// <summary> 
        /// Place Holder template.
        /// </summary>
        internal Border ElementPlaceHolder { get; set; }
        internal const string ElementPlaceHolderName = "ElementPlaceHolder";

        /// <summary> 
        /// Drag and Drop indicator line template.
        /// </summary>
        internal Rectangle ElementDropLine { get; set; }
        internal const string ElementDropLineName = "ElementDropLine";


        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the visibility of the vertical scrollbar
        /// </summary>
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(Tree), null);
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)this.GetValue(VerticalScrollBarVisibilityProperty); }
            set { base.SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Gets or sets the visibility of the vertical scrollbar
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(Tree), null);
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)this.GetValue(HorizontalScrollBarVisibilityProperty); }
            set { base.SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        #endregion

        #region Private Properties

        private DispatcherTimer _timer = new DispatcherTimer();
        private int _numberOfTicksToExpandOnHover = 30;

        private Node _selected = null;
        private List<Node> _selectedItems = new List<Node>();
        private Node _mousedOver = null;
        private Node _dragging = null;
        private Node _underDrag = null;
        private bool _underDragAfter = false;
        private bool _overIcon = false;

        private Point _dragStart = new Point();
        private Point _drag = new Point();

        private Point _lastMousePosition = new Point();
        private int _framesSinceLastMouseMove = 0;
        private bool _enableLines = false;

        private int _ticksSinceLastSelected = 0;
        private Node _lastClicked = null;
        private bool _enableMultiSelect = false;
        private bool _checkBoxHover = false;

        #endregion

        #region Internal Properties

        internal int TicksSinceLastMouseDown = 0;
        internal Node CurrentEditingNode = null;

        #endregion

        #region Public Enumerators

        public enum DropActions
        {
            Automatic,
            AppendAsFirstChild,
            AppendAsLastChild,
            InsertBefore,
            InsertAfter
        }

        public enum SortActions
        {
            Default = 0,
            Descending = 1,
            ContainersFirst = 2,
            ApplyToChildren = 4
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets whether the nodes can be navigated using the keyboard cursor keys
        /// </summary>
        public bool EnableKeyboardNavigation { get; set; }

        /// <summary>
        /// Gets or sets whether nodes when expanded will use the fade in animation
        /// </summary>
        public bool EnableExpandFadeIn { get; set; }

        /// <summary>
        /// Gets or set whether checking a node will cause its chidren to become checked too
        /// </summary>
        public bool ApplyCheckChangesToChildren { get; set; }

        /// <summary>
        /// Gets or sets the number of characters allowed in the node title
        /// </summary>
        public int MaxTitleLength { get; set; }

        /// <summary>
        /// Gets or sets the character string to be appended to the title when its length is more than the MaxTitleLength property
        /// </summary>
        public string TitlePostfix { get; set; }

        /// <summary>
        /// Gets or sets when drag and drop is enabled this is the number of timer ticks before the node under the cursor will automatically expand
        /// </summary>
        public int NumberOfTicksToExpandOnHover { get; set; }

        /// <summary>
        /// Gets or sets whether dragging and dropping is enabled
        /// </summary>
        public bool EnableDragAndDrop { get; set; }

        /// <summary>
        /// Gets or sets whether node title text can be edited
        /// </summary>
        public bool EnableNodeEditing { get; set; }

        /// <summary>
        /// Gets or sets whether you can select more than one node using CTRL and SHIFT
        /// </summary>
        public bool EnableMultiSelect
        {
            get { return _enableMultiSelect; }
            set { ClearSelected(); _enableMultiSelect = value; }
        }

        /// <summary>
        /// Gets or sets whether connecting lines are enabled
        /// </summary>
        public bool EnableLines
        {
            get { return _enableLines; }
            set
            {
                _enableLines = value;
                foreach (Node node in Nodes)
                {
                    node.RecursiveUpdateVisuals();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether checkboxes can be in a state of not checked and not clear
        /// </summary>
        public bool EnablePartialCheckboxChecks { get; set; }

        /// <summary>
        /// Gets or sets the currently selected node
        /// </summary>
        public Node Selected
        {
            get { return _selected; }
            set
            {
                TreeEventArgs args = new TreeEventArgs();

                if (NodeSelectionChange(_selected, value))
                {
                    return;
                }

                ClearSelected();

                _selected = value;
                if (_selected != null)
                {
                    _selected.IsSelected = true;

                    if (_selected.IsSelected)
                    {
                        RaiseEvent(NodeClick, _selected, new TreeEventArgs(_selected.ID) { Target = _selected });
                    }

                    args.Target = _selected;
                    RaiseEvent(SelectionChanged, this, args);
                }
            }
        }

        /// <summary>
        /// Gets the currently selected item list
        /// </summary>
        public List<Node> SelectedItems
        {
            get { return _selectedItems; }
        }

        /// <summary>
        /// Gets or sets the Roundness of the expand icons
        /// </summary>
        public double ExpandRoundness { get; set; }

        /// <summary>
        /// Gets or sets whether the drag and drop marker is shown
        /// </summary>
        public Visibility DropMarker { get; set; }

        /// <summary>
        /// Gets or sets whether the drag and drop node highlighting is shown
        /// </summary>
        public Visibility DragAndDropNodeHighlighting { get; set; }

        #endregion

        #region Public Events

        public event TreeEventHandler NodeClick;
        public event TreeEventHandler NodeDoubleClick;
        public event TreeEventHandler Populate;
        public event TreeEventHandler Drag;
        public event TreeEventHandler Drop;
        public event TreeEventHandler Dropped;
        public event TreeEventHandler SelectionChange;
        public event TreeEventHandler SelectionChanged;
        public event TreeEventHandler NodeIconDoubleClick;

        #endregion Public Events

        #region Constructor

        public Tree()
        {
            DefaultStyleKey = typeof(Tree);
            ApplyCheckChangesToChildren = false;
            ChildrenInheritCheckboxVisibility = true;
            MaxTitleLength = -1;
            TitlePostfix = "...";
            NumberOfTicksToExpandOnHover = 30;
            EnableDragAndDrop = false;

            IsTabStop = true;
            TabNavigation = KeyboardNavigationMode.Once;
            ExpandRoundness = 5;
            DropMarker = Visibility.Visible;
            DragAndDropNodeHighlighting = Visibility.Collapsed;
            Base = this;
            EnableExpandFadeIn = true;
            EnableKeyboardNavigation = true;

            this.PopulateChildren += new TreeEventHandler(RaisePopulate);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Build's the root element of the tree
        /// </summary>
        public void BuildRoot()
        {
            TreeEventArgs args = new TreeEventArgs();
          
            if (ElementContainer != null)
            {
                ElementContainer.ScrollToVerticalOffset(0);
            }
            Clear();

            RaisePopulate(this, args);
        }

        /// <summary>
        /// Gets a list of nodes between two specified nodes using the current view of the tree
        /// </summary>
        /// <param name="start">Start Node</param>
        /// <param name="end">End Node</param>
        /// <returns>The Start Node through to the End Node</returns>
        public List<Node> GetNodes(Node start, Node end)
        {
            List<Node> results = new List<Node>();
            int s = GetNodeIndex(start);
            int e = GetNodeIndex(end);
            Node current = end;
            Node target = start;
            int nextIndex;

            if (e > s)
            {
                current = start;
                target = end;
            }

            results.Add(current);

            while (current != null && current != target)
            {
                if (current.IsExpanded)
                {
                    current = current.Nodes[0];
                    results.Add(current);
                }
                else
                {
                    nextIndex = current.IndexOfNext();
                    if (nextIndex == -1)
                    {
                        while (current != null && current != target)
                        {
                            current = current.ParentNode;
                            if (current != null)
                            {
                                nextIndex = current.IndexOfNext();

                                if (nextIndex >= 0)
                                {
                                    if (current.ParentNode != null)
                                    {
                                        current = current.ParentNode.Nodes[nextIndex];
                                    }
                                    else
                                    {
                                        current = Nodes[nextIndex];
                                    }
                                    results.Add(current);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        current = current.ParentNode.Nodes[nextIndex];
                        results.Add(current);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Selects a group of nodes
        /// </summary>
        /// <param name="nodes">Nodes to select</param>
        public void SetSelected(List<Node> nodes)
        {
            if (nodes.Count > 0)
            {
                AddSelectedNodes(nodes);
                SetSelected(nodes[0]);
            }
        }

        /// <summary>
        /// Sets the selected node
        /// </summary>
        /// <param name="node">Node to select</param>
        public void SetSelected(Node node)
        {
            if (node != null)
            {
                node.SetHoverState(Visibility.Collapsed);
                Selected = node;
                SelectedItems.Clear();
                SelectedItems.Add(Selected);
            }
        }

        /// <summary>
        /// Sets the selected node and optionally scrolls the node into view
        /// </summary>
        /// <param name="node">Node to select</param>
        /// <param name="scollIntoPosition">Indicates whether the node should be scrolled into view</param>
        public void SetSelected(Node node, bool scollIntoPosition)
        {
            Selected = node;
            node.ScrollIntoPositionAfterFade = true;
        }

        /// <summary>
        /// De-selects the selected nodes
        /// </summary>
        public void ClearSelected()
        {
            foreach (Node node in _selectedItems)
            {
                node.IsSelected = false;
            }
            _selectedItems.Clear();

            if (_selected != null)
            {
                _selected.IsSelected = false;
                if (_selected.IsEditable)
                {
                    //_selected.IsEditable = false;
                }

                _selected = null;
            }
        }

        /// <summary>
        /// Selects the node immediately before the currently selected node
        /// </summary>
        public void SelectPrevious()
        {
            Node newNode = null;
            Node current;
            int temp;

            if (_selected != null)
            {
                temp = _selected.IndexOfPrevious();
                if (temp >= 0)
                {
                    current = _selected.ParentNode.Nodes[temp];

                    while (current != null)
                    {
                        if (current.IsExpanded)
                        {
                            current = current.Nodes[current.Nodes.Count - 1];
                        }
                        else
                        {
                            newNode = current;
                            break;
                        }
                    }
                }
                else if (_selected.ParentNode != this)
                {
                    newNode = _selected.ParentNode;
                }

                if (newNode != null)
                {
                    if (newNode.IsEnabled && !newNode.IsLabel)
                    {
                        Selected = newNode;
                    }
                }

                ScrollIntoPosition();
            }
        }

        /// <summary>
        /// Selects the node after the selected node
        /// </summary>
        public void SelectNext()
        {
            Node newNode = null;
            Node current;
            int temp;

            if (_selected != null)
            {
                if (_selected.IsExpanded)
                {
                    newNode = _selected.Nodes[0];
                }
                else
                {
                    current = _selected;
                    while (current != null)
                    {
                        temp = current.IndexOfNext();
                        if (temp >= 0)
                        {
                            newNode = current.ParentNode.Nodes[temp];
                            break;
                        }
                        current = current.ParentNode;
                    }
                }

                if (newNode != null)
                {
                    if (newNode.IsEnabled && !newNode.IsLabel)
                    {
                        Selected = newNode;
                    }
                }

                ScrollIntoPosition();
            }
        }

        /// <summary>
        /// Scrolls the selected node into view if it is not visible in the ScrollViewer
        /// </summary>
        public void ScrollIntoPosition()
        {
            double lineHeight;
            DependencyObject o;
            Point p;
            double lineWidth = 2;

            if (_selected != null)
            {
                o = VisualTreeHelper.GetParent(_selected);
                lineHeight = _selected.OverallHeight;
                if (o != null)
                {
                    p = _selected.TransformToVisual(ElementChildren).Transform(new Point());
                    ScrollIntoPosition(p, lineWidth, lineHeight);
                    _selected.ScrollIntoPosition = false;
                }
                else
                {
                    _selected.ScrollIntoPosition = true;
                }
            }
        }

        /// <summary>
        /// Gets the node at the supplied point
        /// </summary>
        /// <param name="p">Position to check</param>
        /// <returns>Node at the position or null for no node</returns>
        public Node GetNodeAtPoint(Point p)
        {
            bool after = false;
            bool overIcon = false;

            if (ElementRoot != null)
            {
                p.X += ElementContainer.HorizontalOffset;
                p.Y += ElementContainer.VerticalOffset;
            }

            return GetChildNodeFromAll(p, ElementChildren.Children, null, out after, out overIcon);
        }

        public override string ToString()
        {
            return "TreeView";
        }

        #endregion

        #region Dragging And Dropping

        /// <summary>
        /// Drags the node to a new psoition
        /// </summary>
        /// <param name="node">Node to drag</param>
        /// <param name="position">Position to drag</param>
        private void DragNode(Node node, Point position)
        {
            int index;

            if (node == null)
            {
                return;
            }

            TreeEventArgs args = new TreeEventArgs(node.ID);
            Point pos;

            // Don't allow dragging of the root or a null node
            if (EnableDragAndDrop == true && node != null && Nodes.Contains(node) == false)
            {
                args.Source = node;
                RaiseEvent(Drag, this, args);

                if (args.Cancel == false)
                {
                    // Set the dragging node and calculate its absolute position
                    _dragging = node;

                    node.SetOpacity(0.35);
                    node.HideLines();
                    pos = GetNodeAbsolutePosition(_dragging);

                    // Set the initial dragging values
                    _dragStart.X = (double)_dragging.GetValue(Canvas.LeftProperty);
                    _dragStart.Y = (double)_dragging.GetValue(Canvas.TopProperty);
                    _drag = position;

                    // Work out where the node was originally in the tree and remove it
                    if (_dragging.ParentNode != null)
                    {
                        ElementPlaceHolder.Width = (_dragging.RenderSize.Width > 16 ? _dragging.RenderSize.Width - 16 : 16);
                        ElementPlaceHolder.Height = _dragging.RenderSize.Height;

                        index = _dragging.ParentNode.ElementChildren.Children.IndexOf(_dragging);
                        _dragging.ParentNode.ElementChildren.Children.RemoveAt(index);
                        _dragging.ParentNode.ElementChildren.Children.Insert(index, ElementPlaceHolder);
                    }
                    // And add it to the tree root Canvas
                    // This ensures it is always rendered on top of all other nodes
                    if (ElementDraggingCanvas.Children.IndexOf(_dragging) == -1)
                    {
                        ElementDraggingCanvas.Children.Add(_dragging);
                    }

                    _dragging.SetValue(Canvas.LeftProperty, pos.X);
                    _dragging.SetValue(Canvas.TopProperty, pos.Y);

                    this.CaptureMouse();
                }
            }
        }

        /// <summary>
        /// Drops a node at the supplied position.  A Drop event is raised here to let the application decide where the node will go.
        /// </summary>
        /// <param name="node">Node to drop</param>
        /// <param name="position">Position to drop</param>
        private void DropNode(Node node, Point position)
        {
            if (node == null)
            {
                return;
            }

            TreeEventArgs args = new TreeEventArgs(node.ID);
            DropActions action;
            double mouseX = position.X;
            double mouseY = position.Y;
            Node parent;
            int index;

            if (EnableDragAndDrop == true)
            {
                ElementDropLine.Visibility = Visibility.Collapsed;
                // Get the node under the cursor, excluding the node currently being dragged
                node.SetOpacity(1);

                if (_underDrag != null && _underDrag != node)
                {
                    _underDrag.SetHighlightVisibility(Visibility.Collapsed);

                    args.Source = _dragging;
                    args.Target = _underDrag;
                    args.DropAfter = _underDragAfter;
                    args.DropInto = _overIcon;
                    RaiseEvent(Drop, this, args);

                    if (!args.Cancel)
                    {
                        action = args.DropAction;
                        if (action == DropActions.Automatic)
                        {
                            action = (_underDragAfter ? DropActions.InsertAfter : DropActions.InsertBefore);
                            if (_overIcon)
                            {
                                action = DropActions.AppendAsLastChild;
                            }
                        }

                        if (action == DropActions.AppendAsLastChild || action == DropActions.AppendAsFirstChild)
                        {
                            // The dragging node is above a different node so process the move
                            ElementDraggingCanvas.Children.Remove(node);

                            parent = node.ParentNode;
                            parent.Nodes.Remove(node);
                            // Just add the node as a child to the node we are hovering over
                            if (action == DropActions.AppendAsLastChild)
                            {
                                _underDrag.Nodes.Add(node);
                                index = _underDrag.Nodes.IndexOf(node) - 1;
                                if (index >= 0)
                                {
                                    _underDrag.Nodes[index].UpdateVisuals();
                                }
                            }
                            else
                            {
                                _underDrag.Nodes.Insert(0, node);
                                _underDrag.Nodes[0].UpdateVisuals();
                            }

                            _underDrag.UpdateVisuals();

                            if (parent != null)
                            {
                                parent.UpdateChildCount();
                            }
                            RaiseEvent(Dropped, this, args);
                        }
                        else if (action == DropActions.InsertAfter || action == DropActions.InsertBefore)
                        {
                            // The dragging node is above a different node so process the move
                            ElementDraggingCanvas.Children.Remove(node);

                            // Move the node below the node we are hovering over
                            node.ParentNode.Nodes.Remove(node);
                            node.ParentNode.UpdateChildCount();

                            if (_underDrag.ParentNode != null)
                            {
                                // The node we are dropping onto is not a root node
                                index = _underDrag.ParentNode.Nodes.IndexOf(_underDrag);

                                if (action == DropActions.InsertAfter)
                                {
                                    _underDrag.ParentNode.Nodes.Insert(index + 1, node);
                                }
                                else
                                {
                                    _underDrag.ParentNode.Nodes.Insert(index, node);
                                }
                            }
                            else
                            {
                                // Else we are dropping onto a root node
                                index = Nodes.IndexOf(_underDrag);

                                if (action == DropActions.InsertAfter)
                                {
                                    Nodes.Insert(index + 1, node);
                                }
                                else
                                {
                                    Nodes.Insert(index, node);
                                }
                            }

                            _underDrag.UpdateVisuals();
                            RaiseEvent(Dropped, this, args);
                        }
                        else
                        {
                            CancelDragAndDrop(node);
                        }
                    }
                    else
                    {
                        CancelDragAndDrop(node);
                    }
                }
                else
                {
                    CancelDragAndDrop(node);
                }

                node.ShowLines();
                node.UpdateVisuals();
                this.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// This cancels any drag operation already in progress.  The node is restored to it's original position.
        /// </summary>
        /// <param name="node">Node being dragged</param>
        private void CancelDragAndDrop(Node node)
        {
            if (EnableDragAndDrop == true && node != null)
            {
                // The dragging node is either not above a node or is above itself
                // so restore the dragging node to its original position
                ElementDraggingCanvas.Children.Remove(node);
                node.ParentNode.SyncChildren();
            }
        }

        /// <summary>
        /// Moves a node to the supplied position
        /// </summary>
        /// <param name="node">Node to move</param>
        /// <param name="position">New position</param>
        private void MoveNode(Node node, Point position)
        {
            Point underPos;
            Point testPos;
            Node temp;

            if (node != null)
            {
                testPos = new Point(position.X + ElementContainer.HorizontalOffset, position.Y + ElementContainer.VerticalOffset);
                temp = GetChildNodeFromAll(testPos, ElementChildren.Children, node, out _underDragAfter, out _overIcon);

                if (temp != _underDrag && _underDrag != null)
                {
                    _underDrag.SetHighlightVisibility(Visibility.Collapsed);
                }

                _underDrag = temp;

                if (_underDrag != null)
                {
                    _underDrag.SetHighlightVisibility(Visibility.Visible);

                    underPos = _underDrag.TransformToVisual(ElementChildren).Transform(new Point());
                    ElementDropLine.Visibility = (_overIcon ? Visibility.Collapsed : DropMarker);

                    ElementDropLine.SetValue(Canvas.LeftProperty, underPos.X);
                    ElementDropLine.SetValue(Canvas.TopProperty, underPos.Y + (_underDragAfter ? _underDrag.RenderSize.Height : 0));
                    ElementDropLine.Width = _underDrag.RenderSize.Width;
                }

                // Reset the location of the object.
                _dragging.SetValue(Canvas.LeftProperty, (double)_dragging.GetValue(Canvas.LeftProperty) + (position.X - _drag.X));
                _dragging.SetValue(Canvas.TopProperty, (double)_dragging.GetValue(Canvas.TopProperty) + (position.Y - _drag.Y));

                // Update the beginning position of the mouse.
                _drag.X = position.X;
                _drag.Y = position.Y;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns the child node at the given position
        /// </summary>
        /// <param name="position">The position</param>
        /// <param name="parent">Node whose children should be worked on</param>
        /// <param name="exclude">Optional node to exclude</param>
        /// <returns>Node at the supplied position or null for no node</returns>
        private Node GetChildNode(Point position, Node parent, Node exclude, out bool after, out bool overIcon)
        {
            Point nodePos = new Point();
            Point parentPos;
            Node result = null;
            Node node;

            after = false;
            overIcon = false;

            if (parent == null)
            {
                return null;
            }

            parentPos = GetNodeAbsolutePosition(parent);

            if (position.X >= parentPos.X && position.X <= parentPos.X + parent.RenderSize.Width && position.Y >= parentPos.Y && position.Y <= parentPos.Y + parent.ElementGrid.RenderSize.Height)
            {
                overIcon = parent.IsPointOverIcon(position, parentPos);

                if (!parent.IsExpanded)
                {
                    after = (position.Y > parentPos.Y + (parent.ElementGrid.RenderSize.Height * 0.5));
                }
                return parent;
            }

            if (parent.IsExpanded)
            {
                foreach (UIElement e in parent.ElementChildren.Children)
                {
                    if (e is Node)
                    {
                        node = (Node)e;
                        if (node != exclude)
                        {
                            nodePos = node.TransformToVisual(ElementChildren).Transform(new Point());

                            if (position.X >= nodePos.X && position.X <= nodePos.X + node.RenderSize.Width && position.Y >= nodePos.Y && position.Y <= nodePos.Y + node.ElementGrid.RenderSize.Height)
                            {
                                result = node;
                                overIcon = node.IsPointOverIcon(position, nodePos);

                                if (!node.IsExpanded)
                                {
                                    after = (position.Y > nodePos.Y + (node.ElementGrid.RenderSize.Height * 0.5));
                                }
                            }
                            else if (node.HasChildren && node.IsExpanded)
                            {
                                result = GetChildNode(position, node, exclude, out after, out overIcon);
                            }

                            if (result != null)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the node absolute position
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Absolute position</returns>
        private Point GetNodeAbsolutePosition(Node node)
        {
            return node.TransformToVisual(ElementChildren).Transform(new Point());
        }

        /// <summary>
        /// Iterates through the child nodes looking for a node under the supplied x and y
        /// </summary>
        /// <param name="position">x screen position</param>
        /// <param name="nodes">List of nodes to look</param>
        /// <param name="exclude">Optional node to exclude</param>
        /// <returns>Node at the supplied position or null</returns>
        private Node GetChildNodeFromAll(Point position, UIElementCollection nodes, Node exclude, out bool after, out bool overIcon)
        {
            Node result = null;
            Node n;

            after = false;
            overIcon = false;

            foreach (UIElement e in nodes)
            {
                if (e is Node)
                {
                    n = (Node)e;
                    result = GetChildNode(position, n, exclude, out after, out overIcon);
                    if (result != null)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the CTRL button is pressed
        /// </summary>
        /// <returns>True if it is pressed</returns>
        private bool IsCtrlDown()
        {
            return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        }

        /// <summary>
        /// Determines whether the SHIFT button is pressed
        /// </summary>
        /// <returns>True if it is pressed<</returns>
        private bool IsShiftDown()
        {
            return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
        }

        /// <summary>
        /// Iterates up through the sibling and parent nodes returning the position of the node in the current view of the tree
        /// </summary>
        /// <param name="id">The Node object to retrieve the index for</param>
        /// <returns>Index value</returns>
        private int GetNodeIndex(Node target)
        {
            int result = 0;
            Node current = target;
            int prevIndex;

            while (current != null)
            {
                prevIndex = current.IndexOfPrevious();
                if (prevIndex == -1)
                {
                    current = current.ParentNode;
                }
                else if (current.ParentNode != null)
                {
                    current = current.ParentNode.Nodes[prevIndex];
                    result += GetChildCount(current);
                }
                else
                {
                    current = Nodes[prevIndex];
                    result += GetChildCount(current);
                }
                result++;
            }

            return result;
        }

        /// <summary>
        /// Recursively gets the total child count of a node taking into account the Expanded status of the nodes
        /// </summary>
        /// <param name="node">Node to count its children</param>
        /// <returns>Number of descendants</returns>
        private int GetChildCount(Node node)
        {
            int result = 0;

            foreach (Node n in node.Nodes)
            {
                result++;
                if (n.IsExpanded)
                {
                    result += GetChildCount(n);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all the descendants of a specified node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Collection containing all descendant nodes</returns>
        private List<Node> GetAllChildren(Node node)
        {
            List<Node> result = new List<Node>();

            foreach (Node n in node.Nodes)
            {
                result.Add(n);
                if (n.IsExpanded)
                {
                    result.AddRange(GetAllChildren(n));
                }
            }

            return result;
        }

        /// <summary>
        /// Adds the provided nodes to the selected nodes collection
        /// </summary>
        /// <param name="nodes"></param>
        private void AddSelectedNodes(List<Node> nodes)
        {
            TreeEventArgs args = new TreeEventArgs();
            bool changed = false;

            if (nodes.Count > 0)
            {
                if (Selected == nodes[0])
                {
                    args.Target = nodes[0];
                }
                else
                {
                    args.Target = nodes[nodes.Count - 1];
                }
            }

            RaiseEvent(SelectionChange, this, args);

            if (!args.Cancel)
            {
                foreach (Node node in nodes)
                {
                    if (!_selectedItems.Contains(node) && node.IsEnabled)
                    {
                        _selectedItems.Add(node);
                        node.IsSelected = true;
                        args.Target = node;
                        changed = true;
                    }
                }

                if (changed)
                {
                    RaiseEvent(SelectionChanged, this, args);
                }
            }
        }

        /// <summary>
        /// Scrolls the area control into position
        /// </summary>
        /// <param name="position">The position</param>
        /// <param name="width">Width of the area that must be in view</param>
        /// <param name="height">Height of the area that must be in view</param>
        private void ScrollIntoPosition(Point position, double width, double height)
        {
            if (position.Y + height >= ElementContainer.VerticalOffset + ElementContainer.ViewportHeight)
            {
                ElementContainer.ScrollToVerticalOffset((position.Y + height) - ElementContainer.ViewportHeight);
            }
            else if (position.Y < ElementContainer.VerticalOffset)
            {
                ElementContainer.ScrollToVerticalOffset(position.Y);
            }

            if (position.X + width >= ElementContainer.HorizontalOffset + ElementContainer.ViewportWidth)
            {
                ElementContainer.ScrollToHorizontalOffset((position.X + width) - ElementContainer.ViewportWidth);
            }
            else if (position.X < ElementContainer.HorizontalOffset)
            {
                ElementContainer.ScrollToHorizontalOffset(position.X);
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// This is called when the template has been bound to the control
        /// </summary>
        public override void OnApplyTemplate()
        {
            ElementRoot = (Panel)GetTemplateChild(ElementRootName);
            ElementFocusedState = (UIElement)GetTemplateChild(ElementFocusedStateName);
            ElementContainer = (ScrollViewer)GetTemplateChild(ElementContainerName);
            ElementChildren = (StackPanel)GetTemplateChild(ElementChildrenName);
            ElementDraggingCanvas = (Canvas)GetTemplateChild(ElementDraggingCanvasName);
            ElementPlaceHolder = (Border)GetTemplateChild(ElementPlaceHolderName);
            ElementDropLine = (Rectangle)GetTemplateChild(ElementDropLineName);
            ElementContainer.Padding = new Thickness(1);

            ElementDraggingCanvas.Children.Remove(ElementPlaceHolder);

            this.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
            this.MouseMove += new MouseEventHandler(OnMouseMove);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(OnMouseLeftButtonUp);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            this.MouseWheel += new MouseWheelEventHandler(OnMouseWheel);
            this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(Tree_IsEnabledChanged);

            this.NodeTextMouseDown += new EventHandler(OnTextMouseDown);
            this.NodeIconMouseEnter += new EventHandler(OnNodeIconMouseEnter);
            this.NodeIconMouseLeave += new EventHandler(OnNodeIconMouseLeave);
            this.NodeIconMouseDown += new MouseEventHandler(OnNodeIconMouseDown);
            this.NodeIconMouseUp += new MouseEventHandler(OnNodeIconMouseUp);
            
            SyncChildren();

            _timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            _timer.Tick += Tick;
            _timer.Start();
        }

        private void Tree_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue && _timer.IsEnabled)
            {
                _timer.Stop();
                _timer.Tick -= Tick;
            }
            else if ((bool)e.NewValue && !_timer.IsEnabled)
            {
                _timer.Tick += Tick;
                _timer.Start();
            }
        }

        internal override void SyncChildren()
        {
            base.SyncChildren();

            if (Nodes.Count == 0)
            {
                _selected = null;
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ElementContainer.Width = e.NewSize.Width;
            ElementContainer.Height = e.NewSize.Height;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (e.OriginalSource == this && Nodes.Count > 0)
            {
                //Nodes[0].Focus();
            }

            if (ElementFocusedState != null)
            {
                ElementFocusedState.Visibility = Visibility.Visible;
            }

            /*if (ElementWrapper != null)
            {
                ElementWrapper.IsTabStop = true;

                if (_selected != null)
                {
                    if (!_selected.IsEditable)
                    {
                        if (!_checkBoxHover)
                        {
                            ElementWrapper.Focus();
                        }
                    }
                }
                else
                {
                    if (!_checkBoxHover)
                    {
                        ElementWrapper.Focus();
                    }
                }
            }*/
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (ElementFocusedState != null)
            {
                ElementFocusedState.Visibility = Visibility.Collapsed;
            }
        }

        private bool NodeSelectionChange(Node oldNode, Node newNode)
        {
            TreeEventArgs args = new TreeEventArgs();

            if (oldNode != newNode)
            {
                args.Source = oldNode;
                args.Target = newNode;

                RaiseEvent(SelectionChange, this, args);
            }

            return args.Cancel;
        }

        /// <summary>
        /// Is called when the mouse is pressed over the node text
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected void OnTextMouseDown(object sender, EventArgs e)
        {
            Node newNode = (Node)sender;
            TreeEventArgs args;

            args = new TreeEventArgs(newNode.ID);
            args.Source = newNode;
            args.Target = _selected;

            if (newNode == _lastClicked && _ticksSinceLastSelected < 15)
            {	// Double-click
                RaiseEvent(NodeDoubleClick, sender, args);
            }
            else
            {
                RaiseEvent(NodeClick, sender, args);
            }

            if (!args.Cancel)
            {
                if (_selected != newNode || EnableMultiSelect)
                {
                    if (EnableMultiSelect && _selected != null)
                    {
                        if (IsCtrlDown())
                        {
                            if (_selectedItems.Contains(newNode))
                            {
                                RaiseEvent(SelectionChange, this, args);
                                if (!args.Cancel)
                                {
                                    newNode.IsSelected = false;
                                    _selectedItems.Remove(newNode);
                                    RaiseEvent(SelectionChanged, sender, args);
                                }
                            }
                            else
                            {
                                RaiseEvent(SelectionChange, this, args);
                                if (!args.Cancel)
                                {
                                    newNode.IsSelected = true;
                                    _selectedItems.Add(newNode);
                                    RaiseEvent(SelectionChanged, sender, args);
                                }
                            }
                        }
                        else if (IsShiftDown())
                        {
                            AddSelectedNodes(GetNodes(_selected, newNode));
                        }
                        else
                        {
                            if (NodeSelectionChange(_selected, newNode))
                            {
                                return;
                            }

                            ClearSelected();
                            newNode.IsSelected = true;

                            _selected = newNode;
                            _selectedItems.Add(newNode);
                            RaiseEvent(SelectionChanged, this, args);
                        }
                    }
                    else
                    {
                        if (NodeSelectionChange(_selected, newNode))
                        {
                            return;
                        }

                        ClearSelected();

                        _selected = newNode;
                        _selected.IsSelected = true;
                        _selectedItems.Add(newNode);

                        RaiseEvent(SelectionChanged, this, args);
                    }
                }
            }

            _lastClicked = newNode;
            _ticksSinceLastSelected = 0;
        }

        /// <summary>
        /// Is called when the mouse enters the nodes icon
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected void OnNodeIconMouseEnter(object sender, EventArgs e)
        {
            _mousedOver = (Node)sender;
        }

        /// <summary>
        /// Is called when the mouse leaves the nodes icon
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected void OnNodeIconMouseLeave(object sender, EventArgs e)
        {
            _mousedOver = null;
        }

        /// <summary>
        /// Is called when the node icon is clicked
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void OnNodeIconMouseDown(object sender, MouseEventArgs e)
        {
            _nodeIconDown = true;
            _nodeIconDownPos = e.GetPosition(this);
        }

        private void OnNodeIconMouseUp(object sender, MouseEventArgs e)
        {
            _nodeIconDown = false;
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ElementContainer != null)
            {
                ElementContainer.ScrollToVerticalOffset(ElementContainer.VerticalOffset + -e.Delta);
                e.Handled = true;
            }
        }

        private bool _nodeIconDown = false;
        private Point _nodeIconDownPos = new Point();

        /// <summary>
        /// Is called when the mouse button is pressed
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled)
            {
                try
                {
                    Focus();
                    _nodeIconDown = true;
                    _nodeIconDownPos = e.GetPosition(this);
                }
                catch (Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// Is called when the mouse button is released
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled)
            {
                DropNode(_dragging, e.GetPosition(this));
                _dragging = null;
            }
        }

        /// <summary>
        /// Is called when the mouse moves over the node
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (IsEnabled)
            {
                if (_nodeIconDown)
                {
                    _nodeIconDown = false;

                    try
                    {
                        DragNode(_mousedOver, _nodeIconDownPos);
                    }
                    catch
                    {
                    }
                }

                _lastMousePosition = e.GetPosition(this);

                MoveNode(_dragging, _lastMousePosition);
                _framesSinceLastMouseMove = 0;
            }
        }

        /// <summary>
        /// This event is called when the keyboard has been pressed and the dropdown has focus
        /// </summary>
        /// <param name="e">Event arguments</param>
        internal void NodeKeyDown(KeyEventArgs e)
        {
            if (!EnableKeyboardNavigation)
            {
                return;
            }

            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.F2)
            {
                if (_selected == null && Nodes.Count > 0)
                {
                    SetSelected(Nodes[0]);
                    return;
                }
            }

            if (_selected != null)
            {
                switch (e.Key)
                {
                    case Key.Down:
                        SelectNext();
                        break;
                    case Key.Up:
                        SelectPrevious();
                        break;
                    case Key.Right:
                        if (_selected != null)
                        {
                            if (_selected.HasChildren)
                            {
                                if (!_selected.IsExpanded)
                                {
                                    _selected.Expand();
                                }
                                else
                                {
                                    Selected = _selected.Nodes[0];
                                }
                            }
                            ScrollIntoPosition();
                        }
                        break;
                    case Key.Left:
                        if (_selected != null)
                        {
                            if (!_selected.HasChildren || !_selected.IsExpanded)
                            {
                                if (_selected.ParentNode != null)
                                {
                                    Selected = _selected.ParentNode;
                                }
                            }
                            else
                            {
                                _selected.Collapse();
                            }
                            ScrollIntoPosition();
                        }
                        break;
                    case Key.F2:
                        if (EnableNodeEditing)
                        {
                            _selected.IsEditable ^= true;
                        }
                        break;
                    case Key.Enter:
                        if (EnableNodeEditing)
                        {
                            _selected.IsEditable = false;
                        }
                        break;
                    case Key.Escape:
                        if (EnableNodeEditing)
                        {
                            _selected.EditedTitle = _selected.Title;
                            _selected.IsEditable = false;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// This event is called periodically to expand a node when dragging and hovering above it
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected void Tick(object sender, EventArgs e)
        {
            double mouseX = _lastMousePosition.X;
            double mouseY = _lastMousePosition.Y;
            double temp;
            double nodeMoveStep;

            TicksSinceLastMouseDown++;

            if (_dragging != null)
            {
                nodeMoveStep = 10;

                if (mouseY < 0)
                {
                    temp = ElementContainer.VerticalOffset - nodeMoveStep;
                    if (temp < 0)
                    {
                        temp = 0;
                        nodeMoveStep = 0;
                    }

                    ElementContainer.ScrollToVerticalOffset(temp);
                    MoveNode(_dragging, new Point(_lastMousePosition.X, _lastMousePosition.Y - nodeMoveStep));
                    _drag.Y += nodeMoveStep;
                }
                else if (mouseY > ElementContainer.Height)
                {
                    temp = ElementContainer.VerticalOffset + nodeMoveStep;
                    if (temp > ElementContainer.ScrollableHeight)
                    {
                        temp = ElementContainer.ScrollableHeight;
                        nodeMoveStep = 0;
                    }

                    ElementContainer.ScrollToVerticalOffset(temp);
                    MoveNode(_dragging, new Point(_lastMousePosition.X, _lastMousePosition.Y + nodeMoveStep));
                    _drag.Y -= nodeMoveStep;
                }

                // Automatically expand the node we are hovering over after n ticks
                if (_framesSinceLastMouseMove == _numberOfTicksToExpandOnHover)
                {
                    if (_underDrag != null)
                    {
                        if (_underDrag.IsExpanded == false)
                        {
                            _underDrag.Expand();
                        }
                    }
                }
            }

            _framesSinceLastMouseMove++;

            _ticksSinceLastSelected++;
            if (_ticksSinceLastSelected > 1024)
            {
                _ticksSinceLastSelected = 512;
            }
        }


        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a NodeIconDoubleClick event
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        internal void RaiseNodeIconDoubleClick(object sender, TreeEventArgs args)
        {
            if (NodeIconDoubleClick != null)
            {
                NodeIconDoubleClick(sender, args);
            }
        }

        /// <summary>
        /// Generates the provided event
        /// </summary>
        /// <param name="handler">Event handler to raise</param>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected void RaiseEvent(TreeEventHandler handler, object sender, TreeEventArgs args)
        {
            if (handler != null)
            {
                handler(sender, args);
            }
        }

        /// <summary>
        /// Generates a Populate event to indicate a node needs to be populated.  This event occurs when it is expanded for the first time only.
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaisePopulate(object sender, TreeEventArgs args)
        {
            if (Populate != null)
            {
                Populate(sender, args);
            }
        }

        #endregion
    }

    #region TreeEventArgs
    /// <summary>
    /// Event arguments for use with tree related events
    /// </summary>
    public partial class TreeEventArgs : EventArgs
    {
        #region Private Properties

        private string _id;
        private Node _source;
        private Node _target;
        private Tree.DropActions _dropAction;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets whether to cancel an operation
        /// </summary>
        public bool Cancel { get; set; }

        public bool DropAfter { get; set; }

        public bool DropInto { get; set; }

        public bool Copy { get; set; }

        /// <summary>
        /// The ID of the node that generated the event
        /// </summary>
        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// The node that generated the event
        /// </summary>
        public Node Source
        {
            get { return _source; }
            set { _source = value; }
        }

        /// <summary>
        /// When dragging and dropping this is the node that is under the dragged node
        /// </summary>
        public Node Target
        {
            get { return _target; }
            set { _target = value; }
        }

        /// <summary>
        /// When dragging and dropping this controls what action is taken when the node is released
        /// </summary>
        public Tree.DropActions DropAction
        {
            get { return _dropAction; }
            set { _dropAction = value; }
        }

        #endregion

        #region Constructor

        public TreeEventArgs()
        {
            _id = string.Empty;
            Copy = false;
        }

        public TreeEventArgs(string id)
        {
            ID = id;
            _source = null;
            _target = null;
            _dropAction = Tree.DropActions.Automatic;
            Copy = false;
        }

        #endregion
    }

    #endregion
}