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
        private readonly Dictionary<string, int> stringMap;

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
            this.stringMap = new Dictionary<string, int>();
        }

        /// <summary>
        /// Gets the index of the currently used language.
        /// </summary>
        /// <returns>The index of the currently used language if any is loaded; otherwise, -1.</returns>
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
        /// Gets a localization under the specyfic string key.
        /// </summary>
        /// <param name="key">The key of localization. E.g. Group.Key</param>
        /// <returns>A localization under the specyfic key.</returns>
        /// <remarks>This overload only work if the string-key access was anabled during loading.</remarks>
        public string this[string key]
        {
            get
            {
                if(stringMap.TryGetValue(key, out int index))
                {
                    return this[index];
                }
                else
                {
                    return key + " (not string maped)";
                }
            }
        }

        /// <summary>
        /// Begins the process of loading the specified language and returns a handle.
        /// </summary>
        /// <param name="languageIndex">An index of the language to load.</param>
        /// <param name="settings">A settings object that defines what and how to load.</param>
        /// <returns>A handle of the loading process.</returns>
        /// <remarks>If settings object is not passed, all groups are loaded with disabled string-key access.</remarks>
        public LocalizatorRequest SetLanguage(int languageIndex, LocalizatorRequestSettings settings = null)
        {
            Clear();
            ThrowIfLanguageNotSupported(languageIndex);
            
            if(settings == null)
            {
                settings = new LocalizatorRequestSettings(this);
                settings.AddAllGroupsToLoad();
            }
            
            return LocalizatorRequest.RequestLocalizations(languageIndex, settings, 
                localizations, stringMap, () => ActiveLanguage = languageIndex);
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
            stringMap.Clear();
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
        #endregion
    } 
}
