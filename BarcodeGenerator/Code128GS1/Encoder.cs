using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeGenerator.Code128GS1
{
    internal enum CodeSet
    {
        None,
        CodeA,
        CodeB,
        CodeC
    }

    internal enum CodeFunction
    {
        FNC1,
        FNC2,
        FNC3,
        FNC4,
        CodeA,
        CodeB,
        CodeC,
        ShiftA,
        ShiftB,
        StartA,
        StartB,
        StartC,
        Stop
    }

    internal class CodeSetEntry
    {
        #region Private fields

        private CodeSet codeSet;
        private List<int> values = new List<int>();
        private bool setFNC1 = false;

        #endregion

        #region Public properties

        public List<int> Values
        {
            get { return this.values; }
        }

        public CodeSet CodeSet
        {
            get { return this.codeSet; }
            set { this.codeSet = value; }
        }

        public bool SetFNC1
        {
            get { return this.setFNC1; }
        }

        #endregion

        #region Constructor(s)
        
        public CodeSetEntry(CodeSet codeSet, bool setFNC1)
        {
            this.codeSet = codeSet;
            this.setFNC1 = setFNC1;
        }

        #endregion
    }

    public class Encoder
    {
        #region Constants

        private const string CODE_A_VALID_CHARS = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string CODE_B_VALID_CHARS = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ'abcdefghijklmnopqrstuvwxyz{|}~";
        private const string CODE_C_VALID_CHARS = "0123456789";

        #endregion

        #region Fields

        private Dictionary<CodeSet, Dictionary<CodeFunction, int>> codeSetToFunctionMap = 
            new Dictionary<CodeSet, Dictionary<CodeFunction, int>>() 
            {
                { CodeSet.CodeA, new Dictionary<CodeFunction, int>() {
                        { CodeFunction.FNC3, 96 },
                        { CodeFunction.FNC2, 97 },
                        { CodeFunction.ShiftB, 98 },
                        { CodeFunction.CodeC, 99 },
                        { CodeFunction.CodeB, 100 },
                        { CodeFunction.FNC4, 101 },
                        { CodeFunction.FNC1, 102 },
                        { CodeFunction.StartA, 103 },
                        { CodeFunction.StartB, 104 },
                        { CodeFunction.StartC, 105 },
                        { CodeFunction.Stop, 106 } } },
                { CodeSet.CodeB, new Dictionary<CodeFunction, int>() {
                        { CodeFunction.FNC3, 96 },
                        { CodeFunction.FNC2, 97 },
                        { CodeFunction.ShiftA, 98 },
                        { CodeFunction.CodeC, 99 },
                        { CodeFunction.FNC4, 100 },
                        { CodeFunction.CodeA, 101 },
                        { CodeFunction.FNC1, 102 },
                        { CodeFunction.StartA, 103 },
                        { CodeFunction.StartB, 104 },
                        { CodeFunction.StartC, 105 },
                        { CodeFunction.Stop, 106 } } },
                { CodeSet.CodeC, new Dictionary<CodeFunction, int>() {
                        { CodeFunction.CodeB, 100 },
                        { CodeFunction.CodeA, 101 },
                        { CodeFunction.FNC1, 102 },
                        { CodeFunction.StartA, 103 },
                        { CodeFunction.StartB, 104 },
                        { CodeFunction.StartC, 105 },
                        { CodeFunction.Stop, 106 } } }
            };

        #endregion

        #region Private methods

        
        private int[] GenerateBarcodeValues(List<CodeSetEntry> codeSetEntries)
        {
            List<int> values = new List<int>();
            CodeSet currentCodeSet = CodeSet.None;

            for(int i = 0; i < codeSetEntries.Count; i++)
            {
                CodeSetEntry entry = codeSetEntries[i];

                if (entry.CodeSet != currentCodeSet)
                {
                    if(i == 0)
                    {
                        CodeFunction codeFunction = 
                            entry.CodeSet == CodeSet.CodeA ? 
                                CodeFunction.StartA :
                                entry.CodeSet == CodeSet.CodeB ? 
                                    CodeFunction.StartB : 
                                    CodeFunction.StartC;
                        values.Add(codeSetToFunctionMap[entry.CodeSet][codeFunction]);
                    }
                    else
                    {
                        CodeFunction codeFunction = 
                            entry.CodeSet == CodeSet.CodeA ? 
                                CodeFunction.CodeA :
                                entry.CodeSet == CodeSet.CodeB ? 
                                    CodeFunction.CodeB : 
                                    CodeFunction.CodeC;
                        values.Add(codeSetToFunctionMap[currentCodeSet][codeFunction]);
                    }
                    currentCodeSet = entry.CodeSet;
                }
                if (entry.SetFNC1)
                {
                    values.Add(codeSetToFunctionMap[currentCodeSet][CodeFunction.FNC1]);
                }
                if (entry.CodeSet == CodeSet.CodeC)
                {
                    for (int j = 0; j < entry.Values.Count; j += 2)
                    {
                        int value = int.Parse(string.Format(
                            "{0}{1}",
                            Convert.ToChar(entry.Values[j]),
                            Convert.ToChar(entry.Values[j + 1])));
                        values.Add(value);
                    }
                }
                else
                {
                    for (int j = 0; j < entry.Values.Count; j++)
                    {
                        values.Add(GetCodeValueForChar(entry.Values[j]));
                    }
                }
            }
            values.Add(CalculateChecksum(values));
            values.Add(codeSetToFunctionMap[CodeSet.CodeA][CodeFunction.Stop]);
            return values.ToArray();
        }

        internal int CalculateChecksum(List<int> values)
        {
            int sum = values[0];
            for (int i = 1; i < values.Count; i++)
            {
                sum += (i * values[i]);
            }
            return sum % 103;
        }

        private List<CodeSetEntry> OptimizeCodeSetEntries(List<CodeSetEntry> codeSetEntries)
        {
            List<CodeSetEntry> optimizedEntryList = new List<CodeSetEntry>();

            for (int i = 0; i < codeSetEntries.Count; i++)
            {
                CodeSetEntry entry = codeSetEntries[i];

                if (entry.CodeSet == CodeSet.CodeC)
                {
                    if (i == 0)
                    {
                        if (entry.Values.Count < 4)
                        {
                            entry.CodeSet = codeSetEntries[i + 1].CodeSet;
                        }
                    }
                    else if (i > 0 && i < codeSetEntries.Count - 1)
                    {
                        if (entry.Values.Count < 6)
                        {
                            entry.CodeSet = codeSetEntries[i - 1].CodeSet;
                        }
                    }
                    else if (i == codeSetEntries.Count - 1)
                    {
                        if (entry.Values.Count < 4)
                        {
                            entry.CodeSet = codeSetEntries[i - 1].CodeSet;
                        }
                    }
                }
                optimizedEntryList.Add(entry);
            }

            return optimizedEntryList;
        }

        private List<CodeSetEntry> AnalyzeInputString(byte[] asciiBytes)
        {
            List<CodeSetEntry> codeSetEntries = new List<CodeSetEntry>();
            int index = 0;
            CodeSet currentCodeSet = CodeSet.None;
            CodeSetEntry entry = null;
            bool setFNC1 = false;
            do
            {
                char currentChar = Convert.ToChar(asciiBytes[index]);
                if (currentChar == '(')
                {
                    index++;
                    setFNC1 = true;
                    continue;
                }
                char nextChar = index + 1 < asciiBytes.Length ? Convert.ToChar(asciiBytes[index + 1]) : '^';
                if (CODE_C_VALID_CHARS.Contains(currentChar) &&
                    CODE_C_VALID_CHARS.Contains(nextChar))
                {
                    if (currentCodeSet == CodeSet.CodeC && !setFNC1)
                    {
                        entry = codeSetEntries[codeSetEntries.Count - 1];
                    }
                    else
                    {
                        entry = new CodeSetEntry(CodeSet.CodeC, setFNC1);
                        codeSetEntries.Add(entry);
                        currentCodeSet = CodeSet.CodeC;
                    }
                    entry.Values.Add(asciiBytes[index++]);
                    entry.Values.Add(asciiBytes[index++]);
                }
                else if (CODE_A_VALID_CHARS.Contains(currentChar))
                {
                    if (currentCodeSet == CodeSet.CodeA && !setFNC1)
                    {
                        entry = codeSetEntries[codeSetEntries.Count - 1];
                    }
                    else
                    {
                        entry = new CodeSetEntry(CodeSet.CodeA, setFNC1);
                        codeSetEntries.Add(entry);
                        currentCodeSet = CodeSet.CodeA;
                    }
                    entry.Values.Add(asciiBytes[index++]);
                }
                else if (CODE_B_VALID_CHARS.Contains(currentChar))
                {
                    if (currentCodeSet == CodeSet.CodeB && !setFNC1)
                    {
                        entry = codeSetEntries[codeSetEntries.Count - 1];
                    }
                    else
                    {
                        entry = new CodeSetEntry(CodeSet.CodeB, setFNC1);
                        codeSetEntries.Add(entry);
                        currentCodeSet = CodeSet.CodeB;
                    }
                    entry.Values.Add(asciiBytes[index++]);
                }
                else
                {
                    // TODO: Implement exception handling!
                }
                setFNC1 = false;
            } while (index < asciiBytes.Length);
            
            return codeSetEntries;
        }

        internal int GetCodeValueForChar(int asciiCode)
        {
            return (asciiCode >= 32) ? asciiCode - 32 : asciiCode + 64;
        }

        #endregion

        #region Public methods

        public int[] Encode(string value)
        {
            byte[] asciiBytes = Encoding.ASCII.GetBytes(value.Replace(")",""));

            List<CodeSetEntry> codeSetEntries = AnalyzeInputString(asciiBytes);
            codeSetEntries = OptimizeCodeSetEntries(codeSetEntries);
            int[] values = GenerateBarcodeValues(codeSetEntries);

            return values;
        }

        #endregion
    }
}
