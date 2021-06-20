using System.Collections.Generic;

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
                BeginClass(groups[i].groupName);
                int numberOfKeys = groups[i].NumberOfKeys;
                Field("_GroupIndex", i);
                Field("_FirstKey", groupStartIndex);
                Field("_LastKey", groupStartIndex + numberOfKeys - 1);

                for (int j = 0; j < numberOfKeys; j++)
                {
                    Field(groups[i].GetKey(j), groupStartIndex + j);
                }

                groupStartIndex += numberOfKeys;
                EndClass();
            }

            EndClass();
            EndNamespace();
            return result;
        }

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
                groupSizes += groups[i].NumberOfKeys + ", ";
                groupNames += $"\"{groups[i].groupName}\", ";
            }
            groupSizes += groups[numberOfGroups - 1].NumberOfKeys + "}";
            groupNames += $"\"{groups[numberOfGroups - 1].groupName}\"" + "}";

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
