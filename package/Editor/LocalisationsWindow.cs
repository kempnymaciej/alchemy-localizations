using UnityEngine;
using UnityEditor;

namespace AlchemyBow.Localizations.Editor
{
    public class LocalisationsWindow : EditorWindow
    {
        [MenuItem("Window/AlchemyBow/Localizations/Settings", priority = 2050)]
        public static void OpenWindow()
        {
            var window = GetWindow<LocalisationsWindow>();
            window.titleContent = new GUIContent("Localisations Settings");
            window.Show();
        }

        private LocalizationsSettingsGUI settingsEditor;
        private Vector2 scrollPosition;

        private void OnEnable()
        {
            scrollPosition = Vector2.zero;
            settingsEditor = null;
        }
        private void OnDisable()
        {
            settingsEditor = null;
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            var settings = LocalizationsSettings.GetSettings();
            if (settings != null)
            {
                if(settingsEditor == null)
                {
                    settingsEditor = new LocalizationsSettingsGUI(settings);
                }
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                settingsEditor.OnGUI();
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                settingsEditor = null;
                if (GUILayout.Button("Create Localizations Settings"))
                {
                    LocalizationsSettings.CreateDefaultSettingsIfNotExist();
                }
            }
            EditorGUILayout.EndScrollView();
        }
    } 
}
