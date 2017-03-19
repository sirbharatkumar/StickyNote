using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

using Liquid.Components;
using Liquid.Components.Internal;
using System.Windows.Controls.Primitives;

namespace Liquid
{
    #region Delegates

    public delegate void RichTextBoxEventHandler(object sender, RichTextBoxEventArgs e);
    public delegate void RichTextBoxHtmlEventHandler(object sender, RichTextBoxHtmlEventArgs e);

    #endregion

    #region Public Enums

    public enum SelectMode
    {
        Edit,
        ReadOnly,
        Select
    };

    public enum Format
    {
        HTML,
        Text,
        XML,
        XAML
    };

    public enum Formatting
    {
        Bold,
        Italic,
        Underline,
        RemoveBold,
        RemoveItalic,
        RemoveUnderline,
        FontFamily,
        FontSize,
        AlignLeft,
        AlignCenter,
        AlignRight,
        BulletList,
        NumberList,
        BulletImageList,
        RemoveBullet,
        RemoveNumber,
        RemoveBullerImage,
        Indent,
        Outdent,
        Foreground,
        Background,
        Link,
        RemoveLink,
        Painter,
        Strike,
        RemoveStrike,
        SubScript,
        SuperScript,
        RemoveSpecial,
        BorderDashed,
        BorderDotted,
        BorderSolid,
        RemoveBorder,
        ShadowSlight,
        ShadowNormal,
        RemoveShadow,
        Margin,
        Style,
        AlignTop,
        AlignMiddle,
        AlignBottom,
        Metadata
    }

    public enum RichTextSpecialFormatting
    {
        None,
        Subscript,
        Superscript
    }

    public enum RichTextBubbleLip
    {
        None,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public enum RichTextSaveOptions
    {
        None = 0,
        ListStyles = 1,
        InlineStyles = 2,
        ExcludeCustomStyles = 4
    }

    #endregion

    /// <summary>
    /// Displays a block of RichText
    /// </summary>
    public partial class RichTextBlock : TextBox, INotifyPropertyChanged
    {
        #region Visual Elements

        /// <summary> 
        /// Cursor template.
        /// </summary>
        internal Storyboard ElementCursorBlink { get; set; }
        internal const string ElementCursorBlinkName = "ElementCursorBlink";

        /// <summary> 
        /// Children template.
        /// </summary>
        internal RichTextPanel ElementChildren { get; set; }
        internal const string ElementChildrenName = "ElementChildren";

        /// <summary> 
        /// Selection template.
        /// </summary>
        internal Polygon ElementSelection { get; set; }
        internal const string ElementSelectionName = "ElementSelection";

        /// <summary> 
        /// Cursor template.
        /// </summary>
        public Rectangle ElementCursor { get; set; }
        internal const string ElementCursorName = "ElementCursor";

        /// <summary> 
        /// Bubble popup template.
        /// </summary>
        internal Popup ElementBubblePopup { get; set; }
        internal const string ElementBubblePopupName = "ElementBubblePopup";

        /// <summary> 
        /// Bubble template.
        /// </summary>
        internal Canvas ElementBubble { get; set; }
        internal const string ElementBubbleName = "ElementBubble";

        /// <summary> 
        /// Context template.
        /// </summary>
        internal ContentControl ElementContext { get; set; }
        internal const string ElementContextName = "ElementContext";

        /// <summary> 
        /// Object selection canvas template.
        /// </summary>
        internal Canvas ElementObjectSelection { get; set; }
        internal const string ElementObjectSelectionName = "ElementObjectSelection";

        /// <summary> 
        /// Textbox input area for input chinese
        /// </summary>
        internal ContentControl ContentElement { get; set; }
        internal const string ContentElementName = "ContentElement";

        /// <summary> 
        /// Textbox input area background
        /// </summary>
        internal Rectangle ContentElementBackGround { get; set; }
        internal const string ContentElementBackGroundName = "ContentElementBackGround";

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the content in its native XML format
        /// </summary>
        public static readonly DependencyProperty RichTextProperty = DependencyProperty.Register("RichText", typeof(string), typeof(RichTextBlock), new PropertyMetadata(string.Empty, OnRichTextPropertyChanged));
        public string RichText
        {
            get { return (ElementChildren != null ? Save(Format.XML, RichTextSaveOptions.ListStyles) : (string)this.GetValue(RichTextProperty)); }
            set { this.SetValue(RichTextProperty, value); }
        }

        private static void OnRichTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RichTextBlock rtb = d as RichTextBlock;
            rtb.SetRichTextContent((string)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the content as HTML (This is a basic conversion, not all RichText elements are supported in HTML)
        /// </summary>
        public static readonly DependencyProperty HTMLProperty = DependencyProperty.Register("HTML", typeof(string), typeof(RichTextBlock), new PropertyMetadata(string.Empty, OnHTMLPropertyChanged));
        public string HTML
        {
            get { return (ElementChildren != null ? Save(Format.HTML, RichTextSaveOptions.ListStyles) : (string)this.GetValue(HTMLProperty)); }
            set { this.SetValue(HTMLProperty, value); }
        }

        private static void OnHTMLPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RichTextBlock rtb = d as RichTextBlock;
            rtb.SetHTMLContent((string)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the context menu object
        /// </summary>
        public static readonly DependencyProperty ContextMenuProperty = DependencyProperty.Register("ContextMenu", typeof(Control), typeof(RichTextBlock), null);
        public Control ContextMenu
        {
            get { return (Control)this.GetValue(ContextMenuProperty); }
            set { base.SetValue(ContextMenuProperty, value); }
        }

        /// <summary>
        /// Gets or sets the direct richtext content
        /// </summary>
        public static readonly DependencyProperty DirectRichTextProperty = DependencyProperty.Register("DirectRichText", typeof(object), typeof(RichTextBlock), null);
        public object DirectRichText
        {
            get { return this.GetValue(DirectRichTextProperty); }
            set { base.SetValue(DirectRichTextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the title of the bubble popup for spell checking
        /// </summary>
        public static readonly DependencyProperty PopupTitleProperty = DependencyProperty.Register("PopupTitle", typeof(string), typeof(RichTextBlock), null);
        public string PopupTitle
        {
            get { return (string)this.GetValue(PopupTitleProperty); }
            set { base.SetValue(PopupTitleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Add button text of the bubble popup for spell checking
        /// </summary>
        public static readonly DependencyProperty PopupAddTextProperty = DependencyProperty.Register("PopupAddText", typeof(string), typeof(RichTextBlock), null);
        public string PopupAddText
        {
            get { return (string)this.GetValue(PopupAddTextProperty); }
            set { base.SetValue(PopupAddTextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Replace button text of the bubble popup for spell checking
        /// </summary>
        public static readonly DependencyProperty PopupReplaceTextProperty = DependencyProperty.Register("PopupReplaceText", typeof(string), typeof(RichTextBlock), null);
        public string PopupReplaceText
        {
            get { return (string)this.GetValue(PopupReplaceTextProperty); }
            set { base.SetValue(PopupReplaceTextProperty, value); }
        }

        public static readonly DependencyProperty SelectModeProperty = DependencyProperty.Register("SelectMode", typeof(SelectMode), typeof(RichTextBlock), new PropertyMetadata(SelectMode.ReadOnly, OnSelectModePropertyChanged));
        public SelectMode SelectMode
        {
            get { return (SelectMode)this.GetValue(SelectModeProperty); }
            set { base.SetValue(SelectModeProperty, value); }
        }

        private static void OnSelectModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RichTextBlock rtb = d as RichTextBlock;
            rtb.Text = string.Empty;
            rtb.IsReadOnly = ((SelectMode)e.NewValue != SelectMode.Edit);
            rtb.UpdateChildReadOnlyState((SelectMode)e.NewValue);
        }

        #endregion

        #region Static Properties

        public static string LiquidNamespace = "xmlns:liquid=\"clr-namespace:Liquid;assembly=Liquid.RichText\"";
        public static string XMLClose = "</LiquidRichText>";
        public static string TextElement = "text";
        public static string TagElement = "tag";
        public static string StyleElement = "style";
        public static string TableStyleElement = "tablestyle";
        public static string NewlineElement = "newline";
        public static string HorizontalRuleElement = "horizontalrule";
        public static string XamlElement = "xaml";
        public static string DefaultStyle = "Normal";
        public static string CustomStyle = "Custom";
        public static double SuperScriptMultiplier = 0.60;
        public static double SubScriptMultiplier = 0.60;

        /// <summary>
        /// Gets or sets the clipboard data used when copying/pasting
        /// </summary>
        public static string DataClipboard { get; set; }
        public static List<string> StyleClipboard { get; set; }
        public static List<ContentMetadata> MetadataClipboard { get; set; }

        public static List<RichTextBoxStyle> StylesClipboard { get; set; }

        private static string ClipboardWatch = string.Empty;

        /// <summary>
        /// Gets or sets the style used by the style painter
        /// </summary>
        public static string PainterClipboard { get; set; }

        private static int NextStyleNumber = 0;


        #endregion

        #region Protected Properties

        protected bool _setWidth = true;
        protected double _zoom = 1;
        protected bool _mouseDown = false;

        #endregion

        #region Private Properties

        private string _richTextURL = string.Empty;
        private RichTextBoxPosition _dragBegin = new RichTextBoxPosition();
        private RichTextBoxPosition _cursorPosition = new RichTextBoxPosition();
        private RichTextBoxPosition _selectedStart = new RichTextBoxPosition();
        private RichTextBoxPosition _selectedEnd = new RichTextBoxPosition();
        private int _length = 0;
        private bool _painterDown = false;
        private int _ticksSinceLastMouseDown = 0;
        private UIElement _lastClickedElement = null;
        private History _history = new History();
        private List<Image> _imagesLoading = new List<Image>();
        private List<RichTextHilight> _hilights = new List<RichTextHilight>();
        private int _lastIndex = 0;
        private bool _usingIME = false;
        private Brush _errorWordBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        private bool _runInitialSpellCheck = false;
        private RichTextBoxPosition _nextSpellCheckPosition;
        private int _lastSpellCheckIndex = -1;
        private List<string> _spellCheckedWords = new List<string>();
        private RichTextHilight _currentClickedHilight = null;
        private bool _recalculateSelectionOnMouseMove = true;
        private bool _isRightToLeft = false;
        private DispatcherTimer _timer = new DispatcherTimer();
        private bool _enableContextMenu = false;
        private bool _inParagraph = false;
        private string _paragraphTag = string.Empty;
        private bool _inLink = false;
        private int _linkElementCount = 0;
        private bool _inList = false;
        private string _listTag = string.Empty;
        private string _listAttributes = string.Empty;
        private bool _objectSelected = false;
        private Brush _origForeground = null;
        private FontFamily _origFontFamily = null;
        private double _origFontSize = 0;
        private FontWeight _origFontWeight = FontWeights.Normal;
        private FontStyle _origFontStyle = FontStyles.Normal;
        private bool _usingTempStyle = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the current row index within the current active table
        /// </summary>
        public int TableRowIndex
        {
            get
            {
                int row = -1;

                if (Root.ActiveTable != null)
                {
                    FrameworkElement parent = (FrameworkElement)FocusedRTB.Parent;
                    row = (int)parent.GetValue(Grid.RowProperty);
                }

                return row;
            }
        }

        /// <summary>
        /// Gets the element position of the cursor
        /// </summary>
        public RichTextBoxPosition ElementCursorPosition
        {
            get { return _cursorPosition; }
        }

        /// <summary>
        /// Gets the element position of the selection start
        /// </summary>
        public RichTextBoxPosition ElementSelectionStart
        {
            get { return _selectedStart; }
        }

        /// <summary>
        /// Gets the element position of the selection end
        /// </summary>
        public RichTextBoxPosition ElementSelectionEnd
        {
            get { return _selectedEnd; }
        }

        /// <summary>
        /// Returns a collection of UIElement objects that make up the RichText document
        /// </summary>
        public UIElementCollection Children
        {
            get { return ElementChildren.ContentChildren; }
        }

        /// <summary>
        /// Gets a value indicating whether selection is in progress
        /// </summary>
        public bool SelectionInProgress
        {
            get { return _mouseDown; }
        }

        /// <summary>
        /// Gets or sets the last mouse cursor position
        /// </summary>
        public Point LastMousePosition { get; set; }

        /// <summary>
        /// Gets or sets whether the content is fully loaded and ready for editing
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Gets or sets whether the RichTextBlock should pay special attention to a mouse up event
        /// </summary>
        public bool ExternalDragStart { get; set; }

        /// <summary>
        /// Gets or sets the opacity for certain control elements when disabled
        /// </summary>
        public double DisabledOpacity { get; set; }

        /// <summary>
        /// Gets or sets whether the right-mouse click displays the context menu
        /// </summary>
        public bool EnableContextMenu
        {
            get { return _enableContextMenu; }
            set
            {
                _enableContextMenu = value;
                if (value && ElementChildren != null)
                {
                    try
                    {
                        HtmlPage.Document.AttachEvent("oncontextmenu", OnRightMouseClick);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the unique ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets a URL to load RichText from
        /// </summary>
        public string RichTextURL
        {
            get { return _richTextURL; }
            set
            {
                _richTextURL = value;
                ProcessLoad(value);
            }
        }

        /// <summary>
        /// Gets the content as XAML
        /// </summary>
        public string XAML
        {
            get { return Save(Format.XAML, RichTextSaveOptions.ListStyles); }
        }

        /// <summary>
        /// Gets the index of the selection start
        /// </summary>
        public int ContentSelectionStart
        {
            get { return Root.FocusedRTB._selectedStart.GlobalIndex; }
        }

        /// <summary>
        /// Gets the length of the selected content
        /// </summary>
        public int ContentSelectionLength
        {
            get { return Root.FocusedRTB._selectedEnd.GlobalIndex - FocusedRTB._selectedStart.GlobalIndex; }
        }

        /// <summary>
        /// Gets the length of the content
        /// </summary>
        public int ContentLength
        {
            get { return _length; }
        }

        /// <summary>
        /// Gets or sets the selected content styles
        /// </summary>
        public List<StyleSelection> SelectionStyles { get; set; }

        /// <summary>
        /// Gets the style of the selected content
        /// </summary>
        public RichTextBoxStyle SelectionStyle
        {
            get { return (SelectionStyles.Count > 0 ? SelectionStyles[0].Style : null); }
        }

        /// <summary>
        /// Gets or sets the selected content metadata
        /// </summary>
        public List<MetadataSelection> SelectionMetadatas { get; set; }

        /// <summary>
        /// Gets the style of the selected content
        /// </summary>
        public ContentMetadata SelectionMetadata
        {
            get { return (SelectionMetadatas.Count > 0 ? SelectionMetadatas[0].Metadata : null); }
        }

        /// <summary>
        /// Gets the list type of the selected content
        /// </summary>
        public Bullet SelectionListType
        {
            get { return (SelectionStyles.Count > 0 ? SelectionStyles[0].ListType : null); }
        }

        /// <summary>
        /// Gets the alignment of the selected content
        /// </summary>
        public HorizontalAlignment SelectionAlignment
        {
            get
            {
                if (Root.ActiveTable != null && Root.ActiveTable.Selected.Count > 0)
                {
                    return (HorizontalAlignment)Root.ActiveTable.Selected[0].Element.GetValue(FrameworkElement.HorizontalAlignmentProperty);
                }
                else
                {
                    return (SelectionStyles.Count > 0 ? SelectionStyles[0].Alignment : HorizontalAlignment.Left);
                }
            }
        }

        /// <summary>
        /// Gets the vertical alignment of the selected content
        /// </summary>
        public VerticalAlignment SelectionVerticalAlignment
        {
            get
            {
                if (Root.ActiveTable != null && Root.ActiveTable.Selected.Count > 0)
                {
                    return (VerticalAlignment)Root.ActiveTable.Selected[0].Element.GetValue(FrameworkElement.VerticalAlignmentProperty);
                }
                else
                {
                    return (SelectionStyles.Count > 0 ? SelectionStyles[0].VerticalAlignment : VerticalAlignment.Top);
                }
            }
        }

        /// <summary>
        /// Gets or sets the currently active table
        /// </summary>
        public Table ActiveTable { get; set; }

        /// <summary>
        /// Gets or sets the number of operations that can be undone
        /// </summary>
        public int UndoHistory
        {
            get { return Root.History.MaxEvents; }
            set { Root.History.MaxEvents = value; }
        }

        /// <summary>
        /// Gets the undo history
        /// </summary>
        public History History { get { return _history; } }

        /// <summary>
        /// Gets or sets whether selection mouse shortcuts such as double-clicking to select a word is enabled
        /// </summary>
        public bool EnableQuickSelection { get; set; }

        /// <summary>
        /// Gets or sets the brush to use when underlining miss-spelt words
        /// </summary>
        public Brush ErrorWordBrush
        {
            get { return _errorWordBrush; }
            set { _errorWordBrush = value; }
        }

        /// <summary>
        /// Gets whether the initial spell checking is in progress
        /// </summary>
        public bool IsSpellChecking { get { return _runInitialSpellCheck; } }

        /// <summary>
        /// Gets or sets the number of words to check at a time during the initial spell check
        /// </summary>
        public int SpellChecksPerCycle { get; set; }

        /// <summary>
        /// Gets or sets whether text input should slow from right-to-left
        /// </summary>
        public bool IsRightToLeft
        {
            get { return _isRightToLeft; }
            set { _isRightToLeft = value; }
        }

        /// <summary>
        /// Gets or sets whether typed URLs are automatically formatted
        /// </summary>
        public bool EnableURLRecognition { get; set; }

        /// <summary>
        /// Gets or sets whether text pattern recognition is enabled
        /// </summary>
        public bool EnablePatternRecognition { get; set; }

        /// <summary>
        /// Gets or sets text patterns to look for
        /// </summary>
        public List<string> TextPatterns
        {
            get { return AutoComplete.Patterns; }
            set { AutoComplete.Patterns = value; }
        }

        /// <summary>
        /// Gets or sets the base URL used when importing/exporting to HTML
        /// </summary>
        public string BaseURL { get; set; }

        /// <summary>
        /// Gets or sets the prefix to be applied to all images/URLs
        /// </summary>
        public string URLPrefix { get; set; }

        /// <summary>
        /// Gets or sets the style used for links
        /// </summary>
        public RichTextBoxStyle LinkStyle { get; set; }

        /// <summary>
        /// Gets or sets the general content styles
        /// </summary>
        public Dictionary<string, RichTextBoxStyle> Styles = new Dictionary<string, RichTextBoxStyle>();

        /// <summary>
        /// Gets or sets the table styles
        /// </summary>
        public Dictionary<string, RichTextBoxTableStyle> TableStyles = new Dictionary<string, RichTextBoxTableStyle>();

        /// <summary>
        /// Gets the current line number where the cursor is positioned
        /// </summary>
        public int LineNumber
        {
            get
            {
                if (ElementChildren != null && _cursorPosition.Element != null)
                {
                    return ElementChildren.GetRowIndexForElement(_cursorPosition.Element) + 1;
                }
                else
                {
                    return 1;
                }
            }
        }

        /// <summary>
        /// Gets the total number of lines in the document
        /// </summary>
        public int NumberOfLines
        {
            get { return ElementChildren != null ? ElementChildren.Rows.Count : 0; }
        }

        /// <summary>
        /// Gets or sets any namespaces used by custom UIElements
        /// </summary>
        public List<string> CustomNamespaces { get; set; }

        /// <summary>
        /// Gets or sets whether spell checking related events are raised
        /// </summary>
        public bool EnableSpellCheck { get; set; }

        /// <summary>
        /// Gets the current selection as plain text
        /// </summary>
        public new string SelectedText
        {
            get { return SaveSelectionToText(_selectedStart, _selectedEnd, Format.Text, RichTextSaveOptions.None); }
        }

        /// <summary>
        /// Gets or sets whether the default H1,H2,H3 and Normal styles are created when content is loaded
        /// </summary>
        public bool CreateDefaultStyles { get; set; }

        /// <summary>
        /// Gets or sets whether undo/redo is enabled
        /// </summary>
        public bool IsHistoryEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether objects such as images canbe selected by clicking them
        /// </summary>
        public bool EnableObjectSelection { get; set; }

        /// <summary>
        /// Gets or sets whether links should use a global style
        /// </summary>
        public bool EnableGlobalLinkStyle { get; set; }

        /// <summary>
        /// Gets or sets whether content styles from a source other than this RichTextBlock instance are used in a paste operation
        /// </summary>
        public bool EnablePastingExternalStyles { get; set; }

        /// <summary>
        /// Gets or sets whether links are rendered using link buttons
        /// </summary>
        public bool EnableLinkButtons { get; set; }

        /// <summary>
        /// Gets or sets whether the popup bubble should be constrained to a specific area of the screen
        /// </summary>
        public bool RestrictPopupBubble { get; set; }
        /// <summary>
        /// Gets or sets the area the popup bubble must be constrained to
        /// </summary>
        public Rect PopupBubbleArea { get; set; }

        public RichTextPanel Panel
        {
            get { return ElementChildren; }
        }

        public List<RichTextHilight> Hilights
        {
            get { return _hilights; }
        }

        #endregion

        #region Public Events

        public event RichTextBoxEventHandler ContentDropped;
        public event KeyEventHandler GlobalKeyDown;
        public new event RichTextBoxEventHandler SelectionChanged;
        public event RichTextBoxEventHandler Diagnostic;
        public event RichTextBoxEventHandler ElementWrite;
        public event RichTextBoxEventHandler ElementRead;
        public event RichTextBoxEventHandler TextFound;
        public event RichTextBoxEventHandler LinkClicked;
        public event RichTextBoxEventHandler ContentChanged;
        public event RichTextBoxEventHandler CheckWord;
        public event RichTextBoxEventHandler IncorrectWordNotify;
        public event RichTextBoxEventHandler IncorrectWordAdd;
        public event RichTextBoxEventHandler IncorrectWordFinished;
        public event RichTextBoxEventHandler CursorMoved;
        public event RichTextBoxEventHandler ShowContextMenu;
        public event RichTextBoxHtmlEventHandler WriteStyleToHtml;
        public event RichTextBoxEventHandler TextPatternMatch;
        public new event MouseButtonEventHandler MouseLeftButtonUp;
        public new event MouseButtonEventHandler MouseLeftButtonDown;
        public event RichTextBoxEventHandler StyleCreated;
        public event RichTextBoxEventHandler StyleDeleted;
        public event RichTextBoxEventHandler BeforePaste;
        public event RichTextBoxEventHandler ObjectDoubleClick;
        public event RichTextBoxEventHandler UnsupportedImageFormat;
        public event RichTextBoxEventHandler LoadError;
        public event RichTextBoxEventHandler FindEndReached;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #endregion

        #region Constructor

        static RichTextBlock()
        {
            DataClipboard = string.Empty;
            StyleClipboard = new List<string>();
            MetadataClipboard = new List<ContentMetadata>();
            PainterClipboard = string.Empty;
            StylesClipboard = new List<RichTextBoxStyle>();
        }

        public RichTextBlock()
        {
            DefaultStyleKey = this.GetType();
            IsTabStop = true;
            TabNavigation = KeyboardNavigationMode.Once;
            AcceptsReturn = true;
            CreateDefaultStyles = true;
            IsHistoryEnabled = true;
            ID = string.Empty;
            BaseURL = string.Empty;
            URLPrefix = string.Empty;

            DisabledOpacity = 0.7;
            EnableQuickSelection = true;
            SpellChecksPerCycle = 4;
            EnableGlobalLinkStyle = false;
            EnablePastingExternalStyles = true;
            EnableLinkButtons = false;

            this.TextChanged += new TextChangedEventHandler(OnTextChanged);

            AutoComplete.Hyperlink += new AutoCompleteEventHandler(AutoComplete_Hyperlink);
            AutoComplete.PatternMatch += new AutoCompleteEventHandler(AutoComplete_PatternMatch);

            SelectionStyles = new List<StyleSelection>();
            SelectionMetadatas = new List<MetadataSelection>();
            EnableURLRecognition = true;
            EnablePatternRecognition = false;
            IsRightToLeft = false;
            FocusedRTB = this;
            ActiveTable = null;
            CustomNamespaces = new List<string>();
            CustomNamespaces.Add(LiquidNamespace);
            EnableObjectSelection = false;

            LinkStyle = new RichTextBoxStyle()
            {
                Decorations = TextDecorations.Underline,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255))
            };

            EnableSpellCheck = true;
            IsReady = false;

            RestrictPopupBubble = false;
            PopupBubbleArea = new Rect();
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Determines whether a metadata object contains link data
        /// </summary>
        /// <param name="metadata">ContentMetadata object</param>
        /// <returns>True if it is a Link</returns>
        public static bool IsLinkMetadata(ContentMetadata metadata)
        {
            bool result = false;

            if (metadata != null)
            {
                result = metadata.IsLink;
            }

            return result;
        }

        /// <summary>
        /// Converts a block of CSS styles to RichText styles
        /// </summary>
        /// <param name="styles">CSS styles</param>
        /// <returns>RichText styles</returns>
        public static Dictionary<string, object> GetStylesFromCSS(string styles)
        {
            string[] split1;
            RichTextBoxStyle style;
            RichTextBoxTableStyle tableStyle;
            Dictionary<string, object> results = new Dictionary<string, object>();
            Match templateStart;
            Match templateEnd;
            string temp;
            string[] split;

            templateStart = Regex.Match(styles, @"/\* Template Start \*/", RegexOptions.IgnoreCase);
            templateEnd = Regex.Match(styles, @"/\* Template End \*/", RegexOptions.IgnoreCase);
            
            if (templateStart.Success && templateEnd.Success)
            {
                styles = styles.Remove(templateStart.Index, (templateEnd.Index + templateEnd.Length) - templateStart.Index);
            }

            styles = Regex.Replace(styles, @"/\* Template Start \*/.*?/\* Template End \*/", "", RegexOptions.IgnoreCase);
            split1 = styles.Split('}');

            foreach (string s in split1)
            {
                temp = s.Trim();

                if (temp.Length > 0)
                {
                    if (temp.ToLower().StartsWith(".table"))
                    {
                        split = temp.TrimStart('.').Split(' ');
                        if (results.ContainsKey(split[0]))
                        {
                            tableStyle = (RichTextBoxTableStyle)results[split[0]];
                        }
                        else
                        {
                            tableStyle = new RichTextBoxTableStyle();
                            results.Add(split[0], tableStyle);
                        }
                        tableStyle.FromHTML(split[0], s + "}");
                    }
                    else
                    {
                        style = new RichTextBoxStyle();
                        style.FromHTML(s + "}");
                        results.Add(style.ID, style);
                    }
                }
            }

            return results;
        }

        #endregion

        #region Internal Properties

        internal RichTextBlock ParentRTB = null;
        public RichTextBlock FocusedRTB = null;
        internal RichTextBlock SpellCheckingRTB = null;
        internal AutoComplete AutoComplete = new AutoComplete();

        /// <summary>
        /// Gets the root level RichTextBlock
        /// </summary>
        /// <returns></returns>
        internal RichTextBlock Root
        {
            get
            {
                RichTextBlock current = this;

                while (current.ParentRTB != null)
                {
                    current = current.ParentRTB;
                }

                return current;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads RichText from the provided URL
        /// </summary>
        /// <param name="url">URL of a valid RichText XML file</param>
        internal void ProcessLoad(string url)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.XmlResolver = new XmlXapResolver();
            XmlReader reader = XmlReader.Create(url);

            reader.MoveToContent();

            RichText = reader.ReadOuterXml();
        }

        /// <summary>
        /// Inserts the string at the cursor
        /// </summary>
        /// <param name="text">Text to insert</param>
        internal void ProcessInsertText(string text)
        {
            string style;
            string[] split;
            RichTextBoxPosition tempPos;
            List<UIElement> elements = new List<UIElement>();
            int i;

            if (SelectMode != SelectMode.Edit)
            {
                return;
            }

            if (_cursorPosition != null)
            {
                style = GetStyle(_cursorPosition.Element);
                split = text.Replace("\n", "").Split('\r');

                elements.Add(CreateTextBlock(split[0], _cursorPosition.Element, false));
                for (i = 1; i < split.Length; i++)
                {
                    elements.Add(new Newline() { Tag = new RichTextTag(style) });
                    elements.Add(CreateTextBlock(split[i], _cursorPosition.Element, false));
                }

                InsertRichTextAtCursor(string.Empty, elements, null, true, true);
                RefreshAllHilights();

                if (text.Length == 1 && Root.AutoComplete.HyperlinkIndex == -1)
                {
                    if (!Char.IsLetterOrDigit(text[0]))
                    {
                        tempPos = new RichTextBoxPosition(_cursorPosition.GlobalIndex - 1);
                        tempPos.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
                        InformWordEdited(tempPos);
                    }
                }
            }

            Root.AutoComplete.AddCharacter(text, _cursorPosition.GlobalIndex - 1);
            RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
        }

        /// <summary>
        /// Inserts a newline at the cursor position
        /// </summary>
        internal void ProcessInsertNewline()
        {
            Bullet previousBullet;
            Bullet newBullet;
            RichTextPanelRow parent;
            bool insertNewBullet = false;
            int index;
            RichTextState before = null;
            RichTextBoxPosition start;
            RichTextBoxPosition end;
            RichTextBoxPosition tempPosition = null;
            UIElement temp;
            int startIndex;
            int endIndex;
            string styleID = string.Empty;
            int i;

            if (SelectMode != SelectMode.Edit)
            {
                return;
            }

            CheckForSelectedObjects();
            if (ContentSelectionLength > 0)
            {
                before = DeleteSelected(false);
            }

            start = new RichTextBoxPosition(_cursorPosition);
            previousBullet = GetListType(_cursorPosition.Element);

            if (IsRightToLeft)
            {
                int rowIndex = ElementChildren.GetRowIndexForElement(_cursorPosition.Element);
                int rowAfter = rowIndex;
                string tempXAML;

                if (rowIndex >= 0)
                {
                    temp = GetLineStartElement(rowIndex);

                    if (_cursorPosition.Element == temp && _cursorPosition.Index == 0)
                    {   // Cursor is at the start of a line
                        End(false, false);
                        ProcessInsert(new Newline(), false, false);
                    }
                    else
                    {
                        for (i = rowIndex + 1; i < ElementChildren.Rows.Count; i++)
                        {
                            if (ElementChildren.Rows[i - 1].End is Newline)
                            {
                                rowAfter = i;
                                break;
                            }
                        }
                        _selectedStart.Element = temp;
                        _selectedStart.Index = 0;
                        _selectedStart.CalculateGlobalIndex(ElementChildren.ContentChildren);
                        SetSelection(_selectedStart, start, false);

                        if (ContentSelectionLength > 0)
                        {
                            tempXAML = GetSelectedRichText();
                            DeleteSelected(false);

                            _cursorPosition.Element = ElementChildren.Rows[rowAfter].Start;
                            _cursorPosition.Index = 0;
                            _cursorPosition.CalculateGlobalIndex(ElementChildren.ContentChildren);
                            InsertRichTextAtCursor(tempXAML, null, null, false, false);
                        }

                        tempPosition = new RichTextBoxPosition(_cursorPosition);
                        ProcessInsert(new Newline(), false, false);
                        _cursorPosition = tempPosition;
                        SetCursor(tempPosition, false);
                        ProcessClearSelection(true);
                    }
                }
            }
            else
            {
                styleID = GetStyle(_cursorPosition.Element);
                ProcessInsert(new Newline(), false, true);
            }

            if (previousBullet != null)
            {
                insertNewBullet = (previousBullet.Type == BulletType.Indent);
                if (!insertNewBullet)
                {
                    parent = ElementChildren.GetRowForElement(previousBullet);
                    index = ElementChildren.ContentChildren.IndexOf(previousBullet);
                    if (index < ElementChildren.ContentChildren.Count - 1)
                    {
                        if (ElementChildren.ContentChildren[index + 1] is TextBlockPlus)
                        {
                            insertNewBullet = (((TextBlockPlus)ElementChildren.ContentChildren[index + 1]).Text.Length > 0);
                        }
                        else if (!(ElementChildren.ContentChildren[index + 1] is Newline))
                        {
                            insertNewBullet = true;
                        }
                    }
                }

                if (insertNewBullet)
                {
                    // Create a new bullet, the same as the previous
                    newBullet = new Bullet();
                    newBullet.Type = previousBullet.Type;
                    newBullet.Number = previousBullet.Number + 1;
                    newBullet.Indent = previousBullet.Indent;

                    ProcessInsert(newBullet, false, true);

                    index = ElementChildren.ContentChildren.IndexOf(_cursorPosition.Element);

                    switch (newBullet.Type)
                    {
                        case BulletType.Number:
                            ApplyBulletsToSelection(index, index, Formatting.NumberList, null);
                            break;
                        case BulletType.Bullet:
                            ApplyBulletsToSelection(index, index, Formatting.BulletList, null);
                            break;
                        case BulletType.Image:
                            ApplyBulletsToSelection(index, index, Formatting.BulletImageList, previousBullet.BulletImage);
                            break;
                    }
                }
                else
                {
                    // Remove the previous bullet and update the cursor
                    tempPosition = _cursorPosition;
                    _cursorPosition = new RichTextBoxPosition(previousBullet, 0);
                    _cursorPosition.CalculateGlobalIndex(ElementChildren.ContentChildren);
                    SetSelection(_cursorPosition.GlobalIndex, 1, false);

                    before = DeleteSelected(false);

                    _cursorPosition.GlobalIndex = tempPosition.GlobalIndex - 1;
                    _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
                    UpdateCursor();
                }
            }

            start.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
            end = new RichTextBoxPosition(_cursorPosition);

            startIndex = ElementChildren.ContentChildren.IndexOf(start.Element) + 1;
            endIndex = ElementChildren.ContentChildren.IndexOf(end.Element) - 1;

            Root.History.Add(this, HistoryCommand.InsertContent, before, new RichTextState(SaveSelectionAsElements(startIndex, endIndex), start.GlobalIndex, end.GlobalIndex - start.GlobalIndex));
            
            OnCursorMoved();
            RaiseRichTextEvent(Root.CursorMoved, this, new RichTextBoxEventArgs(), false);
            RefreshAllHilights();

            Root.AutoComplete.AddCharacter("\r", _cursorPosition.GlobalIndex - 1);

            RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);

            if (!insertNewBullet && Utility.IsStyleAHeading(styleID) && Root.Styles.ContainsKey("Normal"))
            {
                ApplyFormatting(Formatting.Style, "Normal");
            }
        }

        /// <summary>
        /// Inserts any UIElement at the cursor position
        /// </summary>
        /// <param name="element">Any element derived from UIElement</param>
        /// <param name="updateHistory">Indicates whether the insert should be recorded in the Undo state</param>
        /// <param name="updateSingleLine">Indicates whether only the insertion line should be updated</param>
        internal void ProcessInsert(UIElement element, bool updateHistory, bool updateSingleLine)
        {
            List<UIElement> elements = new List<UIElement>();
            string styleID = GetStyle(_cursorPosition.Element);

            ApplyStyleToElement(element, styleID);
            elements.Add(element);
            InsertRichTextAtCursor(string.Empty, elements, null, updateHistory, updateSingleLine);
        }

        /// <summary>
        /// Inserts a block of rich text at the cursor
        /// </summary>
        /// <param name="richText">Rich Text xml</param>
        /// <returns>Number of content elements created</returns>
        internal void ProcessInsert(string richText, ContentMetadata metadata)
        {
            InsertRichTextAtCursor(richText, null, metadata, true, false);
            RefreshAllHilights();

            RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
        }

        /// <summary>
        /// Deletes the selected text or the character before or after as indicated by the parameter
        /// </summary>
        /// <param name="deleteForward">True to delete the character infront of the cursor</param>
        internal void ProcessDelete(bool deleteForward)
        {
            List<CellReference> selectedCells;
            int rowIndex;
            UIElement temp;
            int length;
            bool lineEnd = false;
            string xaml;
            int i;

            if (ElementChildren == null)
            {
                return;
            }

            if (SelectMode == SelectMode.Edit)
            {
                if (Root.ActiveTable != null)
                {
                    selectedCells = Root.ActiveTable.Selected;

                    if (selectedCells.Count > 0)
                    {
                        foreach (CellReference c in selectedCells)
                        {
                            if (c.Element is RichTextBlock)
                            {
                                ((RichTextBlock)c.Element).Clear();
                            }
                        }
                        RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
                        return;
                    }
                }

                CheckForSelectedObjects();

                if (IsRightToLeft)
                {
                    deleteForward ^= true;
                }

                if (ContentSelectionLength == 0)
                {
                    i = ElementChildren.ContentChildren.IndexOf(_cursorPosition.Element);

                    if (_cursorPosition.GlobalIndex == 0 && !deleteForward)
                    {
                        return;
                    }
                    else if (deleteForward && i < ElementChildren.ContentChildren.Count - 1 && _cursorPosition.Element is TextBlockPlus)
                    {
                        if (ElementChildren.ContentChildren[i + 1] is Newline && _cursorPosition.Index == _cursorPosition.Element.ToString().Length && LineNumber == NumberOfLines)
                        {
                            return;
                        }
                    }
                }

                rowIndex = ElementChildren.GetRowIndexForElement(_cursorPosition.Element);
                if (rowIndex >= 0)
                {
                    i = ElementChildren.ContentChildren.IndexOf(ElementChildren.Rows[rowIndex].End);
                    if (i - 1 >= 0)
                    {
                        temp = ElementChildren.ContentChildren[i - 1];
                        if (temp == _cursorPosition.Element)
                        {
                            length = (temp is TextBlockPlus ? ((TextBlockPlus)temp).Text.Length : 0);
                            lineEnd = (_cursorPosition.Index == length);
                        }
                    }
                }

                if (IsRightToLeft && lineEnd && rowIndex > 0)
                {
                    SelectLine();
                    xaml = GetSelectedRichText().Replace("<Newline />", "");
                    DeleteSelected(false);
                    MoveCursorUpDown(rowIndex - 1);
                    Home(false, false);
                    ProcessInsert(xaml, null);
                }
                else
                {
                    if (ContentSelectionLength == 0)
                    {
                        _selectedStart = new RichTextBoxPosition(_selectedStart);
                        _selectedEnd = new RichTextBoxPosition(_selectedEnd);

                        if (_selectedStart.GlobalIndex >= 0)
                        {
                            _selectedStart.GlobalIndex = (deleteForward ? _cursorPosition.GlobalIndex : _cursorPosition.GlobalIndex - 1);
                            _selectedStart.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
                            _selectedEnd.GlobalIndex = _selectedStart.GlobalIndex + 1;
                            _selectedEnd.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
                        }
                    }

                    if (_selectedStart.GlobalIndex >= 0)
                    {
                        DeleteSelected(true);
                    }
                }
                SetCursor(_cursorPosition, true);
                Root.AutoComplete.Reset();
                RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);

                RefreshAllHilights();
            }
        }

        /// <summary>
        /// Applys formatting to the selected content
        /// </summary>
        /// <param name="formatting">Formatting commang</param>
        /// <param name="param">Optional formatting parameters</param>
        internal void ProcessApplyFormatting(Formatting formatting, object param)
        {
            if (SelectMode == SelectMode.Edit && ElementChildren != null)
            {
                if (Root.ActiveTable != null && Root.ActiveTable.Selected.Count > 0)
                {
                    switch(formatting)
                    {
                        case Formatting.AlignTop:
                            AlignTableCells(formatting);
                            break;
                        case Formatting.AlignMiddle:
                            AlignTableCells(formatting);
                            break;
                        case Formatting.AlignBottom:
                            AlignTableCells(formatting);
                            break;
                        default:
                            ApplyCellFormatting(formatting, param, true);
                            break;
                    }
                    SelectionHasChanged();
                }
                else
                {
                    ApplyFormattingToSelection(formatting, param, true);
                }

                RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
            }
        }

        /// <summary>
        /// Shows the cursor
        /// </summary>
        internal void ProcessShowCursor()
        {
            if (SelectMode == SelectMode.ReadOnly || !IsEnabled || ElementCursor == null || FocusManager.GetFocusedElement() != this || ContentSelectionLength > 0)
            {
                return;
            }

            ElementCursor.Visibility = Visibility.Visible;
            ElementCursorBlink.Begin();
        }

        /// <summary>
        /// Hides the cursor
        /// </summary>
        internal void ProcessHideCursor()
        {
            if (!IsEnabled || ElementCursor == null)
            {
                return;
            }

            ElementCursor.Visibility = Visibility.Collapsed;
            ElementCursorBlink.Stop();
        }

        /// <summary>
        /// Moves the cursor left
        /// </summary>
        internal void ProcessCursorLeft()
        {
            TextBlockPlus temp;
            int newStart = 0;
            int newLength = 0;
            int toMove = -1;
            int i;

            if (SelectMode == SelectMode.ReadOnly)
            {
                return;
            }

            if (IsRightToLeft)
            {
                if (GetLineIndexOffset(_cursorPosition, 1) > 0)
                {
                    i = ElementChildren.GetRowIndexForElement(_cursorPosition.Element);
                    if (i > 0)
                    {
                        MoveCursorUpDown(i - 1);
                        Home(false, false);
                    }
                    return;
                }
                else
                {
                    toMove = 1;
                }
            }

            UpdateFromIME(false);
            MoveCursor(_cursorPosition, toMove);

            // Move the cursor to the beginning of the current word when CTRL is held down
            if (IsCtrlDown() && _cursorPosition.Element is TextBlockPlus)
            {
                temp = (TextBlockPlus)_cursorPosition.Element;
                GetWordAtIndex(temp.Text, _cursorPosition.Index, out newStart, out newLength);

                toMove = newStart - _cursorPosition.Index;
                if (toMove < 0)
                {
                    MoveCursor(_cursorPosition, toMove);
                }
                else
                {
                    MoveCursor(_cursorPosition, -1);
                    GetWordAtIndex(temp.Text, _cursorPosition.Index, out newStart, out newLength);
                    toMove = newStart - _cursorPosition.Index;
                    if (toMove < 0)
                    {
                        MoveCursor(_cursorPosition, toMove);
                    }
                }
            }

            SetCursor(_cursorPosition, true);
            UpdateSelectionOnCursorKeyPress();
        }

        /// <summary>
        /// Moves the cursor right
        /// </summary>
        internal void ProcessCursorRight()
        {
            TextBlockPlus temp;
            int newStart = 0;
            int newLength = 0;
            int toMove = 1;
            int i;

            if (SelectMode == SelectMode.ReadOnly)
            {
                return;
            }

            if (IsRightToLeft)
            {
                if (GetLineIndexOffset(_cursorPosition, -1) < 0)
                {
                    i = ElementChildren.GetRowIndexForElement(_cursorPosition.Element);
                    if (i < ElementChildren.Rows.Count - 1)
                    {
                        MoveCursorUpDown(i + 1);
                        End(false, false);
                    }
                    return;
                }
                else
                {
                    toMove = -1;
                }
            }

            if (!UpdateFromIME(false))
            {
                MoveCursor(_cursorPosition, toMove);

                // Move the cursor to the end of the current word when CTRL is held down
                if (IsCtrlDown() && _cursorPosition.Element is TextBlockPlus)
                {
                    temp = (TextBlockPlus)_cursorPosition.Element;
                    GetWordAtIndex(temp.Text, _cursorPosition.Index, out newStart, out newLength);

                    toMove = (newLength - (_cursorPosition.Index - newStart)) + 1;
                    if (toMove + _cursorPosition.Index > temp.Text.Length)
                    {
                        toMove--;
                    }

                    MoveCursor(_cursorPosition, toMove);
                }
            }

            SetCursor(_cursorPosition, true);
            UpdateSelectionOnCursorKeyPress();
        }

        /// <summary>
        /// Moves the cursor Up
        /// </summary>
        internal void ProcessCursorUp()
        {
            int parentIndex = ElementChildren.GetRowIndexForElement(_cursorPosition.Element);

            if (SelectMode == SelectMode.ReadOnly)
            {
                return;
            }

            if (parentIndex > 0)
            {
                MoveCursorUpDown(parentIndex - 1);
            }
        }

        /// <summary>
        /// Moves the cursor Down
        /// </summary>
        internal void ProcessCursorDown()
        {
            int parentIndex = ElementChildren.GetRowIndexForElement(_cursorPosition.Element);

            if (SelectMode == SelectMode.ReadOnly)
            {
                return;
            }

            if (parentIndex < ElementChildren.ContentChildren.Count - 1)
            {
                MoveCursorUpDown(parentIndex + 1);
            }
        }

        /// <summary>
        /// Moves the cursor to the top of the document
        /// </summary>
        internal void ProcessTop()
        {
            Home(true, true);
        }

        /// <summary>
        /// Moves the cursor to the top of the document
        /// </summary>
        internal void ProcessHome()
        {
            Home(IsCtrlDown(), IsShiftDown());
        }

        /// <summary>
        /// Moves the cursor to the end of the document
        /// </summary>
        internal void ProcessEnd()
        {
            End(IsCtrlDown(), IsShiftDown());
        }

        /// <summary>
        /// Moves the cursor to the bottom of the document
        /// </summary>
        internal void ProcessBottom()
        {
            End(true, true);
        }

        /// <summary>
        /// Clears all content from the RichTextBox
        /// </summary>
        internal void ProcessClear()
        {
            ProcessClearSelection(false);
            SetFromPlainText(string.Empty);
            _cursorPosition.GlobalIndex = 0;
            if (ElementChildren != null)
            {
                _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
            }
            SetCursor(_cursorPosition, false);
        }

        /// <summary>
        /// Selects the content for the provided range
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="length">Content length to select</param>
        internal void ProcessSelect(int startIndex, int length)
        {
            SetSelection(startIndex, length, true);
        }

        /// <summary>
        /// Selects the content of the current line
        /// </summary>
        internal void ProcessSelectLine(RichTextPanelRow row, bool selectNewline)
        {
            int i;

            if (row != null)
            {
                _selectedStart = new RichTextBoxPosition(row.Start, 0);
                _selectedStart.CalculateGlobalIndex(ElementChildren.ContentChildren);

                i = ElementChildren.ContentChildren.IndexOf(row.End);
                _selectedEnd = new RichTextBoxPosition(row.End, (row.End is TextBlockPlus ? ((TextBlockPlus)row.End).Text.Length : 0));

                if (selectNewline && row.End is Newline && i + 1 < ElementChildren.ContentChildren.Count)
                {
                    _selectedEnd.Index = 1;
                }
                _selectedEnd.CalculateGlobalIndex(ElementChildren.ContentChildren);

                SetSelection(_selectedStart, _selectedEnd, true);
            }
        }

        /// <summary>
        /// Selects all the content
        /// </summary>
        internal void ProcessSelectAll()
        {
            SetSelection(0, _length - 1, true);
        }

        /// <summary>
        /// Clears the cursor selection
        /// </summary>
        internal void ProcessClearSelection(bool indicateSelectionChanged)
        {
            if (ElementSelection != null)
            {
                _dragBegin = _cursorPosition;
                _selectedStart = new RichTextBoxPosition(_cursorPosition);
                _selectedEnd = new RichTextBoxPosition(_cursorPosition);

                ElementSelection.Visibility = Visibility.Collapsed;
                if (ElementCursor.Visibility == Visibility.Collapsed)
                {
                    ShowCursor();
                }
                if (indicateSelectionChanged)
                {
                    SelectionHasChanged();
                }

                ObjectDeselect(null);
                RefreshObjectMarker();
            }
        }

        /// <summary>
        /// Cuts the selected content and places it on the internal clipboard
        /// </summary>
        internal void ProcessCut()
        {
            if (SelectMode == SelectMode.Edit && ContentSelectionLength > 0)
            {
                ProcessCopy();
                ProcessDelete(false);
                RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
            }
        }

        /// <summary>
        /// Copies the selected content to the internal clipboard
        /// </summary>
        internal void ProcessCopy()
        {
            if (SelectMode != SelectMode.ReadOnly && ContentSelectionLength > 0)
            {
                RichTextBox.DataClipboard = GetSelectedRichText();

                StylesClipboard.Clear();
                foreach (StyleSelection style in SelectionStyles)
                {
                    StylesClipboard.Add(new RichTextBoxStyle(style.Style));
                }

                ClipboardWatch = SaveSelectionToText(_selectedStart.GlobalIndex, ContentSelectionLength, Format.Text, RichTextSaveOptions.ListStyles).Replace("\r", "\r\n");

                this.TextChanged -= new TextChangedEventHandler(OnTextChanged);
                base.Text = ClipboardWatch;
                base.Select(0, ClipboardWatch.Length);
                this.TextChanged += new TextChangedEventHandler(OnTextChanged);
            }
        }

        /// <summary>
        /// Pastes the content from the internal clipboard to the cursor position
        /// </summary>
        internal void ProcessPaste()
        {
            int startIndex = _cursorPosition.GlobalIndex;
            RichTextBoxEventArgs args = new RichTextBoxEventArgs();
            string clipboard = RichTextBlock.DataClipboard;

            if (SelectMode != SelectMode.Edit)
            {
                return;
            }

            RaiseRichTextEvent(Root.BeforePaste, this, args, false);

            if (!args.Cancel)
            {
                if (clipboard.Length > 0 && ClipboardWatch == Text)
                {
                    RichTextBoxStyle temp;
                    RichTextBoxStyle newStyle;

                    foreach (RichTextBoxStyle style in StylesClipboard)
                    {
                        newStyle = new RichTextBoxStyle(style);
                        temp = GetStyle(newStyle);

                        if (!EnablePastingExternalStyles && temp.ID.StartsWith(RichTextBlock.CustomStyle))
                        {
                            clipboard = clipboard.Replace("<Text Style=\"" + style.ID + "\"", "<Text Style=\"" + RichTextBlock.DefaultStyle + "\"");
                            continue;
                        }

                        if (style.ID != temp.ID)
                        {
                            clipboard = clipboard.Replace("<Text Style=\"" + style.ID + "\"", "<Text Style=\"" + temp.ID + "\"");
                        }
                    }

                    // Paste the contents of the internal clipboard
                    InsertRichTextAtCursor(clipboard, null, null, true, false);
                    RefreshAllHilights();
                }
                else
                {
                    if (clipboard.Length > 0)
                    {
                        // Paste the contents of the system clipboard
                        Insert(clipboard);
                    }
                }

                RunSpellChecker(startIndex, _cursorPosition.GlobalIndex);
                RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
            }
        }

        /// <summary>
        /// Copies the current style to the clipboard and pastes it to the next selection
        /// </summary>
        internal void ProcessPainter()
        {
            if (SelectMode == SelectMode.Edit)
            {
                PainterClipboard = GetStyle(_selectedStart.Element);
                _painterDown = true;
            }
        }

        /// <summary>
        /// Un-does the last operation
        /// </summary>
        internal void ProcessUndo(HistoryEvent e)
        {
            RichTextState before;
            RichTextState after;

            if (!IsHistoryEnabled || SelectMode != SelectMode.Edit)
            {
                return;
            }

            ProcessClearSelection(true);

            before = e.Parameter1 as RichTextState;
            after = e.Parameter2 as RichTextState;

            _cursorPosition.GlobalIndex = (before != null ? before.SelectionStart : after.SelectionStart);
            _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);

            switch (e.Command)
            {
                case HistoryCommand.Delete:
                    InsertRichTextAtCursor(string.Empty, before.Elements, null, false, false);
                    if (before.Length > 1)
                    {
                        SetSelection(before.SelectionStart, before.Length, true);
                    }
                    break;
                case HistoryCommand.InsertContent:
                    SetSelection(after.SelectionStart, after.Length, false);
                    DeleteSelected(false);
                    if (before != null)
                    {
                        InsertRichTextAtCursor(string.Empty, before.Elements, null, false, false);
                        if (before.Length > 1)
                        {
                            SetSelection(before.SelectionStart, before.Length, true);
                        }
                    }
                    break;
                case HistoryCommand.FormatChange:
                    SetSelection(after.SelectionStart, after.Length, false);
                    DeleteSelected(false);
                    InsertRichTextAtCursor(before.Text, null, null, false, false);
                    SetSelection(before.SelectionStart, before.Length, true);
                    break;
                case HistoryCommand.InsertColumn:
                    after.Table.DeleteColumn(after.Index);
                    break;
                case HistoryCommand.InsertRow:
                    after.Table.DeleteRow(after.Index);
                    break;
                case HistoryCommand.DeleteColumn:
                    after.Table.InsertColumn(after.Index, after.Column.Width, after.Elements);
                    break;
                case HistoryCommand.DeleteRow:
                    after.Table.InsertRow(after.Index, after.Row.Height, after.Elements);
                    break;
            }

            Root.History.ActionReversed();
            SetCursor(_cursorPosition, false);
            RefreshAllHilights();
            RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
        }

        /// <summary>
        /// Re-does the last operation
        /// </summary>
        internal void ProcessRedo(HistoryEvent e)
        {
            RichTextState before;
            RichTextState after;

            if (!IsHistoryEnabled || SelectMode != SelectMode.Edit)
            {
                return;
            }

            ProcessClearSelection(true);

            before = e.Parameter1 as RichTextState;
            after = e.Parameter2 as RichTextState;

            _cursorPosition.GlobalIndex = (before != null ? before.SelectionStart : after.SelectionStart);
            _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);

            switch (e.Command)
            {
                case HistoryCommand.Delete:
                    SetSelection(before.SelectionStart, before.Length, false);
                    DeleteSelected(false);
                    break;
                case HistoryCommand.InsertContent:
                    if (before != null)
                    {
                        SetSelection(before.SelectionStart, before.Length, false);
                        DeleteSelected(false);
                    }
                    InsertRichTextAtCursor(string.Empty, after.Elements, null, false, false);
                    if (after.Length > 1)
                    {
                        SetSelection(after.SelectionStart, after.Length, true);
                    }
                    break;
                case HistoryCommand.FormatChange:
                    SetSelection(before.SelectionStart, before.Length, false);
                    DeleteSelected(false);
                    InsertRichTextAtCursor(after.Text, null, null, false, false);
                    SetSelection(after.SelectionStart, after.Length, true);
                    break;
                case HistoryCommand.InsertColumn:
                    after.Table.InsertColumn(after.Index, after.Count);
                    break;
                case HistoryCommand.InsertRow:
                    after.Table.InsertRow(after.Index, after.Count);
                    break;
                case HistoryCommand.DeleteColumn:
                    after.Table.DeleteColumn(after.Index);
                    break;
                case HistoryCommand.DeleteRow:
                    after.Table.DeleteRow(after.Index);
                    break;
            }

            _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
            SetCursor(_cursorPosition, false);
            RefreshAllHilights();
            RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
        }

        /// <summary>
        /// Searches through the text content for the provided terms and moves the cursor to a match
        /// </summary>
        /// <param name="terms">Terms to search for</param>
        internal bool ProcessFind(string terms)
        {
            TextBlockPlus tempTextBlock;
            int indexInElements;
            bool success = false;
            RichTextBoxEventArgs args;
            int i;

            if (ElementChildren != null)
            {
                _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
                indexInElements = ElementChildren.ContentChildren.IndexOf(_cursorPosition.Element);

                for (i = indexInElements; i < ElementChildren.ContentChildren.Count; i++)
                {
                    if (ElementChildren.ContentChildren[i] is TextBlockPlus)
                    {
                        tempTextBlock = (TextBlockPlus)ElementChildren.ContentChildren[i];
                        success = FindInTextBlock(tempTextBlock, terms, (i == indexInElements ? _cursorPosition.Index : 0));
                        if (success)
                        {
                            break;
                        }
                    }
                }
                if (!success)
                {
                    args = new RichTextBoxEventArgs();
                    RaiseRichTextEvent(FindEndReached, this, args, false);

                    if (!args.Cancel)
                    {
                        for (i = 0; i <= indexInElements; i++)
                        {
                            if (ElementChildren.ContentChildren[i] is TextBlockPlus)
                            {
                                tempTextBlock = (TextBlockPlus)ElementChildren.ContentChildren[i];
                                success = FindInTextBlock(tempTextBlock, terms, 0);
                                if (success)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                FindInPlainText(terms);
            }

            return success;
        }

        internal void ProcessReplaceWord(string newWord)
        {
            if (_currentClickedHilight != null)
            {
                SetSelection(_currentClickedHilight.Start, _currentClickedHilight.End, true);
                ProcessInsertText(newWord);
            }

            HideSuggestionsPopup();
        }

        internal void ProcessAddWord()
        {
            RichTextBoxEventArgs args = new RichTextBoxEventArgs(null, _currentClickedHilight.Text);

            RaiseRichTextEvent(Root.IncorrectWordAdd, this, args, true);

            if (!args.Cancel)
            {
                ProcessAddNewWord();
            }
        }

        /// <summary>
        /// Removes all spell checker references to the selected word
        /// </summary>
        /// <returns>Selected word text</returns>
        internal string ProcessAddNewWord()
        {
            string word = _currentClickedHilight.Text;

            if (_currentClickedHilight != null)
            {
                _currentClickedHilight.Element.ClearHilight(_currentClickedHilight.Hilight);
                DeleteHilightsForWord(_currentClickedHilight.Text);
                _currentClickedHilight = null;
            }

            HideSuggestionsPopup();

            return word;
        }

        /// <summary>
        /// Attaches events to an element to make it suitable for insertion
        /// </summary>
        /// <param name="element">Any UIElement</param>
        /// <returns>The element</returns>
        internal UIElement SetupElement(UIElement element)
        {
            UIElement result = element;

            if (element is Table)
            {
                SetupTable((Table)element);
            }
            else if (element is RichTextBlock)
            {
                ((RichTextBlock)element).ParentRTB = this;
            }
            else
            {
                if (element is Control)
                {
                    ((Control)element).IsTabStop = false;
                }
                else if (element is Image)
                {
                    element.Opacity = 0;
                    ((Image)element).Cursor = Cursors.Arrow;
                    _imagesLoading.Add((Image)element);
                    UpdateTimerEnabledStatus();
                }
/*                else if (element is Image)
                {
                    ((Image)element).Stretch = Stretch.Fill;
                }
                if (!(element is Bullet))
                {
                    //resizable = new Resizable() { Content = element };
                    //result = resizable;
                    element.MouseLeftButtonDown += new MouseButtonEventHandler(Element_MouseLeftButtonDown);
                }*/
            }

            if (result is FrameworkElement)
            {
                ((FrameworkElement)result).SizeChanged += new SizeChangedEventHandler(xamlElement_SizeChanged);
            }

            return result;
        }

        internal void ProcessLinkEntered()
        {
            RichTextBoxPosition pos;
            string link;

            if (EnableURLRecognition)
            {
                pos = new RichTextBoxPosition(_cursorPosition);
                link = Root.AutoComplete.Text.StartsWith("www.") ? "http://" + Root.AutoComplete.Text : Root.AutoComplete.Text;

                ProcessSelect(Root.AutoComplete.Index, Root.AutoComplete.Text.Length);

                ContentMetadata metadata = new ContentMetadata();
                
                metadata["URL"] = link;
                metadata.Add("Title", link);
                metadata.Add("Target", "");
                ApplyFormatting(Formatting.Link, metadata);

                ClearSelection();
                pos.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);

                _cursorPosition = pos;
                SetCursor(_cursorPosition, false);

                _selectedStart = new RichTextBoxPosition(_cursorPosition);
                _selectedEnd = new RichTextBoxPosition(_cursorPosition);
            }
        }

        internal void ObjectSelect(FrameworkElement obj)
        {
            if (Root.EnableObjectSelection)
            {
                Root.ObjectDeselect(null);
                ObjectDeselect(null);
                CreateObjectSelection(obj);
                _objectSelected = true;

                SelectElement((UIElement)obj);
            }
        }

        private void SelectElement(UIElement element)
        {
            RichTextBoxPosition start = new RichTextBoxPosition(element, 0);
            RichTextBoxPosition end = new RichTextBoxPosition(element, 1);

            start.CalculateGlobalIndex(ElementChildren.ContentChildren);
            end.GlobalIndex = start.GlobalIndex + 1;

            SetSelection(start, end, true);
            //SetSelection(start.GlobalIndex, 1, true);
        }

        internal void ObjectDeselect(FrameworkElement element)
        {
            UIElement marker;

            if (Root.EnableObjectSelection)
            {
                if (element != null)
                {
                    marker = GetObjectMarker(element);
                    if (marker != null)
                    {
                        ElementObjectSelection.Children.Remove(marker);
                    }
                }
                else
                {
                    for (int i = ElementObjectSelection.Children.Count - 1; i >= 0; i--)
                    {
                        ElementObjectSelection.Children.RemoveAt(i);
                    }
                }
            }
            _objectSelected = false;
        }

        #endregion

        #region Public Methods

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);

            System.Diagnostics.Debug.WriteLine(e.Text);
        }

        /// <summary>
        /// Loads RichText from the provided URL
        /// </summary>
        /// <param name="url">URL of a valid RichText XML file</param>
        public void Load(string url)
        {
            FocusedRTB.ProcessLoad(url);
        }

        /// <summary>
        /// Inserts the string at the cursor
        /// </summary>
        /// <param name="text">Text to insert</param>
        public void InsertText(string text)
        {
            FocusedRTB.ProcessInsertText(text);
        }

        /// <summary>
        /// Inserts a newline at the cursor position
        /// </summary>
        public void InsertNewline()
        {
            FocusedRTB.ProcessInsertNewline();
        }

        /// <summary>
        /// Inserts a block of rich text at the cursor
        /// </summary>
        /// <param name="richText">Rich Text xml</param>
        /// <returns>Number of content elements created</returns>
        public void Insert(string richText)
        {
            FocusedRTB.ProcessInsert(richText, null);
        }

        /// <summary>
        /// Inserts a block of rich text at the cursor with metadata
        /// </summary>
        /// <param name="richText">Rich Text xml</param>
        /// <param name="metadata">Metadata to be associated with the content</param>
        /// <returns>Number of content elements created</returns>
        public void Insert(string richText, ContentMetadata metadata)
        {
            FocusedRTB.ProcessInsert(richText, metadata);
        }

        /// <summary>
        /// Inserts the provided table object at the cursor position
        /// </summary>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="headerRows">Number of header rows</param>
        /// <param name="headerColumns">Number of header columns</param>
        /// <param name="styleID">The table style ID</param>
        public Table InsertTable(int rows, int columns, int headerRows, int headerColumns, string styleID)
        {
            Table table = null;
            RichTextBoxTableStyle style = Root.TableStyles[styleID];

            // Tables within tables not yet supported
            if (FocusedRTB == Root)
            {
                table = new Table(columns, rows)
                {
                    Tag = new RichTextTag(styleID),
                    HeaderColumns = headerColumns,
                    HeaderRows = headerRows,
                    CellPadding = new Thickness(3),
                    EnableEditingFadeout = true
                };

                if (ElementCursor != null)
                {
                    table.AutoWidth = false;
                    table.Width = (ActualWidth - (double)ElementCursor.GetValue(Canvas.LeftProperty)) - 2;
                }

                table.SizeChanged += new SizeChangedEventHandler(xamlElement_SizeChanged);
                SetupTable(table);
                FocusedRTB.ProcessInsert(table, true, true);
                RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
                
            }

            return table;
        }

        /// <summary>
        /// Inserts a before at the currently selected cell row
        /// </summary>
        /// <param name="insertAfter">Specifying true here, the row will be inserted after the selected row</param>
        public void InsertTableRow(bool insertAfter)
        {
            InsertTableRow(insertAfter, true);
        }

        /// <summary>
        /// Inserts a column before the currently selected cell column
        /// </summary>
        /// <param name="insertAfter">Specifying true here, the column will be inserted after the selected column</param>
        public void InsertTableColumn(bool insertAfter)
        {
            InsertTableColumn(insertAfter, true);
        }

        /// <summary>
        /// Deletes the currently selected row
        /// </summary>
        public void DeleteTableRow()
        {
            DeleteTableRow(true);
        }

        /// <summary>
        /// Deletes the currently selected column
        /// </summary>
        public void DeleteTableColumn()
        {
            DeleteTableColumn(true);
        }

        /// <summary>
        /// Deletes the selected text or the character before or after as indicated by the parameter
        /// </summary>
        /// <param name="deleteForward">True to delete the character infront of the cursor</param>
        public void Delete(bool deleteForward)
        {
            FocusedRTB.ProcessDelete(deleteForward);
        }

        /// <summary>
        /// Applys formatting to the selected content
        /// </summary>
        /// <param name="formatting">Formatting commang</param>
        /// <param name="param">Optional formatting parameters</param>
        public void ApplyFormatting(Formatting formatting, object param)
        {
            FocusedRTB.ProcessApplyFormatting(formatting, param);
        }

        /// <summary>
        /// Shows the cursor
        /// </summary>
        public void ShowCursor()
        {
            FocusedRTB.ProcessShowCursor();
        }

        /// <summary>
        /// Hides the cursor
        /// </summary>
        public void HideCursor()
        {
            FocusedRTB.ProcessHideCursor();
        }

        /// <summary>
        /// Moves the cursor left
        /// </summary>
        public void CursorLeft()
        {
            FocusedRTB.ProcessCursorLeft();
        }

        /// <summary>
        /// Moves the cursor right
        /// </summary>
        public void CursorRight()
        {
            FocusedRTB.ProcessCursorRight();
        }

        /// <summary>
        /// Moves the cursor Up
        /// </summary>
        public void CursorUp()
        {
            FocusedRTB.ProcessCursorUp();
        }

        /// <summary>
        /// Moves the cursor Down
        /// </summary>
        public void CursorDown()
        {
            FocusedRTB.ProcessCursorDown();
        }

        /// <summary>
        /// Moves the cursor to the top of the document
        /// </summary>
        public void Top()
        {
            FocusedRTB.ProcessTop();
        }

        /// <summary>
        /// Moves the cursor to the top of the document
        /// </summary>
        public void Home()
        {
            FocusedRTB.ProcessHome();
        }

        /// <summary>
        /// Moves the cursor to the end of the document
        /// </summary>
        public void End()
        {
            FocusedRTB.ProcessEnd();
        }

        /// <summary>
        /// Moves the cursor to the bottom of the document
        /// </summary>
        public void Bottom()
        {
            FocusedRTB.ProcessBottom();
        }

        /// <summary>
        /// Clears all content from the RichTextBox
        /// </summary>
        public void Clear()
        {
            FocusedRTB.ProcessClear();
        }

        /// <summary>
        /// Selects the content for the provided range
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="length">Content length to select</param>
        public new void Select(int startIndex, int length)
        {
            FocusedRTB.ProcessSelect(startIndex, length);
        }

        /// <summary>
        /// Selects the content of the current line
        /// </summary>
        public void SelectLine()
        {
            RichTextPanelRow row = ElementChildren.GetRowForElement(_cursorPosition.Element);

            FocusedRTB.ProcessSelectLine(row, true);
        }

        /// <summary>
        /// Selects all the content
        /// </summary>
        public new void SelectAll()
        {
            FocusedRTB.ProcessSelectAll();
        }

        /// <summary>
        /// Clears the cursor selection
        /// </summary>
        public void ClearSelection()
        {
            FocusedRTB.ProcessClearSelection(true);
        }

        /// <summary>
        /// Cuts the selected content and places it on the internal clipboard
        /// </summary>
        public void Cut()
        {
            FocusedRTB.ProcessCut();
        }

        /// <summary>
        /// Copies the selected content to the internal clipboard
        /// </summary>
        public void Copy()
        {
            FocusedRTB.ProcessCopy();
        }

        /// <summary>
        /// Pastes the content from the internal clipboard to the cursor position
        /// </summary>
        public void Paste()
        {
            FocusedRTB.ProcessPaste();
        }

        /// <summary>
        /// Copies the current style to the clipboard and pastes it to the next selection
        /// </summary>
        public void Painter()
        {
            FocusedRTB.ProcessPainter();
        }

        /// <summary>
        /// Un-does the last operation
        /// </summary>
        public void Undo()
        {
            HistoryEvent e = Root.History.GetCurrent();

            if (e != null)
            {
                ((RichTextBlock)e.Source).ProcessUndo(e);
            }
        }

        /// <summary>
        /// Re-does the last operation
        /// </summary>
        public void Redo()
        {
            HistoryEvent e;

            Root.History.ActionCouterReversed();
            e = Root.History.GetCurrent();

            if (e != null)
            {
                ((RichTextBlock)e.Source).ProcessRedo(e);
            }
            else
            {
                Root.History.ActionReversed();
            }
        }

        /// <summary>
        /// Searches through the text content for the provided terms and moves the cursor to a match
        /// </summary>
        /// <param name="terms">Terms to search for</param>
        /// <returns>True if a match was found</returns>
        public bool Find(string terms)
        {
            return FocusedRTB.ProcessFind(terms);
        }

        /// <summary>
        /// Positions the cursor on the provided line
        /// </summary>
        /// <param name="lineNumber">Line number</param>
        public void Goto(int lineNumber)
        {
            lineNumber -= 1;

            if (ElementChildren != null)
            {
                if (lineNumber <= ElementChildren.Rows.Count)
                {
                    _cursorPosition = new RichTextBoxPosition(ElementChildren.Rows[lineNumber].Start, 0);
                    _cursorPosition.CalculateGlobalIndex(ElementChildren.ContentChildren);
                    ProcessClearSelection(false);
                    SetCursor(_cursorPosition, true);
                }
            }
        }

        /// <summary>
        /// Applies focus to the control
        /// </summary>
        public void ReturnFocus()
        {
            FocusedRTB.Focus();
        }

        /// <summary>
        /// Displays a popup with spelling suggestions
        /// </summary>
        /// <param name="suggestions">String list of suggestions</param>
        /// <param name="e">RichTextBoxEventArgs object that generated the event</param>
        public void ShowSuggestions(IEnumerable<string> suggestions, RichTextBoxEventArgs e)
        {
            ComboBox list = (ComboBox)ElementBubble.FindName("ElementSuggestions");
            ComboBoxItem item;
            int i;
            Point position = e.Hilight.Hilight.Element.TransformToVisual(ElementChildren).Transform(new Point());

            list.SelectedIndex = -1;
            // Delete all existing items.  list.Items.Clear() causes an exception to be thrown
            for (i = list.Items.Count - 1; i >= 0; i--)
            {
                list.Items.RemoveAt(i);
            }

            foreach (string s in suggestions)
            {
                item = new ComboBoxItem();

                item.Content = s;
                list.Items.Add(item);
            }

            ((Button)ElementBubble.FindName("ElementReplace")).IsEnabled = (list.Items.Count > 0);
            list.IsEnabled = (list.Items.Count > 0);

            if (list.Items.Count > 0)
            {
                list.SelectedIndex = 0;
            }

            SetBubble(new Point(Math.Round(position.X), Math.Round(position.Y)));
            ElementBubblePopup.IsOpen = true;
        }

        /// <summary>
        /// Retrieves the screen coordinates of the cursor
        /// </summary>
        /// <returns>Position of the cursor</returns>
        public virtual Point GetCursorCoordinates()
        {
            Point result = new Point();

            if (ElementChildren != null)
            {
                result = _cursorPosition.Position;
            }

            return result;
        }

        /// <summary>
        /// Saves the content as the specified format
        /// </summary>
        /// <param name="format">The output format</param>
        /// <param name="options">Save options</param>
        /// <returns>Rich text</returns>
        public string Save(Format format, RichTextSaveOptions options)
        {
            return SaveSelectionToText(0, _length - 1, format, options);
        }

        /// <summary>
        /// Loads the Rich TextBox with content
        /// </summary>
        /// <param name="format">Format of the provided content</param>
        /// <param name="content">Content to load</param>
        public void Load(Format format, string content)
        {
            RichTextBoxStyle currentStyle;
            int numberAdded;

            IsReady = false;

            if (ElementChildren != null)
            {
                ObjectDeselect(null);
                if (format == Format.Text || (format == Format.XML && !content.StartsWith("<LiquidRichText")))
                {
                    SetFromPlainText(content);
                    return;
                }

                if (this == Root)
                {
                    AddDefaultStyles();
                }
                _length = 0;
                currentStyle = Root.Styles[DefaultStyle];

                ElementChildren.Clear();
                ElementChildren.DisableUpdates = true;

                BuildPortion(format, 0, currentStyle, content, out numberAdded, true);

                ElementChildren.DisableUpdates = false;
                RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);

                ProcessSelect(0, 0);
                RunSpellChecker(0, -1);
                if (this == Root)
                {
                    Root.History.Reset();
                }
                UpdateChildReadOnlyState(SelectMode);
                IsReady = true;
            }
            else
            {
                switch (format)
                {
                    case Format.HTML:
                        HTML = content;
                        break;
                    default:
                        RichText = content;
                        break;
                }
            }
        }

        /// <summary>
        /// Re-applies styles to the content, this should be called if any modifications are made
        /// to the Styles or TableStyles collections outside of the RichTextBlock/RichTextBox
        /// </summary>
        public void RefreshStyles()
        {
            object tag;

            if (ElementChildren != null)
            {
                foreach (UIElement e in ElementChildren.ContentChildren)
                {
                    tag = e.GetValue(FrameworkElement.TagProperty);

                    if (tag is RichTextTag)
                    {
                        ApplyStyleToElement(e, ((RichTextTag)tag).StyleID);
                    }
                }
                ElementChildren.Update();
            }
        }

        /// <summary>
        /// Simulates a left mouse click
        /// </summary>
        /// <param name="position">The pixel position within the content area</param>
        public void SimulateLeftClick(Point position)
        {
            RichTextBoxPosition temp;
            string currentStyle = string.Empty;
            RichTextBoxEventArgs args = new RichTextBoxEventArgs();

            if (!IsEnabled)
            {
                return;
            }

            temp = GetPosition(position, false);
            Point p = temp.Position;

            if (Root.ElementBubble != null)
            {
                HideSuggestionsPopup();
            }

            if (Root.EnableObjectSelection && !(temp.Element is TextBlockPlus))
            {
                ObjectSelect((FrameworkElement)temp.Element);

                if (temp.Element == _lastClickedElement && _ticksSinceLastMouseDown < 10)
                {
                    RaiseRichTextEvent(Root.ObjectDoubleClick, this, new RichTextBoxEventArgs(temp.Element), false);
                }
            }
            else
            {
                ObjectDeselect(null);

                _mouseDown = true;
                UpdateTimerEnabledStatus();

                if (EnableQuickSelection && SelectMode != SelectMode.ReadOnly && temp.Element == _lastClickedElement && _ticksSinceLastMouseDown < 10)
                {	// Double-click
                    SelectWord(temp);
                    CalculateSelection();
                }
                else
                {	// Single click
                    if (SelectMode != SelectMode.Edit || IsCtrlDown())
                    {
                        // Follow the link
                        if (temp != null && !EnableLinkButtons)
                        {
                            currentStyle = GetStyle(temp.Element);
                            ContentMetadata metadata = GetMetadata(temp.Element);

                            if (metadata != null)
                            {
                                if (metadata.IsLink)
                                {
                                    args.Source = temp.Element;
                                    args.Parameter = metadata["URL"];

                                    RaiseRichTextEvent(Root.LinkClicked, this, args, false);
                                    _mouseDown = false;
                                }
                            }
                        }

                        if (SelectMode == SelectMode.ReadOnly)
                        {
                            _mouseDown = false;
                            ProcessClearSelection(true);
                            return;
                        }
                    }

                    ElementChildren.CaptureMouse();
                    InformWordEdited(_cursorPosition);
                    Root.AutoComplete.Reset();

                    if (IsShiftDown())
                    {
                        SetSelection(temp, _cursorPosition, true);
                    }
                    else
                    {
                        _cursorPosition = temp;
                        ProcessClearSelection(true);
                        SetCursor(_cursorPosition, false);
                    }

                    RaiseRichTextEvent(Root.CursorMoved, this, new RichTextBoxEventArgs(), false);
                    ShowCursor();

                    InformIfPositionInHilight(temp);
                    Focus();
                }
            }

            Root.AutoComplete.Reset();
            _ticksSinceLastMouseDown = 0;
            _lastClickedElement = temp.Element;
            ActiveTable = null;
        }

        /// <summary>
        /// Replaces the currently selected spelling mistake with a new word
        /// </summary>
        /// <param name="newWord">New word</param>
        public void ReplaceMisspeltWord(string newWord)
        {
            Root.FocusedRTB.ProcessReplaceWord(newWord);
        }

        /// <summary>
        /// Adds the currently selected spelling as a new word and clears any further hilighting of the word
        /// </summary>
        public string AddMisspeltWord()
        {
            string newWord = Root.FocusedRTB.ProcessAddNewWord();

            ReplaceMisspeltWord(newWord);

            return newWord;
        }

        /// <summary>
        /// Gets a collection of selected objects
        /// </summary>
        /// <returns>Collection of selected objects</returns>
        public List<UIElement> GetSelectedObjects()
        {
            List<UIElement> result = new List<UIElement>();
            int index;

            if (Root.FocusedRTB.ElementObjectSelection != null)
            {
                foreach (UIElement e in Root.FocusedRTB.ElementObjectSelection.Children)
                {
                    index = (int)((FrameworkElement)e).Tag;
                    if (index >= 0)
                    {
                        result.Add(Root.FocusedRTB.ElementChildren.ContentChildren[index]);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the cursor UIElement
        /// </summary>
        /// <returns>Cursor UIElement</returns>
        public FrameworkElement GetCursorElement()
        {
            return ElementCursor;
        }

        /// <summary>
        /// Gets the index of content at the provided pixel position
        /// </summary>
        /// <param name="position">Pixel position</param>
        /// <returns>Content index</returns>
        public virtual int GetContentIndexAtPosition(Point position)
        {
            RichTextBoxPosition pos = GetPosition(position, false);

            return pos.GlobalIndex;
        }

        /// <summary>
        /// Gets the content at a provided index
        /// </summary>
        /// <param name="startIndex">Content start index</param>
        /// <param name="length">Content length</param>
        /// <returns>Plain text content</returns>
        public string GetContentAtIndex(int startIndex, int length, Format format, RichTextSaveOptions options)
        {
            if (startIndex < 0)
            {
                startIndex = 0;
            }

            if (startIndex + length >= _length)
            {
                length = _length - startIndex;
            }

            if (length > 0)
            {
                return SaveSelectionToText(startIndex, length, format, options);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Joins text elements with the same style and metadata properties
        /// </summary>
        public void JoinSimilarElements()
        {
            if (ElementChildren != null)
            {
                ElementChildren.CompactTextElements();
            }
        }

        /// <summary>
        /// Removes all unused styles
        /// </summary>
        public void RemoveUnusedStyles()
        {
            List<string> toDelete = new List<string>();

            foreach (KeyValuePair<string, RichTextBoxStyle> pair in Styles)
            {
                if (!IsStyleUsed(pair.Key))
                {
                    toDelete.Add(pair.Key);
                }
            }

            foreach (string s in toDelete)
            {
                Styles.Remove(s);
            }

            History.Reset();
        }

        public void RemoveElement(UIElement element)
        {
            if (ElementChildren != null)
            {
                ElementChildren.Remove(element);
            }
        }

        public void RemoveElement(int index)
        {
            if (ElementChildren != null)
            {
                RemoveElement(Children[index]);
            }
        }

        public void SwapElement(UIElement source, UIElement replaceWith)
        {
            if (ElementChildren != null)
            {
                int i = ElementChildren.ContentChildren.IndexOf(source);

                ElementChildren.Remove(source);
                InsertElement(i, replaceWith);
            }
        }

        public void InsertElement(UIElement element)
        {
            FocusedRTB.ProcessInsert(FocusedRTB.SetupElement(element), true, true);
        }

        public void InsertElement(int index, UIElement element)
        {
            string style = RichTextBlock.DefaultStyle;

            if (ElementChildren != null)
            {
                style = RichTextBlock.DefaultStyle;

                if (element.GetValue(FrameworkElement.TagProperty) != null)
                {
                    style = element.GetValue(FrameworkElement.TagProperty).ToString();
                }

                ApplyStyleToElement(element, RichTextBlock.DefaultStyle);

                Children.Insert(index, element);
                SetupElement(element);
                RefreshElement(element);
            }
        }

        /// <summary>
        /// Refreshes the layout for a given element
        /// </summary>
        /// <param name="element">The RichTextBox element</param>
        public void RefreshElement(UIElement element)
        {
            if (ElementChildren != null)
            {
                ElementChildren.Update(element, false);
                ObjectDeselect((FrameworkElement)element);
            }
        }

        /// <summary>
        /// Resizes the provided image
        /// </summary>
        /// <param name="image">Image element that is contained in the RichTextBlock</param>
        /// <param name="newSize">New image size</param>
        public void ResizeImage(Image image, Size newSize)
        {
            image.Width = newSize.Width;
            image.Height = newSize.Height;

            if (image.Tag is RichTextTag)
            {
                RichTextTag tag = (RichTextTag)image.Tag;
                if (!tag.Metadata.ContainsKey("ImageWidth"))
                {
                    tag.Metadata.Add("ImageWidth", newSize.Width.ToString());
                }
                else
                {
                    tag.Metadata["ImageWidth"] = newSize.Width.ToString();
                }

                if (!tag.Metadata.ContainsKey("ImageHeight"))
                {
                    tag.Metadata.Add("ImageHeight", newSize.Height.ToString());
                }
                else
                {
                    tag.Metadata["ImageHeight"] = newSize.Height.ToString();
                }
            }

            RefreshElement(image);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converts a selected object to selected content
        /// </summary>
        private void CheckForSelectedObjects()
        {
            List<UIElement> objects = GetSelectedObjects();

            foreach (UIElement e in objects)
            {
                ObjectDeselect((FrameworkElement)e);
            }
        }

        private void RefreshObjectMarker()
        {
            List<UIElement> selected = GetSelectedObjects();
            UIElement marker;

            if (selected.Count > 0)
            {
                marker = GetObjectMarker((FrameworkElement)selected[0]);

                if (marker != null)
                {
                    ElementObjectSelection.Children.Remove(marker);
                    CreateObjectSelection((FrameworkElement)selected[0]);
                }
            }
        }

        private UIElement GetObjectMarker(FrameworkElement element)
        {
            FrameworkElement e;
            int index;
            UIElement result = null;

            if (element == null)
            {
                return result;
            }

            index = ElementChildren.Children.IndexOf((UIElement)element);

            for (int i = ElementObjectSelection.Children.Count - 1; i >= 0; i--)
            {
                e = (FrameworkElement)ElementObjectSelection.Children[i];

                if (e.Tag == (object)index || index < 0)
                {
                    result = ElementObjectSelection.Children[i];
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a visual marker for the specified object
        /// </summary>
        /// <param name="element">Element to be markes</param>
        private void CreateObjectSelection(FrameworkElement element)
        {
            Point p = element.TransformToVisual(ElementChildren).Transform(new Point());
            Size size = new Size(double.IsNaN(element.Width) ? element.ActualWidth : element.Width,
                double.IsNaN(element.Height) ? element.ActualHeight : element.Height);

            Rectangle marker = new Rectangle()
            {
                Stroke = new SolidColorBrush(Color.FromArgb(224, 64, 64, 64)),
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection() { 0, 1, 2 },
                StrokeDashOffset = 3,
                Width = size.Width,
                Height = size.Height,
                Tag = ElementChildren.ContentChildren.IndexOf((UIElement)element)
            };

            Canvas.SetLeft(marker, p.X);
            Canvas.SetTop(marker, p.Y);

            ElementObjectSelection.Children.Add(marker);
        }

        /// <summary>
        /// Gets the full XML opening tag
        /// </summary>
        /// <returns></returns>
        private string GetXMLOpen()
        {
            StringBuilder sb = new StringBuilder();

            foreach (string s in CustomNamespaces)
            {
                sb.Append(" " + s);
            }

            return "<LiquidRichText" + sb.ToString() + ">";
        }

        /// <summary>
        /// Updates the editable state of any child tables
        /// </summary>
        internal void UpdateChildReadOnlyState(SelectMode newMode)
        {
            if (ElementChildren != null)
            {
                foreach (UIElement e in ElementChildren.ContentChildren)
                {
                    if (e is Table)
                    {
                        ((Table)e).SelectMode = newMode;
                    }
                }
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Inserts a before at the currently selected cell row
        /// </summary>
        /// <param name="insertAfter">Specifying true here, the row will be inserted after the selected row</param>
        /// <param name="updateHistory">Indicates whether to update the undo history</param>
        private void InsertTableRow(bool insertAfter, bool updateHistory)
        {
            RichTextState state;
            Table table = Root.ActiveTable;
            int rowIndex;
            int rowCount;
            int index;

            if (table != null)
            {
                if (table.SelectedColumnCount == 0)
                {
                    rowIndex = table.SelectedCellRow;
                    rowCount = 1;
                }
                else
                {
                    rowIndex = table.SelectedRowStart;
                    rowCount = table.SelectedRowCount;
                }
                index = (insertAfter ? rowIndex + 1 : rowCount);
                table.InsertRow(index, rowCount);
                SetupTableChildren(table);
                if (updateHistory)
                {
                    state = new RichTextState()
                    {
                        Table = table,
                        Index = index,
                        Count = table.SelectedRowCount,
                        SelectionStart = _cursorPosition.GlobalIndex,
                        Length = 0
                    };
                    Root.History.Add(this, HistoryCommand.InsertRow, null, state);
                    RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
                }
            }
        }

        /// <summary>
        /// Inserts a column before the currently selected cell column
        /// </summary>
        /// <param name="insertAfter">Specifying true here, the column will be inserted after the selected column</param>
        /// <param name="updateHistory">Indicates whether to update the undo history</param>
        private void InsertTableColumn(bool insertAfter, bool updateHistory)
        {
            RichTextState state;
            Table table = Root.ActiveTable;
            int colIndex;
            int colCount;
            int index;

            if (table != null)
            {
                if (table.SelectedColumnCount == 0)
                {
                    colIndex = table.SelectedCellColumn;
                    colCount = 1;
                }
                else
                {
                    colIndex = table.SelectedColumnStart;
                    colCount = table.SelectedColumnCount;
                }
                index = (insertAfter ? colIndex + 1 : colIndex);
                table.InsertColumn(index, colCount);
                SetupTableChildren(table);
                if (updateHistory)
                {
                    state = new RichTextState()
                    {
                        Table = table,
                        Index = index,
                        Count = colCount,
                        SelectionStart = _cursorPosition.GlobalIndex,
                        Length = 0
                    };
                    Root.History.Add(this, HistoryCommand.InsertColumn, null, state);
                }
                RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
            }
        }

        /// <summary>
        /// Deletes the currently selected row
        /// </summary>
        /// <param name="updateHistory">Indicates whether to update the undo history</param>
        private void DeleteTableRow(bool updateHistory)
        {
            RichTextState state;
            Table table = Root.ActiveTable;
            int index;
            int count;
            int i;

            if (table != null)
            {
                if (table.SelectedColumnCount == 0)
                {
                    index = table.SelectedCellRow;
                    count = 1;
                }
                else
                {
                    index = table.SelectedRowStart;
                    count = table.SelectedRowCount;
                }

                if (index + count <= table.RowDefinitions.Count)
                {
                    for (i = (index + count) - 1; i >= index; i--)
                    {
                        if (updateHistory)
                        {
                            state = new RichTextState()
                            {
                                Table = table,
                                Index = i,
                                Row = table.RowDefinitions[i],
                                Elements = table.GetRowElements(i),
                                SelectionStart = _cursorPosition.GlobalIndex,
                                Length = 0
                            };
                            Root.History.Add(this, HistoryCommand.DeleteRow, null, state);
                        }
                        table.DeleteRow(i);
                    }
                    RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
                }
                Root.Focus();
            }
        }

        /// <summary>
        /// Deletes the currently selected column
        /// </summary>
        private void DeleteTableColumn(bool updateHistory)
        {
            RichTextState state;
            Table table = Root.ActiveTable;
            int index;
            int count;
            int i;

            if (table != null)
            {
                if (table.SelectedColumnCount == 0)
                {
                    index = table.SelectedCellColumn;
                    count = 1;
                }
                else
                {
                    index = table.SelectedColumnStart;
                    count = table.SelectedColumnCount;
                }

                if (index + count <= table.ColumnDefinitions.Count)
                {
                    for (i = (index + count) - 1; i >= index; i--)
                    {
                        if (updateHistory)
                        {
                            state = new RichTextState()
                            {
                                Table = table,
                                Index = i,
                                Column = table.ColumnDefinitions[i],
                                Elements = table.GetColumnElements(i),
                                SelectionStart = _cursorPosition.GlobalIndex,
                                Length = 0
                            };
                            Root.History.Add(this, HistoryCommand.DeleteColumn, null, state);
                        }
                        table.DeleteColumn(i);
                    }
                    RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
                }
                Root.Focus();
            }
        }

        #region Spell Checker checking/hilighting

        /// <summary>
        /// Starts the spellchecker from a given index
        /// </summary>
        /// <param name="globalIndex">Global index to begin from</param>
        /// <param name="endIndex">Last index to check</param>
        private void RunSpellChecker(int globalIndex, int endIndex)
        {
            _nextSpellCheckPosition = new RichTextBoxPosition() { GlobalIndex = globalIndex };
            _nextSpellCheckPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
            _runInitialSpellCheck = true;
            _lastSpellCheckIndex = endIndex;
            _spellCheckedWords.Clear();
            UpdateTimerEnabledStatus();
        }

        /// <summary>
        /// This method is called when a word is edited.  The word string is raised in an event allowing for external spell checking
        /// </summary>
        /// <param name="position">Position to check the edited word</param>
        private int InformWordEdited(RichTextBoxPosition position)
        {
            RichTextBoxEventArgs args = new RichTextBoxEventArgs();
            RichTextBoxPosition selectionStart;
            RichTextBoxPosition selectionEnd;
            string word = string.Empty;
            string tempWord;
            int result = 0;

            if (!EnableSpellCheck)
            {
                return result;
            }

            if (position.Element is TextBlockPlus)
            {
                GetWordSelection(position, out selectionStart, out selectionEnd, 1);

                if (GetHilight(selectionStart, selectionEnd) == null)
                {
                    DeletePartialHilights(selectionStart, selectionEnd);
                    word = SaveSelectionToText(selectionStart, selectionEnd, Format.Text, RichTextSaveOptions.None).Trim();
                    tempWord = word.ToLower();

                    if (word.Length > 1 && !_spellCheckedWords.Contains(tempWord))
                    {
                        args.Source = position.Element;
                        args.Parameter = word;

                        RaiseRichTextEvent(Root.CheckWord, this, args, true);

                        if (!args.Cancel)
                        {
                            ApplyErrorFormattingToSelection(selectionStart, selectionEnd, word);
                        }
                        else
                        {
                            _spellCheckedWords.Add(tempWord);
                        }
                    }
                }
                result = selectionEnd.GlobalIndex - selectionStart.GlobalIndex;
            }

            return result;
        }

        /// <summary>
        /// If the provided position is within a hilight then an event will be raised to allow
        /// for the display of the spell check popup
        /// </summary>
        /// <param name="position">Rich Text Position</param>
        private void InformIfPositionInHilight(RichTextBoxPosition position)
        {
            RichTextBoxEventArgs args = new RichTextBoxEventArgs();
            RichTextBoxPosition selectionStart;
            RichTextBoxPosition selectionEnd;
            RichTextHilight hilight;
            RichTextHilight temp = _currentClickedHilight;

            if (EnableSpellCheck)
            {
                _currentClickedHilight = null;

                if (position.Element is TextBlockPlus)
                {
                    GetWordSelection(position, out selectionStart, out selectionEnd, 1);
                    hilight = GetHilight(selectionStart, selectionEnd);

                    if (hilight != null && hilight != temp)
                    {
                        args = new RichTextBoxEventArgs(position.Element, hilight.Text);
                        args.Source = this;
                        args.Hilight = GetHilight(selectionStart, selectionEnd);

                        _currentClickedHilight = args.Hilight;
                        RaiseRichTextEvent(Root.IncorrectWordNotify, this, args, true);

                        if (args.Cancel)
                        {
                            _currentClickedHilight = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a visible underline to indicate a spelling error
        /// </summary>
        /// <param name="start">Spelling start</param>
        /// <param name="end">Spelling end</param>
        /// <param name="word">Error word</param>
        private void ApplyErrorFormattingToSelection(RichTextBoxPosition start, RichTextBoxPosition end, string word)
        {
            int elementStart = ElementChildren.ContentChildren.IndexOf(start.Element);
            int elementEnd = ElementChildren.ContentChildren.IndexOf(end.Element);
            TextBlockPlus current;

            if (elementStart >= 0 && elementEnd >= 0)
            {
                if (elementStart == elementEnd)
                {
                    if (ElementChildren.ContentChildren[elementStart] is TextBlockPlus)
                    {
                        current = (TextBlockPlus)ElementChildren.ContentChildren[elementStart];
                        CreateHilight(current, start, end, start.Index, end.Index - start.Index, word);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a spelling mistake hilight
        /// </summary>
        /// <param name="element">TextBlockPlus this applies to</param>
        /// <param name="start">Hilight start</param>
        /// <param name="end">Hilight end</param>
        /// <param name="startIndex">Text start index</param>
        /// <param name="length">Text length</param>
        /// <param name="word">Hilight word</param>
        private void CreateHilight(TextBlockPlus element, RichTextBoxPosition start, RichTextBoxPosition end, int startIndex, int length, string word)
        {
            TextBlockPlusHilight tbHilight;
            RichTextHilight hilight;

            if (GetHilight(start, end) == null)
            {
                tbHilight = new TextBlockPlusHilight(TextBlockPlusSelectionEffect.DottedUnderline, _errorWordBrush, startIndex, length);
                hilight = new RichTextHilight(element, tbHilight, start, end, word);

                hilight.Element.AddHilight(tbHilight);

                _hilights.Add(hilight);
            }
        }

        /// <summary>
        /// Determines whether a hilight for the provided area already exists
        /// </summary>
        /// <param name="start">Area start</param>
        /// <param name="end">Area end</param>
        /// <returns>True if the area already exists</returns>
        private RichTextHilight GetHilight(RichTextBoxPosition start, RichTextBoxPosition end)
        {
            RichTextHilight result = null;

            foreach (RichTextHilight hilight in _hilights)
            {
                if (hilight.Start.CompareTo(start) == 0 && hilight.End.CompareTo(end) == 0)
                {
                    result = hilight;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Deletes all spell checker hilights
        /// </summary>
        private void DeleteHilights()
        {
            for (int i = _hilights.Count - 1; i >= 0; i--)
            {
                _hilights[i].Element.ClearHilights();
                _hilights.RemoveAt(i);
            }
        }

        /// <summary>
        /// Deletes all hilights contained within the specified area but not one matching the the exact area size
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        private void DeletePartialHilights(RichTextBoxPosition start, RichTextBoxPosition end)
        {
            for (int i = _hilights.Count - 1; i >= 0; i--)
            {
                if ((_hilights[i].Start.GlobalIndex >= start.GlobalIndex && _hilights[i].Start.GlobalIndex <= end.GlobalIndex) &&
                    (_hilights[i].End.GlobalIndex >= start.GlobalIndex && _hilights[i].End.GlobalIndex <= end.GlobalIndex))
                {
                    if (_hilights[i].Start.CompareTo(start) != 0 || _hilights[i].End.CompareTo(end) != 0)
                    {
                        _hilights[i].Element.ClearHilight(_hilights[i].Hilight);
                        _hilights.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes a spelling mistake hilight
        /// </summary>
        /// <param name="hilight">TextBoxPlusHilight object</param>
        private void DeleteHilight(TextBlockPlusHilight hilight)
        {
            for (int i = _hilights.Count - 1; i >= 0; i--)
            {
                if (_hilights[i].Hilight == hilight)
                {
                    _hilights[i].Element.ClearHilight(hilight);
                    _hilights.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Deletes all spelling mistake hilights for a specific word
        /// </summary>
        /// <param name="word">Word</param>
        private void DeleteHilightsForWord(string word)
        {
            for (int i = _hilights.Count - 1; i >= 0; i--)
            {
                if (_hilights[i].Text.ToLower() == word.ToLower())
                {
                    _hilights[i].Element.ClearHilight(_hilights[i].Hilight);
                    _hilights.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// This is called when two textblocks are being merged allowing us to merge the hilights also
        /// </summary>
        /// <param name="source">New combined textblock</param>
        /// <param name="old">Textblock that will be deleted</param>
        /// <param name="originalLength">Length of the first textblock to merge</param>
        private void MergeHilights(TextBlockPlus source, TextBlockPlus old, int originalLength)
        {
            List<TextBlockPlusHilight> sourceExtraHilights = new List<TextBlockPlusHilight>();
            RichTextBoxPosition start;
            RichTextBoxPosition end;
            string word = string.Empty;

            sourceExtraHilights.AddRange(old.Hilights);

            foreach (TextBlockPlusHilight hilight in sourceExtraHilights)
            {
                DeleteHilight(hilight);

                hilight.Start += originalLength;

                if (hilight.Start >= 0 && hilight.Start + hilight.Length <= source.Text.Length)
                {
                    start = new RichTextBoxPosition(source, hilight.Start);
                    start.CalculateGlobalIndex(ElementChildren.ContentChildren);
                    end = new RichTextBoxPosition(source, hilight.Start + hilight.Length);
                    end.CalculateGlobalIndex(ElementChildren.ContentChildren);

                    word = source.Text.Substring(hilight.Start, hilight.Length);
                    CreateHilight(source, start, end, hilight.Start, hilight.Length, word);
                }
            }
        }

        /// <summary>
        /// This is called when a textblock is split
        /// </summary>
        /// <param name="source">The original textblock (left-side)</param>
        /// <param name="created">the new textblock (right-side)</param>
        private void SplitHilights(TextBlockPlus source, TextBlockPlus created)
        {
            List<TextBlockPlusHilight> sourceExtraHilights = source.GetHilights(source.Text.Length, -1);
            RichTextBoxPosition start;
            RichTextBoxPosition end;
            string word = string.Empty;

            foreach (TextBlockPlusHilight hilight in sourceExtraHilights)
            {
                DeleteHilight(hilight);

                hilight.Start -= source.Text.Length;

                if (hilight.Start >= 0 && hilight.Start + hilight.Length <= created.Text.Length)
                {
                    start = new RichTextBoxPosition(created, hilight.Start);
                    start.CalculateGlobalIndex(ElementChildren.ContentChildren);
                    end = new RichTextBoxPosition(created, hilight.Start + hilight.Length);
                    end.CalculateGlobalIndex(ElementChildren.ContentChildren);

                    word = created.Text.Substring(hilight.Start, hilight.Length);
                    CreateHilight(created, start, end, hilight.Start, hilight.Length, word);
                }
            }
        }

        /// <summary>
        /// Updates all hilight indexes
        /// </summary>
        private void RefreshAllHilights()
        {
            RichTextHilight hilight;
            int i;

            for (i = _hilights.Count - 1; i >= 0; i--)
            {
                if (_hilights[i].LastTextLength != _hilights[i].Element.Text.Length)
                {
                    hilight = _hilights[i];

                    if (ElementChildren.ContentChildren.IndexOf(hilight.Element) >= 0 && hilight.Hilight.Start >= 0 && hilight.Hilight.Start < hilight.Element.Text.Length)
                    {
                        hilight.Start.CalculateGlobalIndex(ElementChildren.ContentChildren);
                        hilight.End.CalculateGlobalIndex(ElementChildren.ContentChildren);
                        hilight.Element.CalculateHilight(hilight.Hilight);
                    }
                    else
                    {
                        DeleteHilight(hilight.Hilight);
                    }

                    hilight.LastTextLength = hilight.Element.Text.Length;
                }
            }
        }

        /// <summary>
        /// Gets the height available for displaying the popup spell checking bubble
        /// </summary>
        /// <returns>Available height</returns>
        protected virtual double GetAvailableHeight()
        {
            return ElementChildren.Height;
        }

        /// <summary>
        /// Sets the bubble to the correct position
        /// </summary>
        /// <param name="position">Bubble position</param>
        private void SetBubble(Point position)
        {
            double height = GetAvailableHeight();
            double cornerRadius = 8;
            Polygon poly;
            double bubbleWidth = 168;
            double bubbleHeight = 110;

            if (ElementChildren == null)
            {
                return;
            }

            poly = (Polygon)ElementBubble.FindName("ElementBubbleBackground");

            if (position.Y > height - bubbleHeight)
            {	// Bubble should be above the word
                if (position.X > ActualWidth - bubbleWidth)
                {	// and the lip to the right
                    position.X -= 150 / _zoom;
                    SetShape(RichTextBubbleLip.BottomRight, poly, ElementBubble.Width, ElementBubble.Height, cornerRadius);
                }
                else
                {	// the lip to the left
                    SetShape(RichTextBubbleLip.BottomLeft, poly, ElementBubble.Width, ElementBubble.Height, cornerRadius);
                }

                poly.SetValue(Canvas.TopProperty, 0d);
                position.Y -= bubbleHeight;
            }
            else
            {	// Bubble should be under the word
                if (position.X > ActualWidth - bubbleWidth)
                {	// and the lip to the right
                    position.X -= 150 / _zoom;
                    SetShape(RichTextBubbleLip.TopRight, poly, ElementBubble.Width, ElementBubble.Height, cornerRadius);
                }
                else
                {	// the lip to the left
                    SetShape(RichTextBubbleLip.TopLeft, poly, ElementBubble.Width, ElementBubble.Height, cornerRadius);
                }
                poly.SetValue(Canvas.TopProperty, -10d);
                position.Y += 10;
            }

            if (_currentClickedHilight != null)
            {
                SetSelection(_currentClickedHilight.Start, _currentClickedHilight.End, true);
            }

            /*PopupBubbleArea = new Rect(0, 0, this.RenderSize.Width, height);

            if (RestrictPopupBubble)
            {
                if (position.X + bubbleWidth < PopupBubbleArea.Left)
                {
                    position = new Point(PopupBubbleArea.Left, position.Y);
                }
                else if (position.X + bubbleWidth > PopupBubbleArea.Right)
                {
                    position = new Point(PopupBubbleArea.Left - bubbleWidth, position.Y);
                }

                if (position.Y + bubbleHeight < PopupBubbleArea.Top)
                {
                    position = new Point(position.X, PopupBubbleArea.Top);
                }
                else if (position.Y + bubbleHeight > PopupBubbleArea.Bottom)
                {
                    position = new Point(position.X, PopupBubbleArea.Bottom - bubbleHeight);
                }
            }*/

            ElementBubblePopup.HorizontalOffset = position.X;
            ElementBubblePopup.VerticalOffset = position.Y;
        }

        private void SetShape(RichTextBubbleLip lip, Polygon background, double width, double height, double cornerRadius)
        {
            Point[] pointsBL = null;
            double halfway = cornerRadius * 0.40f;
            Point inner = new Point(width - (cornerRadius * 2), height - (cornerRadius * 2));
            double mainHeight = height - 10;

            switch (lip)
            {
                case RichTextBubbleLip.BottomLeft:
                    pointsBL = new Point[16] { new Point(cornerRadius, 0), new Point(width - cornerRadius, 0),
                        new Point(width - halfway,halfway), new Point(width,cornerRadius),
						new Point(width,mainHeight -cornerRadius), new Point(width - halfway,mainHeight - halfway),
                        new Point(width - cornerRadius,mainHeight), new Point(cornerRadius + 12,mainHeight),
                        new Point(cornerRadius + 12,height), new Point(cornerRadius + 2,mainHeight),
                        new Point(cornerRadius,mainHeight), new Point(3,mainHeight - halfway), new Point(0,mainHeight - cornerRadius),
                        new Point(0,cornerRadius), new Point(halfway,halfway), new Point(cornerRadius,0) };
                    break;
                case RichTextBubbleLip.BottomRight:
                    pointsBL = new Point[16] { new Point(cornerRadius, 0), new Point(inner.X + cornerRadius, 0),
                        new Point(width - halfway,halfway), new Point(width,cornerRadius),
						new Point(width,mainHeight - cornerRadius), new Point(width - halfway,mainHeight - halfway),
                        new Point(inner.X + cornerRadius,mainHeight), new Point((width - cornerRadius) - 2,mainHeight),
                        new Point((width - cornerRadius) - 2,height), new Point((width - cornerRadius) - 12,mainHeight),
						new Point(cornerRadius,mainHeight), new Point(halfway,mainHeight - halfway),
                        new Point(0,mainHeight - cornerRadius), new Point(0,cornerRadius), new Point(halfway,halfway),
                        new Point(cornerRadius,0) };
                    break;
                case RichTextBubbleLip.TopRight:
                    pointsBL = new Point[16] { new Point(cornerRadius, 10), new Point((width - cornerRadius) - 12, 10),
                        new Point((width - cornerRadius) - 2,0), new Point((width - cornerRadius) - 2,10),
						new Point(width - cornerRadius, 10), new Point(width - halfway,10 + halfway),
                        new Point(width,10 + cornerRadius), new Point(width,height - cornerRadius),
                        new Point(width - halfway,height - halfway), new Point(width - cornerRadius,height),
						new Point(cornerRadius,height), new Point(halfway,height - halfway), new Point(0,height - cornerRadius),
                        new Point(0,10 + cornerRadius), new Point(halfway,10 + halfway), new Point(cornerRadius,10) };
                    break;
                case RichTextBubbleLip.TopLeft:
                    pointsBL = new Point[16] { new Point(cornerRadius, 10), new Point(cornerRadius + 2, 10),
                        new Point(cornerRadius + 2,0), new Point(cornerRadius + 12,10), new Point(width - cornerRadius, 10),
                        new Point(width - halfway,10 + halfway), new Point(width,10 + cornerRadius),
                        new Point(width,height - cornerRadius), new Point(width - halfway,height - halfway),
                        new Point(width - cornerRadius,height), new Point(cornerRadius,height), new Point(halfway,height - halfway),
                        new Point(0,height - cornerRadius), new Point(0,10 + cornerRadius), new Point(halfway,10 + halfway),
                        new Point(cornerRadius,10) };
                    break;
                default:
                    break;
            }

            if (pointsBL != null)
            {
                background.Points.Clear();

                foreach (Point p in pointsBL)
                {
                    background.Points.Add(p);
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets the first element of a line taking into account whether the line is bulleted or indented
        /// </summary>
        /// <param name="rowIndex">Line index</param>
        /// <returns>The first element</returns>
        private UIElement GetLineStartElement(int rowIndex)
        {
            UIElement result = null;
            int i;

            if (rowIndex >= 0)
            {
                result = ElementChildren.Rows[rowIndex].Start;
                if (result is Bullet)
                {
                    i = ElementChildren.ContentChildren.IndexOf(result);
                    if (i + 1 < ElementChildren.ContentChildren.Count)
                    {
                        result = ElementChildren.ContentChildren[i + 1];
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Moves the cursor to the top/left of the document
        /// </summary>
        private void Home(bool ctrlDown, bool shiftDown)
        {
            UIElement temp;

            if (SelectMode == SelectMode.ReadOnly || ElementChildren == null)
            {
                return;
            }

            temp = GetLineStartElement(ElementChildren.GetRowIndexForElement(_cursorPosition.Element));

            if (shiftDown)
            {
                _selectedEnd = new RichTextBoxPosition(_cursorPosition);
            }

            if (ctrlDown)
            {
                // Move to the start of the content
                _cursorPosition.GlobalIndex = 0;
                _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
            }
            else
            {
                // Move to the start of the line
                _cursorPosition.Element = temp;
                _cursorPosition.Index = 0;
                _cursorPosition.CalculateGlobalIndex(ElementChildren.ContentChildren);
            }

            if (!shiftDown)
            {
                ProcessClearSelection(true);
            }
            else
            {
                _selectedStart = new RichTextBoxPosition(_cursorPosition);
                SetSelection(_selectedStart, _selectedEnd, true);
            }

            SetCursor(_cursorPosition, true);
        }

        /// <summary>
        /// Moves the cursor to the end of the document
        /// </summary>
        private void End(bool ctrlDown, bool shiftDown)
        {
            RichTextPanelRow line;

            if (SelectMode == SelectMode.ReadOnly || ElementChildren == null)
            {
                return;
            }

            line = ElementChildren.GetRowForElement(_cursorPosition.Element);

            if (shiftDown)
            {
                _selectedStart = new RichTextBoxPosition(_cursorPosition);
            }

            if (ctrlDown)
            {
                // Move to the end of the content
                _cursorPosition.GlobalIndex = _length - 1;
                _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
            }
            else
            {
                // Move to the end of the line
                if (line.End is Newline)
                {
                    _cursorPosition.Element = ElementChildren.ContentChildren[ElementChildren.ContentChildren.IndexOf(line.End) - 1];
                }
                else
                {
                    _cursorPosition.Element = line.End;
                }
                _cursorPosition.Index = (_cursorPosition.Element is TextBlockPlus ? ((TextBlockPlus)_cursorPosition.Element).Text.Length : 0);
                _cursorPosition.CalculateGlobalIndex(ElementChildren.ContentChildren);
            }

            if (!shiftDown)
            {
                ProcessClearSelection(true);
            }
            else
            {
                _selectedEnd = new RichTextBoxPosition(_cursorPosition);
                SetSelection(_selectedStart, _selectedEnd, true);
            }

            SetCursor(_cursorPosition, true);
        }

        /// <summary>
        /// Inserts a block of rich text at the cursor
        /// </summary>
        /// <param name="text">Rich Text xml</param>
        /// <param name="elements">A collection of elements to insert</param>
        /// <param name="metadata">A metadata tag</param>
        /// <param name="updateHistory">Indicates whether to update the undo history</param>
        /// <param name="updateSingleLine">Indicates whether to update just the line of insertion or the whole document</param>
        /// <returns>Number of content elements created</returns>
        private void InsertRichTextAtCursor(string text, List<UIElement> elements, ContentMetadata metadata, bool updateHistory, bool updateSingleLine)
        {
            int numberAdded = 0;
            RichTextState before = null;
            RichTextBoxPosition temp;
            TextBlockPlus textblock;
            int indexInElements;
            string newText;
            int i;

            if (SelectMode != SelectMode.Edit)
            {
                return;
            }

            CheckForSelectedObjects();
            if (ContentSelectionLength > 0)
            {
                if (updateHistory)
                {
                    before = DeleteSelected(false);
                }
            }

            PrepareSelection();
            if (elements == null)
            {
                elements = BuildPortion(_cursorPosition, text, out numberAdded);
            }
            else
            {
                indexInElements = ElementChildren.ContentChildren.IndexOf(_cursorPosition.Element);

                for (i = elements.Count - 1; i >= 0; i--)
                {
                    if (elements[i] is TextBlockPlus)
                    {
                        newText = ((TextBlockPlus)elements[i]).Text;
                        if (MaxLength == 0 || newText.Length + numberAdded + _length <= MaxLength + 1)
                        {
                            textblock = CreateTextBlock(newText, elements[i], true);
                            ElementChildren.Insert(indexInElements, textblock);
                            numberAdded += textblock.Text.Length;
                        }
                    }
                    else
                    {
                        if (MaxLength == 0 || _length + 1 <= MaxLength + 1)
                        {
                            ElementChildren.Insert(indexInElements, elements[i]);
                            numberAdded += 1;
                        }
                    }
                }
                _length += numberAdded;
            }

            if (metadata != null)
            {
                foreach (UIElement e in elements)
                {
                    ApplyMetadataToElement(e, metadata);
                }
            }

            ElementChildren.CompactRow(_cursorPosition.Element);
            _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);

            if (!updateSingleLine || elements.Count > 1)
            {
                ElementChildren.Update();
            }
            else
            {
                ElementChildren.Update(_cursorPosition.Element, false);
            }

            temp = new RichTextBoxPosition();
            temp.GlobalIndex = _cursorPosition.GlobalIndex + numberAdded;
            temp.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);

            _cursorPosition.GlobalIndex += numberAdded;
            _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);

            SetCursor(_cursorPosition, true);
            ProcessClearSelection(true);

            if (updateHistory)
            {
                Root.History.Add(this, HistoryCommand.InsertContent, before, new RichTextState(elements, _selectedStart.GlobalIndex - numberAdded, numberAdded));
            }
        }

        /// <summary>
        /// Gets the selected content as rich text
        /// </summary>
        /// <returns></returns>
        private string GetSelectedRichText()
        {
            return GetRichText(_selectedStart.GlobalIndex, ContentSelectionLength);
        }

        /// <summary>
        /// Gets the selected content as rich text
        /// </summary>
        /// <param name="selectionStart">Start Index</param>
        /// <param name="length">Length</param>
        /// <returns>Rich text</returns>
        private string GetRichText(int selectionStart, int length)
        {
            return SaveSelectionToText(selectionStart, length, Format.XML, RichTextSaveOptions.None);
        }

        /// <summary>
        /// Searches a text string for a given term
        /// </summary>
        /// <param name="terms">Search terms</param>
        /// <returns>Returns true if a match was found</returns>
        private bool FindInPlainText(string terms)
        {
            RichTextBoxEventArgs args = new RichTextBoxEventArgs();
            string text = Save(Format.Text, RichTextSaveOptions.None);
            Match m;

            m = Regex.Match(text, Regex.Escape(terms), RegexOptions.IgnoreCase);
            if (m.Success)
            {
                RaiseRichTextEvent(Root.TextFound, this, args, false);
            }
            return m.Success;
        }

        /// <summary>
        /// Searches a textblock for a given text string
        /// </summary>
        /// <param name="textblock">Textblock to search in</param>
        /// <param name="terms">Search terms</param>
        /// <param name="startIndex">Index within the textblock to begin the search</param>
        /// <returns>Returns true if a match was found</returns>
        private bool FindInTextBlock(TextBlockPlus textblock, string terms, int startIndex)
        {
            RichTextBoxEventArgs args = new RichTextBoxEventArgs();
            RichTextBoxPosition start;
            RichTextBoxPosition end;
            Match m;

            if (startIndex >= textblock.Text.Length)
            {
                return false;
            }
            m = Regex.Match(textblock.Text.Substring(startIndex), Regex.Escape(terms), RegexOptions.IgnoreCase);

            if (m.Success)
            {
                RaiseRichTextEvent(Root.TextFound, this, args, false);
                if (!args.Cancel)
                {
                    _cursorPosition.Element = textblock;
                    _cursorPosition.Index = startIndex + m.Index + terms.Length;
                    _cursorPosition.CalculateGlobalIndex(ElementChildren.ContentChildren);

                    start = new RichTextBoxPosition(textblock, startIndex + m.Index);
                    start.CalculateGlobalIndex(ElementChildren.ContentChildren);

                    end = new RichTextBoxPosition(textblock, startIndex + m.Index + terms.Length);
                    end.CalculateGlobalIndex(ElementChildren.ContentChildren);

                    SetSelection(start, end, true);
                    SetCursor(_cursorPosition, true);
                }
            }

            return m.Success;
        }

        /// <summary>
        /// Updates the cursor position to either the previous or next line
        /// </summary>
        /// <param name="lineIndex"></param>
        private void MoveCursorUpDown(int lineIndex)
        {
            int newIndex = 0;
            RichTextPanelRow line;
            UIElement element;
            double x;

            if (lineIndex < 0 || lineIndex >= ElementChildren.Rows.Count)
            {
                return;
            }

            InformWordEdited(_cursorPosition);
            Root.AutoComplete.Reset();

            line = ElementChildren.Rows[lineIndex];

            element = ElementChildren.GetRowElementAtPosition(_cursorPosition.Position, line);
            if (element != null)
            {
                if (element is TextBlockPlus)
                {
                    x = (double)element.GetValue(Canvas.LeftProperty);
                    newIndex = GetPositionInString((TextBlockPlus)element, new Point(_cursorPosition.Position.X - x, 0));
                }
                _cursorPosition.Element = element;
                _cursorPosition.Index = newIndex;

                if (_cursorPosition.Element is Newline)
                {
                    _cursorPosition.Element = ElementChildren.ContentChildren[ElementChildren.ContentChildren.IndexOf(_cursorPosition.Element) - 1];
                    if (_cursorPosition.Element is TextBlockPlus)
                    {
                        _cursorPosition.Index = ((TextBlockPlus)_cursorPosition.Element).Text.Length;
                    }
                }
            }

            _cursorPosition.CalculateGlobalIndex(ElementChildren.ContentChildren);

            SetCursor(_cursorPosition, true);
            UpdateSelectionOnCursorKeyPress();
        }

        /// <summary>
        /// This is called when the cursor has been moved left or right to update the selection and cursor states
        /// </summary>
        private void UpdateSelectionOnCursorKeyPress()
        {
            if (IsShiftDown())
            {
                if (_cursorPosition.GlobalIndex > _dragBegin.GlobalIndex)
                {
                    _selectedStart = RoundPositionUp(_dragBegin);
                    _selectedEnd = _cursorPosition;
                }
                else
                {
                    _selectedStart = RoundPositionUp(_cursorPosition);
                    _selectedEnd = _dragBegin;
                }
                CalculateSelection();
            }
            else
            {
                ProcessClearSelection(true);
            }
        }

        /// <summary>
        /// Deletes the selected elements
        /// </summary>
        private RichTextState DeleteSelected(bool updateHistory)
        {
            RichTextState state;
            TextBlockPlus textBlock;
            int startIndex = _cursorPosition.GlobalIndex;
            int selectionLength = ContentSelectionLength;
            int lineToUpdateFrom;
            int lineEndUpdate;
            int startElementIndex;
            int endElementIndex;
            int i;

            if (_cursorPosition.GlobalIndex > _selectedStart.GlobalIndex)
            {
                startIndex = _selectedStart.GlobalIndex;
            }

            PrepareSelection();

            startElementIndex = ElementChildren.ContentChildren.IndexOf(_selectedStart.Element);
            endElementIndex = ElementChildren.ContentChildren.IndexOf(_selectedEnd.Element);
            if (_selectedEnd.Index == 0 && _selectedEnd.Element is TextBlockPlus)
            {
                _selectedEnd.Element = ElementChildren.ContentChildren[endElementIndex - 1];
                _selectedEnd.Index = (_selectedEnd.Element is TextBlockPlus ? ((TextBlockPlus)_selectedEnd.Element).Text.Length : 1);
            }

            if (endElementIndex + 1 < ElementChildren.ContentChildren.Count)
            {
                if (ElementChildren.ContentChildren[endElementIndex + 1] is Newline)
                {
                    ApplyStyleToElement(ElementChildren.ContentChildren[endElementIndex + 1], ElementChildren.ContentChildren[startElementIndex].GetValue(TagProperty).ToString());
                }
            }

            lineToUpdateFrom = ElementChildren.GetRowIndexForElement(_selectedStart.Element);
            lineEndUpdate = ElementChildren.GetRowIndexForElement(_selectedEnd.Element);
            if (lineToUpdateFrom > 0)
            {
                lineToUpdateFrom--;
            }
            else if (lineToUpdateFrom == 0 && lineEndUpdate == 0)
            {
                lineEndUpdate++;
            }

            state = new RichTextState(_selectedStart.GlobalIndex, ContentSelectionLength);

            if (startElementIndex < 0 || endElementIndex < 0)
            {
                return state;
            }

            for (i = endElementIndex; i >= startElementIndex; i--)
            {
                state.Elements.Insert(0, ElementChildren.ContentChildren[i]);
                ElementChildren.ContentChildren.RemoveAt(i);
            }

            if (state.Elements.Count == 0)
            {
                return state;
            }

            if (state.Elements[0] is TextBlockPlus && startElementIndex < ElementChildren.ContentChildren.Count)
            {
                if (!(ElementChildren.ContentChildren[startElementIndex] is TextBlockPlus))
                {
                    textBlock = CreateTextBlock(string.Empty, state.Elements[0], false);
                    ElementChildren.Insert(startElementIndex, textBlock);
                }
            }

            if (ElementChildren.ContentChildren.Count > 1 && lineToUpdateFrom >= 0 && lineEndUpdate - lineToUpdateFrom == 1)
            {
                if (ElementChildren.Rows[lineToUpdateFrom].Start == state.Elements[0] && startElementIndex < ElementChildren.ContentChildren.Count)
                {
                    ElementChildren.Rows[lineToUpdateFrom].Start = ElementChildren.ContentChildren[startElementIndex];
                }
                ElementChildren.Update(ElementChildren.Rows[lineToUpdateFrom], false, null);
            }
            else
            {
                ElementChildren.Update();
            }

            if (updateHistory)
            {
                Root.History.Add(this, HistoryCommand.Delete, state, null);
            }

            _cursorPosition.GlobalIndex = startIndex;
            _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);

            _length -= ContentSelectionLength;
            ProcessClearSelection(true);

            return state;
        }

        /// <summary>
        /// Moves the provided position object the specified number of positions
        /// </summary>
        /// <param name="current">A position object</param>
        /// <param name="positions">Number of positions to move</param>
        private void MoveCursor(RichTextBoxPosition current, int positions)
        {
            InformWordEdited(_cursorPosition);
            Root.AutoComplete.Reset();

            current.GlobalIndex += positions;
            if (current.GlobalIndex < 0)
            {
                current.GlobalIndex = 0;
            }
            else if (current.GlobalIndex > _length - 1)
            {
                current.GlobalIndex = _length - 1;
            }

            current.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
        }

        #region XML Content Read/Write
        
        private List<UIElement> BuildHTMLPortionAsElementList(ref RichTextBoxStyle currentStyle, string html, string baseImageURL, string imageURLPrefix, ref int numberAdded)
        {
            List<UIElement> elements = new List<UIElement>();
            List<UIElement> tableElements = new List<UIElement>();
            Stack<RichTextBoxStyle> styleStack = new Stack<RichTextBoxStyle>();
            ContentMetadata currentMetadata = null;
            Dictionary<string, string> styleKeyValues = new Dictionary<string, string>();
            string tableStyle = "TableDefault";
            Table table;
            int tableWidth = 0;
            int tableRows = 0;
            List<int> tableColumns = new List<int>();
            int columnWidth;
            int rowHeaders = 0;
            int columnHeaders = 0;
            PlaceHolder holder;
            Image image;
            XmlReader reader;
            string tag;
            string styleID;
            string inlineStyle;
            string temp;
            BulletType bulletType = BulletType.Bullet;
            Bullet bullet;
            int nextBulletNumber = 1;
            bool inList = false;
            VerticalAlignment vAlign;
            int i;

            if (html.Length == 0 || html == "<p>[RTB_HTML_SPACER]</p>")
            {
                return elements;
            }

            styleStack.Push(Root.Styles[DefaultStyle]);

            reader = XmlReader.Create(new StringReader("<content>" + html.Replace("&nbsp;", "[RTB_HTML_SPACER]") + "</content>"));
            reader.Read();

            while (!reader.EOF)
            {
                tag = reader.Name.ToUpper();

                if (reader.NodeType == XmlNodeType.Element)
                {
                    styleID = reader.GetAttribute("class");
                    inlineStyle = reader.GetAttribute("style");
                    currentStyle = new RichTextBoxStyle(styleStack.Peek());

                    if (styleID != null)
                    {
                        if (tag == "TABLE")
                        {
                            tableStyle = styleID;
                        }
                        else if (Root.Styles.ContainsKey(styleID))
                        {
                            currentStyle = new RichTextBoxStyle(Root.Styles[styleID]);
                        }
                    }
                    if (inlineStyle != null)
                    {
                        if (tag == "P" && inList)
                        {
                        }
                        else
                        {
                            currentStyle.FromInlineStyle(inlineStyle);
                        }
                    }

                    switch (tag)
                    {
                        case "STYLE":
                            GetStylesFromHTML(reader.ReadInnerXml());
                            currentStyle = Root.Styles[DefaultStyle];
                            styleStack.Clear();
                            styleStack.Push(Root.Styles[DefaultStyle]);
                            break;
                        case "H1":
                        case "H2":
                        case "H3":
                            if (Root.Styles.ContainsKey(tag))
                            {
                                currentStyle = new RichTextBoxStyle(Root.Styles[tag]);
                            }
                            break;
                        case "IMG":
                            double width = double.NaN;
                            double height = double.NaN;

                            temp = reader.GetAttribute("src");
                            ContentMetadata imageMetadata = new ContentMetadata(currentMetadata);

                            imageMetadata.Add("ImageSrc", reader.GetAttribute("src"));
                            if (reader.GetAttribute("width") != null)
                            {
                                width = double.Parse(reader.GetAttribute("width"));
                                imageMetadata.Add("ImageWidth", reader.GetAttribute("width"));
                            }
                            if (reader.GetAttribute("height") != null)
                            {
                                height = double.Parse(reader.GetAttribute("height"));
                                imageMetadata.Add("ImageHeight", reader.GetAttribute("height"));
                            }
                            if (reader.GetAttribute("alt") != null)
                            {
                                imageMetadata.Add("ImageAlt", reader.GetAttribute("alt"));
                            }

                            image = new Image()
                            {
                                Source = new BitmapImage(new Uri(imageURLPrefix + (temp.ToLower().StartsWith(baseImageURL.ToLower()) ? temp : baseImageURL + temp), UriKind.RelativeOrAbsolute)),
                                Tag = new RichTextTag(currentStyle.ID)
                                {
                                    Metadata = imageMetadata
                                },
                                Stretch = Stretch.Fill
                            };

                            if (!double.IsNaN(width))
                            {
                                image.Width = width;
                            }
                            if (!double.IsNaN(height))
                            {
                                image.Height = height;
                            }

                            CheckImageFormat(image);

                            if (imageMetadata.ContainsKey("ImageAlt") && imageMetadata["ImageAlt"].Length > 0)
                            {
                                ToolTipService.SetToolTip(image, imageMetadata["ImageAlt"]);
                            }

                            ApplyStyleToElement(image, currentStyle);
                            SetupElement(image);
                            elements.Add(image);
                            numberAdded++;
                            break;
                        case "BR":
                            AddNewline(elements, styleStack.Peek(), ref numberAdded);
                            break;
                        case "A":
                            currentMetadata = new ContentMetadata();
                            
                            currentMetadata.Add("URL", reader.GetAttribute("href") != null ? reader.GetAttribute("href") : "");
                            currentMetadata.Add("Title", reader.GetAttribute("title") != null ? reader.GetAttribute("title") : "");
                            currentMetadata.Add("Target", reader.GetAttribute("target") != null ? reader.GetAttribute("target") : "");
                            break;
                        case "B":
                        case "STRONG":
                            currentStyle.Weight = FontWeights.Bold;
                            break;
                        case "I":
                        case "ITALIC":
                            currentStyle.Style = FontStyles.Italic;
                            break;
                        case "U":
                        case "UNDERLINE":
                            currentStyle.Decorations = TextDecorations.Underline;
                            break;
                        case "UL":
                            bulletType = BulletType.Bullet;
                            nextBulletNumber = 1;
                            inList = true;
                            break;
                        case "OL":
                            bulletType = BulletType.Number;
                            nextBulletNumber = 1;
                            inList = true;
                            break;
                        case "LI":
                            bullet = new Bullet()
                            {
                                Type = bulletType,
                                Number = nextBulletNumber
                            };
                            SetupElement(bullet);
                            ApplyStyleToElement(bullet, styleStack.Peek());
                            elements.Add(bullet);
                            numberAdded++;
                            nextBulletNumber++;
                            break;
                        case "TABLE":
                            tableRows = 0;
                            tableElements.Clear();

                            if (reader.GetAttribute("width") != null)
                            {
                                tableWidth = int.Parse(reader.GetAttribute("width"));
                            }
                            else
                            {
                                tableWidth = 0;
                            }
                            break;
                        case "TR":
                            tableRows++;
                            tableColumns.Clear();
                            break;
                        case "TD":
                        case "TH":
                            styleKeyValues = RichTextBoxStyle.GetDictionaryFromStyle(reader.GetAttribute("style"));
                            columnWidth = -1;
                            if (styleKeyValues.ContainsKey("width"))
                            {
                                if (!styleKeyValues["width"].EndsWith("%"))
                                {
                                    columnWidth = int.Parse(styleKeyValues["width"].Replace("px", ""));
                                }
                            }
                            vAlign = VerticalAlignment.Top;

                            if (styleKeyValues.ContainsKey("vertical-align"))
                            {
                                switch (styleKeyValues["vertical-align"])
                                {
                                    case "middle":
                                        vAlign = VerticalAlignment.Center;
                                        break;
                                    case "bottom":
                                        vAlign = VerticalAlignment.Bottom;
                                        break;
                                    default:
                                        vAlign = VerticalAlignment.Top;
                                        break;
                                }
                            }
                            tableColumns.Add(columnWidth);

                            tableElements.Add(new RichTextBlock() { HTML = reader.ReadInnerXml(), VerticalAlignment = vAlign });
                            if (tag == "TH")
                            {
                                rowHeaders = tableRows;
                                columnHeaders = tableColumns.Count;
                            }
                            break;
                        case "PLACEHOLDER":
                            holder = new PlaceHolder()
                            {
                                Content = reader.GetAttribute("content"),
                                Value = reader.GetAttribute("value"),
                                Cursor = Cursors.Arrow
                            };

                            ApplyStyleToElement(holder, styleStack.Peek());
                            SetupElement(holder);
                            elements.Add(holder);
                            numberAdded++;
                            break;
                    }
                    if ((Utility.IsStyleAHeading(tag) || tag == "DIV" || tag == "P") && !inList && elements.Count > 0)
                    {
                        if (!(elements[elements.Count - 1] is Newline))
                        {
                            AddNewline(elements, styleStack.Peek(), ref numberAdded);
                        }
                    }
                    if (tag != "IMG" && tag != "BR" && tag != "CONTENT" && tag != "STYLE")
                    {
                        styleStack.Push(GetStyle(currentStyle));
                    }
                }
                else if (reader.NodeType == XmlNodeType.Text)
                {
                    AddTextBlock(elements, reader.Value.Replace("[RTB_HTML_SPACER]", " "), styleStack.Peek(), currentMetadata, ref numberAdded);
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    switch (tag)
                    {
                        case "TABLE":
                            GridLength length;

                            columnHeaders = (columnHeaders == tableColumns.Count ? 0 : columnHeaders);
                            rowHeaders = (rowHeaders == tableRows ? 0 : rowHeaders);

                            table = new Table(tableColumns.Count, tableRows, rowHeaders, columnHeaders, tableElements);
                            table.EnableEditingFadeout = true;
                            if (tableWidth > 0)
                            {
                                table.Width = tableWidth;
                                table.AutoWidth = false;
                            }
                            for(i=0; i<tableColumns.Count; i++)
                            {
                                if (tableColumns[i] >= 0)
                                {
                                    length = new GridLength(tableColumns[i]);
                                    table.ColumnDefinitions[i].SetValue(ColumnDefinition.WidthProperty, length);
                                }
                            }

                            table.Tag = new RichTextTag(tableStyle);
                            table.SizeChanged += new SizeChangedEventHandler(xamlElement_SizeChanged);

                            SetupTable(table);
                            elements.Add(table);
                            numberAdded++;
                            break;
                        case "H1":
                        case "H2":
                        case "H3":
                        case "P":
                        case "DIV":
                            AddNewline(elements, styleStack.Peek(), ref numberAdded);
                            break;
                        case "UL":
                        case "OL":
                            inList = false;
                            break;
                        case "LI":
                            if (elements.Count > 0)
                            {
                                if (!(elements[elements.Count - 1] is Newline))
                                {
                                    AddNewline(elements, styleStack.Peek(), ref numberAdded);
                                }
                            }
                            break;
                        case "A":
                            if (currentMetadata != null)
                            {
                                if (currentMetadata.IsLink)
                                {
                                    currentMetadata = null;
                                }
                            }
                            break;
                    }
                    if (styleStack.Count > 0)
                    {
                        styleStack.Pop();
                    }
                }

                if (reader.NodeType == XmlNodeType.Element && (tag == "TD" || tag == "TH"))
                {
                }
                else
                {
                    reader.Read();
                }
            }

            return elements;
        }

        private void AddNewline(List<UIElement> elements, RichTextBoxStyle style, ref int numberAdded)
        {
            Newline newLine = new Newline();

            ApplyStyleToElement(newLine, style);
            elements.Add(CreateTextBlock(string.Empty, style, null));
            elements.Add(newLine);
            numberAdded++;
        }

        private void AddTextBlock(List<UIElement> elements, string text, RichTextBoxStyle style, ContentMetadata metadata, ref int numberAdded)
        {
            TextBlockPlus currentTextblock = CreateTextBlock(text, style, metadata);
            elements.Add(currentTextblock);
            numberAdded += text.Length;
        }

        private RichTextBoxStyle AddDefaultStyles()
        {
            if (!CreateDefaultStyles && Styles.ContainsKey(DefaultStyle))
            {
                return Root.Styles[DefaultStyle];
            }

            RichTextBoxStyle h1 = new RichTextBoxStyle("H1", "Arial", 28, FontWeights.Bold);
            RichTextBoxStyle h2 = new RichTextBoxStyle("H2", "Arial", 24, FontWeights.Bold);
            RichTextBoxStyle h3 = new RichTextBoxStyle("H3", "Arial", 18, FontWeights.Bold);
            RichTextBoxStyle currentStyle = new RichTextBoxStyle(DefaultStyle, FontFamily.Source, FontSize, FontWeights.Normal);
            RichTextBoxTableStyle tableStyle = new RichTextBoxTableStyle("TableDefault", null, null, null,
                new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)), new Thickness(1),
                new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)), new Thickness(1), 2);

            TableStyles.Clear();

            if (Foreground != null)
            {
                h1.Foreground = Foreground;
                h2.Foreground = Foreground;
                h3.Foreground = Foreground;
                currentStyle.Foreground = Foreground;
            }
            if (FontStyle != null)
            {
                h1.Style = FontStyle;
                h2.Style = FontStyle;
                h3.Style = FontStyle;
                currentStyle.Style = FontStyle;
            }
            h1.Family = FontFamily.Source;
            h2.Family = FontFamily.Source;
            h3.Family = FontFamily.Source;
            currentStyle.Family = FontFamily.Source;

            AddStyle(h1);
            AddStyle(h2);
            AddStyle(h3);
            AddStyle(currentStyle);
            TableStyles.Add(tableStyle.ID, tableStyle);

            return currentStyle;
        }

        /// <summary>
        /// Builds the specified portion of rich text at the given position
        /// </summary>
        /// <param name="position">Position to insert the text</param>
        /// <param name="richTextXML">Rich text</param>
        /// <param name="numberAdded">Out: Number of elements created</param>
        /// <returns>The number of objects built</returns>
        private List<UIElement> BuildPortion(RichTextBoxPosition position, string richTextXML, out int numberAdded)
        {
            RichTextBoxStyle style = Root.Styles[GetStyle(position.Element)];
            int indexInElements = ElementChildren.ContentChildren.IndexOf(position.Element);

            return BuildPortion(Format.XML, indexInElements, style, richTextXML, out numberAdded, false);
        }

        /// <summary>
        /// Builds the specified portion of rich text at the given position
        /// </summary>
        /// <param name="format">The format of the content to load</param>
        /// <param name="elementsIndex">The index within the _elements collection to build at</param>
        /// <param name="currentStyle">The current style object</param>
        /// <param name="content">Content to load</param>
        /// <param name="numberAdded">Out: Number of elements created</param>
        /// <param name="appendNewline">Indicates whether a Newline should be appended</param>
        /// <returns>The number of objects built</returns>
        private List<UIElement> BuildPortion(Format format, int elementsIndex, RichTextBoxStyle currentStyle, string content, out int numberAdded, bool appendNewline)
        {
            List<UIElement> elements = new List<UIElement>();
            Newline newline;
            int i;

            numberAdded = 0;

            if (ElementChildren != null)
            {
                switch (format)
                {
                    case Format.XML:
                        elements = BuildXMLPortionAsElementList(currentStyle, content, out numberAdded);
                        break;
                    case Format.HTML:
                        elements = BuildHTMLPortionAsElementList(ref currentStyle, content, Root.BaseURL, Root.URLPrefix, ref numberAdded);
                        break;
                }

                if (appendNewline)
                {
                    newline = new Newline();
                    ApplyStyleToElement(newline, DefaultStyle);
                    if (elements.Count > 0)
                    {
                        if (!(elements[elements.Count - 1] is Newline))
                        {
                            elements.Add(newline);
                            numberAdded++;
                        }
                    }
                    else
                    {
                        elements.Add(newline);
                        numberAdded++;
                    }
                }

                for (i = 0; i < elements.Count; i++)
                {
                    ElementChildren.Insert(elementsIndex + i, elements[i]);
                }
                _length += numberAdded;
            }

            return elements;
        }

        /// <summary>
        /// Adds a style to the Styles collection
        /// </summary>
        /// <param name="style"></param>
        private void AddStyle(RichTextBoxStyle style)
        {
            RichTextBoxEventArgs args = new RichTextBoxEventArgs();
            RichTextBlock root = Root;

            args.Parameter = style;

            if (root.Styles.ContainsKey(style.ID))
            {
                DeleteStyle(Root.Styles[style.ID]);
            }
            root.Styles.Add(style.ID, style);

            RaiseRichTextEvent(Root.StyleCreated, this, args, false);
        }

        internal bool IsStyleUsed(string styleID)
        {
            bool used = false;

            foreach (UIElement element in ElementChildren.ContentChildren)
            {
                if (GetStyle(element) == styleID || styleID == DefaultStyle)
                {
                    used = true;
                    break;
                }
                if (element is Table)
                {
                    used = ((Table)element).IsStyleUsed(styleID);
                    if (used)
                    {
                        break;
                    }
                }
            }

            return used;
        }

        private void TryDeleteStyle(string styleID)
        {
            bool used = IsStyleUsed(styleID);

            if (!used)
            {
                System.Diagnostics.Debug.WriteLine("Deleting style: " + styleID);
                DeleteStyle(Root.Styles[styleID]);
            }
        }

        /// <summary>
        /// Removes the specified style from the Styles collection
        /// </summary>
        /// <param name="style">Style to remove</param>
        private void DeleteStyle(RichTextBoxStyle style)
        {
            RichTextBoxEventArgs args = new RichTextBoxEventArgs();

            Root.Styles.Remove(style.ID);
            args.Parameter = style;

            RaiseRichTextEvent(Root.StyleDeleted, this, args, false);
        }

        /// <summary>
        /// Builds the specified portion of rich text at the given position
        /// </summary>
        /// <param name="currentStyle">The current style object</param>
        /// <param name="richTextXML">Rich text</param>
        /// <param name="numberAdded">The number of objects built</param>
        /// <returns>A collection of elements built</returns>
        private List<UIElement> BuildXMLPortionAsElementList(RichTextBoxStyle currentStyle, string richTextXML, out int numberAdded)
        {
            List<UIElement> elements = new List<UIElement>();
            XmlReader reader;
            TextBlockPlus currentTextblock;
            string styleID;
            RichTextBoxStyle style;
            RichTextBoxTableStyle tableStyle;
            UIElement xamlElement;
            RichTextBoxEventArgs args = new RichTextBoxEventArgs();
            RichTextBlock root = Root;
            SolidColorBrush foreground = Foreground as SolidColorBrush;
            ContentMetadata currentMetadata = null;
            RichTextTag tag;
            Image image;
            string temp;

            if (!richTextXML.StartsWith("<LiquidRichText"))
            {
                richTextXML = GetXMLOpen() + richTextXML + "</LiquidRichText>";
            }

            numberAdded = 0;
            reader = XmlReader.Create(new StringReader(richTextXML));
            reader.Read();

            while (!reader.EOF)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    temp = reader.Name.ToLower();
                    if (temp == RichTextBox.StyleElement)
                    {
                        style = new RichTextBoxStyle(reader);
                        if (foreground != null)
                        {
                            if (foreground.Color.A != 255 || foreground.Color.R != 0 || foreground.Color.G != 0 || foreground.Color.B != 0)
                            {
                                style.Foreground = Foreground;
                            }
                        }

                        CheckForCustomStyle(style.ID);
                        AddStyle(style);
                    }
                    else if (temp == RichTextBox.TableStyleElement)
                    {
                        tableStyle = new RichTextBoxTableStyle(reader);
                        if (root.TableStyles.ContainsKey(tableStyle.ID))
                        {
                            root.TableStyles.Remove(tableStyle.ID);
                        }
                        root.TableStyles.Add(tableStyle.ID, tableStyle);
                    }
                    else if (temp == RichTextBox.TagElement)
                    {
                        tag = new RichTextTag(reader);

                        if (tag.Metadata != null)
                        {
                            ApplyMetadataToElement(elements[elements.Count - 1], tag.Metadata);
                        }

                        if (tag.StyleID == null)
                        {
                            tag.StyleID = GetStyle(elements[elements.Count - 1]);
                        }

                        if (tag.StyleID != null && tag.StyleID != string.Empty)
                        {
                            currentStyle = root.Styles[tag.StyleID];
                            ApplyStyleToElement(elements[elements.Count - 1], currentStyle);
                        }
                    }
                    else if (temp == RichTextBox.TextElement)
                    {
                        ContentMetadata metadata = null;

                        styleID = reader.GetAttribute("Style");
                        if (styleID != null)
                        {
                            currentStyle = root.Styles[styleID];
                        }

                        if (reader.GetAttribute("Link") != null)
                        {
                            metadata = new ContentMetadata();
                            metadata.Add("URL", reader.GetAttribute("Link"));
                        }

                        reader.Read();
                        if (reader.NodeType == XmlNodeType.CDATA)
                        {
                            currentTextblock = CreateTextBlock(reader.Value, currentStyle, metadata);
                            elements.Add(currentTextblock);
                            numberAdded += reader.Value.Length;
                        }
                    }
                    else if (temp == RichTextBox.NewlineElement)
                    {
                        styleID = reader.GetAttribute("Style");
                        if (styleID != null)
                        {
                            currentStyle = root.Styles[styleID];
                        }

                        AddNewline(elements, currentStyle, ref numberAdded);
                    }
                    else if (temp == RichTextBox.XamlElement)
                    {
                        xamlElement = CreateElementFromXaml(reader.ReadInnerXml().Trim());

                        if (!(xamlElement is Table))
                        {
                            if (xamlElement is Image)
                            {
                                image = (Image)xamlElement;
                                ContentMetadata imageMetadata = new ContentMetadata(currentMetadata);

                                imageMetadata.Add("ImageSrc", ((BitmapImage)image.Source).UriSource.ToString());
                                if (!double.IsNaN(image.Width))
                                {
                                    imageMetadata.Add("ImageWidth", image.Width.ToString());
                                }
                                if (!double.IsNaN(image.Height))
                                {
                                    imageMetadata.Add("ImageHeight", image.Height.ToString());
                                }
                                imageMetadata.Add("ImageAlt", "");
                                ApplyMetadataToElement(image, imageMetadata);

                                CheckImageFormat(xamlElement);
                            }
                            ApplyStyleToElement(xamlElement, currentStyle);
                        }

                        args.Source = xamlElement;
                        RaiseRichTextEvent(Root.ElementRead, this, args, false);

                        if (!args.Cancel)
                        {
                            elements.Add(xamlElement);
                            numberAdded++;
                        }
                        continue;
                    }
                }

                reader.Read();
            }

            reader.Close();

            return elements;
        }

        private void CheckImageFormat(UIElement xamlElement)
        {
            RichTextBoxEventArgs args;
            string imageURL = ((BitmapImage)((Image)xamlElement).Source).UriSource.ToString();

            if (!imageURL.ToLower().EndsWith(".png") && !imageURL.ToLower().EndsWith(".jpg"))
            {
                args = new RichTextBoxEventArgs(xamlElement, imageURL);
                RaiseRichTextEvent(Root.UnsupportedImageFormat, this, args, false);
            }
        }

        /// <summary>
        /// Checks for the existence of the custom style ID and sets the ID to next use
        /// </summary>
        /// <param name="styleID">Style ID</param>
        private void CheckForCustomStyle(string styleID)
        {
            string temp;
            int nextID;

            if (styleID.ToLower().StartsWith(CustomStyle.ToLower()))
            {
                temp = styleID.ToLower().Replace(CustomStyle.ToLower(), "");

                if (int.TryParse(temp, out nextID))
                {
                    NextStyleNumber = nextID + 1;
                }
            }
        }

        /// <summary>
        /// Saves a selection as elements
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <returns>Elements collection</returns>
        private List<UIElement> SaveSelectionAsElements(RichTextBoxPosition start, RichTextBoxPosition end)
        {
            int startElementIndex = ElementChildren.ContentChildren.IndexOf(start.Element);
            int endElementIndex = ElementChildren.ContentChildren.IndexOf(end.Element);

            return SaveSelectionAsElements(startElementIndex, endElementIndex);
        }

        /// <summary>
        /// Saves a selection as elements
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="endIndex">End index</param>
        /// <returns>Elements collection</returns>
        private List<UIElement> SaveSelectionAsElements(int startIndex, int endIndex)
        {
            List<UIElement> elements = new List<UIElement>();
            int i;

            for (i = startIndex; i <= endIndex; i++)
            {
                elements.Add(ElementChildren.ContentChildren[i]);
            }

            return elements;
        }

        /// <summary>
        /// Saves a selection to rich text or plain text
        /// </summary>
        /// <param name="startGlobalIndex">Start index</param>
        /// <param name="globalLength">Length</param>
        /// <param name="format">Indicates which format is generated</param>
        /// <param name="options">Sets options as to how styles are to be handled</param>
        /// <returns>Resulting text</returns>
        private string SaveSelectionToText(int startGlobalIndex, int globalLength, Format format, RichTextSaveOptions options)
        {
            RichTextBoxPosition start = new RichTextBoxPosition();
            RichTextBoxPosition end = new RichTextBoxPosition();

            if (ElementChildren != null)
            {
                start.GlobalIndex = startGlobalIndex;
                start.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
                end.GlobalIndex = startGlobalIndex + globalLength;
                end.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);

                return SaveSelectionToText(start, end, format, options);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Saves a selection to rich text or plain text
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="format">Indicates which format is generated</param>
        /// <param name="options">Options to control how styles are handled</param>
        /// <returns>Resulting text</returns>
        private string SaveSelectionToText(RichTextBoxPosition start, RichTextBoxPosition end, Format format, RichTextSaveOptions options)
        {
            StringBuilder result = new StringBuilder();
            int startElementIndex = ElementChildren.ContentChildren.IndexOf(start.Element);
            int endElementIndex = ElementChildren.ContentChildren.IndexOf(end.Element) - (end.Index == 0 && !(end.Element is TextBlockPlus) ? 1 : 0);
            RichTextBoxStyle currentStyle = new RichTextBoxStyle(Root.Styles[DefaultStyle]);
            TextBlockPlus tb;
            int index = 0;
            int length = 0;
            int i;

            _inParagraph = false;
            _paragraphTag = string.Empty;
            _inLink = false;
            _linkElementCount = 0;
            _inList = false;
            _listAttributes = string.Empty;

            if (format == Format.XML)
            {
                result.Append(GetXMLOpen() + "\r");
            }

            if ((options & RichTextSaveOptions.ListStyles) != 0)
            {
                switch (format)
                {
                    case Format.XML:
                        result.Append(GetStylesAsText(format, options));
                        break;
                    case Format.HTML:
                        if (Styles.Count > 0)
                        {
                            result.Append("<style type=\"text/css\">\r");
                            result.Append(GetStylesAsText(format, options));
                            result.Append("</style>\r");
                        }
                        break;
                }
            }

            if (startElementIndex >= 0 && endElementIndex >= 0)
            {
                for (i = startElementIndex; i <= endElementIndex; i++)
                {
                    if (ElementChildren.ContentChildren[i] is TextBlockPlus)
                    {
                        tb = (TextBlockPlus)ElementChildren.ContentChildren[i];
                        if (tb.Text.Length > 0)
                        {
                            index = (startElementIndex == i ? start.Index : 0);
                            length = (endElementIndex == i ? (startElementIndex == endElementIndex ? end.Index - start.Index : end.Index) : tb.Text.Length - index);
                        }
                        else
                        {
                            index = 0;
                            length = 0;
                        }
                    }

                    result.Append(GetElementAsText(ElementChildren.ContentChildren[i], i, index, length, format, ref currentStyle, options));
                }
            }
            switch (format)
            {
                case Format.HTML:
                    if (_inLink)
                    {
                        result.Append("</a>");
                        _inLink = false;
                        _linkElementCount = 0;
                    }
                    if (_inParagraph)
                    {
                        result.Append("</" + _paragraphTag + ">\r");
                    }
                    if (_inList)
                    {
                        result.Append("</li>\r</" + _listTag + ">\r");
                    }
                    break;
                case Format.XML:
                    result.Append(RichTextBox.XMLClose);
                    break;
                case Format.XAML:
                    if (_inParagraph)
                    {
                        result.Append("</Paragraph>");
                    }
                    break;
            }

            return result.ToString();
        }

        private string GetAttributes(UIElement element, string styleID, RichTextBoxStyle currentStyle, RichTextSaveOptions options, bool list)
        {
            string attributes = string.Empty;
            string inlineStyle = string.Empty;

            if (_inList && list)
            {
                return _listAttributes;
            }

            if (!Utility.IsStyleAHeading(styleID))
            {
                if ((options & RichTextSaveOptions.InlineStyles) != 0)
                {
                    if ((options & RichTextSaveOptions.ExcludeCustomStyles) != 0 && !styleID.StartsWith(CustomStyle))
                    {
                        attributes = " class=\"" + styleID + "\"";
                    }
                    else
                    {
                        inlineStyle = Root.Styles[styleID].ToInlineCSS(currentStyle, element);
                        if (inlineStyle.Length > 0)
                        {
                            attributes = " style=\"" + inlineStyle + "\"";
                        }

                        if (_inList)
                        {
                            _listAttributes = attributes;
                        }
                    }
                }
                else
                {
                    attributes = " class=\"" + styleID + "\"";
                }
            }

            return attributes;
        }

        /// <summary>
        /// Creates rich text or plain text out of an element
        /// </summary>
        /// <param name="element">Element to convert</param>
        /// <param name="elementIndex">The index of the current element</param>
        /// <param name="startIndex">Element index to start the conversion from</param>
        /// <param name="length">Length within the element to convert</param>
        /// <param name="format">Indicates which format is generated</param>
        /// <param name="currentStyle">The current style</param>
        /// <returns>Resulting text</returns>
        private string GetElementAsText(UIElement element, int elementIndex, int startIndex, int length, Format format, ref RichTextBoxStyle currentStyle, RichTextSaveOptions options)
        {
            StringBuilder result = new StringBuilder();
            string temp = string.Empty;
            string styleID = GetStyle(element);
            TextBlockPlus tempTextblock;
            RichTextTag tag = null;
            ContentMetadata metadata = null;
            string attributes;
            Bullet tempBullet;
            BitmapImage image;
            string tempTag;
            HorizontalAlignment lastAlignment = HorizontalAlignment.Left;

            if (element.GetValue(TagProperty) is RichTextTag)
            {
                tag = (RichTextTag)element.GetValue(TagProperty);
                metadata = tag.Metadata;
                if (!IsLinkMetadata(metadata) && _inLink)
                {
                    result.Append("</a>");
                    _inLink = false;
                    _linkElementCount = 0;
                }
            }

            if (element is TextBlockPlus)
            {
                // Textblock, we need just the text
                if (length > 0)
                {
                    tempTextblock = (TextBlockPlus)element;
                    if (startIndex + length <= tempTextblock.Text.Length)
                    {
                        temp = tempTextblock.Text.Substring(startIndex, length);

                        switch (format)
                        {
                            case Format.HTML:
                                lastAlignment = CheckIfInParagraph(element, result, lastAlignment, styleID);
                                result.Append(CheckIfInLink(element, currentStyle, options));

                                attributes = GetAttributes(element, styleID, currentStyle, options, false);

                                if (!Utility.IsStyleAHeading(styleID) || Utility.IsStyleAHeading(_paragraphTag))
                                {
                                    tempTag = "span";
                                }
                                else
                                {
                                    tempTag = styleID;
                                }
                                temp = HttpUtility.HtmlEncode(temp);
                                temp = temp.Replace("  ", "&nbsp;&nbsp;");
                                if (temp.StartsWith(" "))
                                {
                                    temp = "&nbsp;" + temp.Substring(1);
                                }

                                if (_inLink && _linkElementCount == 0)
                                {
                                    result.Append(temp);
                                }
                                else
                                {
                                    result.Append("<" + tempTag + attributes + ">" + temp + "</" + tempTag + ">");
                                }
                                break;
                            case Format.XML:
                                result.Append("<Text Style=\"" + styleID + "\"><![CDATA[" + temp + "]]></Text>\r");
                                
                                if (tag != null && tag.Metadata != null && tag.Metadata.Count > 0)
                                {
                                    result.Append(tag.ToXML());
                                }
                                break;
                            case Format.XAML:
                                if (!_inParagraph)
                                {
                                    result.Append("<Paragraph TextAlignment=\"" + Root.Styles[styleID].Alignment.ToString() + "\">");
                                }
                                result.Append(GetTextStyleTag(Root.Styles[styleID], temp));
                                break;
                            case Format.Text:
                                result.Append(temp);
                                break;
                        }
                    }
                    _inParagraph = true;
                }
            }
            else if (element is Newline)
            {
                // Newlines don't need any properties recording
                switch (format)
                {
                    case Format.HTML:
                        if (!_inParagraph)
                        {
                            result.Append("<br />");
                        }
                        else
                        {
                            if (_inLink)
                            {
                                result.Append("</a>");
                                _inLink = false;
                                _linkElementCount = 0;
                            }
                            result.Append("</" + _paragraphTag + ">\r");
                        }
                        _inParagraph = false;
                        _paragraphTag = "p";
                        CheckIfInList(result, elementIndex);
                        break;
                    case Format.XML:
                        result.Append("<Newline Style=\"" + styleID + "\" />\r");
                        break;
                    case Format.XAML:
                        if (!_inParagraph)
                        {
                            result.Append("<Paragraph>");
                        }
                        result.Append("</Paragraph>");
                        _inParagraph = false;
                        break;
                    case Format.Text:
                        result.Append("\r");
                        break;
                }
            }
            else if (element is Bullet)
            {
                tempBullet = (Bullet)element;

                // Bullets we need the type and number
                switch (format)
                {
                    case Format.HTML:
                        if (!_inList)
                        {
                            _inList = true;
                            _listTag = tempBullet.Type == BulletType.Bullet ? "ul" : "ol";
                            //attributes = GetAttributes(styleID, currentStyle, options, true);
                            attributes = "";
                            result.Append("<" + _listTag + attributes + ">\r");
                        }
                        styleID = DefaultStyle;
                        result.Append("<li>");
                        break;
                    case Format.XML:
                        result.Append("<Xaml><liquid:Bullet Type=\"" + tempBullet.Type.ToString() + "\" Number=\"" + tempBullet.Number.ToString() + "\" /></Xaml>\r");
                        break;
                    case Format.Text:
                        result.Append(GetOtherElementAsText(element, string.Empty, format));
                        break;
                }
            }
            else if (element is Table)
            {
                switch (format)
                {
                    case Format.HTML:
                        lastAlignment = CheckIfInParagraph(element, result, lastAlignment, styleID);
                        _inParagraph = true;
                        result.Append(Serialize.TableAsHTML((Table)element, options));
                        break;
                    case Format.XML:
                        result.Append(Serialize.TableAsXML((Table)element, options));
                        break;
                    case Format.Text:
                        result.Append(GetOtherElementAsText(element, string.Empty, format));
                        break;
                }
            }
            else if (element is PlaceHolder)
            {
                switch (format)
                {
                    case Format.HTML:
                    case Format.Text:
                        result.Append(((PlaceHolder)element).Value);
                        break;
                    case Format.XML:
                        result.Append(Serialize.PlaceHolderAsXML((PlaceHolder)element));
                        break;
                }
            }
            else if (element is Image)
            {
                image = (BitmapImage)((Image)element).Source;
                temp = image.UriSource.ToString();

                switch (format)
                {
                    case Format.HTML:
                        lastAlignment = CheckIfInParagraph(element, result, lastAlignment, styleID);
                        result.Append(CheckIfInLink(element, currentStyle, options));

                        attributes = string.Empty;
                        if (metadata != null)
                        {
                            if (metadata.IsImage)
                            {
                                if (metadata.ContainsKey("ImageWidth"))
                                {
                                    attributes += " width=\"" + metadata["ImageWidth"] + "\"";
                                }
                                if (metadata.ContainsKey("ImageHeight"))
                                {
                                    attributes += " height=\"" + metadata["ImageHeight"] + "\"";
                                }
                                if (metadata.ContainsKey("ImageAlt"))
                                {
                                    attributes += " alt=\"" + metadata["ImageAlt"] + "\"";
                                }
                                if (metadata.ContainsKey("ImageSrc"))
                                {
                                    temp = metadata["ImageSrc"];
                                }
                            }
                        }

                        result.Append("<img src=\"" + (temp.ToLower().StartsWith(Root.BaseURL.ToLower()) ? temp : Root.BaseURL + temp) + "\"" + attributes + " />");
                        _inParagraph = true;
                        break;
                    case Format.XML:
                    case Format.Text:
                        result.Append(GetOtherElementAsText(element, string.Empty, format));
                        break;
                }
            }
            else
            {
                result.Append(GetOtherElementAsText(element, string.Empty, format));
            }

            currentStyle = Root.Styles[styleID];

            if (_inLink)
            {
                _linkElementCount++;
            }

            return result.ToString();
        }

        private void CheckIfInList(StringBuilder html, int elementIndex)
        {
            if (_inList)
            {
                html.Append("</li>\r");
                if (elementIndex + 1 < ElementChildren.ContentChildren.Count)
                {
                    if (!(ElementChildren.ContentChildren[elementIndex + 1] is Bullet))
                    {
                        html.Append("</" + _listTag + ">\r");
                        _inList = false;
                    }
                }
            }
        }

        private HorizontalAlignment CheckIfInParagraph(UIElement element, StringBuilder html, HorizontalAlignment current, string styleID)
        {
            string attributes;
            string styleTag = string.Empty;
            int elementIndex = ElementChildren.ContentChildren.IndexOf(element);
            int count = 0;
            int i;

            _paragraphTag = Utility.IsStyleAHeading(styleID) ? styleID : "p";
            if (!_inParagraph)
            {
                attributes = string.Empty;
                if (Root.Styles[styleID].Alignment != current)
                {
                    styleTag += "text-align:" + Root.Styles[styleID].Alignment.ToString() + ";";
                    current = Root.Styles[styleID].Alignment;
                }

                for (i = elementIndex; i < ElementChildren.ContentChildren.Count; i++)
                {
                    if (ElementChildren.ContentChildren[i] is Newline)
                    {
                        count++;
                        if (count >= 2)
                        {
                            break;
                        }
                    }
                }

                if (_paragraphTag == "p" && count <= 1)
                {
                    styleTag += "margin:0px;";
                }

                if (styleTag.Length > 0)
                {
                    attributes += " style=\"" + styleTag + "\"";
                }

                html.Append("<" + _paragraphTag + attributes + ">");
            }

            return current;
        }

        private string CheckIfInLink(UIElement element, RichTextBoxStyle currentStyle, RichTextSaveOptions options)
        {
            ContentMetadata metadata = GetMetadata(element);
            string attributes = string.Empty;
            string result = string.Empty;
            string style;

            if (IsLinkMetadata(metadata) && !_inLink)
            {
                if (metadata.ContainsKey("Target") && metadata["Target"].Length > 0)
                {
                    attributes += " target=\"" + metadata["Target"] + "\"";
                }

                if (metadata.ContainsKey("Title") && metadata["Title"].Length > 0)
                {
                    attributes += " title=\"" + metadata["Title"] + "\"";
                }

                if (element is TextBlockPlus)
                {
                    style = GetStyle(element);

                    if (style != "Normal")
                    {
                        attributes = GetAttributes(element, style, currentStyle, options, false);
                    }
                }

                result = "<a href=\"" + metadata["URL"] + "\"" + attributes + ">";
                _inLink = true;
            }

            return result;
        }

        /// <summary>
        /// Builds a XAML string representation using the provided style
        /// </summary>
        /// <param name="style">Style to use</param>
        /// <param name="text">Text</param>
        /// <returns>Run tag with style/effects included</returns>
        private string GetTextStyleTag(RichTextBoxStyle style, string text)
        {
            StringBuilder result = new StringBuilder();
            SolidColorBrush foreground = (SolidColorBrush)style.Foreground;
            SolidColorBrush background = (SolidColorBrush)style.Background;

            result.Append("<Run ");
            result.Append("FontFamily=\"" + style.Family + "\" ");
            result.Append("FontSize=\"" + style.Size + "\" ");
            result.Append("Foreground=\"" + foreground.Color.ToString() + "\" ");
            if (background.Color.A > 0)
            {
                result.Append("Background=\"" + background.Color.ToString() + "\" ");
            }

            if (style.Weight == FontWeights.Bold)
            {
                result.Append("FontWeight=\"Bold\" ");
            }
            if (style.Style == FontStyles.Italic)
            {
                result.Append("FontStyle=\"Italic\" ");
            }
            if (style.Decorations == TextDecorations.Underline)
            {
                result.Append("TextDecorations=\"Underline\" ");
            }
            if (style.Special == RichTextSpecialFormatting.Subscript)
            {
                result.Append("BaselineAlignment=\"Subscript\" ");
            }
            if (style.Special == RichTextSpecialFormatting.Superscript)
            {
                result.Append("BaselineAlignment=\"Superscript\" ");
            }

            result.Append(">");
            result.Append(Utility.XmlEncode(text));
            result.Append("</Run>");

            return result.ToString();
        }

        /// <summary>
        /// Converts a non-standard element to a text format
        /// </summary>
        /// <param name="element">Element to convert</param>
        /// <param name="prefix">An optional prefix</param>
        /// <param name="format">Format of the output text</param>
        /// <returns>Resulting text</returns>
        private string GetOtherElementAsText(UIElement element, string prefix, Format format)
        {
            string result = string.Empty;
            string[] split;
            string temp;
            string tag;
            RichTextBoxEventArgs args = new RichTextBoxEventArgs(element);
            RichTextTag richTextTag;

            if (element == null)
            {
                return result;
            }

            args.Format = format;
            args.Prefix = prefix;
            args.Content = string.Empty;
            args.Parameter = string.Empty;
            RaiseRichTextEvent(Root.ElementWrite, this, args, false);

            if (!args.Cancel)
            {
                switch (format)
                {
                    case Format.XML:
                        if (args.Parameter.ToString().StartsWith("<"))
                        {
                            result = "<Xaml>" + args.Parameter.ToString() + "</Xaml>";
                        }
                        else
                        {
                            split = element.GetType().ToString().Split('.');
                            temp = split[split.Length - 1];
                            tag = (args.Prefix.Length > 0 ? args.Prefix.TrimEnd(':') + ":" : "") + temp;

                            result = "<Xaml><" + tag;
                            temp = " Width=\"" + (double.IsNaN((double)element.GetValue(Canvas.WidthProperty)) ? "Auto\"" : element.GetValue(Canvas.WidthProperty).ToString() + "\"");
                            result += temp;
                            temp = " Height=\"" + (double.IsNaN((double)element.GetValue(Canvas.HeightProperty)) ? "Auto\"" : element.GetValue(Canvas.HeightProperty).ToString() + "\"");
                            result += temp;

                            richTextTag = ((FrameworkElement)element).Tag as RichTextTag;

                            if (element is Image)
                            {
                                temp = ((BitmapImage)((Image)element).Source).UriSource.ToString();

                                if (richTextTag != null && richTextTag.Metadata != null)
                                {
                                    if (richTextTag.Metadata.ContainsKey("ImageSrc"))
                                    {
                                        temp = Root.BaseURL + richTextTag.Metadata["ImageSrc"];
                                    }
                                }
                                
                                result += " Source=\"" + temp + "\"";
                            }

                            result += " " + args.Parameter;
                            result += ">";
                            result += args.Content;
                            result += "</" + tag + ">\r";
                            result += "</Xaml>\r";

                            if (richTextTag != null)
                            {
                                result += richTextTag.ToXML();
                            }
                        }
                        break;
                    case Format.Text:
                        result = (args.Content.Length > 0 ? args.Content : "[Liquid XAML Object]");
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates an XML declaration of the current styles
        /// </summary>
        /// <returns>Styles in XML format</returns>
        private string GetStylesAsText(Format format, RichTextSaveOptions options)
        {
            Dictionary<string, Dictionary<string, string>> temp;
            Dictionary<string, string> keyValues;
            StringBuilder result = new StringBuilder();
            bool includeStyle;

            if (format == Format.HTML)
            {
                if ((options & RichTextSaveOptions.ListStyles) != 0)
                {
                    keyValues = new Dictionary<string, string>();
                    keyValues.Add("margin", "0px 0px 0px 2px");
                    result.Append(WriteStyleAsHTML("p", keyValues));

                    keyValues.Clear();
                    keyValues.Add("margin-top", "2px");
                    keyValues.Add("margin-bottom", "2px");
                    result.Append(WriteStyleAsHTML("ul", keyValues));

                    keyValues.Clear();
                    keyValues.Add("margin-top", "2px");
                    keyValues.Add("margin-bottom", "2px");
                    result.Append(WriteStyleAsHTML("ol", keyValues));
                }

                foreach (KeyValuePair<string, RichTextBoxStyle> style in Styles)
                {
                    includeStyle = !((options & RichTextSaveOptions.ExcludeCustomStyles) == RichTextSaveOptions.ExcludeCustomStyles && style.Key.StartsWith(CustomStyle));
                    if ((options & RichTextSaveOptions.ListStyles) != 0 && includeStyle)
                    {
                        if (IsStyleUsed(style.Key))
                        {
                            result.Append(WriteStyleAsHTML(Utility.IsStyleAHeading(style.Key) ? style.Key : "." + style.Key, style.Value.ToHTML()));
                        }
                    }
                }
                foreach (KeyValuePair<string, RichTextBoxTableStyle> style in TableStyles)
                {
                    temp = style.Value.ToHTML();

                    foreach (KeyValuePair<string, Dictionary<string, string>> pair in temp)
                    {
                        result.Append(WriteStyleAsHTML("." + pair.Key, pair.Value));
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<string, RichTextBoxStyle> style in Styles)
                {
                    includeStyle = !((options & RichTextSaveOptions.ExcludeCustomStyles) == RichTextSaveOptions.ExcludeCustomStyles && style.Key.StartsWith(CustomStyle));
                    if ((options & RichTextSaveOptions.ListStyles) != 0 && includeStyle)
                    {
                        if (IsStyleUsed(style.Key))
                        {
                            result.Append(style.Value.ToXML());
                        }
                    }
                }
                foreach (KeyValuePair<string, RichTextBoxTableStyle> style in TableStyles)
                {
                    result.Append(style.Value.ToXML());
                }
            }

            return result.ToString();
        }

        private string WriteStyleAsHTML(string id, Dictionary<string, string> keyValues)
        {
            StringBuilder results = new StringBuilder();
            RichTextBoxHtmlEventArgs args = new RichTextBoxHtmlEventArgs()
            {
                ID = id,
                Styles = keyValues
            };

            RaiseWriteStyleToHtml(this, args);
            if (!args.Cancel)
            {
                results.Append(args.ID + " {");
                foreach (KeyValuePair<string, string> pair in args.Styles)
                {
                    results.Append(pair.Key + ":" + pair.Value + ";");
                }
                results.Append("}\r");
            }

            return results.ToString();
        }

        /// <summary>
        /// Converts a block of CSS styles to RichText styles
        /// </summary>
        /// <param name="styles">CSS styles</param>
        /// <returns>RichText styles</returns>
        private void GetStylesFromHTML(string styles)
        {
            string[] split1 = styles.Split('}');
            RichTextBoxStyle style;
            RichTextBoxTableStyle tableStyle;
            RichTextBlock root = Root;
            string temp;
            string[] split;

            foreach (string s in split1)
            {
                temp = s.Trim();

                if (temp.Length > 0)
                {
                    if (temp.ToLower().StartsWith(".table"))
                    {
                        split = temp.TrimStart('.').Split(' ');
                        if (root.TableStyles.ContainsKey(split[0]))
                        {
                            tableStyle = root.TableStyles[split[0]];
                        }
                        else
                        {
                            tableStyle = new RichTextBoxTableStyle();
                            root.TableStyles.Add(split[0], tableStyle);
                        }
                        tableStyle.FromHTML(split[0], temp + "}");
                    }
                    else
                    {
                        if (temp.StartsWith("."))
                        {
                            style = new RichTextBoxStyle();
                            style.FromHTML(temp + "}");
                            AddStyle(style);
                            CheckForCustomStyle(style.ID);
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Creates a new TextBlock based on the provided template and sets the text
        /// </summary>
        /// <param name="text">Text to use</param>
        /// <param name="template">Element to base the styles on</param>
        /// <param name="useTemplateMetadata">Determines whether the template metadata is copied over</param>
        /// <returns>A new TextBlock element</returns>
        private TextBlockPlus CreateTextBlock(string text, UIElement template, bool useTemplateMetadata)
        {
            string style = DefaultStyle;
            RichTextTag tag = null;

            if (template != null)
            {
                if (template is Table)
                {
                }
                else
                {
                    tag = (RichTextTag)template.GetValue(Canvas.TagProperty);
                    style = tag.StyleID;
                }
            }
            if (style.Length > 0 && Root.Styles.ContainsKey(style))
            {
                return CreateTextBlock(text, Root.Styles[style], (useTemplateMetadata ? tag.Metadata : null));
            }

            return null;
        }

        /// <summary>
        /// Creates a new TextBlock object
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="style">The y-position</param>
        /// <returns>The created TextBlock</returns>
        private TextBlockPlus CreateTextBlock(string text, RichTextBoxStyle style, ContentMetadata metadata)
        {
            TextBlockPlus tb = new TextBlockPlus();

            ApplyMetadataToElement(tb, metadata);
            ApplyStyleToElement(tb, style);

            tb.Text = text;

            return tb;
        }
      
        /// <summary>
        /// Creates an element from the supplied XAML string
        /// </summary>
        /// <param name="xaml">XAML string</param>
        /// <returns>Instantiated element</returns>
        private UIElement CreateElementFromXaml(string xaml)
        {
            return SetupElement((UIElement)Utility.CreateFromXaml(xaml));
        }

        /// <summary>
        /// Sets the table events and hooks all the child RichTextBlocks up
        /// </summary>
        /// <param name="table">Table object</param>
        private void SetupTable(Table table)
        {
            string styleID = "TableDefault";

            if (table.Tag is RichTextTag)
            {
                styleID = ((RichTextTag)table.Tag).StyleID;
            }
            else if (table.Tag != null)
            {
                styleID = table.Tag.ToString();
            }

            if (Root.TableStyles.ContainsKey(styleID))
            {
                Root.TableStyles[styleID].ApplyToTable(table);
            }

            table.Clicked += new TableEventHandler(table_Clicked);
            table.SelectionChanged += new TableEventHandler(table_SelectionChanged);
            table.ContentChanged += new TableEventHandler(table_ContentChanged);
            SetupTableChildren(table);
        }

        private void SetupTableChildren(Table table)
        {
            Border border;

            foreach (UIElement e in table.Children)
            {
                // Hook-up all child richtextblocks to this instance
                if (e is Border)
                {
                    border = (Border)e;
                    if (border.Child is RichTextBlock)
                    {
                        ((RichTextBlock)border.Child).ParentRTB = this;
                    }
                }
            }
        }

        private void table_Clicked(object sender, TableEventArgs e)
        {
            ActiveTable = (Table)sender;
        }

        private void table_SelectionChanged(object sender, TableEventArgs e)
        {
            SelectionHasChanged();
        }

        private void table_ContentChanged(object sender, TableEventArgs e)
        {
            RaiseRichTextEvent(Root.ContentChanged, this, new RichTextBoxEventArgs(), false);
        }

        private void xamlElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UIElement marker;

            if (ElementChildren != null)
            {
                ElementChildren.Update((UIElement)sender, false);
                SetCursor(_cursorPosition, false);

                marker = GetObjectMarker((FrameworkElement)sender);
                if (marker != null)
                {
                    ElementObjectSelection.Children.Remove(marker);
                }
            }
        }

        /// <summary>
        /// Gets an existing style for the provided style, creating one if one doesn't exist
        /// </summary>
        /// <param name="style">The style object</param>
        /// <returns>The matching style object</returns>
        private RichTextBoxStyle GetStyle(RichTextBoxStyle style)
        {
            RichTextBoxStyle result = null;

            foreach (KeyValuePair<string, RichTextBoxStyle> r in Root.Styles)
            {
                if (CompareStyles(r.Value, style))
                {
                    result = r.Value;
                    break;
                }
            }

            if (result == null)
            {
                result = style;
                result.ID = CustomStyle + NextStyleNumber.ToString();
                AddStyle(result);

                NextStyleNumber++;
            }

            return result;
        }

        /// <summary>
        /// Determines whether two styles are the same
        /// </summary>
        /// <param name="a">Style 1</param>
        /// <param name="b">Style 2</param>
        /// <returns>True if they match, false if not</returns>
        private bool CompareStyles(RichTextBoxStyle a, RichTextBoxStyle b)
        {
            return (a.Family == b.Family && a.Size == b.Size && a.Style == b.Style && a.Weight == b.Weight &&
                    a.Decorations == b.Decorations && a.Alignment == b.Alignment &&
                    a.Special == b.Special && a.Effect == b.Effect && a.BorderType == b.BorderType &&
                    a.Shadow == b.Shadow &&
                    CompareBrush(a.Foreground, b.Foreground) &&
                    CompareBrush(a.Background, b.Background) &&
                    a.Margin.Left == b.Margin.Left && a.Margin.Right == b.Margin.Right &&
                    a.Margin.Top == b.Margin.Top && a.Margin.Bottom == b.Margin.Bottom &&
                    a.VerticalAlignment == b.VerticalAlignment);
        }

        /// <summary>
        /// Determines whether 2 brushes are the same
        /// </summary>
        /// <param name="a">Brush A</param>
        /// <param name="b">Brush B</param>
        /// <returns>Indicates whether the brushes are the same</returns>
        private bool CompareBrush(Brush a, Brush b)
        {
            bool result = (a == b);

            if (!result && a is SolidColorBrush && b is SolidColorBrush)
            {
                if (((SolidColorBrush)a).Color == ((SolidColorBrush)b).Color)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the style name used by a given element
        /// </summary>
        /// <param name="element">An element</param>
        /// <returns>The style name used</returns>
        private string GetStyle(UIElement element)
        {
            if (element == null || element is Table)
            {
                return DefaultStyle;
            }

            return (element.GetValue(Canvas.TagProperty) is RichTextTag ? ((RichTextTag)element.GetValue(Canvas.TagProperty)).StyleID : DefaultStyle);
        }

        /// <summary>
        /// Gets the metadata tag of a given element
        /// </summary>
        /// <param name="element">Element</param>
        /// <returns>ContentMetadata object</returns>
        private ContentMetadata GetMetadata(UIElement element)
        {
            ContentMetadata result = null;
            RichTextTag tag;

            if (element == null)
            {
                return null;
            }

            if (element.GetValue(TagProperty) is RichTextTag)
            {
                tag = (RichTextTag)element.GetValue(TagProperty);
                result = tag.Metadata;
            }

            return result;
        }

        /// <summary>
        /// Applies a style to an element
        /// </summary>
        /// <param name="element">An element to apply the style to</param>
        /// <param name="style">A style name</param>
        private void ApplyStyleToElement(UIElement element, string style)
        {
            if (Root.Styles.ContainsKey(style))
            {
                ApplyStyleToElement(element, Root.Styles[style]);
            }
            else if (!Root.TableStyles.ContainsKey(style))
            {
                ApplyStyleToElement(element, Root.Styles[DefaultStyle]);
            }
        }

        /// <summary>
        /// Applies a style to an element
        /// </summary>
        /// <param name="element">An element to apply the style to</param>
        /// <param name="style">A style object</param>
        private void ApplyStyleToElement(UIElement element, RichTextBoxStyle style)
        {
            Bullet tempBullet;
            RichTextTag currentTag;
            RichTextTag newTag = new RichTextTag(style.ID);

            if (element == null)
            {
                return;
            }
            else if (element is Table)
            {
                element.SetValue(FrameworkElement.HorizontalAlignmentProperty, style.Alignment);
                return;
            }

            if (element.GetValue(FrameworkElement.TagProperty) is RichTextTag)
            {
                currentTag = (RichTextTag)element.GetValue(FrameworkElement.TagProperty);
                newTag.StyleID = style.ID;
                newTag.Tag = currentTag.Tag;
                newTag.Metadata = currentTag.Metadata;
            }

            element.SetValue(Canvas.TagProperty, newTag);

            if (element is TextBlockPlus)
            {
                ((TextBlockPlus)element).ApplyStyle(style, (EnableGlobalLinkStyle ? LinkStyle : style), EnableLinkButtons);
                return;
            }
            else if (element is Bullet)
            {
                tempBullet = (Bullet)element;
                tempBullet.ApplyStyle(style);
            }
            else
            {
                element.SetValue(Canvas.HorizontalAlignmentProperty, style.Alignment);
                element.SetValue(Canvas.VerticalAlignmentProperty, style.VerticalAlignment);
            }

            element.SetValue(Canvas.MarginProperty, new Thickness(style.Margin.Left, style.Margin.Top, style.Margin.Right, style.Margin.Bottom));
        }

        /// <summary>
        /// Applies a metadata tag to an element
        /// </summary>
        /// <param name="element">Element to apply to</param>
        /// <param name="metadata">A ContentMetadata object</param>
        private void ApplyMetadataToElement(UIElement element, ContentMetadata metadata)
        {
            RichTextTag currentTag;
            RichTextTag newTag = new RichTextTag();

            if (element == null || element is Table || metadata == null)
            {
                return;
            }

            if (element.GetValue(FrameworkElement.TagProperty) is RichTextTag)
            {
                currentTag = (RichTextTag)element.GetValue(FrameworkElement.TagProperty);
                newTag.StyleID = currentTag.StyleID;
                newTag.Tag = currentTag.Tag;
            }

            newTag.Metadata = metadata;

            if (element is Image && metadata.IsImage)
            {
                if (metadata.ContainsKey("ImageWidth"))
                {
                    element.SetValue(FrameworkElement.WidthProperty, double.Parse(metadata["ImageWidth"]));
                }
                if (metadata.ContainsKey("ImageHeight"))
                {
                    element.SetValue(FrameworkElement.HeightProperty, double.Parse(metadata["ImageHeight"]));
                }
                if (metadata.ContainsKey("ImageAlt") && metadata["ImageAlt"].Length > 0)
                {
                    ToolTipService.SetToolTip(element, metadata["ImageAlt"]);
                }
            }

            element.SetValue(Canvas.TagProperty, newTag);
        }

        /// <summary>
        /// Recalculates the position of the cursor
        /// </summary>
        private void UpdateCursor()
        {
            if (ElementChildren != null)
            {
                _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
                _selectedStart.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
                _selectedEnd.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
            }
        }

        /// <summary>
        /// Sets the position of the cursor
        /// </summary>
        private void SetCursor(RichTextBoxPosition position, bool updateScrolling)
        {
            RichTextPanelRow row;
            double top;
            double height;

            if (position.Element != null && ElementCursor != null)
            {
                row = ElementChildren.GetRowForElement(position.Element);

                if (row != null)
                {
                    if (position.Element is TextBlockPlus)
                    {
                        height = ((TextBlockPlus)position.Element).ContentHeight;
                        top = (double)position.Element.GetValue(Canvas.TopProperty) + ((Thickness)position.Element.GetValue(Canvas.MarginProperty)).Top;
                    }
                    else
                    {
                        height = row.Dimensions.Y;
                        top = row.Position.Y;
                    }
                    ElementCursor.SetValue(Canvas.TopProperty, Math.Round(top, 0));
                    ElementCursor.SetValue(Canvas.LeftProperty, Math.Round(position.Position.X, 0));
                    ElementCursor.Height = height;
                }

                if (updateScrolling)
                {
                    OnCursorMoved();
                    RaiseRichTextEvent(Root.CursorMoved, this, new RichTextBoxEventArgs(), false);
                }
            }
        }

        /// <summary>
        /// Recalculates the positioning and raises the appropriate events
        /// </summary>
        private void SelectionHasChanged()
        {
            RichTextBlock root = Root;

            ProcessUpdateSelectionStyle();
            root.SelectionStyles = SelectionStyles;
            root.SelectionMetadatas = SelectionMetadatas;
                        
            RaiseRichTextEvent(root.SelectionChanged, this, null, false);
        }

        /// <summary>
        /// Sets the selection style, this is a union of all styles that may appy to the selected elements
        /// </summary>
        private void ProcessUpdateSelectionStyle()
        {
            if (ElementChildren == null)
            {
                return;
            }

            int startIndex = ElementChildren.ContentChildren.IndexOf(_selectedStart.Element);
            int endIndex = ElementChildren.ContentChildren.IndexOf(_selectedEnd.Element);
            StyleSelection current = null;
            StyleSelection previous;
            MetadataSelection currentMetadata = null;
            ContentMetadata metadata;
            UIElement element;
            int currentIndex = _selectedStart.GlobalIndex;
            int i;

            SelectionStyles.Clear();
            SelectionMetadatas.Clear();

            if (_selectedStart.Element is TextBlockPlus)
            {
                if (ContentSelectionLength > 0 && ((TextBlockPlus)_selectedStart.Element).Text.Length == _selectedStart.Index && startIndex < ElementChildren.ContentChildren.Count - 1)
                {
                    startIndex++;
                }
            }

            if (startIndex < 0)
            {
                return;
            }

            for (i = startIndex; i <= endIndex; i++)
            {
                element = ElementChildren.ContentChildren[i];
                metadata = GetMetadata(element);

                current = new StyleSelection()
                {
                    Style = Root.Styles[GetStyle(element)],
                    StartIndex = currentIndex,
                    ListType = GetListType(i),
                    Alignment = (HorizontalAlignment)element.GetValue(Canvas.HorizontalAlignmentProperty),
                    VerticalAlignment = (VerticalAlignment)element.GetValue(Canvas.VerticalAlignmentProperty)
                };

                if (startIndex == endIndex)
                {
                    current.Length = ContentSelectionLength;
                }
                else if (i == startIndex && element is TextBlockPlus)
                {
                    current.Length = ((TextBlockPlus)element).Text.Length - _selectedStart.Index;
                }
                else if (i == endIndex && element is TextBlockPlus)
                {
                    current.Length = _selectedEnd.Index;
                }
                else
                {
                    current.Length = (element is TextBlockPlus ? ((TextBlockPlus)element).Text.Length : 1);
                }

                currentMetadata = new MetadataSelection()
                {
                    StartIndex = current.StartIndex,
                    Length = current.Length,
                    Metadata = metadata
                };

                SelectionMetadatas.Add(currentMetadata);

                if (SelectionStyles.Count > 0)
                {
                    previous = SelectionStyles[SelectionStyles.Count - 1];
                    if (current.Style.ID != previous.Style.ID || current.Alignment != previous.Alignment || current.VerticalAlignment != previous.VerticalAlignment || current.ListType != current.ListType)
                    {
                        SelectionStyles.Add(current);
                    }
                    else
                    {
                        previous.Length += current.Length;
                    }
                }
                else
                {
                    SelectionStyles.Add(current);
                }

                currentIndex += current.Length;
            }
        }

        /// <summary>
        /// Calculates the visual position of the selection area
        /// </summary>
        private void CalculateSelection()
        {
            SetSelection(_selectedStart, _selectedEnd, true);
        }

        /// <summary>
        /// Sets the selected area
        /// </summary>
        /// <param name="startIndex">Selection Index</param>
        /// <param name="length">Selection Length</param>
        /// <param name="indicateSelectionChanged">Indicates whether the selection changed event should be raised</param>
        private void SetSelection(int startIndex, int length, bool indicateSelectionChanged)
        {
            if (ElementChildren == null)
            {
                return;
            }

            _selectedStart = new RichTextBoxPosition();
            _selectedEnd = new RichTextBoxPosition();

            _selectedStart.GlobalIndex = startIndex;
            _selectedEnd.GlobalIndex = startIndex + length;

            _selectedStart.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
            _selectedEnd.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);

            SetSelection(_selectedStart, _selectedEnd, false);
            _cursorPosition = new RichTextBoxPosition(_selectedEnd);
            SetCursor(_cursorPosition, true);

            if (indicateSelectionChanged)
            {
                SelectionHasChanged();
            }
        }

        /// <summary>
        /// Sets the selected text
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        private void SetSelection(RichTextBoxPosition start, RichTextBoxPosition end, bool indicateSelectionChanged)
        {
            RichTextPanelRow startLine;
            RichTextPanelRow endLine;
            RichTextPanelRow currentLine;
            int startLineIndex;
            int endLineIndex;
            RichTextBoxPosition temp;
            Point p;
            int i;

            if (start.Element == null || end.Element == null || ElementSelection == null)
            {
                return;
            }

            startLineIndex = ElementChildren.GetRowIndexForElement(start.Element);
            endLineIndex = ElementChildren.GetRowIndexForElement(end.Element);

            if (startLineIndex == -1 || endLineIndex == -1 ||
                startLineIndex >= ElementChildren.Rows.Count || endLineIndex >= ElementChildren.Rows.Count)
            {
                return;
            }

            if (startLineIndex > endLineIndex)
            {
                temp = end;
                end = start;
                start = temp;
                i = endLineIndex;
                endLineIndex = startLineIndex;
                startLineIndex = i;
            }

            startLine = ElementChildren.Rows[startLineIndex];
            endLine = ElementChildren.Rows[endLineIndex];

            p = startLine.Position;

            ElementSelection.Points.Clear();
            ElementSelection.Points.Add(new Point(start.Position.X, p.Y));

            for (i = startLineIndex; i < endLineIndex; i++)
            {
                currentLine = ElementChildren.Rows[i];
                p = currentLine.Position;

                ElementSelection.Points.Add(new Point(p.X + currentLine.Dimensions.X, p.Y));
                ElementSelection.Points.Add(new Point(p.X + currentLine.Dimensions.X, p.Y + currentLine.Dimensions.Y));
            }

            p = endLine.Position;

            ElementSelection.Points.Add(new Point(end.Position.X, p.Y));
            ElementSelection.Points.Add(new Point(end.Position.X, p.Y + endLine.Dimensions.Y));

            for (i = endLineIndex; i > startLineIndex; i--)
            {
                currentLine = ElementChildren.Rows[i];
                p = currentLine.Position;

                ElementSelection.Points.Add(new Point(p.X, p.Y + currentLine.Dimensions.Y));
                ElementSelection.Points.Add(new Point(p.X, p.Y));
            }

            p = startLine.Position;
            ElementSelection.Points.Add(new Point(start.Position.X, p.Y + startLine.Dimensions.Y));
            ElementSelection.Points.Add(new Point(start.Position.X, p.Y));

            _selectedStart = new RichTextBoxPosition(start);
            _selectedEnd = new RichTextBoxPosition(end);

            ElementSelection.Visibility = (_objectSelected ? Visibility.Collapsed : Visibility.Visible);
            ElementSelection.SetValue(Canvas.LeftProperty, 0d);
            ElementSelection.SetValue(Canvas.TopProperty, 0d);

            if (ContentSelectionLength > 0 || _objectSelected)
            {
                HideCursor();
            }

            if (indicateSelectionChanged)
            {
                SelectionHasChanged();
            }
        }

        /// <summary>
        /// Splits the elements and prepares the selected elements for processing
        /// </summary>
        private void PrepareSelection()
        {
            bool temp;

            _cursorPosition = new RichTextBoxPosition(_cursorPosition);

            if (ContentSelectionLength > 0)
            {
                SplitElement(_selectedStart, false, true);

                _selectedEnd.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);

                SplitElement(_selectedEnd, true, false);
            }
            else
            {
                _selectedStart = new RichTextBoxPosition(_cursorPosition);
                _selectedEnd = new RichTextBoxPosition(_cursorPosition);

                temp = SplitElement(_selectedStart, false, true);

                _selectedEnd.Element = _selectedStart.Element;
                _selectedEnd.Index = 0;

                if (!temp)
                {
                    SplitElement(_selectedEnd, true, true);
                }

                _selectedStart = new RichTextBoxPosition(_selectedEnd);
            }
            _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
        }

        /// <summary>
        /// Splits an element
        /// </summary>
        /// <param name="position">Position obeject to work with</param>
        /// <param name="selectNew">Indicates whether the new element should be selected</param>
        private bool SplitElement(RichTextBoxPosition position, bool splitAfter, bool selectNew)
        {
            TextBlockPlus currentTextblock;
            TextBlockPlus newTextblock = null;
            RichTextPanelRow row = ElementChildren.GetRowForElement(position.Element);
            int indexInElements = ElementChildren.ContentChildren.IndexOf(position.Element);
            bool result = false;

            if (row != null && position.Element is TextBlockPlus)
            {
                currentTextblock = (TextBlockPlus)position.Element;

                if (!splitAfter && position.Index == 0)
                {
                    return false;
                }
                if (splitAfter && position.Index == currentTextblock.Text.Length)
                {
                    return false;
                }

                if (position.Index == 0)
                {
                    newTextblock = CreateTextBlock(string.Empty, currentTextblock, false);

                    ElementChildren.Insert(indexInElements, newTextblock);
                    selectNew = true;
                    result = true;
                }
                else if (position.Index > 0 && position.Index <= currentTextblock.Text.Length)
                {
                    newTextblock = CreateTextBlock(currentTextblock.Text.Substring(position.Index), currentTextblock, true);
                    currentTextblock.Text = currentTextblock.Text.Substring(0, position.Index);
                    ElementChildren.Insert(indexInElements + 1, newTextblock);

                    SplitHilights(currentTextblock, newTextblock);
                    if (position.Element == row.End)
                    {
                        row.End = newTextblock;
                    }
                    result = false;
                }

                if (selectNew && newTextblock != null)
                {
                    position.Element = newTextblock;
                    position.Index = 0;
                }
                else
                {
                    position.Index = currentTextblock.Text.Length;
                }
            }

            return result;
        }

        /// <summary>
        /// This method checks the supplied position and if it is on an end-boundary with another textblock it selects the next textblock
        /// </summary>
        /// <param name="position">RichTextBoxPosition position</param>
        /// <returns>New RichTextBoxPosition</returns>
        private RichTextBoxPosition RoundPositionUp(RichTextBoxPosition position)
        {
            TextBlockPlus tb;
            int indexInElements = ElementChildren.ContentChildren.IndexOf(position.Element);

            if (position.Element is TextBlockPlus)
            {
                tb = (TextBlockPlus)position.Element;

                if (position.Index == tb.Text.Length && indexInElements < ElementChildren.ContentChildren.Count - 1)
                {
                    if (!(ElementChildren.ContentChildren[indexInElements + 1] is Newline))
                    {
                        position.Element = ElementChildren.ContentChildren[indexInElements + 1];
                        position.Index = 0;
                    }
                }
            }

            return new RichTextBoxPosition(position);
        }

        /// <summary>
        /// Gets the position within an element
        /// </summary>
        /// <param name="p">Position</param>
        /// <returns>A RichTextBoxPosition obejct</returns>
        protected RichTextBoxPosition GetPosition(Point p, bool multiSelecting)
        {
            UIElement element;
            RichTextBoxPosition result = null;
            int index;
            int i;

            element = GetElementAtPosition(p, out index, true);

            if (element != null)
            {
                if (element is Bullet && !multiSelecting)
                {
                    i = ElementChildren.ContentChildren.IndexOf(element);
                    if (i < ElementChildren.ContentChildren.Count - 1)
                    {
                        element = ElementChildren.ContentChildren[i + 1];
                    }
                }

                if (element is Newline)
                {
                    i = ElementChildren.ContentChildren.IndexOf(element);
                    if (i > 0)
                    {
                        element = ElementChildren.ContentChildren[i - 1];
                        if (element is TextBlockPlus)
                        {
                            index = ((TextBlockPlus)element).Text.Length;
                        }
                    }
                }

                result = new RichTextBoxPosition(element, index);
                result.CalculateGlobalIndex(ElementChildren.ContentChildren);
            }

            return result;
        }

        /// <summary>
        /// Calculates the element at a given pixel position
        /// </summary>
        /// <param name="position">Pixel position</param>
        /// <param name="index">Index within the element</param>
        /// <param name="roundPosition">Indicates whether the position should be rounded left/right</param>
        /// <returns>The element at the position</returns>
        private UIElement GetElementAtPosition(Point position, out int index, bool roundPosition)
        {
            UIElement result = null;
            Point localPos;
            RichTextPanelRow row;
            List<UIElement> rowElements;
            int indexInRow;
            int i;

            index = 0;
            row = ElementChildren.GetRowAtPosition(position);
            if (row != null)
            {
                result = ElementChildren.GetRowElementAtPosition(position, row);
                if (result == null)
                {
                    result = row.End;
                    index = (result is TextBlockPlus ? ((TextBlockPlus)result).Text.Length : 0);
                }
                else if (result is TextBlockPlus)
                {
                    localPos = new Point(position.X - (double)result.GetValue(Canvas.LeftProperty), position.Y - row.Position.Y);
                    index = GetPositionInString(result as TextBlockPlus, localPos);

                    if (index == 0)
                    {
                        rowElements = row.GetChildren(ElementChildren.ContentChildren);
                        indexInRow = rowElements.IndexOf(result);
                        if (indexInRow > 0)
                        {
                            if (rowElements[indexInRow - 1] is TextBlockPlus)
                            {
                                result = rowElements[indexInRow - 1];
                                index = ((TextBlockPlus)rowElements[indexInRow - 1]).Text.Length;
                            }
                        }
                    }
                }
                else if (!(result is Newline) && !roundPosition)
                {
                    localPos = new Point(position.X - (double)result.GetValue(Canvas.LeftProperty), position.Y - row.Position.Y);
                    double thisWidth = ((FrameworkElement)result).ActualWidth;

                    if (localPos.X > thisWidth * 0.5)
                    {
                        i = ElementChildren.ContentChildren.IndexOf(result);
                        if (i + 1 < ElementChildren.ContentChildren.Count)
                        {
                            result = ElementChildren.ContentChildren[i + 1];
                        }
                    }
                }
            }
            else if (ElementChildren.ContentChildren.Count > 0)
            {
                result = ElementChildren.ContentChildren[ElementChildren.ContentChildren.Count - 1];
                index = (result is TextBlockPlus ? ((TextBlockPlus)result).Text.Length : 0);
            }

            return result;
        }

        /// <summary>
        /// Gets the index within a string of the provided position
        /// </summary>
        /// <param name="template">The textblock to work with</param>
        /// <param name="position">The pixel position</param>
        /// <returns>Index within the string</returns>
        private int GetPositionInString(TextBlockPlus template, Point position)
        {
            double current = 0;
            int result = 0;
            double halfway;
            double charWidth = 0;
            string temp = template.Text;
            char c;
            int i;

            if (position.X <= 0)
            {
                return result;
            }

            for (i = 0; i < temp.Length; i++)
            {
                c = temp[i];

                template.Text = c.ToString();
                charWidth = template.ContentWidth;

                if (position.X >= current && position.X < current + charWidth)
                {
                    halfway = (current + (current + charWidth)) * 0.5;
                    if (position.X >= halfway)
                    {
                        current += charWidth;
                        i++;
                    }

                    result = i;
                    break;
                }

                current += charWidth;
            }

            if (i == temp.Length)
            {
                result = i;
            }

            template.Text = temp;

            return result;
        }

        /// <summary>
        /// Selects the word at the given position
        /// </summary>
        /// <param name="position">A position obejct</param>
        private void SelectWord(RichTextBoxPosition position)
        {
            RichTextBoxPosition newSelectionStart;
            RichTextBoxPosition newSelectionEnd;

            if (position.Element is TextBlockPlus)
            {
                GetWordSelection(position, out newSelectionStart, out newSelectionEnd, 0);
                _cursorPosition = new RichTextBoxPosition(newSelectionEnd);
                _cursorPosition.CalculateGlobalIndex(ElementChildren.ContentChildren);

                SetCursor(_cursorPosition, false);
                SetSelection(newSelectionStart, newSelectionEnd, true);
            }
        }

        /// <summary>
        /// Calculates the start and end positions of a word
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="start">Output start position</param>
        /// <param name="end">Output end position</param>
        /// <returns>Indiocates whether it matched</returns>
        private bool GetWordSelection(RichTextBoxPosition position, out RichTextBoxPosition start, out RichTextBoxPosition end, int indexModifier)
        {
            TextBlockPlus tempText;
            int startIndex = 0;
            int length = 0;

            if (position.Element is TextBlockPlus)
            {
                tempText = (TextBlockPlus)position.Element;
                GetWordAtIndex(tempText.Text, (position.Index > 0 ? position.Index - indexModifier : 0), out startIndex, out length);

                start = new RichTextBoxPosition(position.Element, startIndex);
                start.CalculateGlobalIndex(ElementChildren.ContentChildren);
                end = new RichTextBoxPosition(position.Element, startIndex + length);
                end.CalculateGlobalIndex(ElementChildren.ContentChildren);
            }
            else
            {
                start = new RichTextBoxPosition(position);
                end = new RichTextBoxPosition(position);
            }

            return (position.Element is TextBlockPlus);
        }

        /// <summary>
        /// Applies formatting to a selection of content
        /// </summary>
        /// <param name="formatting">Formatting to apply</param>
        /// <param name="param">A formatting dependant parameter</param>
        /// <param name="updateHistory">Determines whether the undo history is updated</param>
        private void ApplyFormattingToSelection(Formatting formatting, object param, bool updateHistory)
        {
            int startIndex;
            int endIndex;
            RichTextBoxStyle tempStyle;
            RichTextBoxStyle currentStyle = null;
            ContentMetadata currentMetadata = null;
            RichTextPanelRow panel;
            UIElement ignore = null;
            TextBlockPlus currentTextBlock;
            string originalStyleID = string.Empty;
            int i;
            string beforeRichText;
            int beforeSelectionStart;
            int beforeLength;
            int startLineIndex;
            int endLineIndex;
            bool bullets = false;
            bool selection = (ContentSelectionLength > 0);
            bool formatList = IsFormattingListed(formatting);
            bool formatAlignment = (formatting == Formatting.AlignLeft || formatting == Formatting.AlignCenter || formatting == Formatting.AlignRight);

            if (!formatList && !formatAlignment)
            {
                if (!(_selectedStart.Element is TextBlockPlus) && ContentSelectionLength == 1)
                {
                }
                else
                {
                    PrepareSelection();
                }
            }

            if (formatAlignment)
            {
                // Get the first element on the line of the selection start element
                panel = ElementChildren.GetRowForElement(_selectedStart.Element);
                startIndex = ElementChildren.ContentChildren.IndexOf(panel.Start);

                // Get the last element on the line of the selection end element
                panel = ElementChildren.GetRowForElement(_selectedEnd.Element);
                endIndex = ElementChildren.ContentChildren.IndexOf(panel.End);
            }
            else
            {
                startIndex = ElementChildren.ContentChildren.IndexOf(_selectedStart.Element);
                endIndex = ElementChildren.ContentChildren.IndexOf(_selectedEnd.Element);
            }

            if (startIndex == -1 || endIndex == -1)
            {
                return;
            }

            startLineIndex = ElementChildren.GetRowIndexForElement(_selectedStart.Element);
            endLineIndex = ElementChildren.GetRowIndexForElement(_selectedEnd.Element);

            if (formatList)
            {
                ProcessSelectLine(ElementChildren.Rows[endLineIndex], false);
                _selectedStart.Element = ElementChildren.Rows[startLineIndex].Start;
                _selectedStart.Index = 0;
                _selectedStart.CalculateGlobalIndex(ElementChildren.ContentChildren);

                _cursorPosition = new RichTextBoxPosition(_selectedEnd);
                SetCursor(_cursorPosition, false);
                bullets = true;
            }

            beforeSelectionStart = _selectedStart.GlobalIndex;
            beforeLength = _selectedEnd.GlobalIndex - _selectedStart.GlobalIndex;
            beforeRichText = GetRichText(beforeSelectionStart, beforeLength);

            if (bullets)
            {
                ApplyBulletsToSelection(startIndex, endIndex, formatting, param);
            }
            else
            {
                if (_selectedStart.Element is TextBlockPlus)
                {
                    if (ContentSelectionLength > 0 && ((TextBlockPlus)_selectedStart.Element).Text.Length == _selectedStart.Index && startIndex < ElementChildren.ContentChildren.Count - 1)
                    {
                        startIndex++;
                    }
                }

                for (i = startIndex; i <= endIndex; i++)
                {
                    currentMetadata = null;
                    currentStyle = Root.Styles[GetStyle(ElementChildren.ContentChildren[i])];
                    tempStyle = new RichTextBoxStyle(currentStyle);

                    switch (formatting)
                    {
                        case Formatting.Painter:
                            if (Root.Styles.ContainsKey(PainterClipboard))
                            {
                                tempStyle = Root.Styles[PainterClipboard];
                            }
                            break;
                        case Formatting.Bold:
                            tempStyle.Weight = FontWeights.Bold;
                            break;
                        case Formatting.RemoveBold:
                            tempStyle.Weight = FontWeights.Normal;
                            break;
                        case Formatting.Italic:
                            tempStyle.Style = FontStyles.Italic;
                            break;
                        case Formatting.RemoveItalic:
                            tempStyle.Style = FontStyles.Normal;
                            break;
                        case Formatting.Underline:
                            tempStyle.Decorations = TextDecorations.Underline;
                            break;
                        case Formatting.RemoveUnderline:
                            tempStyle.Decorations = null;
                            break;
                        case Formatting.FontFamily:
                            tempStyle.Family = param.ToString();
                            break;
                        case Formatting.FontSize:
                            tempStyle.Size = (double)param;
                            break;
                        case Formatting.Background:
                            tempStyle.Background = ConvertParamToBrush(param);
                            break;
                        case Formatting.Foreground:
                            tempStyle.Foreground = ConvertParamToBrush(param);
                            break;
                        case Formatting.AlignLeft:
                            tempStyle.Alignment = HorizontalAlignment.Left;
                            break;
                        case Formatting.AlignCenter:
                            tempStyle.Alignment = HorizontalAlignment.Center;
                            break;
                        case Formatting.AlignRight:
                            tempStyle.Alignment = HorizontalAlignment.Right;
                            break;
                        case Formatting.Link:
                            if (param is ContentMetadata)
                            {
                                currentMetadata = (ContentMetadata)param;
                            }
                            else
                            {
                                currentMetadata = new ContentMetadata();
                                currentMetadata.Add("URL", param.ToString());
                            }
                            break;
                        case Formatting.Metadata:
                            currentMetadata = (ContentMetadata)param;
                            break;
                        case Formatting.RemoveLink:
                            if (ElementChildren.ContentChildren[i].GetValue(TagProperty) is RichTextTag)
                            {
                                ((RichTextTag)ElementChildren.ContentChildren[i].GetValue(TagProperty)).Metadata = null;
                            }
                            break;
                        case Formatting.Strike:
                            tempStyle.Effect = TextBlockPlusEffect.Strike;
                            break;
                        case Formatting.RemoveStrike:
                            tempStyle.Effect = TextBlockPlusEffect.None;
                            break;
                        case Formatting.SubScript:
                            tempStyle.Special = RichTextSpecialFormatting.Subscript;
                            break;
                        case Formatting.SuperScript:
                            tempStyle.Special = RichTextSpecialFormatting.Superscript;
                            break;
                        case Formatting.RemoveSpecial:
                            tempStyle.Special = RichTextSpecialFormatting.None;
                            break;
                        case Formatting.RemoveBorder:
                            tempStyle.BorderType = BorderEffect.None;
                            break;
                        case Formatting.BorderSolid:
                            tempStyle.BorderType = BorderEffect.Solid;
                            tempStyle.BorderBrush = ConvertParamToBrush(param);
                            break;
                        case Formatting.BorderDotted:
                            tempStyle.BorderType = BorderEffect.Dotted;
                            tempStyle.BorderBrush = ConvertParamToBrush(param);
                            break;
                        case Formatting.BorderDashed:
                            tempStyle.BorderType = BorderEffect.Dashed;
                            tempStyle.BorderBrush = ConvertParamToBrush(param);
                            break;
                        case Formatting.RemoveShadow:
                            tempStyle.Shadow = ShadowEffect.None;
                            break;
                        case Formatting.ShadowSlight:
                            tempStyle.Shadow = ShadowEffect.Slight;
                            tempStyle.ShadowBrush = ConvertParamToBrush(param);
                            break;
                        case Formatting.ShadowNormal:
                            tempStyle.Shadow = ShadowEffect.Normal;
                            tempStyle.ShadowBrush = ConvertParamToBrush(param);
                            break;
                        case Formatting.Margin:
                            tempStyle.Margin = (Thickness)param;
                            break;
                        case Formatting.Style:
                            if (param is string)
                            {
                                if (Root.Styles.ContainsKey(param.ToString()))
                                {
                                    tempStyle = Root.Styles[param.ToString()];
                                }
                            }
                            else
                            {
                                tempStyle = (RichTextBoxStyle)param;
                            }
                            break;
                        case Formatting.AlignTop:
                            tempStyle.VerticalAlignment = VerticalAlignment.Top;
                            break;
                        case Formatting.AlignMiddle:
                            tempStyle.VerticalAlignment = VerticalAlignment.Center;
                            break;
                        case Formatting.AlignBottom:
                            tempStyle.VerticalAlignment = VerticalAlignment.Bottom;
                            break;
                    }

                    if (currentMetadata != null)
                    {
                        ApplyMetadataToElement(ElementChildren.ContentChildren[i], currentMetadata);
                    }

                    originalStyleID = currentStyle.ID;
                    currentStyle = GetStyle(tempStyle);
                    ApplyStyleToElement(ElementChildren.ContentChildren[i], currentStyle);
                    //TryDeleteStyle(originalStyleID);
                    ignore = ElementChildren.ContentChildren[i];
                }

                if (startLineIndex >= 0)
                {
                    for (i = startLineIndex; i <= endLineIndex; i++)
                    {
                        if (i < ElementChildren.Rows.Count)
                        {
                            ElementChildren.Update(ElementChildren.Rows[i], false, ignore);
                        }
                    }
                }
            }

            if (updateHistory)
            {
                Root.History.Add(this, HistoryCommand.FormatChange,
                    new RichTextState(beforeRichText, beforeSelectionStart, beforeLength),
                    new RichTextState(GetSelectedRichText(), beforeSelectionStart, ContentSelectionLength));
            }

            if (!selection)
            {
                UpdateCursor();
                ProcessClearSelection(true);
                SetCursor(_cursorPosition, false);
            }
            else
            {
                _selectedStart.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
                _selectedEnd.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
            }
            
            CalculateSelection();
            RefreshAllHilights();
            RefreshObjectMarker();
        }

        private void ApplyCellFormatting(Formatting format, object param, bool updateHistory)
        {
            List<CellReference> cells = Root.ActiveTable.Selected;
            RichTextBlock rtb;

            foreach (CellReference r in cells)
            {
                if (r.Element is RichTextBlock)
                {
                    rtb = (RichTextBlock)r.Element;
                    rtb.SelectAll();
                    rtb.ApplyFormattingToSelection(format, param, updateHistory);
                    rtb.ClearSelection();
                }
            }
        }

        private void AlignTableCells(Formatting format)
        {
            List<CellReference> cells = Root.ActiveTable.Selected;

            foreach (CellReference r in cells)
            {
                switch (format)
                {
                    case Formatting.AlignTop:
                        r.Element.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Top);
                        break;
                    case Formatting.AlignMiddle:
                        r.Element.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                        break;
                    case Formatting.AlignBottom:
                        r.Element.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Bottom);
                        break;
                }
            }
        }

        private bool IsFormattingListed(Formatting formatting)
        {
            return (formatting == Formatting.BulletList || formatting == Formatting.BulletImageList || formatting == Formatting.NumberList
                || formatting == Formatting.RemoveBullet || formatting == Formatting.RemoveBullerImage || formatting == Formatting.RemoveNumber
                || formatting == Formatting.Indent || formatting == Formatting.Outdent);
        }

        /// <summary>
        /// Takes a brush object or a string color code and returns a Brush object
        /// </summary>
        /// <param name="param">Brush object or color hex code</param>
        /// <returns>Brush object</returns>
        private Brush ConvertParamToBrush(object param)
        {
            if (param == null)
            {
                return null;
            }

            if (param is Brush)
            {
                return (Brush)param;
            }
            else
            {
                return new SolidColorBrush(RichTextBoxStyle.StringToColor(param.ToString()));
            }
        }

        #region Bullet/Indent Handling

        /// <summary>
        /// Gets the list type the element is located in
        /// </summary>
        /// <param name="element">Content element</param>
        /// <returns>Bullet for the first preceeding list item</returns>
        private Bullet GetListType(UIElement element)
        {
            return GetListType(ElementChildren.ContentChildren.IndexOf(element));
        }

        /// <summary>
        /// Gets the list type the element index is located in
        /// </summary>
        /// <param name="elementIndex">Content element index</param>
        /// <returns>Bullet for the first preceeding list item</returns>
        private Bullet GetListType(int elementIndex)
        {
            Bullet type = null;

            for (int i = elementIndex; i >= 0; i--)
            {
                if (ElementChildren.ContentChildren[i] is Bullet)
                {
                    type = (Bullet)ElementChildren.ContentChildren[i];
                    break;
                }
                else if (ElementChildren.ContentChildren[i] is Newline)
                {
                    break;
                }
            }

            return type;
        }

        /// <summary>
        /// Gets the first line in a bulleted/Numbered/Indented list
        /// </summary>
        /// <param name="elementIndex">Index of the element to start looking from</param>
        /// <returns>The index of the first element in the list</returns>
        private int GetFirstLineInList(int elementIndex)
        {
            int tempIndex = elementIndex;
            int indexOfLastBullet = -1;

            // Look backwards for the start of the bulleted list
            for (int i = elementIndex; i >= 0; i--)
            {
                if (ElementChildren.ContentChildren[i] is Bullet && i == 0)
                {
                    indexOfLastBullet = i;
                }
                else if (ElementChildren.ContentChildren[i] is Newline && i < ElementChildren.ContentChildren.Count - 1)
                {
                    if (ElementChildren.ContentChildren[i + 1] is Bullet)
                    {
                        indexOfLastBullet = i + 1;
                    }
                    else
                    {
                        tempIndex = (indexOfLastBullet >= 0 ? indexOfLastBullet : i + 1);
                        break;
                    }
                }
            }

            if (indexOfLastBullet >= 0)
            {
                tempIndex = indexOfLastBullet;
            }

            return ElementChildren.GetRowIndexForElement(ElementChildren.ContentChildren[tempIndex]);
        }

        /// <summary>
        /// Gets the last line in a bulleted/Numbered/Indented list
        /// </summary>
        /// <param name="elementIndex">Index of the element to start looking from</param>
        /// <returns>The index of the last element in the list</returns>
        private int GetLastLineInList(int elementIndex)
        {
            int tempIndex = elementIndex;
            int i = 0;

            // Look forwards for the end of the bulleted list
            for (i = elementIndex; i < ElementChildren.ContentChildren.Count - 1; i++)
            {
                if (ElementChildren.ContentChildren[i] is Newline)
                {
                    tempIndex = i;
                    if (!(ElementChildren.ContentChildren[i + 1] is Bullet))
                    {
                        break;
                    }
                }
            }

            if (i == ElementChildren.ContentChildren.Count - 1)
            {
                tempIndex = i;
            }

            return ElementChildren.GetRowIndexForElement(ElementChildren.ContentChildren[tempIndex]);
        }

        /// <summary>
        /// Applies bulleting/numbering/indenting to a selection of content
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="endIndex">End index</param>
        /// <param name="formatting">Formatting to apply</param>
        /// <param name="param">A formatting dependant parameter</param>
        private void ApplyBulletsToSelection(int startIndex, int endIndex, Formatting formatting, object param)
        {
            int startLineIndex;
            int endLineIndex;
            RichTextPanelRow current;
            List<UIElement> currentChildren;
            Bullet newBullet;
            int indent = 0;
            BulletType? currentType = null;
            int indexInElements;
            bool insertBullet = true;
            double leftMargin = 0;
            int listNumber = 1;
            int lastLineIndex;
            int numberInserted = 0;
            string currentStyle;
            int i;

            if (formatting == Formatting.Indent || formatting == Formatting.Outdent)
            {
                startLineIndex = ElementChildren.GetRowIndexForElement(ElementChildren.ContentChildren[startIndex]);
                endLineIndex = ElementChildren.GetRowIndexForElement(ElementChildren.ContentChildren[endIndex]);
            }
            else
            {
                // Look backwards for the start of the bulleted list
                startLineIndex = GetFirstLineInList(startIndex);

                // Look forwards for the end of the bulleted list
                endLineIndex = GetLastLineInList(endIndex);
            }

            lastLineIndex = ElementChildren.ContentChildren.Count - endLineIndex;

            for (i = startLineIndex; i <= endLineIndex; i++)
            {
                indent = 0;
                current = ElementChildren.Rows[i];
                currentChildren = current.GetChildren(ElementChildren.ContentChildren);

                if (currentChildren.Count > 0)
                {
                    indexInElements = ElementChildren.ContentChildren.IndexOf(currentChildren[0]);
                    if (currentChildren[0] is Bullet)
                    {
                        indent = ((Bullet)currentChildren[0]).Indent;
                        currentType = ((Bullet)currentChildren[0]).Type;
                        if (formatting == Formatting.Indent || formatting == Formatting.Outdent)
                        {
                            listNumber = ((Bullet)currentChildren[0]).Number;
                        }

                        ElementChildren.Remove(currentChildren[0]);
                        current.Start = currentChildren[1];
                        numberInserted--;
                        _length--;
                    }
                }
                else
                {
                    indexInElements = -1;
                }

                if (formatting == Formatting.RemoveBullet || formatting == Formatting.RemoveNumber)
                {
                    //ElementChildren.Update(current, false, null);
                    //endLineIndex = ElementChildren.ContentChildren.Count - lastLineIndex;
                    continue;
                }

                if (insertBullet)
                {
                    newBullet = new Bullet();
                    newBullet.Height = current.Dimensions.Y - (current.Margin.Top + current.Margin.Bottom);
                    newBullet.Number = listNumber;

                    switch (formatting)
                    {
                        case Formatting.BulletList:
                            newBullet.Type = BulletType.Bullet;
                            if (indent == 0)
                            {
                                indent = 1;
                            }
                            break;
                        case Formatting.BulletImageList:
                            newBullet.Type = BulletType.Image;

                            if (param is Uri)
                            {
                                newBullet.BulletImage = (Uri)param;
                            }
                            else
                            {
                                newBullet.BulletImage = new Uri(param.ToString());
                            }

                            if (indent == 0)
                            {
                                indent = 1;
                            }
                            break;
                        case Formatting.NumberList:
                            newBullet.Type = BulletType.Number;
                            if (indent == 0)
                            {
                                indent = 1;
                            }
                            break;
                        case Formatting.Indent:
                            newBullet.Type = (currentType != null ? (BulletType)currentType : BulletType.Indent);
                            indent++;
                            break;
                        case Formatting.Outdent:
                            newBullet.Type = (currentType != null ? (BulletType)currentType : BulletType.Indent);
                            indent--;
                            if (indent < 0)
                            {
                                indent = 0;
                            }
                            break;
                    }

                    newBullet.Indent = indent;
                    leftMargin = newBullet.IndentWidth;

                    if (formatting == Formatting.Outdent && indent == 0)
                    {
                    }
                    else
                    {
                        currentStyle = GetStyle(ElementChildren.ContentChildren[indexInElements]);
                        ApplyStyleToElement(newBullet, currentStyle);
                        ElementChildren.Insert(indexInElements, newBullet);
                        current.Start = newBullet;
                        numberInserted++;
                        _length++;
                    }
                    listNumber++;
                    insertBullet = false;
                }

                if (currentChildren[currentChildren.Count - 1] is Newline)
                {
                    insertBullet = true;
                }
            }

            for (i = startLineIndex; i <= endLineIndex; i++)
            {
                current = ElementChildren.Rows[i];
                // Update the line to ensure the bullet hasn't pushed content off the edge
                ElementChildren.Update(current, false, null);
            }

            _selectedEnd.GlobalIndex += numberInserted;
            _selectedEnd.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
            _cursorPosition = new RichTextBoxPosition(_selectedEnd);
        }

        #endregion

        /// <summary>
        /// Gets the word in a string at a given index
        /// </summary>
        /// <param name="text">Text string</param>
        /// <param name="index">Index within the string</param>
        /// <param name="startIndex">Resulting word start index</param>
        /// <param name="length">Resulting word length</param>
        private void GetWordAtIndex(string text, int index, out int startIndex, out int length)
        {
            int s;
            int e;

            startIndex = index;
            length = 0;

            if (text.Length > 0)
            {
                if (index >= text.Length)
                {
                    index--;
                }

                if (index < text.Length)
                {
                    for (s = index; s >= 0; s--)
                    {
                        if (!Char.IsLetterOrDigit(text[s]))
                        {
                            break;
                        }
                    }

                    for (e = index; e < text.Length; e++)
                    {
                        if (!Char.IsLetterOrDigit(text[e]))
                        {
                            break;
                        }
                    }
                    startIndex = s + 1;
                    length = e - startIndex;
                }
            }

            if (length < 0)
            {
                length = -length;
                startIndex -= length;
            }
        }

        /// <summary>
        /// Determines whether the CTRL button is pressed
        /// </summary>
        /// <returns>True if it is pressed</returns>
        private bool IsCtrlDown()
        {
            return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.None;
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
        /// Determines whether the character before the provided position is numeric
        /// </summary>
        /// <param name="globalIndex">Position to look at</param>
        /// <returns>True if it is numeric, false if not</returns>
        private bool IsCharNumeric(int globalIndex)
        {
            bool result = false;
            TextBlockPlus textblock;
            RichTextBoxPosition position = new RichTextBoxPosition(globalIndex);

            position.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);

            if (position.Element is TextBlockPlus)
            {
                textblock = (TextBlockPlus)position.Element;
                if (position.Index < textblock.Text.Length)
                {
                    result = Regex.Match(textblock.Text.Substring(position.Index, 1), @"[0-9]").Success;
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates the difference in lines between the provided position and the position + offset
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="offset">Offset</param>
        /// <returns>Number of lines difference, typically -1, 0 and 1</returns>
        private int GetLineIndexOffset(RichTextBoxPosition position, int offset)
        {
            int result = 0;
            RichTextBoxPosition temp = new RichTextBoxPosition(position.GlobalIndex + offset);

            if (offset < 0 && position.GlobalIndex == 0)
            {
                return offset;
            }
            else if (offset > 0 && position.GlobalIndex == _length - 1)
            {
                return offset;
            }

            temp.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
            result = ElementChildren.GetRowIndexForElement(temp.Element) - ElementChildren.GetRowIndexForElement(position.Element);

            return result;
        }

        /// <summary>
        /// Gets the position of the first numeric character before the provided position
        /// </summary>
        /// <param name="position">Position to look before</param>
        /// <returns>The position of the first numeric character</returns>
        private RichTextBoxPosition GetStartEndOfNumerics(RichTextBoxPosition position, int direction)
        {
            RichTextBoxPosition result = new RichTextBoxPosition(position);

            do
            {
                result.GlobalIndex += direction;
            }
            while (IsCharNumeric(result.GlobalIndex));

            result.GlobalIndex -= direction;
            result.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);

            return result;
        }

        /// <summary>
        /// Is called when an IME update may be needed
        /// </summary>
        /// <param name="resetFlag">Determines whether the _usingIME flag should be reset</param>
        /// <returns>Value of the _usingIME flag</returns>
        private bool UpdateFromIME(bool resetFlag)
        {
            if (_usingIME)
            {
                SetSelection(_cursorPosition.GlobalIndex, 0, true);

                HideGlobalInputArea();
                if (resetFlag)
                {
                    _usingIME = false;
                }
            }

            return _usingIME;
        }

        /// <summary>
        /// Sets the RichText content from the provided plain text
        /// </summary>
        /// <param name="plainText">Some plain text</param>
        private void SetFromPlainText(string plainText)
        {
            string tempRichText = string.Empty;
            string[] split;
            int i;

            plainText += "\r";

            split = plainText.Split('\r');
            tempRichText = GetXMLOpen();

            for (i = 0; i < split.Length; i++)
            {
                if (split[i].Length > 0)
                {
                    tempRichText += "<Text Style=\"" + DefaultStyle + "\"><![CDATA[" + split[i].Replace("\n", "") + "]]></Text>";
                }
                if (i < split.Length - 1)
                {
                    tempRichText += "<Newline />";
                }
            }

            if (!tempRichText.EndsWith("<Newline />"))
            {
                tempRichText += "<Newline />";
            }
            tempRichText += RichTextBox.XMLClose;

            RichText = tempRichText;
        }

        protected void SetupFlowPanelEvents()
        {
            ElementChildren.TextBlockSplit += new RichTextPanelEventHandler(ElementChildren_TextBlockSplit);
            ElementChildren.TextBlockMerge += new RichTextPanelEventHandler(ElementChildren_TextBlockMerge);
            ElementChildren.ApplyFormatting += new RichTextPanelEventHandler(ElementChildren_TextBlockSplit);
        }

        protected void SetInitialContent()
        {
            if (GetBindingExpression(RichTextProperty) != null) { return; }

            string content = (string)this.GetValue(RichTextProperty);
            string html = (string)this.GetValue(HTMLProperty);

            if (DirectRichText is TextBlock)
            {
                content = ((TextBlock)DirectRichText).Text;
            }
            else if (Text.Length > 0)
            {
                content = Text;
                Text = string.Empty;
            }

            if (content.Length == 0 && html.Length > 0)
            {
                if ((string)GetValue(HTMLProperty) != html)
                {
                    HTML = html;
                }
                else
                {
                    SetHTMLContent(html);
                }
            }
            else
            {
                if ((string)GetValue(RichTextProperty) != content)
                {
                    RichText = content;
                }
                else
                {
                    SetRichTextContent(content);
                }
            }
        }

        internal void SetRichTextContent(string richText)
        {
            if (richText.Length > 0)
            {
                Load(Format.XML, richText);
                try
                {
                    
                }
                catch
                {
                    RichTextBoxEventArgs args = new RichTextBoxEventArgs();

                    if (LoadError != null)
                    {
                        RaiseRichTextEvent(LoadError, this, args, false);
                    }

                    if (!args.Cancel)
                    {
                        throw new Exception("Unable to convert to Rich Text");
                    }
                }
            }
            else
            {
                ProcessClear();
            }
        }

        internal void SetHTMLContent(string content)
        {
            if (content.Length > 0)
            {
                Load(Format.HTML, content);
                try
                {
                    //Load(Format.HTML, content);
                }
                catch (Exception ex)
                {
                    RichTextBoxEventArgs args = new RichTextBoxEventArgs();

                    if (LoadError != null)
                    {
                        RaiseRichTextEvent(LoadError, this, args, false);
                    }

                    if (!args.Cancel)
                    {
                        throw new Exception("Unable to convert to convert from HTML - " + ex.Message);
                    }
                }
            }
            else
            {
                ProcessClear();
            }
        }

        protected virtual void OnCursorMoved()
        {
            // Implement functionality when the content cursor has moved
        }

        protected void SizeUpdated(Size PreviousSize, Size NewSize)
        {
            if (NewSize.Width != PreviousSize.Width || NewSize.Height != PreviousSize.Height)
            {
                UpdateVisualState();

                UpdateCursor();
                SetCursor(_cursorPosition, false);
                SetSelection(_selectedStart, _selectedEnd, false);
            }

            if (NewSize.Width != PreviousSize.Width)
            {
                if (ElementChildren != null)
                {
                    DeleteHilights();
                    RunSpellChecker(0, -1);
                }
            }
        }

        /// <summary>
        /// Inserts the provided content at the cursor position
        /// </summary>
        /// <param name="content">Content, either RichText or UIElement</param>
        private void InsertUnknownContent(object content)
        {
            string newText = content.ToString();

            if (content is UIElement)
            {   // Insert a UIElement
                FocusedRTB.ProcessInsert(FocusedRTB.SetupElement((UIElement)content), true, true);
            }
            else
            {
                if (newText.StartsWith("<LiquidRichText"))
                {   // Insert a block of RichText
                    FocusedRTB.ProcessInsert(newText, null);
                }
                else
                {   // Insert plain text
                    FocusedRTB.ProcessInsertText(newText);
                }
            }
        }

        /// <summary>
        /// Hides the spelling suggestions popup
        /// </summary>
        public void HideSuggestionsPopup()
        {
            RaiseRichTextEvent(Root.IncorrectWordFinished, this, new RichTextBoxEventArgs(), false);
            Root.ElementBubblePopup.IsOpen = false;
        }

        /// <summary>
        /// Updates the visual state of the control
        /// </summary>
        protected virtual void UpdateVisualState()
        {
            if (ElementChildren != null)
            {
                ElementChildren.Opacity = (IsEnabled ? 1.0 : DisabledOpacity);
                if (_setWidth)
                {
                    ElementChildren.Width = Width;
                }

                if (SelectMode == SelectMode.ReadOnly)
                {
                    HideCursor();
                    ElementChildren.Cursor = Cursors.Arrow;
                }
                else
                {
                    ElementChildren.Cursor = Cursors.IBeam;
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

            ElementCursor = (Rectangle)GetTemplateChild(ElementCursorName);
            ElementCursorBlink = (Storyboard)GetTemplateChild(ElementCursorBlinkName);
            ElementChildren = (RichTextPanel)GetTemplateChild(ElementChildrenName);
            ElementSelection = (Polygon)GetTemplateChild(ElementSelectionName);
            ElementBubblePopup = (Popup)GetTemplateChild(ElementBubblePopupName);
            ElementBubble = (Canvas)GetTemplateChild(ElementBubbleName);
            ElementContext = (ContentControl)GetTemplateChild(ElementContextName);
            ElementObjectSelection = (Canvas)GetTemplateChild(ElementObjectSelectionName);
            ContentElement = (ContentControl)GetTemplateChild(ContentElementName);
            ContentElementBackGround = (Rectangle)GetTemplateChild(ContentElementBackGroundName);

            SetupFlowPanelEvents();
            ElementChildren.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            ElementChildren.MouseLeftButtonUp += new MouseButtonEventHandler(OnMouseLeftButtonUp);
            ElementChildren.MouseMove += new MouseEventHandler(OnMouseMove);
            ElementChildren.SizeChanged += new SizeChangedEventHandler(ElementChildren_SizeChanged);
            ContentElement.SizeChanged += new SizeChangedEventHandler(ContentElement_SizeChanged);

            this.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);

            ((Button)ElementBubble.FindName("ElementAdd")).Click += new RoutedEventHandler(RichTextBox_Add_Click);
            ((Button)ElementBubble.FindName("ElementReplace")).Click += new RoutedEventHandler(RichTextBox_Replace_Click);

            UpdateVisualState();
            SetInitialContent();

            EnableContextMenu = _enableContextMenu;
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
        }

        private void OnRightMouseClick(object sender, HtmlEventArgs e)
        {
            RichTextBoxEventArgs args = new RichTextBoxEventArgs();
            int positionIndex = 0;
            Point elementPosition;
            Point position = new Point();
            Size size;

            elementPosition = ElementChildren.TransformToVisual(null).Transform(new Point());
            size = ElementChildren.RenderSize;

            if (e.OffsetX >= elementPosition.X && e.OffsetX < elementPosition.X + size.Width &&
                e.OffsetY >= elementPosition.Y && e.OffsetY < elementPosition.Y + size.Height)
            {
                position = new Point(e.OffsetX - elementPosition.X, e.OffsetY - elementPosition.Y);
                e.PreventDefault();
            }

            args.Source = GetElementAtPosition(position, out positionIndex, true);
            args.Parameter = position;

            RaiseRichTextEvent(Root.ShowContextMenu, this, args, false);
            if (ContextMenu != null && !args.Cancel)
            {
                position.X /= _zoom;
                position.Y /= _zoom;

                if (position.X + ElementContext.RenderSize.Width > ElementChildren.Width)
                {
                    position.X = ElementChildren.Width - ElementContext.RenderSize.Width;
                }

                ElementContext.SetValue(Canvas.LeftProperty, position.X);
                ElementContext.SetValue(Canvas.TopProperty, position.Y);
                ContextMenu.Visibility = Visibility.Visible;
                ContextMenu.Focus();
            }
        }

        private void AutoComplete_PatternMatch(object sender, AutoCompleteEventArgs e)
        {
            RichTextBlock root = Root;
            RichTextBoxEventArgs args;
            RichTextBoxPosition pos;

            if (EnablePatternRecognition)
            {
                args = new RichTextBoxEventArgs();
                args.Parameter = e.Text;
                RaiseRichTextEvent(root.TextPatternMatch, this, args, false);

                if (!args.Cancel && args.Parameter.ToString() != e.Text)
                {
                    pos = new RichTextBoxPosition(_cursorPosition);
                    FocusedRTB.ProcessSelect(root.AutoComplete.Index, root.AutoComplete.Text.Length);
                    InsertUnknownContent(args.Parameter);
                }
            }
        }

        private void AutoComplete_Hyperlink(object sender, AutoCompleteEventArgs e)
        {
            FocusedRTB.ProcessLinkEntered();
        }

        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SizeUpdated(e.PreviousSize, e.NewSize);
        }

        protected virtual void ElementChildren_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Height = e.NewSize.Height;
        }

        /// <summary>
        /// Processes a key down event
        /// </summary>
        /// <param name="e">KeyEventArgs parameter</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (SelectMode == SelectMode.ReadOnly || !IsEnabled)
            {
                return;
            }
            
            if (GlobalKeyDown != null)
            {
                GlobalKeyDown(this, e);
                if (e.Handled)
                {
                    return;
                }
            }

            if (!_usingIME)
            {
                switch (e.Key)
                {
                    case Key.A:
                        if (IsCtrlDown())
                        {
                            SelectAll();
                        }
                        break;
                    case Key.C:
                        if (IsCtrlDown())
                        {
                            Copy();
                        }
                        break;
                    case Key.X:
                        if (IsCtrlDown())
                        {
                            Cut();
                        }
                        break;
                    case Key.Z:
                        if (IsCtrlDown())
                        {
                            Undo();
                            e.Handled = true;
                        }
                        break;
                    case Key.Y:
                        if (IsCtrlDown())
                        {
                            Redo();
                            e.Handled = true;
                        }
                        break;
                    case Key.Home:
                        Home();
                        e.Handled = true;
                        break;
                    case Key.End:
                        End();
                        e.Handled = true;
                        break;
                    case Key.Up:
                        CursorUp();
                        e.Handled = true;
                        break;
                    case Key.Down:
                        CursorDown();
                        e.Handled = true;
                        break;
                    case Key.Left:
                        CursorLeft();
                        e.Handled = true;
                        break;
                    case Key.Right:
                        CursorRight();
                        e.Handled = true;
                        break;
                    case Key.Shift:
                        _dragBegin = new RichTextBoxPosition(_cursorPosition);
                        break;
                    case Key.Back:
                        Delete(false);
                        e.Handled = true;
                        break;
                    case Key.Delete:
                        Delete(true);
                        e.Handled = true;
                        break;
                    case Key.Enter:
                        if (AcceptsReturn)
                        {
                            UpdateFromIME(false);
                            if (!_usingIME) InsertNewline();
                            e.Handled = true;
                        }
                        break;
                    case Key.Unknown:
                        ShowGlobalInputArea(e.PlatformKeyCode);
                        break;
                    default:
                        e.Handled = false;
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        UpdateFromIME(false);
                        if (!_usingIME) InsertNewline();
                        e.Handled = true;
                        break;
                    default:
                        e.Handled = false;
                        break;
                }
            }

            base.OnKeyDown(e);
        }

        private string globalOldText = string.Empty;
        /// <summary>
        /// after input chinese words, when press Enter or Space key, we will put these word on the RichTextBlock.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (_usingIME && (e.Key == Key.Enter || e.Key == Key.Space && globalOldText == Text))
            {
                _usingIME = false;
                HideGlobalInputArea();
                OnTextChanged(this, null);
            }
            base.OnKeyUp(e);
            globalOldText = Text;
        }

        /// <summary>
        /// Show the global input area(espically chinese input textbox)
        /// </summary>
        /// <param name="keycode">KeyEventArgs.PlatformKeyCode</param>
        private void ShowGlobalInputArea(int keycode)
        {
            //IME type is simplied chinese
            if (keycode == 229)
            {
                RichTextBlock root = Root;

                if (root.SelectionStyle != null)
                {
                    _origFontFamily = this.FontFamily;
                    _origFontSize = this.FontSize;
                    _origFontWeight = this.FontWeight;
                    _origFontStyle = this.FontStyle;
                    _origForeground = this.Foreground;
                    this.FontFamily = new FontFamily(root.SelectionStyle.Family);
                    if (root.SelectionStyle.Size.HasValue)
                    {
                        this.FontSize = root.SelectionStyle.Size.Value;
                    }
                    this.Foreground = root.SelectionStyle.Foreground;
                    if (root.SelectionStyle.Weight.HasValue)
                    {
                        this.FontWeight = root.SelectionStyle.Weight.Value;
                    }
                    if (root.SelectionStyle.Style.HasValue)
                    {
                        this.FontStyle = root.SelectionStyle.Style.Value;
                    }
                    _usingTempStyle = true;
                }

                _usingIME = true;
                ContentElement.Opacity = 1;
                ContentElementBackGround.Opacity = 1;
                RichTextPanelRow row;
                double top;
                double height;
                double left;
                RichTextBoxPosition position = _cursorPosition;
                if (position.Element != null && ElementCursor != null)
                {
                    row = ElementChildren.GetRowForElement(position.Element);

                    if (row != null)
                    {
                        if (position.Element is TextBlockPlus)
                        {
                            height = ((TextBlockPlus)position.Element).ContentHeight;
                            top = (double)position.Element.GetValue(Canvas.TopProperty) + ((Thickness)position.Element.GetValue(Canvas.MarginProperty)).Top;
                        }
                        else
                        {
                            height = row.Dimensions.Y;
                            top = row.Position.Y;
                        }
                        double textboxMinSize = this.FontSize * 4;
                        left = Math.Round(position.Position.X, 0);
                        if (ElementChildren.ActualWidth - left < textboxMinSize) left -= textboxMinSize;
                        if (left < 0) left = 0;
                        ContentElement.SetValue(Canvas.TopProperty, Math.Round(top, 0));
                        ContentElement.SetValue(Canvas.LeftProperty, left);
                        ContentElementBackGround.SetValue(Canvas.TopProperty, Math.Round(top, 0));
                        ContentElementBackGround.SetValue(Canvas.LeftProperty, left);
                        ContentElement.Height = height;
                        ContentElement.Width = Math.Max(textboxMinSize, ElementChildren.ActualWidth - left);
                        HideCursor();
                    }
                }
            }
            else
            {
                _usingIME = false;
                ContentElement.Opacity = 0;
                ContentElementBackGround.Opacity = 0;
                ShowCursor();
            }
        }
        /// <summary>
        /// Hide the global input area(espically chinese input textbox)
        /// </summary>
        private void HideGlobalInputArea()
        {
            if (_usingTempStyle)
            {
                this.FontFamily = _origFontFamily;
                this.FontSize = _origFontSize;
                this.FontWeight = _origFontWeight;
                this.FontStyle = _origFontStyle;
                this.Foreground = _origForeground;
                _usingTempStyle = false;
            }

            ContentElement.Opacity = 0;
            ContentElementBackGround.Opacity = 0;
            ShowCursor();
        }
        /// <summary>
        /// change the contentElementBackground size after contentElement size changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ContentElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ContentElementBackGround.Height = e.NewSize.Height;
            ContentElementBackGround.Width = e.NewSize.Width;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (e.OriginalSource is RichTextBlock)
            {
                Root.FocusedRTB = (RichTextBlock)e.OriginalSource;
            }

            if (SelectMode != SelectMode.ReadOnly)
            {
                Root.FocusedRTB.ShowCursor();
            }

            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            object current = FocusManager.GetFocusedElement();
            
            if (current is RichTextBlock && current != this)
            {
                Root.FocusedRTB.ProcessClearSelection(false);
                Root.FocusedRTB.HideCursor();
            }
            else if (this == Root)
            {
                System.Diagnostics.Debug.WriteLine("Lost focus");
                Root.FocusedRTB.HideCursor();
                if (this.PropertyChanged != null)
                {
                    System.Diagnostics.Debug.WriteLine("1");
                    System.Diagnostics.Debug.WriteLine(HTML);
                    this.PropertyChanged(this, new PropertyChangedEventArgs("HTML"));
                    this.PropertyChanged(this, new PropertyChangedEventArgs("RichText"));
                }
            }

            if (_usingIME)
            {
                _usingIME = false;
                HideGlobalInputArea();
                OnTextChanged(this, null);
            }

            base.OnLostFocus(e);
        }

        /// <summary>
        /// This event is called when the base textbox has its text changed
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectMode == SelectMode.Edit)
            {
                if (Text.Length > 0 && Text != ClipboardWatch)
                {
                    if (base.SelectionLength == 0)
                    {
                        if (_usingIME)
                        {
                            if (_lastIndex < Text.Length && ContentSelectionLength == 1)
                            {
                                ProcessInsertText(Text.Substring(_lastIndex, 1));
                                _usingIME = false;
                            }
                            //If the first charactor is digitals or other special charactor, we just put the charactor in richtextblock directly
                            else if (Regex.Match(Text, "^[0-9\\-=\\[\\]/~£¡¡¤¡¢£»£¬¡££¿¡¡¡¢#%^&*()_+{}|]+$").Success || Text == "\\")
                            {
                                ProcessInsertText(Text);
                                HideGlobalInputArea();
                                Text = string.Empty;
                                _usingIME = false;
                            }
                        }
                        else
                        {
                            if (!Regex.Match(Text, @"[0-9]").Success && IsRightToLeft)
                            {
                                if (IsCharNumeric(_cursorPosition.GlobalIndex - 1))
                                {
                                    _cursorPosition = GetStartEndOfNumerics(_cursorPosition, -1);
                                    SetCursor(_cursorPosition, false);
                                    ProcessClearSelection(true);
                                }

                                // Right-to-left
                                int oldIndex = _cursorPosition.GlobalIndex;
                                ProcessInsertText(Text);
                                _cursorPosition.GlobalIndex = oldIndex;
                                _cursorPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
                                RoundPositionUp(_cursorPosition);
                                SetCursor(_cursorPosition, false);
                            }
                            else
                            {
                                // Left-to-right
                                ProcessInsertText(Text);
                            }
                        }

                        if (!_usingIME) Text = string.Empty;
                        _lastIndex = 0;
                    }
                }
                else if(Text.Length > 0)
                {
                    if (FocusedRTB == this)
                    {
                        ProcessPaste();
                    }
                    Text = string.Empty;
                }
            }
            else
            {
                Text = string.Empty;
            }
        }

        private void RichTextBox_Replace_Click(object sender, RoutedEventArgs e)
        {
            ComboBox list = (ComboBox)ElementBubble.FindName("ElementSuggestions");
            string newWord = string.Empty;

            if (list.SelectedIndex >= 0)
            {
                newWord = ((ComboBoxItem)list.SelectedItem).Content.ToString();
            }

            FocusedRTB.ProcessReplaceWord(newWord);
        }

        private void RichTextBox_Add_Click(object sender, RoutedEventArgs e)
        {
            FocusedRTB.ProcessAddWord();
        }

        private void ElementChildren_TextBlockMerge(object sender, RichTextPanelEventArgs e)
        {
            if (e.Source is TextBlockPlus && e.Created is TextBlockPlus)
            {
                MergeHilights((TextBlockPlus)e.Source, (TextBlockPlus)e.Created, e.Index);
            }
        }

        private void ElementChildren_TextBlockSplit(object sender, RichTextPanelEventArgs e)
        {
            TextBlockPlus source;
            TextBlockPlus created;
            RichTextTag tag;

            if (e.Source is TextBlockPlus && e.Created is TextBlockPlus)
            {
                source = (TextBlockPlus)e.Source;
                created = (TextBlockPlus)e.Created;

                if (e.Created != e.Source)
                {
                    SplitHilights(source, created);
                }
            }

            if (e.Source != null)
            {
                if (e.Source.GetValue(TagProperty) is RichTextTag)
                {
                    tag = (RichTextTag)e.Source.GetValue(TagProperty);

                    if (tag.Metadata != null)
                    {
                        ApplyMetadataToElement(e.Created, tag.Metadata);
                    }

                    ApplyStyleToElement(e.Created, tag.StyleID);
                }
            }
        }

        /// <summary>
        /// This event is called when the mouse button is clicked for the richtextbox
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RaiseMouseLeftButtonDown(this, e);
            SimulateLeftClick(e.GetPosition(ElementChildren));
        }

        /// <summary>
        /// This event is called when the mouse button is released for the richtextbox
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RichTextBoxEventArgs args;
            Point position;
            int positionIndex;

            RaiseMouseLeftButtonUp(this, e);

            if (SelectMode == SelectMode.ReadOnly || !IsEnabled)
            {
                return;
            }

            if (ExternalDragStart)
            {
                position = e.GetPosition(ElementChildren);
                args = new RichTextBoxEventArgs();
                args.Source = GetElementAtPosition(position, out positionIndex, true);
                args.Parameter = position;

                RaiseRichTextEvent(ContentDropped, this, args, false);
                if (!args.Cancel)
                {
                    SimulateLeftClick(position);
                    _mouseDown = false;
                    InsertUnknownContent(args.Parameter);
                }
                ExternalDragStart = false;
            }
            else
            {
                _mouseDown = false;
                UpdateTimerEnabledStatus();
                ElementChildren.ReleaseMouseCapture();

                // If the format painter command has been used then apply the format
                if (_painterDown)
                {
                    if (ContentSelectionLength == 0)
                    {
                        // Select the word at the cursor
                        if (_cursorPosition.Element is TextBlockPlus)
                        {
                            SelectWord(_cursorPosition);
                            CalculateSelection();
                        }
                    }

                    ApplyFormatting(Formatting.Painter, null);
                    _painterDown = false;
                }
                SelectionHasChanged();
            }
        }

        /// <summary>
        /// This event is called when the mouse moves over the content area
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnMouseMove(object sender, MouseEventArgs e)
        {
            RichTextBoxPosition temp = null;
            string currentStyle = string.Empty;
            ContentMetadata metadata = null;
            bool overLink = false;
            Point p;

            if (!IsEnabled)
            {
                return;
            }

            LastMousePosition = e.GetPosition(ElementChildren);

            p = new Point(LastMousePosition.X, LastMousePosition.Y);

            if (p.X < 0)
            {
                p.X = 0;
            }
            else if (p.X > ElementChildren.Width)
            {
                p.X = ElementChildren.Width;
            }

            if (p.Y < 0)
            {
                p.Y = 0;
            }
            else if (p.Y > ElementChildren.Height)
            {
                p.Y = ElementChildren.Height;
            }

            if (ExternalDragStart)
            {
                _cursorPosition = GetPosition(p, false);
                SetCursor(_cursorPosition, false);
                ElementCursor.Visibility = Visibility.Visible;
                ElementCursorBlink.Begin();
            }
            else
            {
                if (SelectMode == SelectMode.ReadOnly)
                {
                    temp = GetPosition(p, false);
                    ElementChildren.Cursor = Cursors.Arrow;

                    if (temp != null)
                    {
                        metadata = GetMetadata(temp.Element);
                        if (IsLinkMetadata(metadata))
                        {
                            ElementChildren.Cursor = Cursors.Hand;
                        }
                    }
                    return;
                }

                if (_mouseDown && _recalculateSelectionOnMouseMove)
                {
                    temp = GetPosition(p, true);
                    if (temp != null)
                    {
                        _cursorPosition = temp;

                        if (_cursorPosition.GlobalIndex > _dragBegin.GlobalIndex)
                        {
                            _selectedStart = RoundPositionUp(_dragBegin);
                            _selectedEnd = _cursorPosition;
                        }
                        else
                        {
                            _selectedStart = RoundPositionUp(_cursorPosition);
                            _selectedEnd = _dragBegin;
                        }

                        SetCursor(_cursorPosition, false);
                    }
                    SetSelection(_selectedStart, _selectedEnd, false);
                    _recalculateSelectionOnMouseMove = false;
                }
                else if (IsCtrlDown())
                {
                    temp = GetPosition(p, false);
                    if (temp != null)
                    {
                        metadata = GetMetadata(temp.Element);
                        if (IsLinkMetadata(metadata))
                        {
                            overLink = true;
                        }
                    }
                    ElementChildren.Cursor = (overLink ? Cursors.Hand : Cursors.IBeam);
                }
            }
        }

        /// <summary>
        /// Enables/Disbales the timer depending on whether it is needed
        /// </summary>
        private void UpdateTimerEnabledStatus()
        {
            bool needsToBeRunning = _mouseDown | (_imagesLoading.Count > 0) | _runInitialSpellCheck | (_ticksSinceLastMouseDown < 10);

            if (_timer.IsEnabled && !needsToBeRunning)
            {
                _timer.Stop();
                _timer.Tick -= Tick;
            }
            else if (!_timer.IsEnabled && needsToBeRunning)
            {
                _timer.Tick += Tick;
                _timer.Start();
            }
        }

        /// <summary>
        /// This event is called periodically as a ticker
        /// </summary>
        protected virtual void Tick(object sender, EventArgs e)
        {
            RichTextBlock root = Root;
            int i;
            int a;
            int end;

            _ticksSinceLastMouseDown++;

            if (ElementChildren != null)
            {
                if (!_recalculateSelectionOnMouseMove)
                {
                    _recalculateSelectionOnMouseMove = true;
                }

                UpdateLoadedImages();

                if (EnableSpellCheck)
                {
                    end = _length;
                    if (_lastSpellCheckIndex >= 0)
                    {
                        end = _lastSpellCheckIndex;
                    }

                    if (root.SpellCheckingRTB == null && root.CheckWord != null)
                    {
                        root.SpellCheckingRTB = this;
                    }

                    if (root.SpellCheckingRTB == this && _runInitialSpellCheck)
                    {
                        for (i = 0; i < SpellChecksPerCycle; i++)
                        {
                            // Update the initial spell check
                            if (_nextSpellCheckPosition.GlobalIndex < end)
                            {
                                a = InformWordEdited(_nextSpellCheckPosition);

                                _nextSpellCheckPosition.GlobalIndex += (a > 0 ? a : 1);
                                _nextSpellCheckPosition.CalculatePositionFromGlobalIndex(ElementChildren.ContentChildren);
                            }
                            else
                            {
                                _runInitialSpellCheck = false;
                                root.SpellCheckingRTB = null;
                                break;
                            }
                        }
                    }
                    else if (root.SpellCheckingRTB == null && _runInitialSpellCheck)
                    {
                        _runInitialSpellCheck = false;
                    }
                }
            }
            
            UpdateTimerEnabledStatus();
        }

        /// <summary>
        /// Updates the content when an image has been loaded, at this point we know its dimensions
        /// </summary>
        protected void UpdateLoadedImages()
        {
            // Watch the images as they load with a valid width/height
            for (int i = _imagesLoading.Count - 1; i >= 0; i--)
            {
                if (_imagesLoading[i].ActualWidth > 0 && _imagesLoading[i].ActualHeight > 0)
                {
                    _imagesLoading[i].Opacity = 1;
                    ElementChildren.Update(_imagesLoading[i], false);
                    _imagesLoading.Remove(_imagesLoading[i]);
                    SetCursor(_cursorPosition, false);
                    SetSelection(_selectedStart, _selectedEnd, false);
                }
            }
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates the provided event
        /// </summary>
        /// <param name="handler">Event handler to raise</param>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        /// <param name="setCancelIfNull">If thr handler is null the Cancel property is set to true</param>
        protected void RaiseRichTextEvent(RichTextBoxEventHandler handler, object sender, RichTextBoxEventArgs args, bool setCancelIfNull)
        {
            if (handler != null)
            {
                handler(sender, args);
            }
            else if(setCancelIfNull)
            {
                args.Cancel = true;
            }
        }

        /// <summary>
        /// Generates a ElementWrite event to indicate a style is being written to CSS format
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected virtual void RaiseWriteStyleToHtml(object sender, RichTextBoxHtmlEventArgs args)
        {
            if (WriteStyleToHtml != null)
            {
                WriteStyleToHtml(sender, args);
            }
        }

        /// <summary>
        /// Generates a MouseLeftButtonUp event
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected virtual void RaiseMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
        {
            if (MouseLeftButtonUp != null)
            {
                MouseLeftButtonUp(sender, args);
            }
        }

        /// <summary>
        /// Generates a MouseLeftButtonDown event
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        protected virtual void RaiseMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            if (MouseLeftButtonDown != null)
            {
                MouseLeftButtonDown(sender, args);
            }
        }

        #endregion
    }
}
