using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.InteractiveTutorials
{
    public static class EditorWindowUtils
    {
        /// <summary>
        /// Finds the first open EditorWindow instance, if such exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FindOpenInstance<T>() where T : EditorWindow =>
            Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();

        public static void CenterOnMainWindow(EditorWindow win)
        {
            var main = GetEditorMainWindowPos();
            var pos = win.position;
            float w = (main.width - pos.width) * 0.5f;
            float h = (main.height - pos.height) * 0.5f;
            pos.x = main.x + w;
            pos.y = main.y + h;
            win.position = pos;
        }

        // http://answers.unity.com/answers/960709/view.html
        public static Rect GetEditorMainWindowPos()
        {
            var containerWinType = GetAllDerivedTypes(AppDomain.CurrentDomain, typeof(ScriptableObject))
                .Where(t => t.Name == "ContainerWindow")
                .FirstOrDefault();
            if (containerWinType == null)
                throw new MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");

            var showModeField = containerWinType.GetField("m_ShowMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var positionProperty = containerWinType.GetProperty("position", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (showModeField == null || positionProperty == null)
                throw new MissingFieldException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");

            foreach (var win in Resources.FindObjectsOfTypeAll(containerWinType))
            {
                var showmode = (int)showModeField.GetValue(win);
                if (showmode == 4) // main window
                {
                    var pos = (Rect)positionProperty.GetValue(win, null);
                    return pos;
                }
            }

            throw new NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity");
        }

        // TODO copy-pasta, generalise and clean up the code with GetEditorMainWindowPos
        public static void SetEditorMainWindowPos(Rect pos)
        {
            var containerWinType = GetAllDerivedTypes(AppDomain.CurrentDomain, typeof(ScriptableObject))
                .Where(t => t.Name == "ContainerWindow")
                .FirstOrDefault();
            if (containerWinType == null)
                throw new MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");

            var showModeField = containerWinType.GetField("m_ShowMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var positionProperty = containerWinType.GetProperty("position", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (showModeField == null || positionProperty == null)
                throw new MissingFieldException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");

            foreach (var win in Resources.FindObjectsOfTypeAll(containerWinType))
            {
                var showmode = (int)showModeField.GetValue(win);
                if (showmode == 4) // main window
                {
                    positionProperty.SetValue(win, pos);
                    return;
                }
            }

            throw new NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity");
        }

        static IEnumerable<Type> GetAllDerivedTypes(AppDomain appDomain, Type parentType)
        {
            return appDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(parentType));
        }
    }
}
