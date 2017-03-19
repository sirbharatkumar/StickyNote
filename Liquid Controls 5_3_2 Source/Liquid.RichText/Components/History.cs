using System.Collections.Generic;

namespace Liquid.Components.Internal
{
    public enum HistoryCommand
    {
        InsertXAML,
        InsertContent,
        Delete,
        FormatChange,
        InsertRow,
        InsertColumn,
        DeleteRow,
        DeleteColumn
    }

    public partial class History
    {
        #region Private Properties

        private List<HistoryEvent> _events;
        private int _current = -1;

        #endregion

        #region Public Properties

        public List<HistoryEvent> Events
        {
            get { return _events; }
        }

        /// <summary>
        /// Gets or sets the maximum number of eventsthat can be remembered
        /// </summary>
        public int MaxEvents { get; set; }

        #endregion

        #region Constructor

        public History()
        {
            _events = new List<HistoryEvent>();
            MaxEvents = 128;
            Reset();
        }

        #endregion

        #region Public Methods

        public void Add(object source, HistoryCommand command, object param1, object param2)
        {
            if (_events.Count - 1 > _current)
            {
                for (int i = _events.Count - 1; i > _current; i--)
                {
                    _events.RemoveAt(i);
                }
            }

            // Check if we're at our limit
            if (_events.Count == MaxEvents)
            {
                // Expire the first item in the collection
                _events.RemoveAt(0);
            }

            _events.Add(new HistoryEvent(source, command, param1, param2));
            _current = _events.Count - 1;
        }

        public void ActionReversed()
        {
            _current--;
            if (_current < -1)
            {
                _current = -1;
            }
        }

        public void ActionCouterReversed()
        {
            _current++;
            if (_current >= _events.Count)
            {
                _current = _events.Count;
            }
        }

        public HistoryEvent GetCurrent()
        {
            return (_current >= 0 && _current < _events.Count ? _events[_current] : null);
        }

        public void Reset()
        {
            _events.Clear();
            _current = -1;
        }

        #endregion
    }
}
