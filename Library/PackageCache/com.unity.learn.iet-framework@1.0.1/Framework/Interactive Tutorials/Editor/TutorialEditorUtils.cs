using System.IO;
using System.Linq;
using UnityEditor;

namespace Unity.InteractiveTutorials
{
    /// <summary>
    /// Contains different utilities used in Tutorials custom editors
    /// </summary>
    public static class TutorialEditorUtils
    {
        /// <summary>
        /// Same as ProjectWindowUtil.GetActiveFolderPath() but works also in 1-panel view.
        /// </summary>
        /// <returns>Returns path with forward slashes</returns>
        public static string GetActiveFolderPath()
        {
            return Selection.assetGUIDs
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path =>
                {
                    if (Directory.Exists(path))
                        return path;
                    if (File.Exists(path))
                        return Path.GetDirectoryName(path);
                    return path;
                })
                .FirstOrDefault()
                .AsNullIfEmpty()
                ?? "Assets";
        }

        /// <summary>
        /// Checks if a UnityEvent property is not in a specific execution state
        /// </summary>
        /// <param name="eventProperty">A property representing the UnityEvent (or derived class)</param>
        /// <param name="state"></param>
        /// <returns>True if the event is in the expected state</returns>
        public static bool EventIsNotInState(SerializedProperty eventProperty, UnityEngine.Events.UnityEventCallState state)
        {
            SerializedProperty persistentCalls = eventProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");
            for (int i = 0; i < persistentCalls.arraySize; i++)
            {
                if (persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_CallState").intValue == (int)state) { continue; }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Renders a warning box about the state of the event
        /// </summary>
        public static void RenderEventStateWarning()
        {
            EditorGUILayout.HelpBox(
                Localization.Tr(
                    "One or more callbacks of this event are not using 'Editor and Runtime' mode." +
                    "If you want to be able to execute them in Edit mode as well (that is, when running tutorials out of Play mode), " +
                    "be sure to choose 'Editor and Runtime' mode. "),
                 MessageType.Warning
            );
        }
    }
}
