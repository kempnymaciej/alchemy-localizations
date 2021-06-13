using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AlchemyBow.Localizations.CsvProcessing;
using UnityEngine.Networking;
using System.IO;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using AlchemyBow.Localizations.Editor.Utilities;

namespace AlchemyBow.Localizations.Editor
{
    public sealed class LocalizationsSettingsGUI 
    {
        private const string SheetUrlPattern = "https://docs.google.com/spreadsheets/d/{0}/gviz/tq?tqx=out:csv&sheet={1}";

        private readonly LocalizationsSettings settings;
        private readonly SerializedObject serializedObject;

        private readonly SerializedProperty spreadsheetIdProp;
        private readonly SerializedProperty classNameProp;
        private readonly SerializedProperty classNamespaceNameProp;
        private readonly SerializedProperty classFolderPathProp;
        private readonly SerializedProperty languagesProp;
        private readonly SerializedProperty groupNamesProp;

        public LocalizationsSettingsGUI(LocalizationsSettings settings)
        {
            this.settings = settings;
            this.serializedObject = new SerializedObject(settings);

            spreadsheetIdProp = serializedObject.FindProperty("spreadsheetId");
            classNameProp = serializedObject.FindProperty("className");
            classNamespaceNameProp = serializedObject.FindProperty("classNamespaceName");
            classFolderPathProp = serializedObject.FindProperty("classFolderPath");
            languagesProp = serializedObject.FindProperty("languages");
            groupNamesProp = serializedObject.FindProperty("groupNames");
        }

        #region GUI
        public void OnGUI()
        {
            serializedObject.Update();
            DrawFieldsGUI();
        }

        private void DrawFieldsGUI()
        {
            EditorGUILayout.LabelField("Localizatons Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            EditorGUILayout.PropertyField(spreadsheetIdProp);
            EditorGUILayout.PropertyField(classNameProp);
            EditorGUILayout.PropertyField(classNamespaceNameProp);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(classFolderPathProp);
            EditorGUI.EndDisabledGroup();
            DrawSelectClassFolderPathGUI();
            EditorGUILayout.PropertyField(languagesProp);
            EditorGUILayout.PropertyField(groupNamesProp);

            serializedObject.ApplyModifiedProperties();

            if (ValidateSettingsGUI())
            {
                if (GUILayout.Button("Synchronize Localizations"))
                {
                    if (DisplaySynchronizationConfirmation())
                    {
                        try
                        {
                            Synchronize();
                            Debug.Log("Localizations synchronization was successful.");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(e.Message);
                        }
                    }
                }
            }
        }

        private void DrawSelectClassFolderPathGUI()
        {
            if (GUILayout.Button("Update Class Directory Path"))
            {
                string directoryPath = EditorUtility.OpenFolderPanel("Select the folder to store baked class.", "Assets", "");
                if(directoryPath.Length == 0)
                {
                    return;
                }

                if (directoryPath.Contains(Application.dataPath))
                {
                    directoryPath = directoryPath.Replace(Application.dataPath, "");
                    if (directoryPath.Length == 0)
                    {
                        directoryPath = "";
                    }
                    else
                    {
                        directoryPath = directoryPath.Substring(1);
                    }
                    classFolderPathProp.stringValue = directoryPath;
                }
                else
                {
                    Debug.LogError("Select path in the Assets folder.");
                }
            }
        }

        private bool ValidateSettingsGUI()
        {
            bool result = true;
            result = result && ValidateClassNamespaceNameGUI();
            result = result && ValidateClassNameGUI();
            result = result && ValidateClassFolderPathGUI();
            result = result && ValidateLanguagesGUI();
            result = result && ValidateGroupsGUI();
            return result;
        }
        private bool ValidateClassFolderPathGUI()
        {
            bool isCorrent = true;
            string path = classFolderPathProp.stringValue;
            if (path == null)
            {
                EditorGUILayout.HelpBox("The class folder path must be selected.", MessageType.Error);
                isCorrent = false;
            }
            else if (path.Length != 0 && !AssetDatabase.IsValidFolder("Assets/" + path))
            {
                EditorGUILayout.HelpBox("The class folder path must point to valid directory", MessageType.Error);
                isCorrent = false;
            }
            return isCorrent;
        }
        private bool ValidateClassNamespaceNameGUI()
        {
            bool isCorrect = true;
            string classNamespaceName = classNamespaceNameProp.stringValue;
            if (string.IsNullOrWhiteSpace(classNamespaceName))
            {
                isCorrect = false;
                EditorGUILayout.HelpBox("The class namespace name must be non-empty.", MessageType.Error);
            }
            else if(classNamespaceName.Trim().Length != classNamespaceName.Length)
            {
                isCorrect = false;
                EditorGUILayout.HelpBox("The class namespace name cannot contain white spaces.", MessageType.Error);
            }
            return isCorrect;
        }
        private bool ValidateClassNameGUI()
        {
            bool isCorrect = true;
            string className = classNameProp.stringValue;
            if (string.IsNullOrWhiteSpace(className))
            {
                isCorrect = false;
                EditorGUILayout.HelpBox("The class name must be non-empty.", MessageType.Error);
            }
            else if (className.Trim().Length != className.Length)
            {
                isCorrect = false;
                EditorGUILayout.HelpBox("The class name cannot contain white spaces.", MessageType.Error);
            }
            return isCorrect;
        }
        private bool ValidateLanguagesGUI()
        {
            bool isCorrect = true;
            if (languagesProp.arraySize == 0)
            {
                isCorrect = false;
                EditorGUILayout.HelpBox("Enter at least one language.", MessageType.Error);
            }
            return isCorrect;
        }
        private bool ValidateGroupsGUI()
        {
            bool isCorrect = true;
            if (groupNamesProp.arraySize == 0)
            {
                isCorrect = false;
                EditorGUILayout.HelpBox("Enter at least one group.", MessageType.Error);
            }
            return isCorrect;
        }

        private static bool DisplaySynchronizationConfirmation()
        {
            return EditorUtility.DisplayDialog(
                "Confirm Synchronization",
                "If the folders of 'Addressables' does not exist, it will be created during synchronization process. The localizations file will be added to the default addressables group.",
                "Continue", "Cancel");
        }
        #endregion

        #region Synchronization
        private void Synchronize()
        {
            var sheetsContent = LoadSheetsContent(settings);

            int numberOfGroups = settings.GroupNames.Count;
            var groups = new KeyGroup[numberOfGroups];

            var csvBuilder = new CsvBuilder();
            var fileBuilder = new LocalizationsFileBuilder();
            var csvReader = new CsvReader();

            for (int i = 0; i < numberOfGroups; i++)
            {
                string groupName = settings.GroupNames[i];
                using (var reader = new StringReader(sheetsContent[i]))
                {
                    var row = new List<string>();
                    csvReader.ReadRow(row, reader);
                    ValidateGroupHeadRow(groupName, row, settings.Languages);

                    var keys = new List<string>();
                    while (true)
                    {
                        row.Clear();
                        if (csvReader.ReadRow(row, reader) > 0)
                        {
                            csvBuilder.AddRow(row);
                            keys.Add(row[0]);
                            ValidateRow(groupName, keys.Count - 1, row, settings.Languages);
                        }
                        else
                        {
                            break;
                        }
                    }
                    var groupKeys = keys.ToArray();
                    ValidateGroupContentKeys(groupName, groupKeys);
                    groups[i] = new KeyGroup(groupName, groupKeys);
                }
            }

            File.WriteAllText(LocalizationsSettings.LocalisationsFilePath, csvBuilder.ToString());
            File.WriteAllText(settings.AdaptiveAbsoluteClassPath, fileBuilder.Build(settings, groups));

            AssetDatabase.Refresh();
            UpdateAddressableFile();
            OfferAddressablesBuild();
        }



        private static string[] LoadSheetsContent(LocalizationsSettings settings)
        {
            int numberOfGruops = settings.GroupNames.Count;
            var result = new string[numberOfGruops];
            for (int i = 0; i < numberOfGruops; i++)
            {
                float minProgress = (float)i / numberOfGruops;
                float maxProgress = (float)(i + 1) / numberOfGruops;
                result[i] = LoadSheetContent(settings, settings.GroupNames[i], minProgress, maxProgress);
            }
            EditorUtility.ClearProgressBar();
            return result;
        }

        private static string LoadSheetContent(LocalizationsSettings settings, string groupName, float minProgress, float maxProgress)
        {
            string barTitle = "Downloading localizations";
            string barInfo = $"Downloading the group: \"{groupName}\".";
            var groupUri = FormatUri(settings.SpreadsheetId, groupName);
            string result = null;
            EditorUtility.DisplayProgressBar(barTitle, barInfo, minProgress);
            using (var webRequest = UnityWebRequest.Get(groupUri))
            {
                var sendRequest = webRequest.SendWebRequest();
                while (!sendRequest.isDone || !webRequest.downloadHandler.isDone)
                {
                    float progress = minProgress + webRequest.downloadProgress * (maxProgress - minProgress);
                    EditorUtility.DisplayProgressBar(barTitle, barInfo, progress);
                }

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    result = webRequest.downloadHandler.text;
                }
                else
                {
                    EditorUtility.ClearProgressBar();
                    throw new System.Exception($"Failed to download the '{groupName}' sheet. ({groupUri})");
                }
            }

            return result;
        }

        private static string FormatUri(string spreadsheetId, string sheetName)
        {
            return string.Format(SheetUrlPattern, spreadsheetId, sheetName);
        }

        private static void ValidateGroupHeadRow(string groupName, IReadOnlyList<string> headRow, IReadOnlyList<string> languages)
        {
            string errorLog = "";
            int numberOfLanguages = languages.Count;
            int numberOfColumns = headRow.Count;
            if (numberOfColumns <= numberOfLanguages)
            {
                errorLog += $"The group has unexpected number of columns({numberOfColumns}/{numberOfLanguages + 1}).\n";
            }
            for (int i = 0; i < numberOfLanguages; i++)
            {
                if (languages[i] != headRow[i + 1])
                {
                    errorLog += $"The column head at index {i + 1} should be {languages[i]}.\n";
                }
            }

            if (!string.IsNullOrWhiteSpace(errorLog))
            {

                throw new System.Exception($"The group ({groupName}) contains errors in the head row: \n{errorLog}");
            }
            return;
        }
        
        private static void ValidateRow(string groupName, int rowIndex, IReadOnlyList<string> row, IReadOnlyList<string> languages)
        {
            string warningLog = "";
            int numberOfItems = row.Count;
            if (numberOfItems <= languages.Count)
            {
                warningLog += "The row has too few columns.";
            }
            for (int i = 0; i < numberOfItems; i++)
            {
                if (string.IsNullOrWhiteSpace(row[i]))
                {
                    warningLog += $"The item at index {i} is null or empty.";
                }
            }

            if (!string.IsNullOrWhiteSpace(warningLog))
            {
                Debug.LogWarning($"The row {groupName}[{rowIndex}] contains warnings: \n{warningLog}");
            }
        }
        
        private static void ValidateGroupContentKeys(string groupName, IReadOnlyList<string> keys)
        {
            string errorLog = "";
            int numberOfKeys = keys.Count;
            if (numberOfKeys == 0)
            {
                errorLog += "The group has no keys.\n";
            }
            var uniqueKeys = new HashSet<string>();
            for (int i = 0; i < numberOfKeys; i++)
            {
                string key = keys[i];
                if (string.IsNullOrWhiteSpace(key))
                {
                    errorLog += $"The key at index {i} is null or white space.\n";
                }
                else if (key.Trim().Length != key.Length)
                {
                    errorLog += $"The key at index {i} contains white spaces.\n";
                }
                else if (!uniqueKeys.Add(key))
                {
                    errorLog += $"The key at index {i} is a duplicate.\n";
                }
            }

            if (!string.IsNullOrWhiteSpace(errorLog))
            {
                throw new System.Exception($"Failed to decode the group '{groupName}' keys: \n{errorLog}");
            }
        }
        
        private static void UpdateAddressableFile()
        {
            var guid = AssetDatabase.AssetPathToGUID(LocalizationsSettings.LocalisationsFilePath);

            if (AddressableAssetSettingsDefaultObject.Settings == null)
                AddressableAssetSettingsDefaultObject.Settings = AddressableAssetSettings.Create(AddressableAssetSettingsDefaultObject.kDefaultConfigFolder, AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName, true, true);
            AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, AddressableAssetSettingsDefaultObject.Settings.DefaultGroup);
        }

        private static void OfferAddressablesBuild()
        {
            bool confirmation = EditorUtility.DisplayDialog("Confirm process", "For localizations to work you need to build addressable. You can skip this step and do it yourself later.", "Build Addressables Now", "Skip");
            if (confirmation)
            {
                AddressableAssetSettings.BuildPlayerContent();
            }
        }
        #endregion
    }
}
