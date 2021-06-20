using AlchemyBow.Localizations.CsvProcessing;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AlchemyBow.Localizations.Editor.Utilities
{
    public sealed class KeyGroup
    {
        public readonly string groupName;
        private readonly List<string[]> rows;

        private KeyGroup(string groupName)
        {
            this.groupName = groupName;
            this.rows = new List<string[]>();
        }

        public int NumberOfKeys => rows.Count;
        public string GetKey(int index) => rows[index][0];
        public IReadOnlyList<string> GetRow(int index) => rows[index];
        

        public static KeyGroup FromSheetContent(string groupName, string sheetContent, LocalizationsSettings settings)
        {
            var group = new KeyGroup(groupName);
            using (var reader = new StringReader(sheetContent))
            {
                var row = new List<string>();
                var csvReader = new CsvReader();
                csvReader.ReadRow(row, reader);
                var languageMap = group.MapLanguages(row.ToArray(), settings);

                row.Clear();
                while (csvReader.ReadRow(row, reader) > 0 && !string.IsNullOrWhiteSpace(row[0]))
                {
                    group.rows.Add(SheetRowToGroupRow(row.ToArray(), languageMap));
                    row.Clear();
                }
            }

            group.ThrowIfInvalidKeys();
            return group;
        }

        private int[] MapLanguages(string[] headRow, LocalizationsSettings settings)
        {
            var languages = settings.Languages;
            int numberOfLanguages = languages.Count;
            var result = new int[numberOfLanguages];
            for (int i = 0; i < numberOfLanguages; i++)
            {
                result[i] = FindIndexOfColumn(headRow, languages[i]);
            }
            return result;
        }

        private int FindIndexOfColumn(string[] headRow, string target)
        {
            for (int i = 0; i < headRow.Length; i++)
            {
                if(headRow[i] == target)
                {
                    return i;
                }
            }

            throw new System.Exception($"There is no '{target}' column in the group '{groupName}'");
        }

        private static string[] SheetRowToGroupRow(string[] sheetRow, int[] languageMap)
        {
            int numberOfLanguages = languageMap.Length;
            var result = new string[numberOfLanguages + 1];
            result[0] = sheetRow[0];
            for (int i = 0; i < numberOfLanguages; i++)
            {
                result[i + 1] = sheetRow[languageMap[i]];
            }
            return result;
        }

        private void ThrowIfInvalidKeys()
        {
            int numberOfKeys = NumberOfKeys;
            if (numberOfKeys == 0)
            {
                throw new System.Exception($"The group '{groupName}' has no keys.");
            }

            var regex = new Regex(@"^[A-Z][a-zA-Z0-9_]{0,}");
            var uniqueKeys = new HashSet<string>();
            
            for (int i = 0; i < numberOfKeys; i++)
            {
                var key = GetKey(i);
                var match = regex.Match(key);
                if(!match.Success || match.Length != key.Length)
                {
                    throw new System.Exception($"The key '{key}'({i}) in the group '{groupName}' is not valid. The key cannot be empty, must start with a capital letter and contain only [a-z, A-Z, 0-9, '_'] characters.");
                }
                if (!uniqueKeys.Add(key))
                {
                    throw new System.Exception($"The key '{key}' in the group '{groupName}' is not unique.");
                }
            }
        }
    } 
}
