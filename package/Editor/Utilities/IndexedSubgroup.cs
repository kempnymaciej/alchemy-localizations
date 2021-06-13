using System.Collections.Generic;

namespace AlchemyBow.Localizations.Editor.Utilities
{
    public sealed class IndexedSubgroup
    {
        public readonly string subgroupName;
        public readonly List<int> groupIndexes;
        public readonly List<int> subgroupIndexes;

        public IndexedSubgroup(string subgroupName)
        {
            this.subgroupName = subgroupName;
            this.groupIndexes = new List<int>();
            this.subgroupIndexes = new List<int>();
        }

        public void Add(int groupIndex, int subgroupIndex)
        {
            groupIndexes.Add(groupIndex);
            subgroupIndexes.Add(subgroupIndex);
        }
    } 
}
