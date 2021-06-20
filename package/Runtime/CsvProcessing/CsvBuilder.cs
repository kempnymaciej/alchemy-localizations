using System.Collections.Generic;
using System.Text;

namespace AlchemyBow.Localizations.CsvProcessing
{
    public sealed class CsvBuilder
    {
        private const string Quote = "\"";
        private const string DoubleQuote = Quote + Quote;
        private const string Delimiter = ",";
        private const string Empty = "";
        private const char EndLine = '\n';

        private readonly StringBuilder builder;

        public CsvBuilder()
        {
            this.builder = new StringBuilder();
        }

        /// <summary>
        /// Adds another row of data.
        /// </summary>
        /// <param name="row">The row of data to add.</param>
        public void AddRow(IReadOnlyList<string> row)
        {
            if(row == null || row.Count == 0)
            {
                throw new System.ArgumentException("The row list cannot be null or empty.", "row");
            }

            int lastCellIndex = row.Count - 1;
            for (int i = 0; i < lastCellIndex; i++)
            {
                builder.Append($"{FormatCell(row[i])},");
            }
            builder.Append($"{FormatCell(row[lastCellIndex])}{EndLine}");
        }

        /// <summary>
        /// Removes all added rows.
        /// </summary>
        public void Clear() => builder.Clear();

        /// <summary>
        /// Converts the value of a builder to string.
        /// </summary>
        /// <returns>A formatted representation of added rows.</returns>
        public override string ToString()
        {
            string result;
            if(builder.Length > 0)
            {
                builder.Length -= 1;
                result = builder.ToString();
                builder.Append(EndLine);
            }
            else
            {
                result = builder.ToString();
            }
            return result;
        }

        private static string FormatCell(string cell)
        {
            if(cell == null)
            {
                return Empty;
            }

            bool quote = false;
            var endLine = EndLine.ToString();
            if (cell.Contains(Delimiter) || cell.Contains(endLine))
            {
                quote = true;
            }
            if (cell.Contains(Quote))
            {
                quote = true;
                cell = cell.Replace(Quote, DoubleQuote);
            }

            if (quote)
            {
                cell = $"{Quote}{cell}{Quote}";
            }
            return cell;
        }
    } 
}
