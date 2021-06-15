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

        private readonly LocalizatorConfig config;
        private readonly string language;
        private readonly string[][] localizations;
        private readonly HashSet<int> groups;

        private readonly Action successfullyCompleded;

        private LocalizatorRequest(LocalizatorConfig config, string language, HashSet<int> groups,
            string[][] localizations, Action successfullyCompleded)
        {
            this.config = config;
            this.language = language;
            this.localizations = localizations;
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
        /// <param name="groups">Indexes of the groups to load.</param>
        /// <param name="config">A configuration of the Localizator.</param>
        /// <param name="localizations">A load destination</param>
        /// <param name="successfullyCompleded">An action to invoke when the process is successfully completed.</param>
        /// <returns>A handle for the loading process.</returns>
        public static LocalizatorRequest RequestLocalizations(int languageIndex, HashSet<int> groups, 
            LocalizatorConfig config, string[][] localizations, Action successfullyCompleded)
        {
            string language = config.SupportedLanguages[languageIndex];
            var request = new LocalizatorRequest(config, language, groups, localizations, successfullyCompleded);
            if (!config.IsLanguageSupported(language))
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
                int columnIndex = config.DetermineFileColumnIndex(language);
                int numberOfGroups = config.NumberOfGroups;
                for (int i = 0; i < numberOfGroups; i++)
                {
                    int groupSize = config.GetGroupSize(i);
                    if (groups == null || groups.Contains(i))
                    {
                        localizations[i] = new string[groupSize];
                        for (int j = 0; j < groupSize; j++)
                        {
                            string translation = csvReader.ReadRow(reader, columnIndex);
                            if (translation != null)
                            {
                                localizations[i][j] = translation;
                            }
                            else
                            {
                                localizations[i][j] = $"NOT_TRANSLATED_[{i}][{j}]";
                                Debug.LogWarning($"No translations for the key[{i}][{j}].");
                            }
                        }
                    }
                    else
                    {
                        localizations[i] = null;
                        for (int j = 0; j < groupSize; j++)
                        {
                            csvReader.ReadRow(reader, columnIndex);
                        }
                    }
                }
            }
        }
    }
}
