using System;
using System.Collections.Generic;

namespace Liquid.Components.Internal
{
    #region Delegates

    public delegate void AutoCompleteEventHandler(object sender, AutoCompleteEventArgs e);

    #endregion

    public partial class AutoComplete
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the starting index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the start index if we are in a hyperlink
        /// </summary>
        public int HyperlinkIndex { get; set; }

        /// <summary>
        /// Gets or sets the current auto complete string
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets text patterns to look for
        /// </summary>
        public List<string> Patterns { get; set; }

        #endregion

        #region Public Events

        public event AutoCompleteEventHandler Hyperlink;
        public event AutoCompleteEventHandler PatternMatch;

        #endregion

        #region Constructor

        public AutoComplete()
        {
            Patterns = new List<string>();
            Reset();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the autocomplete
        /// </summary>
        public void Reset()
        {
            CheckForHyperlink();
            Index = -1;
            Text = string.Empty;
        }

        /// <summary>
        /// Adds the provided character to the autocomplete string
        /// </summary>
        /// <param name="s">Character to add</param>
        public void AddCharacter(string s, int index)
        {
            AutoCompleteEventArgs args;
            string temp;
            string pat;

            if (s == "\r")
            {
                Reset();
                return;
            }
            else if (Index == -1)
            {
                Index = index;
            }

            temp = Text + s;
            foreach (string p in Patterns)
            {
                pat = p.Trim('*');

                if ((p.StartsWith("*") && temp.EndsWith(pat)) || temp == pat)
                {
                    Text = temp.Substring(temp.Length - pat.Length);
                    Index += temp.Length - pat.Length;

                    args = new AutoCompleteEventArgs();
                    args.Text = Text;

                    RaisePatternMatch(this, args);
                    Reset();
                    return;
                }
            }

            if (s == " ")
            {
                Reset();
                return;
            }

            Text = temp;

            switch (Text.ToLower())
            {
                case "http://":
                case "www.":
                    HyperlinkIndex = Index;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if the current input looks like a hyperlink and applies the necessary formatting
        /// </summary>
        private void CheckForHyperlink()
        {
            AutoCompleteEventArgs args = new AutoCompleteEventArgs();

            args.Text = Text;

            if (HyperlinkIndex >= 0)
            {
                RaiseHyperlink(this, args);
            }

            HyperlinkIndex = -1;
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a Hyperlink event to indicate a link has been detected
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaiseHyperlink(object sender, AutoCompleteEventArgs args)
        {
            if (Hyperlink != null)
            {
                Hyperlink(sender, args);
            }
        }

        /// <summary>
        /// Generates a PatternMatch event to indicate a patternhas been matched
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        private void RaisePatternMatch(object sender, AutoCompleteEventArgs args)
        {
            if (PatternMatch != null)
            {
                PatternMatch(sender, args);
            }
        }

        #endregion
    }
}
