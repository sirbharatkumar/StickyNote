using System;
using System.Text.RegularExpressions;

namespace Liquid.Components.Internal
{
    internal partial class AffixEntry
    {
        #region Public Properties

        public string AddCharacters { get; set; }

        public int[] Condition { get; set; }

        public string StripCharacters { get; set; }

        public int ConditionCount { get; set; }

        #endregion

        #region Constructor

        public AffixEntry()
        {
            AddCharacters = string.Empty;
            Condition = new int[256];
            StripCharacters = string.Empty;
        }

        #endregion
    }
}
