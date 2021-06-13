using System.Collections.Generic;

namespace AlchemyBow.Localizations.Editor.Utilities
{
    public class KeyGroup
    {
        private readonly string groupName;
        private readonly string[] groupKeys;

        public KeyGroup(string groupName, string[] groupKeys)
        {
            this.groupName = groupName;
            this.groupKeys = groupKeys;
        }

        public string GroupName => groupName;

        public IReadOnlyList<string> GroupKeys => groupKeys;
    } 
}
