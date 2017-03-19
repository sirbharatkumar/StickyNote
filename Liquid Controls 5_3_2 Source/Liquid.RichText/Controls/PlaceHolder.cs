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
    public partial class PlaceHolder : ContentControl
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the text value of the place holder
        /// </summary>
        public string Value { get; set; }

        #endregion

        #region Constructor

        public PlaceHolder()
        {
            IsTabStop = false;
            DefaultStyleKey = this.GetType();
        }

        #endregion

        #region Public Methods

        #endregion
    }
}
