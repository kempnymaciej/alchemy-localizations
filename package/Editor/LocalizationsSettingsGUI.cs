using UnityEngine;
using UnityEditor;
using AlchemyBow.Localizations.CsvProcessing;
using UnityEngine.Networking;
using System.IO;
using AlchemyBow.Localizations.Editor.Utilities;

namespace AlchemyBow.Localizations.Editor
{
    public sealed class LocalizationsSettingsGUI 
    {
        private const string SheetUrlPattern = "https://docs.google.com/spreadsheets/d/{0}/gviz/tq?tqx=out:csv&sheet={1}";

        private readonly LocalizationsSettings settings;
        private readonly SerializedObject serializedObject;
        private readonly SettingsValidator settingsValidator;

        private readonly SerializedProperty spreadsheetIdProp;
        private readonly SerializedProperty classNameProp;
        private readonly SerializedProperty classNamespaceNameProp;
        private readonly SerializedProperty classFolderPathProp;
        private readonly SerializedProperty languagesProp;
        private readonly SerializedProperty groupNamesProp;

        public LocalizationsSettingsGUI(LocalizationsSettings settings)
        {
            this.settings = settings;
            this.settingsValidator = new SettingsValidator();
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
            DrawFieldsGUI();
            if (settingsValidator.HasErrors)
            {
                foreach (var raport in settingsValidator.FailureRaports)
                {
                    EditorGUILayout.HelpBox(raport, MessageType.Error);
                }
            }
            else 
            {
                if (GUILayout.Button("Synchronize Localizations"))
                {
                    try
                    {
                        Synchronize();
                        Debug.Log("Localizations synchronization finished with result: <color=yellow>Success</color>.");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e.Message);
                        Debug.Log("Localizations synchronization finished with result: <color=red>Failure</color>.");
                    }
                }
            }
        }

        private void DrawFieldsGUI()
        {
            serializedObject.Update();

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
            settingsValidator.UpdateRaport(settings);
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
                        directoryPath = "Assets";
                    }
                    else
                    {
                        directoryPath = "Assets" + directoryPath;
                    }
                    classFolderPathProp.stringValue = directoryPath;
                }
                else
                {
                    Debug.LogError("Select path in the Assets folder.");
                }
            }
        }
        #endregion

        #region Synchronization
        private void Synchronize()
        {
            var sheetsContent = LoadSheetsContent(settings);

            int numberOfGroups = settings.GroupNames.Count;
            var groups = new KeyGroup[numberOfGroups];
            for (int i = 0; i < numberOfGroups; i++)
            {
                groups[i] = KeyGroup.FromSheetContent(settings.GroupNames[i], sheetsContent[i], settings);
            }

            SaveCsvFile(groups, settings);
            SaveCsFile(groups, settings);
            AssetDatabase.Refresh();
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

        private static void SaveCsvFile(KeyGroup[] groups, LocalizationsSettings settings)
        {
            var csvBuilder = new CsvBuilder();
            foreach (var group in groups)
            {
                int numberOfKeys = group.NumberOfKeys;
                for (int i = 0; i < numberOfKeys; i++)
                {
                    csvBuilder.AddRow(group.GetRow(i));
                }
            }

            settings.SaveLocalizationsFile(csvBuilder.ToString());
        }

        private static void SaveCsFile(KeyGroup[] groups, LocalizationsSettings settings)
        {
            var fileBuilder = new LocalizationsFileBuilder();
            File.WriteAllText(settings.ClassProjectRelativePath, fileBuilder.Build(settings, groups));
        }
        #endregion
    }
}
