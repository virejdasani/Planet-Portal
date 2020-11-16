using System;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.InteractiveTutorials
{
    public class TutorialWelcomePage : ScriptableObject
    {
        [Serializable]
        public class ButtonData
        {
            public LocalizableString Text;
            public LocalizableString Tooltip;
            public UnityEvent OnClick;
        }

        public event Action Modified;

        public Texture2D Image => m_Image;
        [SerializeField]
        Texture2D m_Image = default;

        public LocalizableString WindowTitle => m_WindowTitle;
        [SerializeField]
        internal LocalizableString m_WindowTitle = default;

        public LocalizableString Title => m_Title;
        [SerializeField]
        internal LocalizableString m_Title = default;

        public LocalizableString Description => m_Description;
        [SerializeField, LocalizableTextArea(1, 10)]
        internal LocalizableString m_Description;

        public ButtonData[] Buttons => m_Buttons;
        [SerializeField]
        internal ButtonData[] m_Buttons = default;

        internal void RaiseModifiedEvent()
        {
            Modified?.Invoke();
        }

        public static ButtonData CreateCloseButton(TutorialWelcomePage page)
        {
            var data = new ButtonData { Text = "Close", OnClick = new UnityEvent() };
            UnityEventTools.AddVoidPersistentListener(data.OnClick, page.CloseCurrentModalDialog);
            data.OnClick.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
            return data;
        }

        // Providing functionality for three default behaviours of the welcome dialog.

        public void CloseCurrentModalDialog()
        {
            var wnd = EditorWindowUtils.FindOpenInstance<TutorialModalWindow>();
            if (wnd)
                wnd.Close();
        }

        public void ExitEditor()
        {
            EditorApplication.Exit(0);
        }

        public void StartTutorial()
        {
            var projectSettings = TutorialProjectSettings.instance;
            if (projectSettings.startupTutorial)
                TutorialManager.instance.StartTutorial(projectSettings.startupTutorial);
        }
    }
}
