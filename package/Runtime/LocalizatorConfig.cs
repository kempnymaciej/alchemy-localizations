using System.Collections.Generic;
using UnityEngine;

namespace AlchemyBow.Localizations
{
    /// <summary>
    /// Describes a configuration for Localizator class.
    /// </summary>
    public sealed class LocalizatorConfig 
    {
        public const int DefaultLanguage = 0;
        private readonly int numberOfGroups;
        private readonly int[] groupSizes;
        private readonly string[] groupNames;
        private readonly string[] languages;

        /// <summary>
        /// Creates a new instance of the LocalizatorConfig class.
        /// </summary>
        /// <param name="languages">The supported languages.</param>
        /// <param name="numberOfGroups">The number of groups.</param>
        /// <param name="groupSizes">The sizes of groups.</param>
        /// <param name="groupNames">The names of groups.</param>
        /// <remarks>Do not create this object yourself. Use the baked class method. For example: <code>YourClassName.GetLocalizatorConfig()</code></remarks>
        public LocalizatorConfig(string[] languages, 
            int numberOfGroups, int[] groupSizes, string[] groupNames)
        {
            this.languages = languages;
            this.numberOfGroups = numberOfGroups;
            this.groupSizes = groupSizes;
            this.groupNames = groupNames;
        }

        /// <summary>
        /// A list of supported languages.
        /// </summary>
        /// <returns>A list of supported languages.</returns>
        public IReadOnlyList<string> SupportedLanguages => languages;

        /// <summary>
        /// A number of groups.
        /// </summary>
        /// <returns>A number of groups.</returns>
        public int NumberOfGroups => numberOfGroups;

        /// <summary>
        /// Gets a group size.
        /// </summary>
        /// <param name="index">An index of the group.</param>
        /// <returns>A group size.</returns>
        public int GetGroupSize(int index) => groupSizes[index];
        /// <summary>
        /// Gets a group name.
        /// </summary>
        /// <param name="index">An index of the group.</param>
        /// <returns>A group name.</returns>
        public string GetGroupName(int index) => groupNames[index];

        /// <summary>
        /// Gets a total number of keys.
        /// </summary>
        /// <returns>A total number of keys.</returns>
        public int GetGlobalNumberOfKeys()
        {
            int result = 0;
            foreach (var groupSize in groupSizes)
            {
                result += groupSize;
            }
            return result;
        }

        /// <summary>
        /// Determines whether the language is supported.
        /// </summary>
        /// <param name="language">A language.</param>
        /// <returns>true if the language is supported; otherwise, false.</returns>
        public bool IsLanguageSupported(string language)
        {
            foreach (var supportedLanguage in SupportedLanguages)
            {
                if (supportedLanguage == language)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Determines whether the language is supported.
        /// </summary>
        /// <param name="language">A language.</param>
        /// <returns>true if the language is supported; otherwise, false.</returns>
        /// <remarks>Uses ToString() method on the language.</remarks>
        public bool IsLanguageSupported(SystemLanguage language)
        {
            return IsLanguageSupported(language.ToString());
        }

        /// <summary>
        /// Attempts to find an index od the language.
        /// </summary>
        /// <param name="language">A language.</param>
        /// <param name="result">The result index.</param>
        /// <returns>true if the language is supported; otherwise, false.</returns>
        public bool SystemLanguageToSupportedLanguageIndex(SystemLanguage language, out int result)
        {
            string actualLanguage = language.ToString();
            int numberOfSupportedLanguages = SupportedLanguages.Count;
            for (int i = 0; i < numberOfSupportedLanguages; i++)
            {
                if(actualLanguage == SupportedLanguages[i])
                {
                    result = i;
                    return true;
                }
            }
            result = -1;
            return false;
        }

        /// <summary>
        /// Determines an index of the column that holds a specyfic language.
        /// </summary>
        /// <param name="language">A language.</param>
        /// <returns>The index of the column if the language is supported; otherwise, 0 (default).</returns>
        public int DetermineFileColumnIndex(string language)
        {
            int numberOfLanguages = SupportedLanguages.Count;
            for (int i = 0; i < numberOfLanguages; i++)
            {
                if(SupportedLanguages[i] == language)
                {
                    return i + 1;
                }
            }
            return 0; //returns the index of the key column if a language is not supported
        }
    } 
}
