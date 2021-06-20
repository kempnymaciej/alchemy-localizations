using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;

namespace AlchemyBow.Localizations.Editor.Utilities
{
    public class SettingsValidator
    {
        private readonly Regex nameRegex;
        private readonly Regex namespaceRegex;
        private readonly Regex languageRegex;

        private readonly List<string> failureRaports;

        public SettingsValidator()
        {
            failureRaports = new List<string>();
            nameRegex = new Regex(@"^[A-Z][a-zA-Z0-9_]{0,}");
            namespaceRegex = new Regex(@"^[A-Z][a-zA-Z0-9_\.]{0,}");
            languageRegex = languageRegex = new Regex(@"^[A-Z][a-zA-Z]{0,}");
        }

        public bool HasErrors => failureRaports.Count > 0;
        public IReadOnlyList<string> FailureRaports => failureRaports;

        public void UpdateRaport(LocalizationsSettings settings)
        {
            failureRaports.Clear();
            if (!IsSpreadsheetIdValid(settings))
            {
                failureRaports.Add("The 'Spreadsheet Id' cannot be empty.");
            }
            if (!IsClassNameValid(settings))
            {
                failureRaports.Add("The 'Class Name' cannot be empty, it must start with a capital letter and contain only [a-z, A-Z, 0-9, '_'] characters.");
            }
            if (!IsClassNamespaceValid(settings))
            {
                failureRaports.Add("The 'Class Namespace Name' cannot be empty, it must start with a capital letter and contain only [a-z, A-Z, 0-9, '_', '.'] characters.");
            }
            if (!IsClassFolderPathValid(settings))
            {
                failureRaports.Add("The `Class Folder Path` must be selected and point to a valid folder.");
            }
            if (!AreLanguagesValid(settings))
            {
                failureRaports.Add("You must enter at least one language. No language can be empty or a duplicate. It must start with a capital letter and contain only [a-z, A-Z] characters.");
            }
            if (!AreGroupsValid(settings))
            {
                failureRaports.Add("You must enter at least one group. No group can be empty or a duplicate. It must start with a capital letter and contain only [a-z, A-Z, 0-9, '_'] characters.");
            }
        }

        private bool IsSpreadsheetIdValid(LocalizationsSettings settings)
        {
            return !string.IsNullOrWhiteSpace(settings.SpreadsheetId);
        }

        private bool IsClassNameValid(LocalizationsSettings settings)
        {
            return IsValid(nameRegex, settings.ClassName);
        }
        private bool IsClassNamespaceValid(LocalizationsSettings settings)
        {
            return IsValid(namespaceRegex, settings.ClassNamespaceName);
        }

        private bool IsClassFolderPathValid(LocalizationsSettings settings)
        {
            string path = settings.ClassFolderPath;
            return path != null && (path.Length != 0 && AssetDatabase.IsValidFolder(path));
        }

        private bool AreLanguagesValid(LocalizationsSettings settings)
        {
            var languages = settings.Languages;
            if(languages == null || languages.Count == 0)
            {
                return false;
            }
            var uniqueLanguages = new HashSet<string>();
            foreach (var language in languages)
            {
                if (!uniqueLanguages.Add(language))
                {
                    return false;
                }
                if (!IsValid(languageRegex, language))
                {
                    return false;
                }
            }
            return true;
        }

        private bool AreGroupsValid(LocalizationsSettings settings)
        {
            var groups = settings.GroupNames;
            if (groups == null || groups.Count == 0)
            {
                return false;
            }
            var uniqueGroups = new HashSet<string>();
            foreach (var group in groups)
            {
                if (!uniqueGroups.Add(group))
                {
                    return false;
                }
                if (!IsValid(nameRegex, group))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsValid(Regex regex, string value)
        {
            var match = regex.Match(value);
            return match.Success && match.Length == value.Length;
        }
    } 
}
