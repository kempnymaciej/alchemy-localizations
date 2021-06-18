using AlchemyBow.Localizations.CsvProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AlchemyBow.Localizations
{
    /// <summary>
    /// A handle for the loading process.
    /// </summary>
    public sealed class LocalizatorRequest
    {
        private const string ResourcesFilePath = "AlchemyLocalizations/localizations";

        private readonly LocalizatorRequestSettings settings;
        private readonly string language;
        private readonly string[][] localizations;
        private readonly Dictionary<string, int> stringMap;

        private readonly Action successfullyCompleded;

        private LocalizatorRequest(LocalizatorRequestSettings settings, string language,
            string[][] localizations, Dictionary<string, int> stringMap, Action successfullyCompleded)
        {
            this.settings = settings;
            this.language = language;
            this.localizations = localizations;
            this.stringMap = stringMap;
            this.successfullyCompleded = successfullyCompleded;
        }


        /// <summary>
        /// Determines whether the request processing is complete.
        /// </summary>
        /// <returns>true if the request processing is complete; otherwise, false.</returns>
        public bool Finished { get; private set; }

        /// <summary>
        /// Determines whether the request was successfully processed.
        /// </summary>
        /// <returns>true if the request processing was successfully processed; otherwise, false.</returns>
        public bool Success { get; private set; }

        /// <summary>
        /// Schedules the specified language to load asynchronously and returns a handle for the process.
        /// </summary>
        /// <param name="languageIndex">An index of the language to load.</param>
        /// <param name="settings">A settings object that defines what and how to load.</param>
        /// <param name="localizations">A load destination.</param>
        /// <param name="stringMap">A load destination.</param>
        /// <param name="successfullyCompleded">An action to invoke when the process is successfully completed.</param>
        /// <returns>A handle for the loading process.</returns>
        public static LocalizatorRequest RequestLocalizations(int languageIndex, LocalizatorRequestSettings settings,
            string[][] localizations, Dictionary<string,int> stringMap, Action successfullyCompleded)
        {
            string language = settings.config.SupportedLanguages[languageIndex];
            var request = new LocalizatorRequest(settings, language, localizations, stringMap, successfullyCompleded);
            if (!settings.config.IsLanguageSupported(language))
            {
                request.Success = false;
                request.Finished = true;
            }
            else
            {
                var resourceRequest = Resources.LoadAsync<TextAsset>(ResourcesFilePath);
                resourceRequest.completed += (a) => request.OnResourcesLoadDone(resourceRequest.asset);
            }

            return request;
        }

        private void OnResourcesLoadDone(UnityEngine.Object asset)
        {
            if (asset != null)
            {
                var textAsset = asset as TextAsset;
                var localizationsFileContent = textAsset.text;
                Resources.UnloadAsset(asset);
                LoadLanguage(localizationsFileContent);

                Success = true;
                Finished = true;
                successfullyCompleded?.Invoke();
            }
            else
            {
                Success = true;
                Finished = false;
            }
        }

        private void LoadLanguage(string localizationsFileContent)
        {
            using (var reader = new StringReader(localizationsFileContent))
            {
                var csvReader = new CsvReader();
                int columnIndex = settings.config.DetermineFileColumnIndex(language);
                int numberOfGroups = settings.config.NumberOfGroups;
                for (int i = 0; i < numberOfGroups; i++)
                {
                    if (settings.ShouldLoad(i))
                    {
                        if (settings.ShouldStringMap(i))
                        {
                            LoadAndStringMapGroup(columnIndex, i, csvReader, reader);
                        }
                        else
                        {
                            LoadGroup(columnIndex, i, csvReader, reader);
                        }
                    }
                    else
                    {
                        SkipGroup(i, csvReader, reader);
                    }
                }
            }
        }

        private void LoadAndStringMapGroup(int columnIndex, int groupIndex, CsvReader csvReader, StringReader reader)
        {
            int groupSize = settings.config.GetGroupSize(groupIndex);
            localizations[groupIndex] = new string[groupSize];
            string groupName = settings.config.GetGroupName(groupIndex);
            int groupFirstIndex = settings.config.GetGroupFirstIndex(groupIndex);
            var row = new List<string>();
            for (int j = 0; j < groupSize; j++)
            {
                row.Clear();
                if (csvReader.ReadRow(row, reader) > columnIndex)
                {
                    localizations[groupIndex][j] = row[columnIndex];
                    stringMap.Add($"{groupName}.{row[0]}", groupFirstIndex + j);
                }
                else
                {
                    localizations[groupIndex][j] = $"NOT_TRANSLATED_[{groupIndex}][{j}]";
                    Debug.LogWarning($"No translations for the key[{groupIndex}][{j}].");
                }
            }
        }
        private void LoadGroup(int columnIndex, int groupIndex, CsvReader csvReader, StringReader reader)
        {
            int groupSize = settings.config.GetGroupSize(groupIndex);
            localizations[groupIndex] = new string[groupSize];
            for (int j = 0; j < groupSize; j++)
            {
                string translation = csvReader.ReadRow(reader, columnIndex);
                if (translation != null)
                {
                    localizations[groupIndex][j] = translation;
                }
                else
                {
                    localizations[groupIndex][j] = $"NOT_TRANSLATED_[{groupIndex}][{j}]";
                    Debug.LogWarning($"No translations for the key[{groupIndex}][{j}].");
                }
            }
        }
        private void SkipGroup(int groupIndex, CsvReader csvReader, StringReader reader)
        {
            int groupSize = settings.config.GetGroupSize(groupIndex);
            localizations[groupIndex] = null;
            for (int j = 0; j < groupSize; j++)
            {
                csvReader.ReadRow(reader, 0);
            }
        }
    }
}
