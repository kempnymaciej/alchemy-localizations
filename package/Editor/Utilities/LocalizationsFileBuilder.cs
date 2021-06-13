using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AlchemyBow.Localizations.Editor.Utilities
{
    public class LocalizationsFileBuilder
    {
        private int indentLevel;
        private string indent;
        private string result;

        private int IndentLevel
        {
            get => indentLevel;
            set
            {
                indentLevel = value >= 0 ? value : 0;
                indent = "";
                for (int i = 0; i < indentLevel; i++)
                {
                    indent += "\t";
                }
            }
        }

        public string Build(LocalizationsSettings settings, IReadOnlyList<KeyGroup> groups)
        {
            result = "";
            Usings();
            BeginNamespace(settings.ClassNamespaceName);
            BeginClass(settings.ClassName);

            ConfigMethod(settings.Languages, groups);

            int groupStartIndex = 0;
            int numberOfGroups = groups.Count;
            for (int i = 0; i < numberOfGroups; i++)
            {
                BeginClass(groups[i].GroupName);
                var keys = groups[i].GroupKeys;
                int numberOfKeys = keys.Count;
                Field("_GroupIndex", i);
                Field("_FirstKey", groupStartIndex);
                Field("_LastKey", groupStartIndex + numberOfKeys - 1);

                for (int j = 0; j < numberOfKeys; j++)
                {
                    Field(keys[j], groupStartIndex + j);
                }
                var indexedSubgroups = FindIndexedSubgroups(keys);
                if (indexedSubgroups.Count > 0)
                {
                    foreach (var indexedSubgroup in indexedSubgroups)
                    {
                        SubgroupMethod(keys, indexedSubgroup);
                    }
                }
                groupStartIndex += numberOfKeys;
                EndClass();
            }

            EndClass();
            EndNamespace();
            return result;
        }

        private List<IndexedSubgroup> FindIndexedSubgroups(IReadOnlyList<string> groupKeys)
        {
            var result = new List<IndexedSubgroup>();
            int numberOfGroupKeys = groupKeys.Count;
            var regex = CreateFindNumberRegex();
            for (int i = 0; i < numberOfGroupKeys; i++)
            {
                if (regex.IsMatch(groupKeys[i]))
                {
                    string subgroupIndex = regex.Match(groupKeys[i]).Value;
                    var subgroupName = regex.Replace(groupKeys[i], "X");
                    var subgroup = result.Find(r => r.subgroupName == subgroupName);
                    if (subgroup == null)
                    {
                        subgroup = new IndexedSubgroup(subgroupName);
                        result.Add(subgroup);
                    }
                    subgroup.Add(i, int.Parse(subgroupIndex));
                }
            }
            return result;
        }

        private static Regex CreateFindNumberRegex() => new Regex(@"\d+");

        private void Line(string value)
        {
            result += $"{indent}{value}\n";
        }

        private void Usings()
        {
            Line("using AlchemyBow.Localizations;");
            Line("");
        }
        private void BeginNamespace(string namespaceName)
        {
            Line($"namespace {namespaceName}");
            Line("{");
            IndentLevel++;
        }
        private void EndNamespace()
        {
            IndentLevel--;
            Line("}");
        }
        private void BeginClass(string className)
        {
            Line($"public static class {className}");
            Line("{");
            IndentLevel++;
        }
        private void EndClass()
        {
            IndentLevel--;
            Line("}");
        }
        private void Field(string fieldName, int value)
        {
            Line($"public const int {fieldName} = {value};");
        }

        private void SubgroupMethod(IReadOnlyList<string> groupKeys, IndexedSubgroup indexedSubgroup)
        {
            Line($"public static int {indexedSubgroup.subgroupName}(int index)");
            Line("{");
            IndentLevel++;

            Line("switch(index)");
            Line("{");
            IndentLevel++;
            int numberOfElements = indexedSubgroup.subgroupIndexes.Count;
            for (int i = 0; i < numberOfElements; i++)
            {
                Line($"case {indexedSubgroup.subgroupIndexes[i]}: return {groupKeys[indexedSubgroup.groupIndexes[i]]};");
            }
            IndentLevel--;
            Line("}");

            Line("UnityEngine.Debug.LogError($\"Since the key({index}) does not exist, the default key will be used.\");");
            Line($"return {groupKeys[indexedSubgroup.groupIndexes[0]]};");

            IndentLevel--;
            Line("}");
        }

        private void ConfigMethod(IReadOnlyList<string> languages, IReadOnlyList<KeyGroup> groups)
        {
            Line($"public static LocalizatorConfig GetLocalizatorConfig()");
            Line("{");
            IndentLevel++;

            int numberOfLanguages = languages.Count;
            string mlanguages = "{";
            for (int i = 0; i < numberOfLanguages - 1; i++)
            {
                mlanguages += $"\"{languages[i]}\", ";
            }
            mlanguages += $"\"{languages[numberOfLanguages - 1]}\"" + "}";

            int numberOfGroups = groups.Count;
            string groupSizes = "{";
            string groupNames = "{";
            for (int i = 0; i < numberOfGroups - 1; i++)
            {
                groupSizes += groups[i].GroupKeys.Count + ", ";
                groupNames += $"\"{groups[i].GroupName}\", ";
            }
            groupSizes += groups[numberOfGroups - 1].GroupKeys.Count + "}";
            groupNames += $"\"{groups[numberOfGroups - 1].GroupName}\"" + "}";

            Line($"return new LocalizatorConfig(");
            IndentLevel++;
            Line($"new string[]{mlanguages},");
            Line($"{numberOfGroups},");
            Line($"new int[]{groupSizes},");
            Line($"new string[]{groupNames}");
            IndentLevel--;
            Line(");");

            IndentLevel--;
            Line("}");
        }
    } 
}
