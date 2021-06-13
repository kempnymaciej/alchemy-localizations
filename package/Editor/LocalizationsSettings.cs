using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AlchemyBow.Localizations.Editor
{
    public class LocalizationsSettings : ScriptableObject
    {
        public const string SettingsFolderName = "AlchemyLocalizationsData";
        public const string SettingsFolderPath = "Assets/" + SettingsFolderName;
        public const string SettingsFileName = "LocalizationsSettings.asset";
        public const string SettingsFilePath = SettingsFolderPath + "/" + SettingsFileName;
        public const string LocalisationsFileName = "localizations.txt";
        public const string LocalisationsFilePath = SettingsFolderPath + "/" + LocalisationsFileName;

        [SerializeField]
        private string spreadsheetId;
        [SerializeField]
        private string className = "Keys";
        [SerializeField]
        private string classNamespaceName = "AlchemyBow.Localizations";
        [SerializeField]
        private string classFolderPath = "";
        [SerializeField]
        private string[] languages = null;
        [SerializeField]
        private string[] groupNames = null;

        public string SpreadsheetId => spreadsheetId;
        public string ClassName => className;
        public string ClassNamespaceName => classNamespaceName;
        public string AdaptiveAbsoluteClassPath 
            => System.IO.Path.Combine(Application.dataPath, classFolderPath, className + ".cs");
        public IReadOnlyList<string> Languages => languages;
        public IReadOnlyList<string> GroupNames => groupNames;

        public static bool CheckIfSettingsExist()
        {
            return GetSettings() != null;
        }
        public static LocalizationsSettings GetSettings()
        {
            return AssetDatabase.LoadAssetAtPath<LocalizationsSettings>(SettingsFilePath);
        }

        public static void SelectSettingsObjectIfExist()
        {
            var settings = GetSettings();
            if(settings != null)
            {
                Selection.activeObject = settings;
            }
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
    } 
}
