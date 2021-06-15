using System.Collections.Generic;

namespace AlchemyBow.Localizations
{
    /// <summary>
    /// Localizations access unit.
    /// </summary>
    public sealed class Localizator
    {
        /// <summary>
        /// The delegate type used by the Localizator events.
        /// </summary>
        /// <param name="localizator">An instance of Localizator that raised an event.</param>
        public delegate void LocalizationsEventHandler(Localizator localizator);
        /// <summary>
        /// Raised whenever a valid language is set.
        /// </summary>
        public event LocalizationsEventHandler ValidLocalizationsSet;

        /// <summary>
        /// A configuration used by an instance of the Localizator class.
        /// </summary>
        public readonly LocalizatorConfig config;
        private readonly string[][] localizations;

        private int activeLanguage;

        /// <summary>
        /// Creates a new instance of the Localizator class.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        public Localizator(LocalizatorConfig config)
        {
            this.config = config;
            this.localizations = new string[config.NumberOfGroups][];
            this.activeLanguage = -1;
        }

        /// <summary>
        /// The index of the currently used language.
        /// </summary>
        /// <returns>The index of the active language if it is loaded; otherwise, -1.</returns>
        public int ActiveLanguage 
        { 
            get => activeLanguage; 
            private set
            {
                activeLanguage = value;
                OnValidLocalizationsSet();
            } 
        }

        /// <summary>
        /// Gets a localization under the specyfic key.
        /// </summary>
        /// <param name="key">The key of localization.</param>
        /// <returns>A localization under the specyfic key.</returns>
        public string this[int key]
        {
            get
            {
                int orginalKey = key;
                int numberOfGroups = config.NumberOfGroups;
                for (int i = 0; i < numberOfGroups; i++)
                {
                    int groupSize = config.GetGroupSize(i);
                    if (key < groupSize)
                    {
                        return localizations[i][key];
                    }
                    else
                    {
                        key -= groupSize;
                    }
                }
                throw new System.Exception($"The key({orginalKey}) is out of range.");
            }
        }

        /// <summary>
        /// Begins the process of loading the specified language and returns a handle.
        /// </summary>
        /// <param name="languageIndex">An index of the language to load.</param>
        /// <returns>A handle of the loading process.</returns>
        /// <remarks>Loads all groups.</remarks>
        public LocalizatorRequest SetLanguage(int languageIndex)
        {
            Clear();
            ThrowIfLanguageNotSupported(languageIndex);
            var groups = new HashSet<int>();
            int numberOfGroups = config.NumberOfGroups;
            for (int i = 0; i < numberOfGroups; i++)
            {
                groups.Add(i);
            }
            
            return LocalizatorRequest.RequestLocalizations(languageIndex, groups, config, localizations,
                () => ActiveLanguage = languageIndex);
        }

        /// <summary>
        /// Begins the process of loading the specified language and returns a handle.
        /// </summary>
        /// <param name="languageIndex">An index of the language to load.</param>
        /// <param name="groups">Indexes of the groups to load.</param>
        /// <returns>A handle of the loading process.</returns>
        /// <remarks>Loads specific groups only.</remarks>
        public LocalizatorRequest SetLanguageWithExactGroups(int languageIndex, params int[] groups)
        {
            Clear();
            ThrowIfLanguageNotSupported(languageIndex);
            ThrowIfGroupNotSpecyfied(groups);
            string language = config.SupportedLanguages[languageIndex];
            return LocalizatorRequest.RequestLocalizations(languageIndex, new HashSet<int>(groups), config, localizations, 
                () => ActiveLanguage = languageIndex);
        }

        /// <summary>
        /// Clears the data of a loaded language.
        /// </summary>
        /// <remarks>An event is not raised.</remarks>
        public void Clear()
        {
            activeLanguage = -1;
            int numberOfGroups = config.NumberOfGroups;
            for (int i = 0; i < numberOfGroups; i++)
            {
                localizations[i] = null;
            }
        }

        private void OnValidLocalizationsSet()
        {
            var handler = ValidLocalizationsSet;
            handler?.Invoke(this);
        }

        #region Exceptions
        private void ThrowIfLanguageNotSupported(int languageIndex)
        {
            if (languageIndex < 0 || languageIndex >= config.SupportedLanguages.Count)
            {
                throw new System.Exception($"The language index '{languageIndex}' is not supported.");
            }
        }
        private void ThrowIfGroupNotSpecyfied(int[] groups)
        {
            if (groups == null || groups.Length == 0)
            {
                throw new System.Exception("Groups must be specified.");
            }
        } 
        #endregion
    } 
}
