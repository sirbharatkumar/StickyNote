using System;
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
    public partial class MetadataSelection
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the start index of the content for this metadata
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the length of content for this metadata
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the metadata for the specified content
        /// </summary>
        public ContentMetadata Metadata { get; set; }

        #endregion
    }
}
