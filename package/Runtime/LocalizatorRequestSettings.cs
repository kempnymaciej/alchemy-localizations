using System.Collections.Generic;

namespace AlchemyBow.Localizations
{
    /// <summary>
    /// Defines what and how to load throw a localizator request.
    /// </summary>
    public class LocalizatorRequestSettings
    {
        /// <summary>
        /// Configuration used.
        /// </summary>
        public readonly LocalizatorConfig config;
        private readonly HashSet<int> loadGroups;
        private readonly HashSet<int> stringMapGroups;

        /// <summary>
        /// Creates a new instance of the LocalizatorRequestSettings class.
        /// </summary>
        /// <param name="localizator">The target localizator.</param>
        public LocalizatorRequestSettings(Localizator localizator)
        {
            this.config = localizator.config;
            this.loadGroups = new HashSet<int>();
            this.stringMapGroups = new HashSet<int>();
        }

        /// <summary>
        /// Specifies that the group will be loaded.
        /// </summary>
        /// <param name="groups">An index of the group.</param>
        public void AddGroupsToLoad(params int[] groups)
        {
            if(groups != null)
            {
                foreach (var group in groups)
                {
                    loadGroups.Add(group);
                }
            }
        }

        /// <summary>
        /// Specifies that all groups will be loaded.
        /// </summary>
        public void AddAllGroupsToLoad()
        {
            int numberOfGroups = config.NumberOfGroups;
            for (int i = 0; i < numberOfGroups; i++)
            {
                loadGroups.Add(i);
            }
        }

        /// <summary>
        /// Specifies that the group will allow string-key access.
        /// </summary>
        /// <param name="groups">An index of the group.</param>
        public void AddGroupsToStringMap(params int[] groups)
        {
            if (groups != null)
            {
                foreach (var group in groups)
                {
                    stringMapGroups.Add(group);
                }
            }
        }

        /// <summary>
        /// Specifies that all group will allow string-key access.
        /// </summary>
        public void AddAllGroupsToStringMap()
        {
            int numberOfGroups = config.NumberOfGroups;
            for (int i = 0; i < numberOfGroups; i++)
            {
                stringMapGroups.Add(i);
            }
        }

        /// <summary>
        /// Determines wheter the specific group should be loaded.
        /// </summary>
        /// <param name="groupIndex"></param>
        /// <returns>true if the group should be loaded; otherwise, false.</returns>
        public bool ShouldLoad(int groupIndex) => loadGroups.Contains(groupIndex);

        /// <summary>
        /// Determines wheter the specific group allows string-key access.
        /// </summary>
        /// <param name="groupIndex"></param>
        /// <returns>true if the group allows string-key access; otherwise, false.</returns>
        public bool ShouldStringMap(int groupIndex) => stringMapGroups.Contains(groupIndex);
    } 
}
