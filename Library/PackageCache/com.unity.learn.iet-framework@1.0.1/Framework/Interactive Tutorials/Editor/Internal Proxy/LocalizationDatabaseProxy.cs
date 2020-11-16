using UnityEngine;
using UnityEditor;

namespace Unity.InteractiveTutorials
{
    public class LocalizationDatabaseProxy
    {
        public static SystemLanguage currentEditorLanguage =>
            LocalizationDatabase.currentEditorLanguage;

        public static SystemLanguage[] GetAvailableEditorLanguages() =>
            LocalizationDatabase.GetAvailableEditorLanguages();
    }
}
