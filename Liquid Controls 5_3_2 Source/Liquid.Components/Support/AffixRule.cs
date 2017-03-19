using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Liquid.Components.Internal
{
    internal partial class AffixRule
    {
        #region Public Properties

        public bool AllowCombine { get; set; }

        public List<AffixEntry> AffixEntries { get; set; }

        public string Name { get; set; }

        #endregion

        #region Constructor

        public AffixRule()
        {
            AllowCombine = false;
            AffixEntries = new List<AffixEntry>();
            Name = string.Empty;
        }

        #endregion
    }
}
