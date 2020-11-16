using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Unity.InteractiveTutorials
{
    [CustomEditor(typeof(TutorialPage))]
    class TutorialPageEditor : Editor
    {
        static readonly bool k_IsAuthoringMode = ProjectMode.IsAuthoringMode();
        const string k_OnBeforeShownEventPropertyPath = "m_OnBeforePageShown";
        const string k_OnAfterShownEventPropertyPath = "m_OnAfterPageShown";
        const string k_ParagraphPropertyPath = "m_Paragraphs.m_Items";
        const string k_ParagraphMaskingSettingsRelativeProperty = "m_MaskingSettings";
        const string k_ParagraphVideoRelativeProperty = "m_Video";
        const string k_ParagraphImageRelativeProperty = "m_Image";
        const string k_ParagraphNarrativeTitleProperty = "m_Summary";
        const string k_ParagraphTypeProperty = "m_Type";

        const string k_ParagraphNarrativeDescriptionProperty = "m_Description";
        const string k_ParagraphTitleProperty = "m_InstructionBoxTitle";
        const string k_ParagraphDescriptionProperty = "m_InstructionText";

        const string k_ParagraphCriteriaTypePropertyPath = "m_CriteriaCompletion";
        const string k_ParagraphCriteriaPropertyPath = "m_Criteria";

        const string k_ParagraphNextTutorialPropertyPath = "m_Tutorial";
        const string k_ParagraphNextTutorialButtonTextPropertyPath = "m_TutorialButtonText";

        static readonly Regex s_MatchMaskingSettingsPropertyPath =
            new Regex(
                string.Format(
                    "(^{0}\\.Array\\.size)|(^({0}\\.Array\\.data\\[\\d+\\]\\.{1}\\.))",
                    k_ParagraphPropertyPath, k_ParagraphMaskingSettingsRelativeProperty
                )
            );

        static GUIContent s_EventsSectionTitle;
        static GUIContent s_OnBeforeEventsTitle;
        static GUIContent s_OnAfterEventsTitle;
        // Enable to display the old, not simplified, inspector
        static bool s_ForceOldInspector;
        static bool s_ShowEvents;
        // True if we have created a callback script and waiting for a scriptable object instance to be created for it.
        static bool IsCreatingScriptableObject
        {
            get { return SessionState.GetBool("iet_creating_SO", false); }
            set { SessionState.SetBool("iet_creating_SO", value); }
        }

        TutorialPage tutorialPage { get { return (TutorialPage)target; } }

        [NonSerialized]
        string m_WarningMessage;

        readonly string[] k_PropertiesToHide =
        {
            "m_SectionTitle", "m_Paragraphs", "m_Script",
            k_OnBeforeShownEventPropertyPath, k_OnAfterShownEventPropertyPath
        };
        SerializedProperty m_OnBeforePageShown;
        SerializedProperty m_OnAfterPageShown;
        SerializedProperty m_MaskingSettings;
        SerializedProperty m_Type;
        SerializedProperty m_Video;
        SerializedProperty m_Image;

        SerializedProperty m_NarrativeTitle;
        SerializedProperty m_NarrativeDescription;
        SerializedProperty m_InstructionTitle;
        SerializedProperty m_InstructionDescription;

        SerializedProperty m_CriteriaCompletion;
        SerializedProperty m_Criteria;

        SerializedProperty m_TutorialButtonText;
        SerializedProperty m_NextTutorial;

        HeaderMediaType m_HeaderMediaType;

        enum HeaderMediaType
        {
            Image = ParagraphType.Image,
            Video = ParagraphType.Video
        }

        protected virtual void OnEnable()
        {
            InitializeTooltips();
            InitializeSerializedProperties();

            Undo.postprocessModifications += OnPostprocessModifications;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        void InitializeTooltips()
        {
            Texture helpIcon = EditorGUIUtility.IconContent("console.infoicon.sml").image;
            string tooltip = Localization.Tr("You can only assign public, non-static methods here. It is recommended that you define a ScriptableObject class " +
                "that exposes all the methods you'd like to call, " +
                "create an instance of that and assign it to these events in order to access the callbacks.");
            s_EventsSectionTitle = new GUIContent(Localization.Tr("Custom Callbacks"), helpIcon, tooltip);

            tooltip = "These methods will be called right before the page is displayed (even when going back)";
            s_OnBeforeEventsTitle = new GUIContent("OnBeforePageShown", helpIcon, tooltip);
            tooltip = "These methods will be called right after the page is displayed (even when going back)";
            s_OnAfterEventsTitle = new GUIContent("OnAfterPageShown", helpIcon, tooltip);
        }

        protected virtual void OnDisable()
        {
            Undo.postprocessModifications -= OnPostprocessModifications;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        void OnUndoRedoPerformed()
        {
            if (tutorialPage == null) { return; }
            tutorialPage.RaiseTutorialPageMaskingSettingsChangedEvent();
        }

        UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
        {
            if (tutorialPage == null) { return modifications; }

            bool targetModified = false;
            bool maskingChanged = false;

            foreach (var modification in modifications)
            {
                if (modification.currentValue.target != target) { continue; }

                targetModified = true;
                var propertyPath = modification.currentValue.propertyPath;
                if (s_MatchMaskingSettingsPropertyPath.IsMatch(propertyPath))
                {
                    maskingChanged = true;
                    break;
                }
            }

            if (maskingChanged)
            {
                tutorialPage.RaiseTutorialPageMaskingSettingsChangedEvent();
            }
            else if (targetModified)
            {
                tutorialPage.RaiseTutorialPageNonMaskingSettingsChangedEvent();
            }
            return modifications;
        }

        void InitializeSerializedProperties()
        {
            m_OnBeforePageShown = serializedObject.FindProperty(k_OnBeforeShownEventPropertyPath);
            /* [NOTE] this would force the callbacks to be "editor + runtime" every time the tutorial page is selected
             * (but not when the callback is added for the first time),
             * but this might go against the will of the user */
            //ForceCallbacksListenerState(m_OnBeforePageShown, UnityEngine.Events.UnityEventCallState.EditorAndRuntime);

            m_OnAfterPageShown = serializedObject.FindProperty(k_OnAfterShownEventPropertyPath);

            SerializedProperty paragraphs = serializedObject.FindProperty(k_ParagraphPropertyPath);
            if (paragraphs == null)
            {
                m_WarningMessage = string.Format(
                    "Unable to locate property path {0} on this object. Automatic masking updates will not work.",
                    k_ParagraphPropertyPath
                );
            }
            else if (paragraphs.arraySize > 0)
            {
                SerializedProperty firstParagraph = paragraphs.GetArrayElementAtIndex(0);

                m_MaskingSettings = firstParagraph.FindPropertyRelative(k_ParagraphMaskingSettingsRelativeProperty);
                if (m_MaskingSettings == null)
                    m_WarningMessage = string.Format(
                        "Unable to locate property path {0}.Array.data[0].{1} on this object. Automatic masking updates will not work.",
                        k_ParagraphPropertyPath,
                        k_ParagraphMaskingSettingsRelativeProperty
                    );

                m_Type = firstParagraph.FindPropertyRelative(k_ParagraphTypeProperty);
                m_HeaderMediaType = (HeaderMediaType)m_Type.intValue;
                var headerMediaParagraphType = (ParagraphType)m_Type.intValue;
                // Only Image and Video are allowed for the first paragraph which is always the header media in the new fixed tutorial page layout.
                if (headerMediaParagraphType != ParagraphType.Image && headerMediaParagraphType != ParagraphType.Video)
                {
                    m_Type.intValue = (int)ParagraphType.Image;
                }

                m_Video = firstParagraph.FindPropertyRelative(k_ParagraphVideoRelativeProperty);
                m_Image = firstParagraph.FindPropertyRelative(k_ParagraphImageRelativeProperty);

                switch (paragraphs.arraySize)
                {
                    case 2: SetupNarrativeOnlyPage(paragraphs); break;
                    case 4: SetupSwitchTutorialPage(paragraphs); break;
                    case 3:
                    default:
                        SetupNarrativeAndInstructivePage(paragraphs); break;
                }
            }
        }

        void SetupNarrativeParagraph(SerializedProperty paragraphs)
        {
            if (paragraphs.arraySize < 2)
            {
                m_NarrativeTitle = null;
                m_NarrativeDescription = null;
                return;
            }

            SerializedProperty narrativeParagraph = paragraphs.GetArrayElementAtIndex(1);
            m_NarrativeTitle = narrativeParagraph.FindPropertyRelative(k_ParagraphNarrativeTitleProperty);
            m_NarrativeDescription = narrativeParagraph.FindPropertyRelative(k_ParagraphNarrativeDescriptionProperty);
            // TODO refactoring, support the old name of the property for a while still
            if (m_NarrativeDescription == null)
                m_NarrativeDescription = narrativeParagraph.FindPropertyRelative("m_description1");
        }

        void SetupNarrativeOnlyPage(SerializedProperty paragraphs)
        {
            SetupNarrativeParagraph(paragraphs);
        }

        void SetupNarrativeAndInstructivePage(SerializedProperty paragraphs)
        {
            SetupNarrativeParagraph(paragraphs);
            if (paragraphs.arraySize > 2)
            {
                SerializedProperty instructionParagraph = paragraphs.GetArrayElementAtIndex(2);
                m_InstructionTitle = instructionParagraph.FindPropertyRelative(k_ParagraphTitleProperty);
                m_InstructionDescription = instructionParagraph.FindPropertyRelative(k_ParagraphDescriptionProperty);
                m_CriteriaCompletion = instructionParagraph.FindPropertyRelative(k_ParagraphCriteriaTypePropertyPath);
                m_Criteria = instructionParagraph.FindPropertyRelative(k_ParagraphCriteriaPropertyPath);
                return;
            }
            m_InstructionTitle = null;
            m_InstructionDescription = null;
            m_CriteriaCompletion = null;
            m_Criteria = null;
        }

        void SetupSwitchTutorialPage(SerializedProperty paragraphs)
        {
            SetupNarrativeAndInstructivePage(paragraphs);
            if (paragraphs.arraySize > 3)
            {
                SerializedProperty tutorialSwitchParagraph = paragraphs.GetArrayElementAtIndex(3);
                m_NextTutorial = tutorialSwitchParagraph.FindPropertyRelative(k_ParagraphNextTutorialPropertyPath);
                m_TutorialButtonText = tutorialSwitchParagraph.FindPropertyRelative(k_ParagraphNextTutorialButtonTextPropertyPath);
            }
            else
            {
                m_NextTutorial = null;
                m_TutorialButtonText = null;
            }
        }

        public override void OnInspectorGUI()
        {
            if (!string.IsNullOrEmpty(m_WarningMessage))
            {
                EditorGUILayout.HelpBox(m_WarningMessage, MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();

            s_ForceOldInspector = EditorGUILayout.Toggle(Localization.Tr("Force default inspector"), s_ForceOldInspector);
            EditorGUILayout.Space(10);
            if (s_ForceOldInspector)
            {
                base.OnInspectorGUI();
            }
            else
            {
                DrawSimplifiedInspector();
            }

            if (EditorGUI.EndChangeCheck())
            {
                TutorialWindow.GetWindow().ForceInititalizeTutorialAndPage();
            }
        }

        void DrawSimplifiedInspector()
        {
            EditorGUILayout.BeginVertical();

            if (m_Type != null)
            {
                EditorGUILayout.LabelField(Localization.Tr("Header Media Type"));
                m_HeaderMediaType = (HeaderMediaType)EditorGUILayout.EnumPopup(GUIContent.none, m_HeaderMediaType);
                m_Type.intValue = (int)m_HeaderMediaType;

                EditorGUILayout.Space(10);
            }

            RenderProperty(Localization.Tr("Media"), m_HeaderMediaType == HeaderMediaType.Image ? m_Image : m_Video);

            EditorGUILayout.Space(10);

            RenderProperty(Localization.Tr("Narrative Title"), m_NarrativeTitle);

            EditorGUILayout.Space(10);

            RenderProperty(Localization.Tr("Narrative Description"), m_NarrativeDescription);

            EditorGUILayout.Space(10);

            RenderProperty(Localization.Tr("Instruction Title"), m_InstructionTitle);

            EditorGUILayout.Space(10);

            RenderProperty(Localization.Tr("Instruction Description"), m_InstructionDescription);

            if (m_CriteriaCompletion != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField(Localization.Tr("Completion Criteria"));
                EditorGUILayout.PropertyField(m_CriteriaCompletion, GUIContent.none);
                EditorGUILayout.PropertyField(m_Criteria, GUIContent.none);
            }

            if (m_NextTutorial != null)
            {
                EditorGUILayout.Space(10);
                RenderProperty(Localization.Tr("Next Tutorial"), m_NextTutorial);
                RenderProperty(Localization.Tr("Next Tutorial button text"), m_TutorialButtonText);
            }

            EditorStyles.label.wordWrap = true;

            //DrawLabelWithImage(Localization.Tr("Custom Callbacks");
            EditorGUILayout.BeginHorizontal();

            s_ShowEvents = EditorGUILayout.Foldout(s_ShowEvents, s_EventsSectionTitle);
            if (k_IsAuthoringMode && GUILayout.Button(Localization.Tr("Create Callback Handler")))
            {
                CreateCallbackHandlerScript("TutorialCallbacks.cs");
                InitializeEventWithDefaultData(m_OnBeforePageShown);
                InitializeEventWithDefaultData(m_OnAfterPageShown);
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space(10);
            if (s_ShowEvents)
            {
                if (TutorialEditorUtils.EventIsNotInState(m_OnBeforePageShown, UnityEngine.Events.UnityEventCallState.EditorAndRuntime))
                {
                    TutorialEditorUtils.RenderEventStateWarning();
                    EditorGUILayout.Space(8);
                }
                RenderEventProperty(s_OnBeforeEventsTitle, m_OnBeforePageShown);
                EditorGUILayout.Space(5);
                if (TutorialEditorUtils.EventIsNotInState(m_OnAfterPageShown, UnityEngine.Events.UnityEventCallState.EditorAndRuntime))
                {
                    TutorialEditorUtils.RenderEventStateWarning();
                    EditorGUILayout.Space(8);
                }
                RenderEventProperty(s_OnAfterEventsTitle, m_OnAfterPageShown);
                EditorGUILayout.Space(10);
            }

            RenderProperty(Localization.Tr("Enable Masking"), m_MaskingSettings);

            EditorGUILayout.EndVertical();

            DrawPropertiesExcluding(serializedObject, k_PropertiesToHide);

            serializedObject.ApplyModifiedProperties();
        }

        void RenderProperty(string name, SerializedProperty property)
        {
            if (property == null) { return; }
            EditorGUILayout.LabelField(name);
            EditorGUILayout.PropertyField(property, GUIContent.none);
        }

        void RenderEventProperty(GUIContent nameAndTooltip, SerializedProperty property)
        {
            if (property == null) { return; }
            EditorGUILayout.LabelField(nameAndTooltip);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(property);
        }

        void InitializeEventWithDefaultData(SerializedProperty eventProperty)
        {
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/IET/TutorialCallbacks.asset");
            //[TODO] Add listeners here if they are empty (?)
            ForceCallbacksListenerTarget(eventProperty, so);
            ForceCallbacksListenerState(eventProperty, UnityEngine.Events.UnityEventCallState.EditorAndRuntime);
        }

        /// <summary>
        /// Forces all callbacks of a UnityEvent (or derived class) to use a specific state
        /// </summary>
        /// <param name="eventProperty">A UnityEvent (or derived class) property</param>
        /// <param name="state"></param>
        void ForceCallbacksListenerState(SerializedProperty eventProperty, UnityEngine.Events.UnityEventCallState state)
        {
            SerializedProperty persistentCalls = eventProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");
            for (int i = 0; i < persistentCalls.arraySize; i++)
            {
                persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_CallState").intValue = (int)state;
                serializedObject.ApplyModifiedProperties();
            }
        }

        void ForceCallbacksListenerTarget(SerializedProperty eventProperty, UnityEngine.Object target)
        {
            SerializedProperty persistentCalls = eventProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");
            for (int i = 0; i < persistentCalls.arraySize; i++)
            {
                persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Target").objectReferenceValue = target;
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Creates an example callback handler script from a template script.
        /// </summary>
        /// <param name="templateFile">
        /// Template file name, must exist in "Packages/com.unity.learn.iet-framework.authoring/.TemplateAssets" folder.
        /// </param>
        /// <param name="targetDir">Use null to open a dialog for choosing the destination.</param>
        internal static void CreateCallbackHandlerScript(string templateFile, string targetDir = null)
        {
            // TODO preferably these template assets should reside in the authoring package
            var templatePath = $"Packages/com.unity.learn.iet-framework/.TemplateAssets/{templateFile}";
            if (!File.Exists(templatePath))
            {
                Debug.LogError($"Template file '{templateFile}' does not exist.");
                return;
            }

            targetDir = targetDir ??
                EditorUtility.OpenFolderPanel(
                Localization.Tr("Choose Folder for the Callback Handler Files"),
                Application.dataPath,
                string.Empty
                );

            try
            {
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                IsCreatingScriptableObject = true;

                // TODO preferably use the following which would allow renaming the file immediately to user's liking
                // and utilising template script features.
                //ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, templateFile);
                File.Copy(templatePath, Path.Combine(targetDir, templateFile));
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                IsCreatingScriptableObject = false;
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnScriptsReloaded()
        {
            if (!IsCreatingScriptableObject)
                return;

            IsCreatingScriptableObject = false;
            const string className = "TutorialCallbacks";
            const string methodName = "CreateInstance";
            var type = Assembly.Load("Assembly-CSharp").GetType(className);
            if (type == null)
            {
                Debug.LogError($"{className} not found from Assembly-CSharp.");
                return;
            }
            var method = type.GetMethod("CreateInstance");
            if (method == null)
            {
                Debug.LogError($"{methodName} not found from {className}.");
                return;
            }

            method.Invoke(null, null);
        }
    }
}
