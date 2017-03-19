using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Liquid.Components.Internal;

namespace Liquid.Components
{
    public partial class SpellChecker
    {
        #region Private Properties

        private WordDictionary _dictionary;
        private string _currentWord = string.Empty;
        private int _maxSuggestions = 10;
        private List<string> _custom = new List<string>();

        #endregion

        #region Public Properties

        public int MaxSuggestions
        {
            get { return _maxSuggestions; }
            set { _maxSuggestions = value; }
        }

        /// <summary>
        /// Gets or sets the custom dictionary
        /// </summary>
        public List<string> Custom
        {
            get { return _custom; }
            set { _custom = value; }
        }

        #endregion

        #region Constructor

        public SpellChecker(Stream dictionaryResource)
        {
            _dictionary = new WordDictionary(dictionaryResource);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a word to the custom dictionary
        /// </summary>
        /// <param name="word">Word</param>
        public void AddCustom(string word)
        {
            if (!_custom.Contains(word))
            {
                _custom.Add(word);
            }
        }

        /// <summary>
        /// Removes a word from the custom dictionary
        /// </summary>
        /// <param name="word">Word to remove</param>
        public void RemoveCustom(string word)
        {
            if (!_custom.Contains(word))
            {
                _custom.Remove(word);
            }
        }

        /// <summary>
        /// Checks a word
        /// </summary>
        /// <param name="word">Word</param>
        /// <returns>True is the word is spelt correct</returns>
        public bool CheckWord(string word)
        {
            int result = 0;

            if (word.Length > 0)
            {
                if (_custom.Contains(word))
                {
                    return true;
                }
                if (_dictionary.Contains(word))
                {
                    return true;
                }
                else if (_dictionary.Contains(word.ToLower()))
                {
                    return true;
                }
                else if (int.TryParse(word, out result))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets a list of suggestions for a mis-spelt word
        /// </summary>
        /// <param name="word">Word</param>
        /// <returns>List of strings</returns>
        public List<string> GetSuggestions(string word)
        {
            List<string> results = null;

            _currentWord = word;
            results = Suggest();

            return results;
        }

        #endregion

        #region Private Methods

        private List<string> Suggest()
        {
            List<Word> tempSuggestion = new List<Word>();
            Dictionary<string, string> codes = new Dictionary<string, string>();
            List<string> suggestions = new List<string>();
            List<string> words;
            string tempCode;
            Word newWord;
            string tempWord;

            if (_currentWord.Length == 0)
            {
                return suggestions;
            }

            foreach (string temp in _dictionary.PossibleBaseWords)
            {
                tempCode = _dictionary.PhoneticCode(temp);
                if (tempCode.Length > 0 && !codes.ContainsKey(tempCode))
                {
                    codes.Add(tempCode, tempCode);
                }
            }

            if (codes.Count > 0)
            {
                foreach (Word w in _dictionary.BaseWords.Values)
                {
                    if (codes.ContainsKey(w.PhoneticCode))
                    {
                        words = _dictionary.ExpandWord(w);

                        foreach (string expandedWord in words)
                        {
                            newWord = new Word()
                            {
                                Text = expandedWord,
                                EditDistance = EditDistance(_currentWord, expandedWord)
                            };
                            tempSuggestion.Add(newWord);
                        }
                    }
                }
            }

            ReplaceChars(tempSuggestion);
            BadChar(tempSuggestion);
            ExtraChar(tempSuggestion);
            ForgotChar(tempSuggestion);
            TwoWords(tempSuggestion);
            SwapChar(tempSuggestion);

            tempSuggestion.Sort();

            for (int i = 0; i < tempSuggestion.Count; i++)
            {
                tempWord = ((Word)tempSuggestion[i]).Text;
                if (!suggestions.Contains(tempWord))
                {
                    suggestions.Add(tempWord);
                }

                if (suggestions.Count >= _maxSuggestions && _maxSuggestions > 0)
                {
                    break;
                }
            }
            return suggestions;
        }

        private int EditDistance(string source, string target, bool positionPriority)
        {
            Array matrix = Array.CreateInstance(typeof(int), source.Length + 1, target.Length + 1);

            matrix.SetValue(0, 0, 0);

            for (int j = 1; j <= target.Length; j++)
            {
                int val = (int)matrix.GetValue(0, j - 1);
                matrix.SetValue(val + 1, 0, j);
            }

            for (int i = 1; i <= source.Length; i++)
            {
                int val = (int)matrix.GetValue(i - 1, 0);
                matrix.SetValue(val + 1, i, 0);

                for (int j = 1; j <= target.Length; j++)
                {
                    int diag = (int)matrix.GetValue(i - 1, j - 1);

                    if (source.Substring(i - 1, 1) != target.Substring(j - 1, 1))
                    {
                        diag++;
                    }

                    int deletion = (int)matrix.GetValue(i - 1, j);
                    int insertion = (int)matrix.GetValue(i, j - 1);
                    int match = Math.Min(deletion + 1, insertion + 1);
                    matrix.SetValue(Math.Min(diag, match), i, j);
                }
            }

            int dist = (int)matrix.GetValue(source.Length, target.Length);

            if (positionPriority)
            {
                if (source[0] != target[0]) dist++;
                if (source[source.Length - 1] != target[target.Length - 1]) dist++;
            }
            return dist;
        }

        private int EditDistance(string source, string target)
        {
            return EditDistance(source, target, true);
        }

        /// <summary>
        ///	Swap out each char one by one and try all the tryme
        ///	chars in its place to see if that makes a good word
        /// </summary>
        private void BadChar(List<Word> tempSuggestion)
        {
            StringBuilder tempWord;

            for (int i = 0; i < _currentWord.Length; i++)
            {
                tempWord = new StringBuilder(_currentWord);

                for (int x = 0; x < _dictionary.TryCharacters.Length; x++)
                {
                    tempWord[i] = _dictionary.TryCharacters[x];
                    TestSuggestion(tempWord.ToString(), true, tempSuggestion);
                }
            }
        }

        /// <summary>
        /// Try omitting one char of word at a time
        /// </summary>
        private void ExtraChar(List<Word> tempSuggestion)
        {
            string tempWord;

            if (_currentWord.Length > 1)
            {
                for (int i = 0; i < _currentWord.Length; i++)
                {
                    tempWord = _currentWord.Remove(i, 1);
                    TestSuggestion(tempWord.ToString(), true, tempSuggestion);
                }
            }
        }

        /// <summary>
        /// Try inserting a tryme character before every letter
        /// </summary>
        private void ForgotChar(List<Word> tempSuggestion)
        {
            string tempWord;

            for (int i = 0; i <= _currentWord.Length; i++)
            {
                for (int x = 0; x < _dictionary.TryCharacters.Length; x++)
                {
                    tempWord = _currentWord.Insert(i, _dictionary.TryCharacters[x].ToString());
                    TestSuggestion(tempWord, true, tempSuggestion);
                }
            }
        }

        /// <summary>
        /// Suggestions for a typical fault of spelling, that
        /// differs with more, than 1 letter from the right form.
        /// </summary>
        private void ReplaceChars(List<Word> tempSuggestion)
        {
            List<string> replacementChars = _dictionary.ReplaceCharacters;

            for (int i = 0; i < replacementChars.Count; i++)
            {
                int split = replacementChars[i].IndexOf(' ');
                string key = replacementChars[i].Substring(0, split);
                string replacement = replacementChars[i].Substring(split + 1);

                int pos = _currentWord.IndexOf(key);
                while (pos > -1)
                {
                    string tempWord = _currentWord.Substring(0, pos);
                    tempWord += replacement;
                    tempWord += _currentWord.Substring(pos + key.Length);

                    TestSuggestion(tempWord, true, tempSuggestion);
                    pos = _currentWord.IndexOf(key, pos + 1);
                }
            }
        }

        /// <summary>
        /// Try swapping adjacent chars one by one
        /// </summary>
        private void SwapChar(List<Word> tempSuggestion)
        {
            StringBuilder tempWord;

            for (int i = 0; i < _currentWord.Length - 1; i++)
            {
                tempWord = new StringBuilder(_currentWord);

                char swap = tempWord[i];
                tempWord[i] = tempWord[i + 1];
                tempWord[i + 1] = swap;

                TestSuggestion(tempWord.ToString(), true, tempSuggestion);
            }
        }

        /// <summary>
        /// Split the string into two pieces after every char
        ///	If both pieces are good words make them a suggestion
        /// </summary>
        private void TwoWords(List<Word> tempSuggestion)
        {
            for (int i = 1; i < _currentWord.Length - 1; i++)
            {
                string firstWord = _currentWord.Substring(0, i);
                string secondWord = _currentWord.Substring(i);

                if (TestWord(firstWord) && TestWord(secondWord))
                {
                    TestSuggestion(firstWord + " " + secondWord, false, tempSuggestion);
                }
            }
        }

        private void TestSuggestion(string suggestion, bool test, List<Word> tempSuggestion)
        {
            Word ws;

            if (test && !TestWord(suggestion))
            {
                return;
            }

            ws = new Word();
            ws.Text = suggestion.ToLower();
            ws.EditDistance = EditDistance(_currentWord, suggestion);

            tempSuggestion.Add(ws);
        }

        private bool TestWord(string word)
        {
            if (_dictionary.Contains(word))
            {
                return true;
            }
            else if (_dictionary.Contains(word.ToLower()))
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}
