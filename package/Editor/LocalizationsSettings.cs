using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Localizations.Editor
{
    public class LocalizationsSettings : ScriptableObject
    {
        private const string SettingsFolderName = "AlchemyLocalizationsData";
        private const string SettingsFileName = "LocalizationsSettings.asset";

        private const string SettingsFolderPath = "Assets/" + SettingsFolderName;
        private const string SettingsFilePath = SettingsFolderPath + "/" + SettingsFileName;

        private const string LocalisationsFileName = "localizations.txt";

        [SerializeField]
        private string spreadsheetId = "";
        [SerializeField]
        private string className = "Keys";
        [SerializeField]
        private string classNamespaceName = "AlchemyBow.Localizations";
        [SerializeField]
        private string classFolderPath = "Assets";
        [SerializeField]
        private string[] languages = null;
        [SerializeField]
        private string[] groupNames = null;

        public string SpreadsheetId => spreadsheetId;
        public string ClassName => className;
        public string ClassNamespaceName => classNamespaceName;

        public string ClassFolderPath => classFolderPath;
        public string ClassProjectRelativePath => Path.Combine(classFolderPath, className + ".cs");

        public IReadOnlyList<string> Languages => languages;
        public IReadOnlyList<string> GroupNames => groupNames;

        public static LocalizationsSettings GetSettings()
        {
            return AssetDatabase.LoadAssetAtPath<LocalizationsSettings>(SettingsFilePath);
        }

        public static void CreateDefaultSettingsIfNotExist()
        {
            if (!AssetDatabase.IsValidFolder(LocalizationsSettings.SettingsFolderPath))
            {
                CreateDefaultSettingsFolder();
                CreateDefaultSettingsFile();
            }
            else if(GetSettings() == null)
            {
                CreateDefaultSettingsFile();
            }
        }
        private static void CreateDefaultSettingsFolder()
        {
            AssetDatabase.CreateFolder("Assets", LocalizationsSettings.SettingsFolderName);
            AssetDatabase.Refresh();
        }
        private static void CreateDefaultSettingsFile()
        {
            var asset = ScriptableObject.CreateInstance<LocalizationsSettings>();
            AssetDatabase.CreateAsset(asset, SettingsFilePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void SaveLocalizationsFile(string fileContent)
        {
            string folderPath = EnsureSettingsSubfoldersBranchIfNotExist("Resources", "AlchemyLocalizations");
            File.WriteAllText(Path.Combine(folderPath, LocalisationsFileName), fileContent);
            AssetDatabase.Refresh();
        }

        private string EnsureSettingsSubfoldersBranchIfNotExist(params string[] foldersBranch)
        {
            string folderPath = SettingsFolderPath;
            if (foldersBranch != null)
            {
                for (int i = 0; i < foldersBranch.Length; i++)
                {
                    string parentFolderPath = folderPath;
                    folderPath = Path.Combine(parentFolderPath, foldersBranch[i]);
                    if (!AssetDatabase.IsValidFolder(folderPath))
                    {
                        AssetDatabase.CreateFolder(parentFolderPath, foldersBranch[i]);
                    }
                } 
            }

            return folderPath;
        }
    }
}
