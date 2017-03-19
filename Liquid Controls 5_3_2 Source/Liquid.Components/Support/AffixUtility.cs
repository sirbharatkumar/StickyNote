
namespace Liquid.Components.Internal
{
    internal partial class AffixUtility
    {
        #region Constructor

        public AffixUtility()
        {
        }

        #endregion

        #region Public Static Methods

        public static string AddPrefix(string word, AffixRule rule)
        {
            foreach (AffixEntry entry in rule.AffixEntries)
            {
                if (word.Length >= entry.ConditionCount)
                {
                    int passCount = 0;
                    for (int i = 0; i < entry.ConditionCount; i++)
                    {
                        int charCode = (int)word[i];
                        if ((entry.Condition[charCode] & (1 << i)) == (1 << i))
                        {
                            passCount++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (passCount == entry.ConditionCount)
                    {
                        string tempWord = word.Substring(entry.StripCharacters.Length);
                        tempWord = entry.AddCharacters + tempWord;
                        return tempWord;
                    }
                }
            }
            return word;
        }

        public static string AddSuffix(string word, AffixRule rule)
        {
            foreach (AffixEntry entry in rule.AffixEntries)
            {
                if (word.Length >= entry.ConditionCount)
                {
                    int passCount = 0;
                    for (int i = 0; i < entry.ConditionCount; i++)
                    {
                        int charCode = (int)word[word.Length - (entry.ConditionCount - i)];
                        if ((entry.Condition[charCode] & (1 << i)) == (1 << i))
                        {
                            passCount++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (passCount == entry.ConditionCount)
                    {
                        int tempLen = word.Length - entry.StripCharacters.Length;
                        string tempWord = word.Substring(0, tempLen);
                        tempWord += entry.AddCharacters;
                        return tempWord;
                    }
                }
            }
            return word;
        }

        public static void EncodeConditions(string conditionText, AffixEntry entry)
        {
            for (int i = 0; i < entry.Condition.Length; i++)
            {
                entry.Condition[i] = 0;
            }

            if (conditionText == ".")
            {
                entry.ConditionCount = 0;
                return;
            }

            bool neg = false;
            bool group = false;
            bool end = false;
            int num = 0;

            char[] memberChars = new char[200];
            int numMember = 0;

            foreach (char cond in conditionText)
            {
                if (cond == '[')
                {
                    group = true;
                }
                else if (cond == '^' && group)
                {
                    neg = true;
                }
                else if (cond == ']')
                {
                    end = true;
                }
                else if (group)
                {
                    memberChars[numMember] = cond;
                    numMember++;
                }
                else
                {
                    end = true;
                }

                if (end)
                {
                    if (group)
                    {
                        if (neg)
                        {
                            for (int j = 0; j < entry.Condition.Length; j++)
                            {
                                entry.Condition[j] = entry.Condition[j] | (1 << num);
                            }
                            for (int j = 0; j < numMember; j++)
                            {
                                int charCode = (int)memberChars[j];
                                entry.Condition[charCode] = entry.Condition[charCode] & ~(1 << num);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < numMember; j++)
                            {
                                int charCode = (int)memberChars[j];
                                entry.Condition[charCode] = entry.Condition[charCode] | (1 << num);
                            }
                        }
                        group = false;
                        neg = false;
                        numMember = 0;
                    }
                    else
                    {
                        if (cond == '.')
                        {
                            for (int j = 0; j < entry.Condition.Length; j++)
                            {
                                entry.Condition[j] = entry.Condition[j] | (1 << num);
                            }
                        }
                        else
                        {
                            int charCode = (int)cond;
                            entry.Condition[charCode] = entry.Condition[charCode] | (1 << num);
                        }
                    }

                    end = false;
                    num++;
                }
            }

            entry.ConditionCount = num;
            return;

        }

        public static string RemovePrefix(string word, AffixEntry entry)
        {

            int tempLength = word.Length - entry.AddCharacters.Length;
            if ((tempLength > 0)
                && (tempLength + entry.StripCharacters.Length >= entry.ConditionCount)
                && (word.StartsWith(entry.AddCharacters)))
            {
                string tempWord = word.Substring(entry.AddCharacters.Length);
                tempWord = entry.StripCharacters + tempWord;
                int passCount = 0;
                for (int i = 0; i < entry.ConditionCount; i++)
                {
                    int charCode = (int)tempWord[i];
                    if ((entry.Condition[charCode] & (1 << i)) == (1 << i))
                    {
                        passCount++;
                    }
                }
                if (passCount == entry.ConditionCount)
                {
                    return tempWord;
                }

            }
            return word;
        }

        public static string RemoveSuffix(string word, AffixEntry entry)
        {
            int tempLength = word.Length - entry.AddCharacters.Length;
            if ((tempLength > 0)
                && (tempLength + entry.StripCharacters.Length >= entry.ConditionCount)
                && (word.EndsWith(entry.AddCharacters)))
            {
                string tempWord = word.Substring(0, tempLength);
                tempWord += entry.StripCharacters;
                int passCount = 0;
                for (int i = 0; i < entry.ConditionCount; i++)
                {
                    int charCode = (int)tempWord[tempWord.Length - (entry.ConditionCount - i)];
                    if (charCode < entry.Condition.Length && (entry.Condition[charCode] & (1 << i)) == (1 << i))
                    {
                        passCount++;
                    }
                }
                if (passCount == entry.ConditionCount)
                {
                    return tempWord;
                }
            }
            return word;
        }

        #endregion
    }
}
