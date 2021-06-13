using UnityEngine;
using UnityEditor;

namespace AlchemyBow.Localizations.Editor
{
    [CustomEditor(typeof(LocalizationsSettings))]
    public class LocalizationsSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Edit the settings using a dedicated window.", MessageType.Info);
            if(GUILayout.Button("Open Settings Window"))
            {
                LocalisationsWindow.OpenWindow();
            }
        }
    } 
}
