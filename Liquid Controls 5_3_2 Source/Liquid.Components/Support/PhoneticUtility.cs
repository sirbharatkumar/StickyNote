using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Liquid.Components.Internal
{
    internal partial class PhoneticUtility
    {
        #region Constructor

        private PhoneticUtility()
        {
        }

        #endregion

        #region Public Static Methods

        public static void EncodeRule(string ruleText, PhoneticRule rule)
        {
            for (int i = 0; i < rule.Condition.Length; i++)
            {
                rule.Condition[i] = 0;
            }

            bool group = false;
            bool end = false;

            char[] memberChars = new char[200];
            int numMember = 0;

            foreach (char cond in ruleText)
            {
                switch (cond)
                {
                    case '(':
                        group = true;
                        break;
                    case ')':
                        end = true;
                        break;
                    case '^':
                        rule.BeginningOnly = true;
                        break;
                    case '$':
                        rule.EndOnly = true;
                        break;
                    case '-':
                        rule.ConsumeCount++;
                        break;
                    case '<':
                        rule.ReplaceMode = true;
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        rule.Priority = int.Parse(cond.ToString());
                        break;
                    default:
                        if (group)
                        {
                            memberChars[numMember] = cond;
                            numMember++;
                        }
                        else
                        {
                            end = true;
                        }
                        break;
                }

                if (end)
                {
                    if (group)
                    {
                        for (int j = 0; j < numMember; j++)
                        {
                            int charCode = (int)memberChars[j];
                            rule.Condition[charCode] = rule.Condition[charCode] | (1 << rule.ConditionCount);
                        }

                        group = false;
                        numMember = 0;
                    }
                    else
                    {
                        int charCode = (int)cond;
                        rule.Condition[charCode] = rule.Condition[charCode] | (1 << rule.ConditionCount);
                    }
                    end = false;
                    rule.ConditionCount++;
                }
            }
        }

        #endregion
    }
}
