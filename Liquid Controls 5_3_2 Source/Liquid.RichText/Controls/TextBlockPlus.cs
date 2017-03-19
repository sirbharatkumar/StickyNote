using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Liquid
{
    #region Public Enums

    public enum TextBlockPlusEffect
    {
        None,
        Strike
    }

    public enum BorderEffect
    {
        None,
        Solid,
        Dotted,
        Dashed
    }

    public enum ShadowEffect
    {
        None,
        Slight,
        Normal
    }

    public enum TextBlockPlusSelectionEffect
    {
        DottedUnderline
    }

    #endregion

    /// <summary>
    /// An enhanced version of the TextBlock
    /// </summary>
    public partial class TextBlockPlus : Grid
    {
        #region Private Properties

        private TextBlock _element = new TextBlock();
        private HyperlinkButton _link;
        private TextBlock _shadow;
        private TextBlockPlusEffect _effect = TextBlockPlusEffect.None;
        private BorderEffect _border = BorderEffect.None;
        private ShadowEffect _shadowEffect = ShadowEffect.None;
        private Rectangle _strike;
        private Rectangle _background;
        private Brush _shadowBrush;
        private List<TextBlockPlusHilight> _hilights = new List<TextBlockPlusHilight>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the text color
        /// </summary>
        public new Brush Background
        {
            get { return base.Background; }
            set { base.Background = value; UpdateStyle(); }
        }

        /// <summary>
        /// Gets or sets the effect for this block of text
        /// </summary>
        public TextBlockPlusEffect TextBlockPlusEffect
        {
            get { return _effect; }
            set { _effect = value; UpdateStyle(); }
        }

        /// <summary>
        /// Gets or sets the border style
        /// </summary>
        public BorderEffect BorderType
        {
            get { return _border; }
            set { _border = value; UpdateStyle(); }
        }

        /// <summary>
        /// Gets or sets the shadow style
        /// </summary>
        public ShadowEffect Shadow
        {
            get { return _shadowEffect; }
            set { _shadowEffect = value; UpdateStyle(); }
        }

        /// <summary>
        /// Gets or sets the shadow brush
        /// </summary>
        public Brush ShadowBrush
        {
            get { return _shadowBrush; }
            set { _shadowBrush = value; UpdateStyle(); }
        }

        /// <summary>
        /// Gets or sets the text value
        /// </summary>
        public string Text
        {
            get { return _element.Text; }
            set { _element.Text = value; UpdateFormatting(); }
        }

        /// <summary>
        /// Gets or sets the font family
        /// </summary>
        public FontFamily FontFamily
        {
            get { return _element.FontFamily; }
            set { _element.FontFamily = value; UpdateFormatting(); }
        }

        /// <summary>
        /// Gets or sets the font size
        /// </summary>
        public double FontSize
        {
            get { return _element.FontSize; }
            set { _element.FontSize = value; UpdateFormatting(); }
        }

        /// <summary>
        /// Gets or sets the font stretch
        /// </summary>
        public FontStretch FontStretch
        {
            get { return _element.FontStretch; }
            set { _element.FontStretch = value; UpdateFormatting(); }
        }

        /// <summary>
        /// Gets or sets the font style
        /// </summary>
        public FontStyle FontStyle
        {
            get { return _element.FontStyle; }
            set { _element.FontStyle = value; UpdateFormatting(); }
        }

        /// <summary>
        /// Gets or sets the font weight
        /// </summary>
        public FontWeight FontWeight
        {
            get { return _element.FontWeight; }
            set { _element.FontWeight = value; UpdateFormatting(); }
        }

        /// <summary>
        /// Gets or sets the font foreground brush
        /// </summary>
        public Brush Foreground
        {
            get { return _element.Foreground; }
            set { _element.Foreground = value; }
        }

        /// <summary>
        /// Gets or sets the font family
        /// </summary>
        public TextDecorationCollection TextDecorations
        {
            get { return _element.TextDecorations; }
            set { _element.TextDecorations = value; UpdateFormatting(); }
        }

        /// <summary>
        /// Gets or sets a value to apply to the line height
        /// </summary>
        public double LineHeightMultiplier { get; set; }

        /// <summary>
        /// Gets the width of the undelying textblock
        /// </summary>
        public double ContentWidth
        {
            get { return _element.ActualWidth; }
        }

        /// <summary>
        /// Gets the height of the undelying textblock
        /// </summary>
        public double ContentHeight
        {
            get { return _element.ActualHeight; }
        }


        public List<TextBlockPlusHilight> Hilights
        {
            get { return _hilights; }
        }

        /// <summary>
        /// Gets the style ID for this textblock
        /// </summary>
        public string StyleID
        {
            get
            {
                if (Tag is RichTextTag)
                {
                    return ((RichTextTag)Tag).StyleID;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        #endregion

        #region Constructor

        public TextBlockPlus()
        {
            LineHeightMultiplier = 1;
            Children.Add(_element);
        }

        #endregion

        #region Public Methods

        public void ApplyStyle(RichTextBoxStyle style, RichTextBoxStyle linkStyle, bool useLinkButtons)
        {
            RichTextTag tag = new RichTextTag();
            bool isLink = false;

            this.IsHitTestVisible = false;

            if (this.Tag is RichTextTag)
            {
                tag = (RichTextTag)this.Tag;
                if (tag.Metadata != null)
                {
                    isLink = tag.Metadata.IsLink;
                }

                if (useLinkButtons)
                {
                    if (isLink)
                    {
                        if (_link == null)
                        {
                            Children.Remove(_element);
                            _element.Foreground = new SolidColorBrush(Colors.Red);

                            _link = new HyperlinkButton()
                            {
                                Content = _element,
                                NavigateUri = new Uri(tag.Metadata["URL"]),
                                TargetName = (tag.Metadata.ContainsKey("Target") ? tag.Metadata["Target"] : "_blank")
                            };
                            Children.Insert(0, _link);
                        }
                        this.IsHitTestVisible = true;
                    }
                    else
                    {
                        if (_link != null)
                        {
                            Children.Remove(_link);
                            _link = null;
                            Children.Insert(0, _element);
                        }
                    }
                }
            }

            _element.FontFamily = new FontFamily(style.Family);
            _element.FontSize = (style.Size != null ? (double)style.Size : 14);
            base.Background = style.Background;
            _element.Foreground = isLink ? linkStyle.Foreground : style.Foreground;
            _element.FontWeight = (style.Weight != null ? (FontWeight)style.Weight : FontWeights.Normal);
            _element.FontStyle = (style.Style != null ? (FontStyle)style.Style : FontStyles.Normal);
            _element.TextDecorations = isLink ? linkStyle.Decorations : style.Decorations;
            _effect = style.Effect;
            _border = style.BorderType;
            _shadowEffect = style.Shadow;

            HorizontalAlignment = style.Alignment;
            VerticalAlignment = style.VerticalAlignment;

            if (style.ShadowBrush != null)
            {
                _shadowBrush = style.ShadowBrush;
            }

            if (style.Special == RichTextSpecialFormatting.Subscript)
            {
                _element.FontSize *= RichTextBlock.SubScriptMultiplier;
                VerticalAlignment = VerticalAlignment.Bottom;
                LineHeightMultiplier = 1 / RichTextBlock.SubScriptMultiplier;
            }
            else if (style.Special == RichTextSpecialFormatting.Superscript)
            {
                _element.FontSize *= RichTextBlock.SuperScriptMultiplier;
                VerticalAlignment = VerticalAlignment.Top;
                LineHeightMultiplier = 1 / RichTextBlock.SuperScriptMultiplier;
            }
            else
            {
                LineHeightMultiplier = 1;
            }

            Margin = new Thickness(style.Margin.Left, style.Margin.Top, style.Margin.Right, style.Margin.Bottom);

            UpdateStyle();
        }

        public void ClearHilights()
        {
            for (int i = _hilights.Count - 1; i >= 0; i--)
            {
                ClearHilight(_hilights[i]);
            }
        }

        public void ClearHilight(TextBlockPlusHilight hilight)
        {
            Children.Remove(hilight.Element);
            _hilights.Remove(hilight);
        }

        public void AddHilight(TextBlockPlusHilight hilight)
        {
            hilight.Element = new Rectangle();
            hilight.Element.Height = 1;
            hilight.Element.HorizontalAlignment = HorizontalAlignment.Left;
            hilight.Element.VerticalAlignment = VerticalAlignment.Bottom;

            _hilights.Add(hilight);

            CalculateHilight(hilight);

            hilight.Element.StrokeThickness = 1;
            hilight.Element.StrokeDashCap = PenLineCap.Square;
            hilight.Element.StrokeDashArray = new DoubleCollection() { 0, 3 };
            hilight.Element.StrokeDashOffset = 1;
            hilight.Element.Stroke = hilight.Brush;

            Children.Add(hilight.Element);
        }

        public List<TextBlockPlusHilight> GetHilights(int start, int length)
        {
            List<TextBlockPlusHilight> results = new List<TextBlockPlusHilight>();
            int i;

            for (i = _hilights.Count - 1; i >= 0; i--)
            {
                if (length == -1)
                {
                    if (_hilights[i].Start + _hilights[i].Length >= start)
                    {
                        results.Add(_hilights[i]);
                    }
                }
                else if (start >= 0)
                {
                    if ((_hilights[i].Start >= start && _hilights[i].Start <= start + length) ||
                        (_hilights[i].Start + _hilights[i].Length >= start && _hilights[i].Start + _hilights[i].Length <= start + length) ||
                        (start >= _hilights[i].Start && start + length < _hilights[i].Start + _hilights[i].Length))
                    {
                        results.Add(_hilights[i]);
                    }
                }
                else if (_hilights[i].Start + _hilights[i].Length >= Text.Length || _hilights[i].Start >= Text.Length)
                {
                    results.Add(_hilights[i]);
                }
            }

            return results;
        }

        public override string ToString()
        {
            return _element.Text;
        }

        #endregion

        #region Internal Methods

        internal void CalculateHilight(TextBlockPlusHilight hilight)
        {
            Thickness m = new Thickness();
            string temp;

            if (hilight.Start >= 0)
            {
                temp = Text;
                _element.Text = temp.Substring(0, hilight.Start);
                m.Left = _element.ActualWidth;
                hilight.Element.Margin = m;
                _element.Text = temp.Substring(hilight.Start, hilight.Length);
                hilight.Element.Width = _element.ActualWidth;
                hilight.Element.Height = 1;
                Text = temp;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the text formatting
        /// </summary>
        private void UpdateFormatting()
        {
            if (_shadow != null)
            {
                _shadow.Text = _element.Text;
                _shadow.FontFamily = new FontFamily(_element.FontFamily.Source);
                _shadow.FontSize = _element.FontSize;
                _shadow.FontStyle = _element.FontStyle;
                _shadow.FontWeight = _element.FontWeight;
                _shadow.TextDecorations = _element.TextDecorations;
            }
            if (_strike != null)
            {
                _strike.Width = _element.ActualWidth;
                _strike.Height = Math.Ceiling(_element.ActualHeight * 0.05);
                _strike.Fill = _element.Foreground;
            }

            if (_background != null)
            {
                _background.Width = _element.ActualWidth;
                _background.Height = _element.ActualHeight;
                _background.Stroke = _element.Foreground;
            }
        }

        /// <summary>
        /// Applies the current style settings
        /// </summary>
        private void UpdateStyle()
        {
            if (_effect != TextBlockPlusEffect.None)
            {
                if (_strike == null)
                {
                    _strike = new Rectangle();
                }

                if (!Children.Contains(_strike))
                {
                    Children.Add(_strike);
                }
            }
            else if (Children.Contains(_strike))
            {
                Children.Remove(_strike);
            }

            if (_border != BorderEffect.None)
            {
                if (_background == null)
                {
                    _background = new Rectangle();
                    _background.Fill = new SolidColorBrush(Colors.Transparent);
                    _background.StrokeThickness = 1;
                    _background.StrokeDashCap = PenLineCap.Square;
                }

                if (!Children.Contains(_background))
                {
                    Children.Add(_background);
                }
            }
            else if (Children.Contains(_background))
            {
                Children.Remove(_background);
            }

            switch (_border)
            {
                case BorderEffect.Dashed:
                    _background.StrokeDashArray = new DoubleCollection() { 0, 1, 2, 3 };
                    _background.StrokeDashOffset = 5;
                    break;
                case BorderEffect.Dotted:
                    _background.StrokeDashArray = new DoubleCollection() { 0, 2 };
                    _background.StrokeDashOffset = 1;
                    break;
                case BorderEffect.Solid:
                    _background.StrokeDashArray = new DoubleCollection();
                    _background.StrokeDashOffset = 1;
                    break;
                default:
                    break;
            }

            if (_shadowEffect != ShadowEffect.None)
            {
                if (_shadow == null)
                {
                    _shadow = new TextBlock();
                }
                if (_shadowBrush == null)
                {
                    _shadowBrush = new SolidColorBrush(Color.FromArgb(92, 128, 128, 128));
                }

                if (!Children.Contains(_shadow))
                {
                    Children.Insert(0, _shadow);
                }

                _shadow.Foreground = _shadowBrush;
            }
            else if (_shadow != null)
            {
                if (Children.Contains(_shadow))
                {
                    Children.Remove(_shadow);
                }
            }
            switch (_shadowEffect)
            {
                case ShadowEffect.Slight:
                    _shadow.Margin = new Thickness(1, 1, 0, 0);
                    break;
                case ShadowEffect.Normal:
                    _shadow.Margin = new Thickness(2, 2, 0, 0);
                    break;
                default:
                    break;
            }
            UpdateFormatting();
        }

        #endregion
    }
}
