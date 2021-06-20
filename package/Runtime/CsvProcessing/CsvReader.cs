using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AlchemyBow.Localizations.CsvProcessing
{
    public sealed class CsvReader
    {
        private const char Quote = '"';
        private const char Delimiter = ',';
        private const string Empty = "";


        private readonly Regex delimiterRegex;
        private readonly Regex oddQuotesRegex;

        public CsvReader()
        {
            delimiterRegex = new Regex($"{Delimiter}");
            oddQuotesRegex = new Regex($"(?<!{Quote}){Quote}({Quote}{Quote})*(?!{Quote})");
        }

        /// <summary>
        /// Reads a row of data from the stream and adds it to the row list.
        /// </summary>
        /// <param name="row">The list to store results.</param>
        /// <param name="reader">The source stream.</param>
        /// <returns>The number of added cells.</returns>
        /// <remarks>The row list is not cleared.</remarks>
        public int ReadRow(List<string> row, StringReader reader)
        {
            int readCells = 0;
            if (reader.Peek() != -1)
            {
                string rest = null;
                do
                {
                    row.Add(ReadCell(rest, reader, out rest));
                    readCells++;
                } while (rest != null);
            }

            return readCells;
        }

        /// <summary>
        /// Reads the entire row of data from the stream and returns the value of a specific column.
        /// </summary>
        /// <param name="reader">The source stream.</param>
        /// <param name="columnIndex">The index of the column to read.</param>
        /// <returns>The value of the cell if it exists; otherwise, null.</returns>
        public string ReadRow(StringReader reader, int columnIndex)
        {
            string result = null;
            if (reader.Peek() != -1)
            {
                string rest = null;
                do
                {
                    if(columnIndex == 0)
                    {
                        result = ReadCell(rest, reader, out rest);
                    }
                    else
                    {
                        ReadCell(rest, reader, out rest);
                    }
                    columnIndex--;
                } while (rest != null);
            }

            return result;
        }


        private string ReadCell(string beginning, StringReader reader, out string rest)
        {
            if(beginning == null)
            {
                ThrowIfEmptyReader(reader);
                beginning = reader.ReadLine();
            }

            if(beginning.Length == 0)
            {
                rest = null;
                return "";
            }
            else if(beginning[0] != Quote)
            {
                return ReadCellSimpleCase(beginning, out rest);
            }
            else
            {
                return ReadCellQuotesCase(beginning, reader, out rest);
            }
        }

        private string ReadCellSimpleCase(string beginning, out string rest)
        {
            var match = delimiterRegex.Match(beginning);
            if (match.Success)
            {
                int matchIndex = match.Index;
                if (matchIndex + 1 >= beginning.Length) // there is one empty cell left
                {
                    rest = Empty;
                }
                else // there is at least one cell left
                {
                    rest = beginning.Substring(matchIndex + 1);
                }
                return beginning.Substring(0, matchIndex);
            }
            else
            {
                rest = null;
                return beginning;
            }
        }

        private string ReadCellQuotesCase(string beginning, StringReader reader, out string rest)
        {
            beginning = beginning.Substring(1);
            int startIndex = 0;
            var match = oddQuotesRegex.Match(beginning, startIndex);
            while (!match.Success)
            {
                ThrowIfEmptyReader(reader);
                startIndex = beginning.Length;
                beginning += '\n' + reader.ReadLine();
                match = oddQuotesRegex.Match(beginning, startIndex);
            }

            int cellLength = match.Index + match.Length - 1;
            int othersLength = beginning.Length - cellLength - 1;
            
            if(othersLength <= 0) // there are no more cells left
            {
                rest = null;
            }
            else
            {
                if (othersLength == 1) // there is one empty cell left
                {
                    rest = Empty;
                }
                else // there is at least one non-empty cell left
                {
                    rest = beginning.Substring(cellLength + 2);
                }
            }
            return ReplaceDoubleQuotes(beginning.Substring(0, cellLength));
        }

        private static string ReplaceDoubleQuotes(string orginal)
        {
            const string QuoteString = "\"";
            const string DoubleQuoteString = "\"\"";
            return orginal.Replace(DoubleQuoteString, QuoteString);
        }

        private static void ThrowIfEmptyReader(StringReader reader)
        {
            if (reader.Peek() == -1)
            {
                throw new Exception("Failed to read a cell. The stream is empty.");
            }
        }
    }
}
