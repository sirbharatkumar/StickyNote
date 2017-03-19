using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Liquid.Components.Internal
{
    internal partial class WordDictionary
    {
        #region Private Properties

        private Dictionary<string, Word> _baseWords = new Dictionary<string, Word>();
        private string _tryCharacters = string.Empty;
        private List<string> _replaceCharacters = new List<string>();
        private Dictionary<string, AffixRule> _prefixRules = new Dictionary<string, AffixRule>();
        private List<PhoneticRule> _phoneticRules = new List<PhoneticRule>();
        private Dictionary<string, AffixRule> _suffixRules = new Dictionary<string, AffixRule>();
        private List<string> _possibleBaseWords = new List<string>();

        #endregion

        #region Puiblic Properties

        public string TryCharacters
        {
            get { return _tryCharacters; }
            set { _tryCharacters = value; }
        }

        public List<string> ReplaceCharacters
        {
            get { return _replaceCharacters; }
        }

        public Dictionary<string, AffixRule> PrefixRules
        {
            get { return _prefixRules; }
        }

        public List<PhoneticRule> PhoneticRules
        {
            get { return _phoneticRules; }
        }

        public Dictionary<string, AffixRule> SuffixRules
        {
            get { return _suffixRules; }
        }

        public Dictionary<string, Word> BaseWords
        {
            get { return _baseWords; }
        }

        public List<string> PossibleBaseWords
        {
            get { return _possibleBaseWords; }
        }

        #endregion

        #region Constructor

        public WordDictionary(Stream dictionaryResource)
        {
            Load(dictionaryResource);
        }

        #endregion

        #region Public Methods

        public void Load(Stream dictionary)
        {
            StreamReader sr = new StreamReader(dictionary);
            string tempLine;
            Regex _spaceRegx = new Regex(@"[^\s]+", RegexOptions.None);
            MatchCollection partMatches;
            string currentSection = "";
            AffixRule currentRule = null;

            while (sr.Peek() >= 0)
            {
                tempLine = sr.ReadLine().Trim();

                if (tempLine.Length > 0)
                {
                    switch (tempLine)
                    {
                        case "[Copyright]":
                        case "[Try]":
                        case "[Replace]":
                        case "[Prefix]":
                        case "[Suffix]":
                        case "[Phonetic]":
                        case "[Words]":
                            currentSection = tempLine;
                            break;
                        default:
                            switch (currentSection)
                            {
                                case "[Copyright]":
                                    break;
                                case "[Try]":
                                    this.TryCharacters += tempLine;
                                    break;
                                case "[Replace]":
                                    this.ReplaceCharacters.Add(tempLine);
                                    break;
                                case "[Prefix]":
                                case "[Suffix]":
                                    partMatches = _spaceRegx.Matches(tempLine);

                                    if (partMatches.Count == 3)
                                    {
                                        currentRule = new AffixRule();

                                        currentRule.Name = partMatches[0].Value;
                                        if (partMatches[1].Value == "Y") currentRule.AllowCombine = true;

                                        if (currentSection == "[Prefix]")
                                        {
                                            this.PrefixRules.Add(currentRule.Name, currentRule);
                                        }
                                        else
                                        {
                                            this.SuffixRules.Add(currentRule.Name, currentRule);
                                        }
                                    }
                                    else if (partMatches.Count == 4)
                                    {
                                        if (currentRule.Name == partMatches[0].Value)
                                        {
                                            AffixEntry entry = new AffixEntry();

                                            if (partMatches[1].Value != "0") entry.StripCharacters = partMatches[1].Value;
                                            entry.AddCharacters = partMatches[2].Value;
                                            AffixUtility.EncodeConditions(partMatches[3].Value, entry);

                                            currentRule.AffixEntries.Add(entry);
                                        }
                                    }
                                    break;
                                case "[Phonetic]":
                                    partMatches = _spaceRegx.Matches(tempLine);
                                    if (partMatches.Count >= 2)
                                    {
                                        PhoneticRule rule = new PhoneticRule();
                                        PhoneticUtility.EncodeRule(partMatches[0].Value, rule);
                                        rule.ReplaceString = partMatches[1].Value;
                                        _phoneticRules.Add(rule);
                                    }
                                    break;
                                case "[Words]":
                                    string[] parts = tempLine.Split('/');
                                    Word tempWord = new Word();
                                    tempWord.Text = parts[0];
                                    if (parts.Length >= 2) tempWord.AffixKeys = parts[1];
                                    if (parts.Length >= 3) tempWord.PhoneticCode = parts[2];

                                    this.BaseWords.Add(tempWord.Text, tempWord);
                                    break;
                            }
                            break;
                    }
                }
            }

            sr.Close();
        }

        public bool Contains(string word)
        {
            _possibleBaseWords.Clear();

            if (_baseWords.ContainsKey(word))
            {
                return true;
            }
            if (_baseWords.ContainsKey(word.ToLower()))
            {
                return true;
            }

            if (NotInBaseWords(word))
            {
                return true;
            }
            if (NotInBaseWords(word.ToLower()))
            {
                return true;
            }

            return false;
        }

        private bool NotInBaseWords(string word)
        {
            List<string> suffixWords = new List<string>();
            suffixWords.Add(word);

            foreach (AffixRule rule in SuffixRules.Values)
            {
                foreach (AffixEntry entry in rule.AffixEntries)
                {
                    string tempWord = AffixUtility.RemoveSuffix(word, entry);
                    if (tempWord != word)
                    {
                        if (_baseWords.ContainsKey(tempWord))
                        {
                            if (this.VerifyAffixKey(tempWord, rule.Name[0]))
                            {
                                return true;
                            }
                        }

                        if (rule.AllowCombine)
                        {
                            suffixWords.Add(tempWord);
                        }
                        else
                        {
                            _possibleBaseWords.Add(tempWord);
                        }
                    }
                }
            }
            _possibleBaseWords.AddRange(suffixWords);

            foreach (AffixRule rule in PrefixRules.Values)
            {
                foreach (AffixEntry entry in rule.AffixEntries)
                {
                    foreach (string suffixWord in suffixWords)
                    {
                        string tempWord = AffixUtility.RemovePrefix(suffixWord, entry);
                        if (tempWord != suffixWord)
                        {
                            if (_baseWords.ContainsKey(tempWord))
                            {
                                if (this.VerifyAffixKey(tempWord, rule.Name[0]))
                                {
                                    return true;
                                }
                            }

                            _possibleBaseWords.Add(tempWord);
                        }
                    }
                }
            }

            return false;
        }

        private bool VerifyAffixKey(string word, char affixKey)
        {
            Word baseWord = (Word)this.BaseWords[word];
            List<char> keys = new List<char>(baseWord.AffixKeys.ToCharArray());
            return keys.Contains(affixKey);
        }

        public List<string> ExpandWord(Word word)
        {
            List<string> suffixWords = new List<string>();
            List<string> words = new List<string>();

            suffixWords.Add(word.Text);
            string prefixKeys = "";

            foreach (char key in word.AffixKeys)
            {
                if (_suffixRules.ContainsKey(key.ToString()))
                {
                    AffixRule rule = _suffixRules[key.ToString()];
                    string tempWord = AffixUtility.AddSuffix(word.Text, rule);
                    if (tempWord != word.Text)
                    {
                        if (rule.AllowCombine)
                        {
                            suffixWords.Add(tempWord);
                        }
                        else
                        {
                            words.Add(tempWord);
                        }
                    }
                }
                else if (_prefixRules.ContainsKey(key.ToString()))
                {
                    prefixKeys += key.ToString();
                }
            }

            foreach (char key in prefixKeys)
            {
                AffixRule rule = _prefixRules[key.ToString()];

                foreach (string suffixWord in suffixWords)
                {
                    string tempWord = AffixUtility.AddPrefix(suffixWord, rule);
                    if (tempWord != suffixWord)
                    {
                        words.Add(tempWord);
                    }
                }
            }

            words.AddRange(suffixWords);

            return words;
        }

        public string PhoneticCode(string word)
        {
            string tempWord = word.ToUpper();
            string prevWord = "";
            StringBuilder code = new StringBuilder();

            while (tempWord.Length > 0)
            {
                prevWord = tempWord;
                foreach (PhoneticRule rule in _phoneticRules)
                {
                    bool begining = tempWord.Length == word.Length ? true : false;
                    bool ending = rule.ConditionCount == tempWord.Length ? true : false;

                    if ((rule.BeginningOnly == begining || !rule.BeginningOnly)
                        && (rule.EndOnly == ending || !rule.EndOnly)
                        && rule.ConditionCount <= tempWord.Length)
                    {
                        int passCount = 0;

                        for (int i = 0; i < rule.ConditionCount; i++)
                        {
                            int charCode = (int)tempWord[i];
                            if (charCode < rule.Condition.Length && (rule.Condition[charCode] & (1 << i)) == (1 << i))
                            {
                                passCount++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (passCount == rule.ConditionCount)
                        {
                            if (rule.ReplaceMode)
                            {
                                tempWord = rule.ReplaceString + tempWord.Substring(rule.ConditionCount - rule.ConsumeCount);
                            }
                            else
                            {
                                if (rule.ReplaceString != "_")
                                {
                                    code.Append(rule.ReplaceString);
                                }
                                tempWord = tempWord.Substring(rule.ConditionCount - rule.ConsumeCount);
                            }
                            break;
                        }
                    }
                }

                if (prevWord.Length == tempWord.Length)
                {

                    tempWord = tempWord.Substring(1);
                }
            }

            return code.ToString();
        }

        #endregion
    }
}
