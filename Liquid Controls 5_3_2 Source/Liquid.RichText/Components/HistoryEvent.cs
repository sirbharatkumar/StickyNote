
namespace Liquid.Components.Internal
{
    public partial class HistoryEvent
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the soure object
        /// </summary>
        public object Source { get; set; }

        /// <summary>
        /// Gets or sets what this item is
        /// </summary>
        public HistoryCommand Command { get; set; }

        /// <summary>
        /// Gets or sets the 1st parameter associated with this event
        /// </summary>
        public object Parameter1 { get; set; }

        /// <summary>
        /// Gets or sets the 2nd parameter associated with this event
        /// </summary>
        public object Parameter2 { get; set; }

        #endregion

        #region Constructor

        public HistoryEvent(object source, HistoryCommand command, object param1, object param2)
        {
            Source = source;
            Command = command;
            Parameter1 = param1;
            Parameter2 = param2;
        }

        #endregion
    }
}
