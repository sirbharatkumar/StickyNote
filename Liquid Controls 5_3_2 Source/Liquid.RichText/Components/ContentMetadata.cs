using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;

namespace Liquid
{
    public partial class ContentMetadata : Dictionary<string, string>, IComparable
    {
        public bool IsLink
        {
            get { return ContainsKey("URL"); }
        }

        public bool IsImage
        {
            get { return ContainsKey("ImageSrc") | ContainsKey("ImageWidth") | ContainsKey("ImageHeight") | ContainsKey("ImageAlt"); }
        }

        public bool IsAnchor
        {
            get { return ContainsKey("Anchor"); }
        }

        public ContentMetadata()
        {
        }

        public ContentMetadata(ContentMetadata metadata)
        {
            if (metadata != null)
            {
                foreach (KeyValuePair<string, string> pair in metadata)
                {
                    this.Add(pair.Key, pair.Value);
                }
            }
        }

        public override string ToString()
        {
            if (IsLink)
            {
                return this["URL"];
            }
            else
            {
                return base.ToString();
            }
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (IsLink)
            {
                return this["URL"].CompareTo(((ContentMetadata)obj)["URL"]);
            }
            else
            {
                return 0;
            }
        }

        #endregion
    }
}
