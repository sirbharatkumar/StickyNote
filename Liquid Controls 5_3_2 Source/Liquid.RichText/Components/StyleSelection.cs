
using System.Windows;
namespace Liquid
{
    public partial class StyleSelection
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the start index of the content for this style
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the length of content for this style
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the style for the specified content
        /// </summary>
        public RichTextBoxStyle Style { get; set; }

        /// <summary>
        /// Gets or sets the list type of the selected content
        /// </summary>
        public Bullet ListType { get; set; }

        /// <summary>
        /// Gets or sets the alignment of the selected content
        /// </summary>
        public HorizontalAlignment Alignment { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment of the selected content
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; }

        #endregion
    }
}
