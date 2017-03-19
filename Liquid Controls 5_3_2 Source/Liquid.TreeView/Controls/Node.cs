using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media;


namespace Liquid
{
    /// <summary>
    /// A tree node object for use with the treeview control
    /// </summary>
    [ContentPropertyAttribute("Nodes")]
    public partial class Node : BaseTreeViewControl
    {
        #region Visual Elements

        /// <summary>
        /// Gets the root Panel object for this control
        /// </summary>
        public Panel VisualRoot
        {
            get { return (Panel)ElementRoot; }
        }

        /// <summary> 
        /// Text template.
        /// </summary>
        internal TextBlock ElementText { get; set; }
        internal const string ElementTextName = "ElementText";

        /// <summary> 
        /// Focused template.
        /// </summary>
        internal UIElement ElementFocusedState { get; set; }
        internal const string ElementFocusedStateName = "ElementFocusedState";

        /// <summary> 
        /// FadeIn template.
        /// </summary>
        internal Storyboard ElementFadeIn { get; set; }
        internal const string ElementFadeInName = "ElementFadeIn";

        /// <summary> 
        /// Background Hover template.
        /// </summary>
        internal Rectangle ElementBackgroundHover { get; set; }
        internal const string ElementBackgroundHoverName = "ElementBackgroundHover";

        /// <summary> 
        /// Highlight template.
        /// </summary>
        internal UIElement ElementHighlight { get; set; }
        internal const string ElementHighlightName = "ElementHighlight";

        /// <summary> 
        /// Background template.
        /// </summary>
        internal Rectangle ElementBackground { get; set; }
        internal const string ElementBackgroundName = "ElementBackground";

        /// <summary> 
        /// Vertical Line template.
        /// </summary>
        internal Line ElementVerticalLine { get; set; }
        internal const string ElementVerticalLineName = "ElementVerticalLine";

        /// <summary> 
        /// Horizontal Line template.
        /// </summary>
        internal Line ElementHorizontalLine { get; set; }
        internal const string ElementHorizontalLineName = "ElementHorizontalLine";

        /// <summary> 
        /// Icon template.
        /// </summary>
        internal ContentControl ElementIcon { get; set; }
        internal const string ElementIconName = "ElementIcon";

        /// <summary> 
        /// Selected Text template.
        /// </summary>
        internal TextBlock ElementSelectedText { get; set; }
        internal const string ElementSelectedTextName = "ElementSelectedText";

        /// <summary> 
        /// Expand template.
        /// </summary>
        internal Expand ElementExpand { get; set; }
        internal const string ElementExpandName = "ElementExpand";

        /// <summary> 
        /// Checkbox template.
        /// </summary>
        internal CheckBox ElementCheckbox { get; set; }
        internal const string ElementCheckboxName = "ElementCheckbox";

        /// <summary> 
        /// Input template.
        /// </summary>
        internal TextBox ElementInput { get; set; }
        internal const string ElementInputName = "ElementInput";

        /// <summary> 
        /// Grid template.
        /// </summary>
        internal Grid ElementGrid { get; set; }
        internal const string ElementGridName = "ElementGrid";

        /// <summary> 
        /// Children template.
        /// </summary>
        internal StackPanel ElementChildren { get; set; }
        internal const string ElementChildrenName = "ElementChildren";

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty ConnectorLinesBrushProperty = DependencyProperty.Register("ConnectorLinesBrush", typeof(Brush), typeof(Node), null);
        public Brush ConnectorLinesBrush
        {
            get { return (Brush)this.GetValue(ConnectorLinesBrushProperty); }
            set { base.SetValue(ConnectorLinesBrushProperty, value); }
        }

        public static readonly DependencyProperty SelectedForegroundProperty = DependencyProperty.Register("SelectedForeground", typeof(Brush), typeof(Node), null);
        public Brush SelectedForeground
        {
            get { return (Brush)this.GetValue(SelectedForegroundProperty); }
            set { base.SetValue(SelectedForegroundProperty, value); }
        }

        public static readonly DependencyProperty IconContentProperty = DependencyProperty.Register("IconContent", typeof(object), typeof(Node), null);
        public object IconContent
        {
            get { return this.GetValue(IconContentProperty); }
            set { base.SetValue(IconContentProperty, value); UpdateVisualState(); }
        }

        public static readonly DependencyProperty IconExpandedContentProperty = DependencyProperty.Register("IconExpandedContent", typeof(object), typeof(Node), null);
        public object IconExpandedContent
        {
            get { return this.GetValue(IconExpandedContentProperty); }
            set { base.SetValue(IconExpandedContentProperty, value); UpdateVisualState(); }
        }

        #endregion

        #region Private Properties

        private string _text = string.Empty;
        private string _icon = string.Empty;
        private string _iconExpanded = string.Empty;

        private Node _parent = null;
        private bool _hasChildren = false;
        private bool _selected = false;
        private bool _showLines = true;

        private int _childrenToLoad = 0;
        private bool _enableCheckboxes = false;
        private bool? _checked = false;
        private bool _expandAll = false;
        private List<Node> _expandPath = null;
        private bool _editable = false;
        private bool _makeEditableOnApplyTemplate = false;
        private object _popup = null;
        private bool _busy = false;

        #endregion

        #region Internal Properties

        internal bool ScrollIntoPosition { get; set; }

        internal bool ScrollIntoPositionAfterFade { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the base Tree control
        /// </summary>
        public Tree Base { get; set; }

        /// <summary>
        /// Gets a collection of child nodes
        /// </summary>
        public ObservableCollection<Node> Nodes { get; set; }

        /// <summary>
        /// Gets or sets whether the node should have a checkbox
        /// </summary>
        public bool EnableCheckboxes
        {
            get { return _enableCheckboxes; }
            set
            {
                _enableCheckboxes = value;
                UpdateVisualState();

                if (ChildrenInheritCheckboxVisibility)
                {
                    foreach (Node node in Nodes)
                    {
                        node.EnableCheckboxes = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the node text is editable
        /// </summary>
        public bool IsEditable
        {
            get { return _editable; }
            set
            {
                TreeEventArgs args = new TreeEventArgs(this.ID);

                if (Base != null)
                {
                    if (Base.EnableNodeEditing)
                    {
                        if (ElementRoot != null)
                        {
                            if (_editable)
                            {
                                args.Source = this;
                                args.Target = this;

                                RaiseNodeEdited(this, args);
                                if (!args.Cancel)
                                {
                                    _text = EditedTitle;
                                    RaiseNodeEditingFinished(this, args);
                                }
                            }
                            ElementCheckbox.Opacity = (value ? 0.4 : 1);
                            ElementIcon.Opacity = (value ? 0.4 : 1);
                        }

                        _editable = value;
                        if (_editable)
                        {
                            Base.CurrentEditingNode = this;
                        }
                        UpdateVisualState();
                    }
                }
                else
                {
                    _makeEditableOnApplyTemplate = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the parent node
        /// </summary>
        public Node ParentNode
        {
            get { return _parent; }
            set { _parent = value; ParentChanged(); }
        }

        /// <summary>
        /// Gets or sets the node text
        /// </summary>
        public string Title
        {
            get { return _text; }
            set
            {
                _text = value;
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Gets or sets the edited node title
        /// </summary>
        public string EditedTitle
        {
            get { return (ElementInput != null ? ElementInput.Text : string.Empty); }
            set
            {
                if (ElementInput != null)
                {
                    ElementInput.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the image URL when the node is not expanded
        /// </summary>
        public string Icon
        {
            get { return _icon; }
            set
            {
                _icon = value; UpdateVisualState();
            }
        }

        /// <summary>
        /// Gets or sets the image URL when the node is expanded
        /// </summary>
        public string IconExpanded
        {
            get { return _iconExpanded; }
            set { _iconExpanded = value; }
        }

        /// <summary>
        /// Gets or sets whether the node is expanded
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets whether the node has children
        /// </summary>
        public bool HasChildren
        {
            get { return (Nodes.Count > 0 ? true : _hasChildren); }
            set
            {
                _hasChildren = value;
                if (!_hasChildren)
                {
                    ChildrenLoaded = false;
                }

                UpdateVisualState();
            }
        }

        /// <summary>
        /// Gets or sets whether the child nodes have been loaded
        /// </summary>
        public bool ChildrenLoaded { get; set; }

        /// <summary>
        /// Gets or sets whether the node is selected
        /// </summary>
        public bool IsSelected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Gets or sets the height of the node
        /// </summary>
        public double OverallHeight { get; set; }

        /// <summary>
        /// Gets or sets the nodes checked status
        /// </summary>
        public bool? IsChecked
        {
            get { return _checked; }
            set
            {
                bool? oldValue = _checked;

                _checked = value;

                if (ElementCheckbox != null)
                {
                    ElementCheckbox.IsChecked = _checked;
                }

                if (oldValue != value)
                {
                    CheckChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether child checkbox visibility setting is taken from it's parent
        /// </summary>
        public bool ChildrenInheritCheckboxVisibility { get; set; }

        /// <summary>
        /// Gets or sets whether this node can have children
        /// </summary>
        public bool IsContainer { get; set; }

        /// <summary>
        /// Gets or sets the popup content
        /// </summary>
        public object Popup
        {
            get { return _popup; }
            set
            {
                _popup = value;

                if (ElementText != null)
                {
                    ToolTipService.SetToolTip(ElementText, _popup);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this node is busy, this sets the busy animation
        /// </summary>
        public bool IsBusy
        {
            get { return _busy; }
            set
            {
                _busy = value;
                if (ElementExpand != null)
                {
                    if (_busy)
                    {
                        ElementExpand.BeginRotate();
                    }
                    else
                    {
                        ElementExpand.EndRotate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the root node
        /// </summary>
        public Node Top
        {
            get
            {
                Node top = this;
                Node current = this.ParentNode;

                while (current != null)
                {
                    if (current.ParentNode == null)
                    {
                        top = current;
                        break;
                    }
                    current = current.ParentNode;
                }

                return top;
            }
        }

        /// <summary>
        /// Gets or sets whether OnApplyTemplate() has been called
        /// </summary>
        public bool TemplateApplied { get; set; }

        /// <summary>
        /// Gets or sets whether the node can be selected
        /// </summary>
        public bool IsLabel { get; set; }

        #endregion

        #region Public Events

        public event TreeEventHandler NodeExpand;
        public event TreeEventHandler NodeClosed;
        public event EventHandler NodeStateChange;
        public event EventHandler NodeIconMouseEnter;
        public event EventHandler NodeIconMouseLeave;
        public event MouseEventHandler NodeIconMouseDown;
        public event MouseEventHandler NodeIconMouseUp;
        public event MouseEventHandler NodeIconMouseMove;
        public event EventHandler NodeTextMouseDown;
        public event MouseEventHandler NodeMouseOver;
        public event EventHandler NodeMouseLeave;
        public event MouseEventHandler NodeMouseDown;
        public event MouseEventHandler NodeMouseUp;
        public event TreeEventHandler PopulateChildren;
        public event TreeEventHandler NodeCheckChanged;
        public event EventHandler NodeChildTemplateApplied;
        public event TreeEventHandler NodeEdited;
        public event TreeEventHandler NodeEditingFinished;
        public event TreeEventHandler NodeExpanded;

        #endregion

        #region Constructor

        public Node()
        {
            Nodes = new ObservableCollection<Node>();
            OverallHeight = 16;
            ChildrenInheritCheckboxVisibility = true;

            Nodes.CollectionChanged += new NotifyCollectionChangedEventHandler(Children_CollectionChanged);

            TemplateApplied = false;
            IsLabel = false;
            ScrollIntoPosition = false;
            ScrollIntoPositionAfterFade = false;

            DefaultStyleKey = typeof(Node);
        }

        public Node(string id, string title, bool hasChildren)
            : this(id, title, hasChildren, "")
        {
        }

        public Node(string id, string title, bool hasChildren, string icon)
            : this(id, title, hasChildren, icon, icon)
        {
        }

        public Node(string id, string title, bool hasChildren, string icon, string iconExpanded)
            : this(id, title, hasChildren, icon, iconExpanded, false)
        {
        }

        public Node(string id, string title, bool hasChildren, string icon, string iconExpanded, bool container)
            : this()
        {
            ID = id;
            Title = title;
            HasChildren = hasChildren;
            Icon = icon;
            IconExpanded = iconExpanded;
            IsContainer = container;
        }

        #endregion

        #region Internal Methods

        internal void SetHighlightVisibility(Visibility visibility)
        {
            if (ElementHighlight != null)
            {
                if (Base.DragAndDropNodeHighlighting == Visibility.Collapsed)
                {
                    ElementHighlight.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ElementHighlight.Visibility = visibility;
                }
            }
        }

        /// <summary>
        /// Hides the connector lines for this node
        /// </summary>
        internal void HideLines()
        {
            _showLines = false;
            UpdateLines();
        }

        /// <summary>
        /// Shows the connector lines for this node
        /// </summary>
        internal void ShowLines()
        {
            _showLines = true;
            UpdateLines();
        }

        /// <summary>
        /// This method sets all the Event Handlers for the nodes
        /// </summary>
        internal void EnableNodeEvents()
        {
            if (ElementRoot != null)
            {
                ElementCheckbox.Click += new RoutedEventHandler(Checkbox_Clicked);
                ElementIcon.MouseEnter += new MouseEventHandler(DocumentIcon_OnMouseEnter);
                ElementIcon.MouseLeave += new MouseEventHandler(DocumentIcon_OnMouseLeave);
                ElementIcon.MouseLeftButtonDown += new MouseButtonEventHandler(DocumentIcon_OnMouseDown);
                ElementIcon.MouseLeftButtonUp += new MouseButtonEventHandler(DocumentIcon_OnMouseUp);
                ElementIcon.MouseMove += new MouseEventHandler(DocumentIcon_OnMouseMove);
                ElementText.MouseLeftButtonDown += new MouseButtonEventHandler(TitleText_OnMouseDown);
                ElementText.MouseEnter += new MouseEventHandler(TitleText_MouseEnter);
                ElementText.MouseLeave += new MouseEventHandler(TitleText_MouseLeave);
                ElementSelectedText.MouseLeftButtonDown += new MouseButtonEventHandler(TitleText_OnMouseDown);
            }
        }

        /// <summary>
        /// Applies an opactity value to all the child nodes
        /// </summary>
        /// <param name="opacity"></param>
        internal void SetOpacity(double opacity)
        {
            if (ElementRoot != null)
            {
                // Set the opacity values for the elements
                ElementBackground.Opacity = opacity;
                ElementExpand.Opacity = opacity;

                ElementCheckbox.Opacity = opacity;
                ElementIcon.Opacity = opacity;
                ElementText.Opacity = opacity;
                ElementSelectedText.Opacity = opacity;

                foreach (Node node in Nodes)
                {
                    node.SetOpacity(opacity);
                }
            }
        }

        /// <summary>
        /// Recalculates the number of children
        /// </summary>
        internal void UpdateChildCount()
        {
            HasChildren = (Nodes.Count > 0);
            if (HasChildren == false && IsExpanded)
            {
                Collapse();
            }
        }

        /// <summary>
        /// This method sycronizes the Child Nodes collection with the rendered Canvas
        /// </summary>
        internal virtual void SyncChildren()
        {
            int i;

            for (i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].Base = this.Base;
                Nodes[i].ParentNode = this;

                if (Nodes[i].Base != null)
                {
                    if (Base.ApplyCheckChangesToChildren)
                    {
                        IsChecked = Nodes[i].ParentNode.IsChecked;
                    }
                }
            }

            // Ensure we update the parent icons
            if (HasChildren == false && Nodes.Count > 0)
            {
                HasChildren = true;
            }
            else if (Nodes.Count == 0)
            {
                HasChildren = false;
            }

            if (ElementRoot != null)
            {
                ElementChildren.Children.Clear();

                _childrenToLoad = 0;
                for (i = 0; i < Nodes.Count; i++)
                {
                    if (!Nodes[i].TemplateApplied)
                    {
                        Nodes[i].NodeChildTemplateApplied -= new EventHandler(Node_NodeChildTemplateApplied);
                        Nodes[i].NodeChildTemplateApplied += new EventHandler(Node_NodeChildTemplateApplied);
                        _childrenToLoad++;
                    }

                    ElementChildren.Children.Add(Nodes[i]);
                }
            }

            UpdateVisualState();
            UpdateVisuals();
        }

        /// <summary>
        /// Updates the visual state for this and all child nodes
        /// </summary>
        internal void RecursiveUpdateVisuals()
        {
            UpdateVisuals();

            foreach (Node node in Nodes)
            {
                node.RecursiveUpdateVisuals();
            }
        }

        /// <summary>
        /// Updates the visual state
        /// </summary>
        internal void UpdateVisuals()
        {
            UpdateLines();
        }

        /// <summary>
        /// This method updates the checked state based on the checked states of it's children and iterates up through its parents
        /// </summary>
        internal void UpdateCheckedPartialStates()
        {
            int checkedCount = 0;
            bool nullFound = false;

            foreach (Node node in Nodes)
            {
                if (node.IsChecked != null)
                {
                    if (node.IsChecked.Value)
                    {
                        checkedCount++;
                    }
                }
                else
                {
                    nullFound = true;
                    break;
                }
            }

            if (checkedCount == Nodes.Count && !nullFound)
            {
                _checked = true;
            }
            else if (checkedCount == 0 && !nullFound)
            {
                _checked = false;
            }
            else
            {
                _checked = null;
            }

            if (ElementCheckbox != null)
            {
                ElementCheckbox.IsChecked = _checked;
            }

            if (ParentNode != null)
            {
                ParentNode.UpdateCheckedPartialStates();
            }
        }

        internal void SetHoverState(Visibility state)
        {
            if (ElementBackgroundHover != null)
            {
                ElementBackgroundHover.Visibility = state;
            }
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return _text;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Base != null)
            {
                Base.NodeKeyDown(e);
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (ElementFocusedState != null && e.OriginalSource == this)
            {
                ElementFocusedState.Visibility = Visibility.Visible;
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (ElementFocusedState != null)
            {
                ElementFocusedState.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Sorts the child nodes by the node title
        /// </summary>
        public void Sort(Tree.SortActions sort)
        {
            bool sortAll = ((sort & Tree.SortActions.ApplyToChildren) != 0);

            if ((sort & Tree.SortActions.ContainersFirst) != 0)
            {
                if ((sort & Tree.SortActions.Descending) != 0)
                {
                    Sort(sortAll, SortReverseContainersFirstCallback);
                }
                else
                {
                    Sort(sortAll, SortContainersFirstCallback);
                }
            }
            else
            {
                if ((sort & Tree.SortActions.Descending) != 0)
                {
                    Sort(sortAll, SortReverseCallback);
                }
                else
                {
                    Sort(sortAll, SortCallback);
                }
            }
        }

        /// <summary>
        /// Sorts the child nodes by the node title
        /// </summary>
        /// <param name="sortAll">Indicates whether descendant nodes should also be sorted</param>
        /// <param name="comparer">The sort compare method</param>
        public void Sort(bool sortAll, Comparison<Node> comparer)
        {
            List<Node> temp = new List<Node>();

            if (Nodes.Count > 0)
            {
                temp.AddRange(Nodes);
                temp.Sort(comparer);

                BulkUpdateBegin();

                Nodes.Clear();
                foreach (Node node in temp)
                {
                    Nodes.Add(node);
                    if (sortAll)
                    {
                        node.Sort(sortAll, comparer);
                    }
                }

                BulkUpdateEnd();

                foreach (Node node in Nodes)
                {
                    node.UpdateVisuals();
                }
            }
        }

        /// <summary>
        /// Iterates through the this and all child nodes looking for a node with the provided terms in the title text
        /// </summary>
        /// <param name="terms">Search terms</param>
        /// <returns>Matching collection of nodes</returns>
        public List<Node> Find(string terms)
        {
            List<Node> results = new List<Node>();

            if (this != Base && Regex.Match(_text, Regex.Escape(terms), RegexOptions.IgnoreCase).Success)
            {
                results.Add(this);
            }
            foreach (Node node in Nodes)
            {
                results.AddRange(node.Find(terms));
            }

            return results;
        }

        /// <summary>
        /// Iterates through all child nodes looking for a node with the matching ID
        /// </summary>
        /// <param name="id">ID of the node to get</param>
        /// <returns>A matching Node or null</returns>
        public Node Get(string id)
        {
            Node result = null;

            foreach (Node node in Nodes)
            {
                if (node.ID == id)
                {
                    result = node;
                    break;
                }

                result = node.Get(id);
                if (result != null)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks the current and all child node checked state
        /// </summary>
        public void CheckAll()
        {
            IsChecked = true;

            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].CheckAll();
            }
        }

        /// <summary>
        /// Removes all child nodes from the tree
        /// </summary>
        public void Clear()
        {
            int i;

            for (i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].Clear();
            }

            if (ElementChildren != null)
            {
                ElementChildren.Children.Clear();
            }

            Nodes.Clear();
        }

        /// <summary>
        /// Clears the current and all child node checked state
        /// </summary>
        public void ClearAllChecked()
        {
            IsChecked = false;

            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].ClearAllChecked();
            }
        }

        /// <summary>
        /// Gets all checked nodes
        /// </summary>
        /// <returns></returns>
        public List<Node> GetAllChecked()
        {
            List<Node> results = new List<Node>();

            foreach (Node node in Nodes)
            {
                if (node.IsChecked != null)
                {
                    if (node.IsChecked.Value)
                    {
                        results.Add(node);
                    }
                    if (node.Nodes.Count > 0)
                    {
                        results.AddRange(node.GetAllChecked());
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// You can call this method when adding large numbers of child nodes, this significantly increases performance
        /// you must call BulkInsertEnd() when you have finished adding nodes
        /// </summary>
        public void BulkUpdateBegin()
        {
            DisableChildUpdates();
        }

        /// <summary>
        /// This method must be called when you have finished adding nodes and have previously called BulkInsertBegin()
        /// </summary>
        public void BulkUpdateEnd()
        {
            EnableChildUpdates();
            SyncChildren();
            UpdateVisualState();
        }

        /// <summary>
        /// Expands the node, firing off events to populate child nodes
        /// </summary>
        public void Expand()
        {
            TreeEventArgs args = new TreeEventArgs(ID);

            if (this == Base || _busy || IsExpanded || !HasChildren)
            {
                return;
            }

            args.Source = this;
            RaiseNodeExpand(this, args);

            if (!args.Cancel)
            {
                IsExpanded = true;

                if (!ChildrenLoaded)
                {
                    // Call to populate the child nodes
                    Populate();
                }
                else
                {
                    InformNodeExpanded();
                }
                SetChildVisibility(true);
                if (ElementRoot != null)
                {
                    if (Base.EnableExpandFadeIn)
                    {
                        ElementFadeIn.Begin();
                    }
                    else
                    {
                        ElementChildren.Opacity = 1;
                        ElementFadeIn_Completed(this, null);
                    }
                }

                RaiseNodeStateChange(this, new EventArgs());
                UpdateNodeIcon();
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Expands all the parent nodes of this node
        /// </summary>
        public void ExpandOut()
        {
            Node current = this;
            List<Node> path = new List<Node>();

            while (current != null)
            {
                path.Add(current);
                current = current.ParentNode;
            }

            if (path.Count > 0)
            {
                path.Reverse();
                path[0].ExpandAll(path);
            }

        }

        /// <summary>
        /// Expands all nodes
        /// </summary>
        public void ExpandAll()
        {
            ExpandAll(null);
        }

        /// <summary>
        /// Expands all child nodes of the provided parent nodes
        /// </summary>
        public void ExpandAll(List<Node> path)
        {
            _expandAll = true;
            _expandPath = path;

            if (ElementRoot != null)
            {
                Expand();
                foreach (Node node in Nodes)
                {
                    if (_expandPath == null || _expandPath.Contains(node))
                    {
                        node.ExpandAll(_expandPath);
                    }
                    if (_expandPath != null)
                    {
                        if (_expandPath[_expandPath.Count - 1] == node)
                        {
                            Base.SetSelected(node, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Collapses the node, child nodes are hidden
        /// </summary>
        public void Collapse()
        {
            if (this == Base || _busy)
            {
                return;
            }

            IsExpanded = false;

            if (VisualRoot != null)
            {
                SetChildVisibility(false);

                if (HasChildren)
                {
                    RaiseNodeClosed(this, new TreeEventArgs());
                    RaiseNodeStateChange(this, new EventArgs());
                }
            }
            UpdateNodeIcon();
            UpdateVisualState();
        }

        /// <summary>
        /// Collapses all nodes
        /// </summary>
        public void CollapseAll()
        {
            Collapse();

            foreach (Node node in Nodes)
            {
                node.CollapseAll();
            }
        }

        /// <summary>
        /// Deletes the current node, this removes the node from the parent nodes collection
        /// </summary>
        public void Delete()
        {
            if (ParentNode != null)
            {
                ParentNode.Nodes.Remove(this);
                ParentNode.UpdateChildCount();
            }
        }

        /// <summary>
        /// Removes an individual child node
        /// </summary>
        /// <param name="id">Node ID to remove</param>
        public void DeleteChild(string id)
        {
            int childIndex = IndexOfChild(id);

            if (childIndex >= 0)
            {
                Nodes.RemoveAt(childIndex);
                UpdateChildCount();
            }
        }

        /// <summary>
        /// Selects a specific node
        /// </summary>
        /// <param name="id">The ID of the node to select</param>
        public void SelectChild(string id)
        {
            int index = IndexOfChild(id);

            if (index >= 0)
            {
                RaiseNodeTextMouseDown(Nodes[index], null);
            }
        }

        /// <summary>
        /// Removes all the child nodes and calls Populate() to re-populate the node
        /// </summary>
        public void Refresh()
        {
            Nodes.Clear();
            Populate();
            HasChildren = (Nodes.Count > 0);
            SetChildVisibility(IsExpanded);

            UpdateNodeIcon();
            UpdateVisualState();
            if (ElementExpand != null)
            {
                ElementExpand.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Swaps the current node instance with the node above
        /// </summary>
        public void SwapPrevious()
        {
            if (ParentNode == null)
            {
                return;
            }

            int thisIndex;
            int displayIndex;
            ObservableCollection<Node> nodes = ParentNode.Nodes;
            StackPanel canvas = ParentNode.ElementChildren;

            thisIndex = nodes.IndexOf(this);
            displayIndex = canvas.Children.IndexOf(this);

            if (thisIndex > 0)
            {
                ParentNode.DisableChildUpdates();

                nodes.RemoveAt(thisIndex);
                nodes.Insert(thisIndex - 1, this);
                canvas.Children.RemoveAt(displayIndex);
                canvas.Children.Insert(displayIndex - 1, this);

                nodes[thisIndex].UpdateVisuals();
                nodes[thisIndex - 1].UpdateVisuals();

                ParentNode.EnableChildUpdates();
            }
        }

        /// <summary>
        /// Swaps the current node instance with the node below
        /// </summary>
        public void SwapNext()
        {
            if (ParentNode == null)
            {
                return;
            }

            int thisIndex;
            int displayIndex;
            ObservableCollection<Node> nodes = ParentNode.Nodes;
            StackPanel canvas = ParentNode.ElementChildren;

            thisIndex = nodes.IndexOf(this);
            displayIndex = canvas.Children.IndexOf(this);

            if (thisIndex < nodes.Count - 1)
            {
                ParentNode.DisableChildUpdates();

                nodes.RemoveAt(thisIndex);
                nodes.Insert(thisIndex + 1, this);
                canvas.Children.RemoveAt(displayIndex);
                canvas.Children.Insert(displayIndex + 1, this);

                nodes[thisIndex].UpdateVisuals();
                nodes[thisIndex + 1].UpdateVisuals();

                ParentNode.DisableChildUpdates();
            }
        }

        /// <summary>
        /// Gets the previous node
        /// </summary>
        /// <returns>Node object</returns>
        public Node Previous()
        {
            return (ParentNode != null ? ParentNode.GetChild(IndexOfPrevious()) : null);
        }

        /// <summary>
        /// Gets the next node
        /// </summary>
        /// <returns>Node object</returns>
        public Node Next()
        {
            return (ParentNode != null ? ParentNode.GetChild(IndexOfNext()) : null);
        }

        /// <summary>
        /// Gets a node with the matching ID
        /// </summary>
        /// <param name="id">Node ID</param>
        /// <returns>Node</returns>
        public Node GetChild(string id)
        {
            return GetChild(IndexOfChild(id));
        }

        /// <summary>
        /// Gets a node at a given index
        /// </summary>
        /// <param name="index">Node index</param>
        /// <returns></returns>
        public Node GetChild(int index)
        {
            Node result = null;

            if (index >= 0)
            {
                result = Nodes[index];
            }

            return result;
        }

        /// <summary>
        /// Returns the index of the 'previous' node.  This is the node above the current node instance in the tree
        /// </summary>
        /// <returns>The index of the requested node or -1 if the node could not be found</returns>
        public int IndexOfPrevious()
        {
            int result = -1;
            int temp;

            if (ParentNode != null)
            {
                temp = ParentNode.Nodes.IndexOf(this);

                if (temp > 0)
                {
                    result = temp - 1;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the index of the 'next' node.  This is the node below the current node instance in the tree
        /// </summary>
        /// <returns>The index of the requested node or -1 if the node could not be found</returns>
        public int IndexOfNext()
        {
            int result = -1;
            int temp;

            if (ParentNode != null)
            {
                temp = ParentNode.Nodes.IndexOf(this);

                if (temp < ParentNode.Nodes.Count - 1)
                {
                    result = temp + 1;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the index of a specific node
        /// </summary>
        /// <param name="id">The ID of the node to locate</param>
        /// <returns>The index of the requested node or -1 if the node could not be found</returns>
        public int IndexOfChild(string id)
        {
            int i;
            int result = -1;

            for (i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].ID == id)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Sort method for sorting by title with all containers first
        /// </summary>
        /// <param name="a">Node a</param>
        /// <param name="b">Node b</param>
        /// <returns>Sort value</returns>
        public static int SortContainersFirstCallback(Node a, Node b)
        {
            string prefixA = (a.IsContainer ? "0" : "1");
            string prefixB = (b.IsContainer ? "0" : "1");

            return (prefixA + a.Title).CompareTo(prefixB + b.Title);
        }

        /// <summary>
        /// Sort method for sorting by title
        /// </summary>
        /// <param name="a">Node a</param>
        /// <param name="b">Node b</param>
        /// <returns>Sort value</returns>
        public static int SortCallback(Node a, Node b)
        {
            return a.Title.CompareTo(b.Title);
        }

        /// <summary>
        /// Sort method for sorting by title
        /// </summary>
        /// <param name="a">Node a</param>
        /// <param name="b">Node b</param>
        /// <returns>Sort value</returns>
        public static int SortReverseContainersFirstCallback(Node a, Node b)
        {
            string prefixA = (a.IsContainer ? "0" : "1");
            string prefixB = (b.IsContainer ? "0" : "1");

            return (prefixB + b.Title).CompareTo(prefixA + a.Title);
        }

        /// <summary>
        /// Sort method for sorting by title
        /// </summary>
        /// <param name="a">Node a</param>
        /// <param name="b">Node b</param>
        /// <returns>Sort value</returns>
        public static int SortReverseCallback(Node a, Node b)
        {
            return b.Title.CompareTo(a.Title);
        }

        /// <summary>
        /// Disables updates to the child node collection
        /// </summary>
        public void DisableChildUpdates()
        {
            Nodes.CollectionChanged -= new NotifyCollectionChangedEventHandler(Children_CollectionChanged);
        }

        /// <summary>
        /// This method must be called when you have finished adding nodes and have previously called BulkInsertBegin()
        /// </summary>
        public void EnableChildUpdates()
        {
            Nodes.CollectionChanged -= new NotifyCollectionChangedEventHandler(Children_CollectionChanged);
            Nodes.CollectionChanged += new NotifyCollectionChangedEventHandler(Children_CollectionChanged);
        }

        public bool IsPointOverIcon(Point position, Point nodePosition)
        {
            bool result = false;
            Point iconOffset = ElementIcon.TransformToVisual(this).Transform(new Point());

            result = (position.X > nodePosition.X + iconOffset.X && position.X < nodePosition.X + iconOffset.X + ElementIcon.ActualWidth + 4);

            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Is called when a node has expanded
        /// </summary>
        private void InformNodeExpanded()
        {
            if (_childrenToLoad == 0)
            {
                RaiseNodeExpanded(this, new TreeEventArgs(ID));
            }
        }

        /// <summary>
        /// Updates the expanded control of the node
        /// </summary>
        private void UpdateNodeIcon()
        {
            string url = ((IsExpanded && _iconExpanded.Length > 0) ? _iconExpanded : _icon);
            object nodeContent = ((IsExpanded && IconExpandedContent != null) ? IconExpandedContent : IconContent);

            if (ElementRoot != null)
            {
                if (url.Length > 0)
                {
                    Image iconImage = new Image()
                    {
                        Stretch = Stretch.None,
                        Source = new BitmapImage() { UriSource = new Uri(url, UriKind.RelativeOrAbsolute) }
                    };

                    ElementIcon.Content = iconImage;
                }
                else if (nodeContent != null)
                {
                    ElementIcon.Content = nodeContent;
                }
            }
        }

        /// <summary>
        /// Sets the visibility of the children
        /// </summary>
        /// <param name="visible"></param>
        private void SetChildVisibility(bool visible)
        {
            if (ElementRoot != null)
            {
                ElementChildren.Visibility = (visible ? Visibility.Visible : Visibility.Collapsed);
            }
        }

        /// <summary>
        /// Populates the current node instance with its children.  This method raises the PopulateChildren event for which the parent application is responsible for handling
        /// </summary>
        private void Populate()
        {
            TreeEventArgs args = new TreeEventArgs(ID);

            RaisePopulateChildren(this, args);
            SyncChildren();
            ChildrenLoaded = true;
        }

        protected void UpdateVisualState()
        {
            double temp;
            double opacity = (IsEnabled ? 1 : DisabledOpacity);
            GridLength length;

            if (ElementExpand != null)
            {
                if (!_busy)
                {
                    ElementExpand.Visibility = (HasChildren ? Visibility.Visible : Visibility.Collapsed);
                }

                if (HasChildren)
                {
                    ElementExpand.IsExpanded = IsExpanded;
                    ElementExpand.Cursor = (IsEnabled ? Cursors.Hand : Cursors.Arrow);
                }

                length = new GridLength(_enableCheckboxes ? 20 : 0);
                ElementGrid.ColumnDefinitions[1].SetValue(ColumnDefinition.WidthProperty, length);
                if (_icon.Length > 0 || IconContent != null)
                {
                    length = GridLength.Auto;
                }
                else
                {
                    length = new GridLength(0);
                }

                ElementGrid.ColumnDefinitions[2].SetValue(ColumnDefinition.WidthProperty, length);

                ElementCheckbox.Visibility = (_enableCheckboxes ? Visibility.Visible : Visibility.Collapsed);
                if (_enableCheckboxes)
                {
                    ElementCheckbox.IsChecked = _checked;
                    ElementCheckbox.Cursor = (IsEnabled ? Cursors.Hand : Cursors.Arrow);
                }

                if (_icon.Length > 0 || IconContent != null)
                {
                    ElementIcon.Cursor = (IsEnabled && !IsLabel ? Cursors.Hand : Cursors.Arrow);
                    UpdateNodeIcon();
                }

                if (Base != null)
                {
                    if (Base.MaxTitleLength >= 0 && _text.Length > Base.MaxTitleLength)
                    {
                        ElementText.Text = _text.Substring(0, Base.MaxTitleLength) + Base.TitlePostfix;
                    }
                }

                if (_text != null)
                {
                    ElementText.Text = _text;
                    ElementText.Opacity = opacity;
                    ElementSelectedText.Text = _text;
                    ElementInput.Text = _text;
                }

                if (!_editable)
                {
                    ElementBackground.Visibility = (_selected ? Visibility.Visible : Visibility.Collapsed);
                    ElementText.Visibility = (_selected ? Visibility.Collapsed : Visibility.Visible);
                    ElementSelectedText.Visibility = (_selected ? Visibility.Visible : Visibility.Collapsed);
                    ElementInput.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ElementBackground.Visibility = Visibility.Collapsed;
                    ElementText.Visibility = Visibility.Collapsed;
                    ElementSelectedText.Visibility = Visibility.Collapsed;
                    ElementInput.Visibility = Visibility.Visible;
                }

                ElementBackground.Cursor = (IsEnabled && !IsLabel ? Cursors.Hand : Cursors.Arrow);
                ElementBackgroundHover.Cursor = ElementBackground.Cursor;
                ElementText.Cursor = ElementBackground.Cursor;
                ElementSelectedText.Cursor = ElementBackground.Cursor;

                temp = ElementText.ActualWidth;
                ElementInput.Width = ElementText.ActualWidth + 10;

                ElementIcon.Opacity = opacity;
                ElementText.Opacity = opacity;
            }
        }

        private void UpdateLines()
        {
            double height = RenderSize.Height;

            if (ElementVerticalLine != null)
            {
                ElementVerticalLine.Visibility = Visibility.Collapsed;
                ElementHorizontalLine.Visibility = Visibility.Collapsed;

                ElementVerticalLine.Fill = Base.ConnectorLinesBrush;
                ElementHorizontalLine.Fill = Base.ConnectorLinesBrush;

                if (Base != null && Base.EnableLines && _showLines)
                {
                    if (IsExpanded)
                    {
                        ElementVerticalLine.Visibility = Visibility.Visible;
                        ElementVerticalLine.X1 = 5;
                        ElementVerticalLine.Y1 = 0;
                        ElementVerticalLine.X2 = 5;
                        ElementVerticalLine.Y2 = height;

                        if (ParentNode.Nodes.IndexOf(this) == ParentNode.Nodes.Count - 1)
                        {
                            // This node is the last sibling
                            ElementVerticalLine.Y2 = ElementExpand.Height * 0.5;
                        }
                    }
                    else if (!HasChildren)
                    {
                        ElementVerticalLine.Visibility = Visibility.Visible;
                        ElementHorizontalLine.Visibility = Visibility.Visible;

                        ElementVerticalLine.X1 = 5;
                        ElementVerticalLine.Y1 = 0;
                        ElementVerticalLine.X2 = 5;
                        ElementVerticalLine.Y2 = Math.Floor(height * 0.5);

                        ElementHorizontalLine.X1 = 6;
                        ElementHorizontalLine.Y1 = ElementVerticalLine.Y2;
                        ElementHorizontalLine.X2 = 13;
                        ElementHorizontalLine.Y2 = ElementVerticalLine.Y2;

                        if (ParentNode.Nodes.IndexOf(this) < ParentNode.Nodes.Count - 1)
                        {
                            // This node is not the last sibling
                            ElementVerticalLine.Y2 = height;
                        }
                    }
                    else
                    {
                        ElementVerticalLine.Visibility = Visibility.Visible;

                        ElementVerticalLine.X1 = 5;
                        ElementVerticalLine.Y1 = 0;
                        ElementVerticalLine.X2 = 5;
                        ElementVerticalLine.Y2 = (HasChildren ? height * 0.5 : height);

                        if (ParentNode.Nodes.IndexOf(this) < ParentNode.Nodes.Count - 1)
                        {
                            // This node is not the last sibling
                            ElementVerticalLine.Y2 = height;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Is called when the parent node changes, this updates the event handlers
        /// </summary>
        private void ParentChanged()
        {
            if (ParentNode != null)
            {
                this.NodeStateChange = ParentNode.NodeStateChange;
                this.NodeClosed = ParentNode.NodeClosed;
                this.NodeExpand = ParentNode.NodeExpand;
                this.NodeExpanded = ParentNode.NodeExpanded;
                this.NodeIconMouseEnter = ParentNode.NodeIconMouseEnter;
                this.NodeIconMouseLeave = ParentNode.NodeIconMouseLeave;
                this.NodeIconMouseDown = ParentNode.NodeIconMouseDown;
                this.NodeIconMouseUp = ParentNode.NodeIconMouseUp;
                this.NodeIconMouseMove = ParentNode.NodeIconMouseMove;
                this.NodeTextMouseDown = ParentNode.NodeTextMouseDown;
                this.NodeMouseLeave = ParentNode.NodeMouseLeave;
                this.NodeMouseOver = ParentNode.NodeMouseOver;
                this.NodeMouseDown = ParentNode.NodeMouseDown;
                this.NodeMouseUp = ParentNode.NodeMouseUp;
                this.PopulateChildren = ParentNode.PopulateChildren;
                this.NodeCheckChanged = ParentNode.NodeCheckChanged;
                this.NodeEdited = ParentNode.NodeEdited;
                this.NodeEditingFinished = ParentNode.NodeEditingFinished;

                if (this.ParentNode.ChildrenInheritCheckboxVisibility)
                {
                    this.EnableCheckboxes = ParentNode.EnableCheckboxes;
                }
            }
        }

        /// <summary>
        /// Is called when the checkbox IsChecked state changes
        /// </summary>
        private void CheckChanged()
        {
            TreeEventArgs args = new TreeEventArgs(this.ID);
            bool? temp = _checked;

            args.Source = this;

            if (Base != null)
            {
                if (Base.ApplyCheckChangesToChildren)
                {
                    foreach (Node node in Nodes)
                    {
                        node.IsChecked = temp;
                    }
                    _checked = temp;
                }

                if (Base != null)
                {
                    if (Base.EnablePartialCheckboxChecks && ParentNode != null)
                    {
                        ParentNode.UpdateCheckedPartialStates();
                    }
                }
            }

            RaiseNodeCheckChanged(this, args);
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
            ElementFocusedState = (UIElement)GetTemplateChild(ElementFocusedStateName);
            ElementFadeIn = (Storyboard)GetTemplateChild(ElementFadeInName);
            ElementHighlight = (UIElement)GetTemplateChild(ElementHighlightName);
            ElementBackground = (Rectangle)GetTemplateChild(ElementBackgroundName);
            ElementBackgroundHover = (Rectangle)GetTemplateChild(ElementBackgroundHoverName);
            ElementVerticalLine = (Line)GetTemplateChild(ElementVerticalLineName);
            ElementHorizontalLine = (Line)GetTemplateChild(ElementHorizontalLineName);
            ElementIcon = (ContentControl)GetTemplateChild(ElementIconName);
            ElementSelectedText = (TextBlock)GetTemplateChild(ElementSelectedTextName);
            ElementExpand = (Expand)GetTemplateChild(ElementExpandName);
            ElementInput = (TextBox)GetTemplateChild(ElementInputName);
            ElementChildren = (StackPanel)GetTemplateChild(ElementChildrenName);
            ElementCheckbox = (CheckBox)GetTemplateChild(ElementCheckboxName);
            ElementGrid = (Grid)GetTemplateChild(ElementGridName);

            ElementExpand.Click += new EventHandler(PrefixIcon_OnMouseDown);
            ElementInput.LostFocus += new RoutedEventHandler(ElementInput_LostFocus);
            ElementFadeIn.Completed += new EventHandler(ElementFadeIn_Completed);

            this.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
            this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(OnIsEnabledChanged);

            EnableNodeEvents();
            UpdateVisualState();

            TemplateApplied = true;
            RaiseNodeChildTemplateApplied(this, EventArgs.Empty);
            ElementExpand.CornerRadius = Base.ExpandRoundness;

            if (_makeEditableOnApplyTemplate && Base.EnableNodeEditing)
            {
                Base.Selected = this;
                IsEditable = true;
            }

            if (_popup != null)
            {
                ToolTipService.SetToolTip(ElementText, _popup);
            }

            if (IsExpanded)
            {
                SyncChildren();
                UpdateNodeIcon();
                UpdateVisualState();
                if (ElementChildren.Visibility == Visibility.Collapsed)
                {
                    SetChildVisibility(true);
                    if (Base.EnableExpandFadeIn)
                    {
                        ElementFadeIn.Begin();
                    }
                    else
                    {
                        ElementChildren.Opacity = 1;
                        ElementFadeIn_Completed(this, null);
                    }
                }
            }

            if (_busy)
            {
                ElementExpand.BeginRotate();
            }

            if (ScrollIntoPosition && Base != null)
            {
                Base.ScrollIntoPosition();
            }
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualState();
        }

        private void ElementFadeIn_Completed(object sender, EventArgs e)
        {
            if (Base.Selected != null)
            {
                if (Nodes.Contains(Base.Selected) && Base.Selected.ScrollIntoPositionAfterFade)
                {
                    Base.ScrollIntoPosition();
                    Base.Selected.ScrollIntoPositionAfterFade = false;
                }
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateLines();
        }

        private void ElementInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_editable)
            {
                IsEditable = false;
            }
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SyncChildren();
        }

        /// <summary>
        /// This event is called when the mouse button is clicked for the node expand/collapse control
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void PrefixIcon_OnMouseDown(object sender, EventArgs e)
        {
            if (IsEnabled)
            {
                if (!IsExpanded)
                {
                    Expand();
                }
                else
                {
                    Collapse();
                }
            }
        }

        /// <summary>
        /// This event is called when the mouse hovers over the node icon
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void DocumentIcon_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (IsEnabled && !IsLabel)
            {
                RaiseNodeIconMouseEnter(this, e);
            }
        }

        /// <summary>
        /// This event is called when the mouse leaves the node icon
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void DocumentIcon_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (IsEnabled && !IsLabel)
            {
                RaiseNodeIconMouseLeave(this, e);
            }
        }

        /// <summary>
        /// This event is called when the mouse button is clicked for the node icon
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void DocumentIcon_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled && !IsLabel)
            {
                RaiseNodeIconMouseDown(this, e);
                RaiseNodeMouseDown(this, e);

                if (Base.TicksSinceLastMouseDown < 10)
                {
                    TreeEventArgs args = new TreeEventArgs(this.ID)
                    {
                        Source = this,
                        Target = this
                    };

                    Base.RaiseNodeIconDoubleClick(this, args);
                }

                Base.TicksSinceLastMouseDown = 0;
            }
        }

        /// <summary>
        /// This event is called when the mouse button is released for the node icon
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void DocumentIcon_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled && !IsLabel)
            {
                RaiseNodeIconMouseUp(this, e);
                RaiseNodeMouseUp(this, e);
            }
        }

        /// <summary>
        /// This event is called when the mouse button is released for the node icon
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void DocumentIcon_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (IsEnabled && !IsLabel)
            {
                RaiseNodeIconMouseMove(this, e);
            }
        }

        /// <summary>
        /// This event is called when the mouse is clicked on the text area
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void TitleText_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled && !IsLabel)
            {
                if (ElementBackgroundHover != null)
                {
                    ElementBackgroundHover.Visibility = Visibility.Collapsed;
                }

                RaiseNodeTextMouseDown(this, e);

                if (Base.CurrentEditingNode != null && Base.CurrentEditingNode != this)
                {
                    Base.CurrentEditingNode.IsEditable = false;
                }

                this.Focus();
            }
        }

        /// <summary>
        /// This event is called when the mouse enters the text area
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void TitleText_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsEnabled && !IsLabel)
            {
                SetHoverState(Visibility.Visible);
                RaiseNodeMouseOver(this, e);
            }
        }

        /// <summary>
        /// This event is called when the mouse leaves the text area
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void TitleText_MouseLeave(object sender, EventArgs e)
        {
            if (IsEnabled && !IsLabel)
            {
                SetHoverState(Visibility.Collapsed);
                RaiseNodeMouseLeave(this, e);
            }
        }

        /// <summary>
        /// This event is called when the checkbox is checked
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void Checkbox_Clicked(object sender, RoutedEventArgs e)
        {
            if (IsEnabled)
            {
                this.Focus();
                IsChecked = ElementCheckbox.IsChecked;
            }
        }

        private void Node_NodeChildTemplateApplied(object sender, EventArgs e)
        {
            _childrenToLoad--;

            if (_childrenToLoad <= 0)
            {
                _childrenToLoad = 0;
                InformNodeExpanded();

                if (_expandAll)
                {
                    _expandAll = false;
                    foreach (Node node in Nodes)
                    {
                        if (_expandPath == null || _expandPath.Contains(node))
                        {
                            node.ExpandAll(_expandPath);
                        }
                        if (_expandPath != null)
                        {
                            if (_expandPath[_expandPath.Count - 1] == node)
                            {
                                Base.SetSelected(node, true);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a NodeExpand event to indicate node has been expanded
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected void RaiseNodeExpand(object sender, TreeEventArgs args)
        {
            if (NodeExpand != null)
            {
                NodeExpand(sender, args);
            }
        }

        /// <summary>
        /// Generates a NodeClosed event to indicate node has been collapsed
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseNodeClosed(object sender, TreeEventArgs args)
        {
            if (NodeClosed != null)
            {
                NodeClosed(sender, args);
            }
        }

        /// <summary>
        /// Generates a NodeStateChange event to indicate node has been expanded or collapsed
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseNodeStateChange(object sender, EventArgs args)
        {
            if (NodeStateChange != null)
            {
                NodeStateChange(sender, args);
            }
        }

        /// <summary>
        /// Generates a NodeIconMouseEnter event to indicate the mouse is above the node icon
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseNodeIconMouseEnter(object sender, EventArgs args)
        {
            if (NodeIconMouseEnter != null)
            {
                NodeIconMouseEnter(sender, args);
            }
        }

        /// <summary>
        /// Generates a RaiseNodeIconMouseLeave event to indicate the mouse has left the node icon
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseNodeIconMouseLeave(object sender, EventArgs args)
        {
            if (NodeIconMouseLeave != null)
            {
                NodeIconMouseLeave(sender, args);
            }
        }

        /// <summary>
        /// Generates a NodeIconMouseDown event to indicate the mouse has been clicked above the node icon
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseNodeIconMouseDown(object sender, MouseEventArgs args)
        {
            if (NodeIconMouseDown != null)
            {
                NodeIconMouseDown(sender, args);
            }
        }

        // <summary>
        /// Generates a NodeIconMouseUp event to indicate the mouse has been released above the node icon
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseNodeIconMouseUp(object sender, MouseEventArgs args)
        {
            if (NodeIconMouseUp != null)
            {
                NodeIconMouseUp(sender, args);
            }
        }

        // <summary>
        /// Generates a NodeIconMouseMove event to indicate the mouse has moved above the node icon
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseNodeIconMouseMove(object sender, MouseEventArgs args)
        {
            if (NodeIconMouseMove != null)
            {
                NodeIconMouseMove(sender, args);
            }
        }

        /// <summary>
        /// Generates a NodeTextMouseDown event to indicate the mouse has been clicked above the node text
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseNodeTextMouseDown(object sender, EventArgs args)
        {
            if (NodeTextMouseDown != null)
            {
                NodeTextMouseDown(sender, args);
            }
        }

        /// <summary>
        /// Generates a NodeMouseOver event to indicate the mouse has entered the node area
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseNodeMouseOver(object sender, MouseEventArgs args)
        {
            if (NodeMouseOver != null)
            {
                NodeMouseOver(sender, args);
            }
        }

        /// <summary>
        /// Generates a NodeMouseLeave event to indicate the mouse has left the node area
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseNodeMouseLeave(object sender, EventArgs args)
        {
            if (NodeMouseLeave != null)
            {
                NodeMouseLeave(sender, args);
            }
        }

        /// <summary>
        /// Generates a NodeMouseDown event to indicate the mouse has been clicked above the node
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseNodeMouseDown(object sender, MouseEventArgs args)
        {
            if (NodeMouseDown != null)
            {
                NodeMouseDown(sender, args);
            }
        }

        /// <summary>
        /// Generates a NodeMouseUp event to indicate the mouse has been released above the node
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseNodeMouseUp(object sender, MouseEventArgs args)
        {
            if (NodeMouseUp != null)
            {
                NodeMouseUp(sender, args);
            }
        }

        /// <summary>
        /// Generates a PopulateChildren event to indicate a node needs to be populated.  This event occurs when it is expanded for the first time only.
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaisePopulateChildren(object sender, TreeEventArgs args)
        {
            if (PopulateChildren != null)
            {
                PopulateChildren(sender, args);
            }
        }

        /// <summary>
        /// Generates a NodeCheckChanged event to indicate the checkbox has been changed
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected void RaiseNodeCheckChanged(object sender, TreeEventArgs args)
        {
            if (NodeCheckChanged != null)
            {
                NodeCheckChanged(sender, args);
            }
        }

        private void RaiseNodeChildTemplateApplied(object sender, EventArgs args)
        {
            if (NodeChildTemplateApplied != null)
            {
                NodeChildTemplateApplied(sender, args);
            }
        }

        /// <summary>
        /// Generates a NodeEdited event to indicate a node has been edited
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected void RaiseNodeEdited(object sender, TreeEventArgs args)
        {
            if (NodeEdited != null)
            {
                NodeEdited(sender, args);
            }
        }

        /// <summary>
        /// Generates a NodeEditingFinished event to indicate editing has finished
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected void RaiseNodeEditingFinished(object sender, TreeEventArgs args)
        {
            if (NodeEditingFinished != null)
            {
                NodeEditingFinished(sender, args);
            }
        }

        /// <summary>
        /// Generates a NodeExpanded event to indicate editing has finished
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected void RaiseNodeExpanded(object sender, TreeEventArgs args)
        {
            if (NodeExpanded != null)
            {
                NodeExpanded(sender, args);
            }
        }

        #endregion
    }
}
