using System;

namespace Liquid.Components.Internal
{
    internal partial class Word : IComparable
    {
        #region Public Properties

        public string AffixKeys { get; set; }

        public int Index { get; set; }

        public string PhoneticCode { get; set; }

        public string Text { get; set; }

        internal int EditDistance { get; set; }

        internal int Height { get; set; }

        #endregion

        #region Constructor

        public Word()
        {
            AffixKeys = string.Empty;
            EditDistance = 0;
            Height = 0;
            Index = 0;
            PhoneticCode = string.Empty;
        }

        #endregion

        #region Public Methods

        public int CompareTo(object obj)
        {
            return EditDistance.CompareTo(((Word)obj).EditDistance);
        }

        #endregion
    }
}
