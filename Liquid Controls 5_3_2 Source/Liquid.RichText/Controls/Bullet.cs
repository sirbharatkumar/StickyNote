using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Liquid
{
    public enum BulletType
    {
        Bullet,
        Number,
        Image,
        Indent
    }

    /// <summary>
    /// Represents a RichText Bullet or Indent
    /// </summary>
    public partial class Bullet : Control
    {
        #region Visual Elements

        /// <summary> 
        /// Content template.
        /// </summary>
        internal ContentControl ElementContent { get; set; }
        internal const string ElementContentName = "ElementContent";

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the text property
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(Bullet), null);
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { base.SetValue(TextProperty, value); }
        }

        #endregion

        #region Private Properties

        private int _indent = 1;
        private BulletType _type = BulletType.Bullet;
        private int _number = 1;
        private char _bulletCharacter = '●';
        private Uri _bulletImage = null;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the number of indents
        /// </summary>
        public int Indent
        {
            get { return _indent; }
            set { _indent = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets or sets the type of bullet
        /// </summary>
        public BulletType Type
        {
            get { return _type; }
            set { _type = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets or sets the rendered number
        /// </summary>
        public int Number
        {
            get { return _number; }
            set { _number = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets or sets the character used in bulleted lists
        /// </summary>
        public char BulletCharacter
        {
            get { return _bulletCharacter; }
            set { _bulletCharacter = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets or sets the image to be used in bulleted lists
        /// </summary>
        public Uri BulletImage
        {
            get { return _bulletImage; }
            set { _bulletImage = value; UpdateVisualState(); }
        }

        /// <summary>
        /// Gets the pixel width of the indent
        /// </summary>
        public double IndentWidth
        {
            get { return _indent * (FontSize * 3); }
        }

        #endregion

        #region Constructor

        public Bullet()
        {
            IsTabStop = false;
            DefaultStyleKey = this.GetType();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies the provided style to the bullet
        /// </summary>
        /// <param name="style">RichTextBoxStyle object</param>
        public void ApplyStyle(RichTextBoxStyle style)
        {
            FontFamily = new FontFamily(style.Family);
            FontSize = (style.Size != null ? (double)style.Size : 14);
            Foreground = style.Foreground;
            FontWeight = (style.Weight != null ? (FontWeight)style.Weight : FontWeights.Normal);
            FontStyle = (style.Style != null ? (FontStyle)style.Style : FontStyles.Normal);
            HorizontalAlignment = style.Alignment;
            VerticalAlignment = VerticalAlignment.Center;

            if (ElementContent != null && ElementContent.Content is TextBlock)
            {
                TextBlock tb = (TextBlock)ElementContent.Content;

                tb.FontFamily = FontFamily;
                tb.FontSize = FontSize;
                tb.Foreground = Foreground;
                tb.FontWeight = FontWeight;
                tb.FontStyle = FontStyle;
            }

            UpdateVisualState();
        }

        #endregion

        #region Private Methods

        private void UpdateVisualState()
        {
            switch (_type)
            {
                case BulletType.Bullet:
                    Text = _bulletCharacter.ToString();
                    break;
                case BulletType.Number:
                    Text = _number.ToString() + ".";
                    break;
                case BulletType.Image:
                    if (ElementContent != null)
                    {
                        ElementContent.Content = new Image() { Source = new BitmapImage(_bulletImage), Stretch = Stretch.None };
                    }
                    break;
                default:
                    Text = string.Empty;
                    break;
            }
            Width = IndentWidth;
        }

        #endregion

        #region Event Handling

        public override void OnApplyTemplate()
        {
            ElementContent = (ContentControl)GetTemplateChild(ElementContentName);

            base.OnApplyTemplate();
            UpdateVisualState();
        }

        #endregion
    }
}
