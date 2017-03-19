using System;

namespace Liquid
{
    /// <summary>
    /// Event arguments for use with table related events
    /// </summary>
    public partial class TableEventArgs : EventArgs
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the row index this event relates to
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// Gets or sets the column index this event relates to
        /// </summary>
        public int ColumnIndex { get; set; }

        #endregion

        #region Constructor

        public TableEventArgs()
        {
            RowIndex = -1;
            ColumnIndex = -1;
        }

        #endregion
    }
}
