using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.EditorCoroutines.Editor;

using UnityObject = UnityEngine.Object;
using static Unity.InteractiveTutorials.RichTextParser;

namespace Unity.InteractiveTutorials
{
    public sealed class TutorialWindow : EditorWindowProxy
    {
        // IMPORTANT this can be removed only in 2.0.
        // PVS will fail on API check if we remove this without a major version bump.
        public AllStylesHACK allTutorialStyles;

        const int k_MinWidth = 300;
        const int k_MinHeight = 300;
        const string UIAssetPath = "Packages/com.unity.learn.iet-framework/Framework/UIElementsViews";

        int currentEditorLanguage = 0;

        static TutorialWindow instance;

        List<TutorialParagraphView> m_Paragraphs = new List<TutorialParagraphView>();
        int[] m_Indexes;
        [SerializeField]
        List<TutorialParagraphView> m_AllParagraphs = new List<TutorialParagraphView>();

        internal static readonly float s_AuthoringModeToolbarButtonWidth = 115;

        string m_NextButtonText = "";
        string m_BackButtonText = "";

        static readonly bool s_AuthoringMode = ProjectMode.IsAuthoringMode();

        static readonly GUIContent k_WindowTitleContent = new GUIContent(Localization.Tr("Tutorials"));

        static readonly GUIContent k_HomePromptTitle = new GUIContent(Localization.Tr("Return to Tutorials?"));
        static readonly GUIContent k_HomePromptText = new GUIContent(Localization.Tr(
            "Returning to the Tutorial Selection means exiting the tutorial and losing all of your progress\n" +
            "Do you wish to continue?")
        );

        static readonly GUIContent k_PromptYes = new GUIContent(Localization.Tr("Yes"));
        static readonly GUIContent k_PromptNo = new GUIContent(Localization.Tr("No"));
        static readonly GUIContent k_PromptOk = new GUIContent(Localization.Tr("OK"));

        // Unity's menu guide convetion: text in italics, '>' used as a separator
        // TODO EditorUtility.DisplayDialog doesn't support italics so cannot use rich text here.
        static readonly string k_MenuPathGuide =
            Localization.Tr(TutorialWindowMenuItem.Menu) + " > " +
            Localization.Tr(TutorialWindowMenuItem.Item);

        // TODO experimenting with UX that never shows the exit dialog, can be removed for good if deemed good.
        //static readonly GUIContent k_ExitPromptTitle = new GUIContent(Localization.Tr("Exit Tutorial?"));
        //static readonly GUIContent k_ExitPromptText = new GUIContent(
        //    Localization.Tr($"You are about to exit the tutorial and lose all of your progress.\n\n") +
        //    Localization.Tr($"Do you wish to exit?")
        //);

        static readonly GUIContent k_TabClosedDialogTitle = new GUIContent(Localization.Tr("Close Tutorials"));
        static readonly GUIContent k_TabClosedDialogText = new GUIContent(Localization.Tr(
            $"You can find the tutorials later from the menu by choosing {k_MenuPathGuide}."
        ));

        internal Tutorial currentTutorial;

        internal static TutorialWindow CreateWindowAndLoadLayout()
        {
            instance = CreateWindow();
            var readme = FindReadme();
            if (readme != null)
                readme.LoadTutorialProjectLayout();
            return instance;
        }

        internal static TutorialWindow GetWindow()
        {
            if (instance == null)
                instance = CreateWindowAndLoadLayout();
            return instance;
        }

        internal static TutorialWindow CreateWindow()
        {
            instance = GetWindow<TutorialWindow>(k_WindowTitleContent.text);
            instance.minSize = new Vector2(k_MinWidth, k_MinHeight);
            return instance;
        }

        internal TutorialContainer readme
        {
            get { return m_Readme; }
            set
            {
                if (m_Readme)
                    m_Readme.Modified -= OnTutorialContainerModified;

                var oldReadme = m_Readme;
                m_Readme = value;
                if (m_Readme)
                {
                    if (oldReadme != m_Readme)
                        FetchTutorialStates();

                    m_Readme.Modified += OnTutorialContainerModified;
                }
            }
        }
        [SerializeField] TutorialContainer m_Readme;

        TutorialContainer.Section[] Cards => readme?.Sections ?? new TutorialContainer.Section[0];

        bool canMoveToNextPage =>
            currentTutorial != null && currentTutorial.currentPage != null &&
            (currentTutorial.currentPage.allCriteriaAreSatisfied ||
                currentTutorial.currentPage.hasMovedToNextPage);

        bool maskingEnabled
        {
            get
            {
                var forceDisableMask = EditorPrefs.GetBool("Unity.InteractiveTutorials.forceDisableMask", false);
                return !forceDisableMask && (m_MaskingEnabled || !s_AuthoringMode);
            }
            set { m_MaskingEnabled = value; }
        }
        [SerializeField]
        bool m_MaskingEnabled = true;

        TutorialStyles styles { get { return TutorialProjectSettings.instance.TutorialStyle; } }

        [SerializeField]
        int m_FarthestPageCompleted = -1;

        [SerializeField]
        bool m_PlayModeChanging;

        internal VideoPlaybackManager videoPlaybackManager { get; } = new VideoPlaybackManager();

        internal bool showStartHereMarker;
        internal bool showTabClosedDialog = true;
        public VisualElement videoBoxElement;

        void OnTutorialContainerModified()
        {
            // Update the tutorial content in real-time when changed
            OnEnable();
        }

        void TrackPlayModeChanging(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.ExitingPlayMode:
                    m_PlayModeChanging = true;
                    break;
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                    m_PlayModeChanging = false;
                    break;
            }
        }

        void OnFocus()
        {
            readme = FindReadme();
        }

        public void UpdateVideoFrame(Texture newTexture)
        {
            rootVisualElement.Q("TutorialMedia").style.backgroundImage = Background.FromTexture2D((Texture2D)newTexture);
        }

        void UpdateTutorialHeader(TextElement contextText, TextElement titleText, VisualElement backDrop)//, Button exitBtn)
        {
            if (currentTutorial == null && readme && readme.ProjectName.Untranslated.IsNotNullOrEmpty()) // TODO Use ProjectName(.Translated) when localization fixed
            {
                UpdateHeaderNow(contextText, titleText, backDrop);
            }
            else
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(WaitForReadmeAndUpdateHeader(contextText, titleText, backDrop));
            }
        }

        IEnumerator WaitForReadmeAndUpdateHeader(TextElement contextText, TextElement titleText, VisualElement backDrop)
        {
            float waitForMax = 5f;
            while ((!readme || readme.ProjectName.Untranslated.IsNullOrEmpty() && waitForMax > 0f)) // TODO Use ProjectName(.Translated) when localization fixed
            {
                waitForMax -= Time.deltaTime;
                yield return null;
            }
            UpdateHeaderNow(contextText, titleText, backDrop);
        }

        void UpdateHeaderNow(TextElement contextText, TextElement titleText, VisualElement backDrop)
        {
            var context = currentTutorial != null ? "TUTORIAL" : "TUTORIALS";
            var title = (currentTutorial != null
                ? currentTutorial.tutorialTitle
                : readme?.ProjectName.Untranslated)  // TODO Use ProjectName(.Translated) when localization fixed
                ?? string.Empty;
            var bgTex = readme?.HeaderBackground;
            // For now drawing header only for Readme
            if (readme)
            {
                contextText.text = Localization.Tr(context);
                titleText.text = Localization.Tr(title);
                backDrop.style.backgroundImage = bgTex;
            }
        }

        void ScrollToTop()
        {
            ((ScrollView)this.rootVisualElement.Q("TutorialContainer").ElementAt(0)).scrollOffset = Vector2.zero;
        }

        void ShowCurrentTutorialContent()
        {
            if (!m_AllParagraphs.Any() || !currentTutorial)
                return;
            if (m_AllParagraphs.Count() <= currentTutorial.currentPageIndex)
                return;

            ScrollToTop();

            TutorialParagraph paragraph = null;
            TutorialParagraph narrativeParagraph = null;
            Tutorial endLink = null;
            string endText = "";
            string pageTitle = "";

            foreach (TutorialParagraph para in currentTutorial.currentPage.paragraphs)
            {
                if (para.type == ParagraphType.SwitchTutorial)
                {
                    endLink = para.m_Tutorial;
                    endText = para.m_TutorialButtonText;
                }
                if (para.type == ParagraphType.Narrative)
                {
                    narrativeParagraph = para;
                    if (!string.IsNullOrEmpty(para.summary))
                        pageTitle = para.summary;
                }
                if (para.type == ParagraphType.Instruction)
                {
                    if (!string.IsNullOrEmpty(para.summary))
                        pageTitle = para.summary;
                    paragraph = para;
                }
                if (para.type == ParagraphType.Image)
                {
                    rootVisualElement.Q("TutorialMedia").style.backgroundImage = para.image;
                }
                if (para.type == ParagraphType.Video)
                {
                    rootVisualElement.Q("TutorialMedia").style.backgroundImage = videoPlaybackManager.GetTextureForVideoClip(para.video);
                }
            }

            Button linkButton = rootVisualElement.Q<Button>("LinkButton");
            if (endLink != null)
            {
                linkButton.clickable.clicked += () => TutorialManager.instance.StartTutorial(endLink);
                linkButton.text = Localization.Tr(endText);
                ShowElement(linkButton);
            }
            else
            {
                HideElement(linkButton);
            }
            rootVisualElement.Q<Label>("TutorialTitle").text = Localization.Tr(pageTitle);
            if (paragraph != null)
            {
                if (string.IsNullOrEmpty(paragraph.InstructionTitle) && string.IsNullOrEmpty(paragraph.InstructionText))
                {
                    // hide instruction box if empty title
                    HideElement("InstructionContainer");
                }
                else
                {
                    // populate instruction box
                    ShowElement("InstructionContainer");
                    if (string.IsNullOrEmpty(paragraph.InstructionTitle))
                    {
                        HideElement("InstructionTitle");
                    }
                    else
                    {
                        ShowElement("InstructionTitle");
                        rootVisualElement.Q<Label>("InstructionTitle").text = Localization.Tr(paragraph.InstructionTitle);
                    }
                    RichTextToVisualElements(paragraph.InstructionText, rootVisualElement.Q("InstructionDescription"));
                }
            }
            else
            {
                HideElement("InstructionContainer");
            }

            if (narrativeParagraph != null)
            {
                RichTextToVisualElements(narrativeParagraph.Description, rootVisualElement.Q("TutorialStepBox1"));
            }
        }

        // Sets the instruction highlight to green or blue and toggles between arrow and checkmark
        void FixHighlight(bool again = false)
        {
            if (canMoveToNextPage)
            {
                ShowElement("InstructionHighlightGreen");
                HideElement("InstructionHighlightBlue");
                ShowElement("InstructionCheckmark");
                HideElement("InstructionArrow");
            }
            else
            {
                HideElement("InstructionHighlightGreen");
                ShowElement("InstructionHighlightBlue");
                HideElement("InstructionCheckmark");
                ShowElement("InstructionArrow");
            }
            if (again) return;
            EditorApplication.delayCall += () =>
            {
                FixHighlight(true);
            };
        }

        void UpdateNextButton() => SetNextButtonEnabled(canMoveToNextPage);
        void UpdateNextButton(Criterion completedCriterion) => SetNextButtonEnabled(true);

        void SetNextButtonEnabled(bool enable)
        {
            FixHighlight();
            EditorApplication.delayCall += () => rootVisualElement.Q("NextButton").SetEnabled(enable);
        }

        void CreateTutorialMenuCards(VisualTreeAsset vistree, string cardElementName, string linkCardElementName, VisualElement cardContainer)
        {
            var cards = Cards.OrderBy(card => card.OrderInView).ToArray();

            // "Start Here" marker, will be replaced by tooltip when we have such.
            // showStartHereMarker = !tutorials.Any(t => t.section.tutorialCompleted);
            cardContainer.style.alignItems = Align.Center;

            for (int index = 0; index < cards.Length; ++index)
            {
                var card = cards[index];

                // If it's a tutorial, use tutorial card - otherwise link card
                VisualElement cardElement = vistree.CloneTree().Q("TutorialsContainer").Q(card.IsTutorial ? cardElementName : linkCardElementName);
                cardElement.Q<Label>("TutorialName").text = Localization.Tr(card.Heading.Untranslated); // TODO use Heading(.Translated) when localization fixed
                cardElement.Q<Label>("TutorialDescription").text = Localization.Tr(card.Text.Untranslated); // TODO use Heading(.Translated) when localization fixed
                if (card.IsTutorial)
                {
                    cardElement.RegisterCallback((MouseUpEvent evt) =>
                    {
                        card.StartTutorial();
                        //ShowCurrentTutorialContent();
                    });
                }
                if (!string.IsNullOrEmpty(card.Url))
                {
                    AnalyticsHelper.SendExternalReferenceImpressionEvent(card.Url, card.Heading.Untranslated, card.LinkText, card.TutorialId);

                    cardElement.RegisterCallback((MouseUpEvent evt) =>
                    {
                        card.OpenUrl();
                    });
                }

                EditorApplication.delayCall += () =>
                {
                    EditorApplication.delayCall += () =>
                    {
                        // HACK: needs two delaycalls or GenesisHelper gives 404
                        FetchTutorialStates();
                    };
                };

                cardElement.Q<Label>("CompletionStatus").text = cards[index].TutorialCompleted ? Localization.Tr("COMPLETED") : "";
                SetElementVisible(cardElement.Q("TutorialCheckmark"), cards[index].TutorialCompleted);

                EditorCoroutineUtility.StartCoroutineOwnerless(EnforceCheckmark(cards[index], cardElement));

                if (card.Image != null)
                {
                    cardElement.Q("TutorialImage").style.backgroundImage = Background.FromTexture2D(card.Image);
                }
                cardElement.tooltip = card.IsTutorial
                    ? Localization.Tr("Tutorial: ") + card.Text.Untranslated // TODO Use Text(.Translated) when localization fixed
                    : card.Url;
                cardContainer.Add(cardElement);
            }
        }

        IEnumerator EnforceCheckmark(TutorialContainer.Section section, VisualElement element)
        {
            float seconds = 4f;
            while (seconds > 0f && !DoneFetchingTutorialStates)
            {
                yield return null;
                seconds -= Time.deltaTime;
            }
            element.Q<Label>("CompletionStatus").text = section.TutorialCompleted ? Localization.Tr("COMPLETED") : "";
            SetElementVisible(element.Q("TutorialCheckmark"), section.TutorialCompleted);
        }

        void RenderVideoIfPossible()
        {
            var paragraphType = currentTutorial?.currentPage?.paragraphs.ElementAt(0).type;
            if (paragraphType == ParagraphType.Video || paragraphType == ParagraphType.Image)
            {
                var pageCompleted = currentTutorial.currentPageIndex <= m_FarthestPageCompleted;
                var previousTaskState = true;
                GetCurrentParagraph().ElementAt(0).Draw(ref previousTaskState, pageCompleted);
            }
        }

        void OnEnable()
        {
            rootVisualElement.Clear();
            currentEditorLanguage = EditorPrefs.GetInt("EditorLanguage");
            instance = this;

            Criterion.criterionCompleted += UpdateNextButton;

            IMGUIContainer imguiToolBar = new IMGUIContainer(OnGuiToolbar);
            IMGUIContainer videoBox = new IMGUIContainer(RenderVideoIfPossible);
            videoBox.style.alignSelf = new StyleEnum<Align>(Align.Center);
            videoBox.name = "VideoBox";

            var root = rootVisualElement;
            var topBarAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UIAssetPath}/Main.uxml");
            var tutorialContentAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UIAssetPath}/TutorialContents.uxml");
            VisualElement tutorialImage = topBarAsset.CloneTree().Q("TutorialImage");
            VisualElement tutorialMenuCard = topBarAsset.CloneTree().Q("CardContainer");

            VisualElement tutorialContents = tutorialContentAsset.CloneTree().Q("TutorialEmptyContents");
            tutorialContents.style.flexGrow = 1f;
            VisualElement TutorialContentPage = tutorialContentAsset.CloneTree().Q("TutorialPageContainer");
            VisualElement TutorialTopBar = TutorialContentPage.Q("Header");

            VisualElement linkButton = topBarAsset.CloneTree().Q("LinkButton");

            VisualElement cardContainer = topBarAsset.CloneTree().Q("TutorialListScrollView");
            CreateTutorialMenuCards(topBarAsset, "CardContainer", "LinkCardContainer", cardContainer); //[TODO] be careful: this will also trigger analytics events even when you start a tutorial

            tutorialContents.Add(cardContainer);
            VisualElement topBarVisElement = topBarAsset.CloneTree().Q("TitleHeader");
            VisualElement footerBar = topBarAsset.CloneTree().Q("TutorialActions");

            TextElement titleElement = topBarVisElement.Q<TextElement>("TitleLabel");
            TextElement contextTextElement = topBarVisElement.Q<TextElement>("ContextLabel");

            UpdateTutorialHeader(contextTextElement, titleElement, topBarVisElement);

            root.Add(imguiToolBar);
            root.Add(TutorialTopBar);
            root.Add(videoBox);
            root.Add(topBarVisElement);
            root.Add(tutorialContents);

            StyleSheet rootstyle = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{UIAssetPath}/Main.uss");
            root.styleSheets.Add(rootstyle);

            VisualElement tutorialContainer = TutorialContentPage.Q("TutorialContainer");
            tutorialContainer.Add(linkButton);
            root.Add(tutorialContainer);

            footerBar.Q<Button>("PreviousButton").clicked += OnPreviousButtonClicked;
            footerBar.Q<Button>("NextButton").clicked += OnNextButtonClicked;

            // Set here in addition to CreateWindow() so that title of old saved layouts is overwritten.
            instance.titleContent = k_WindowTitleContent;

            videoPlaybackManager.OnEnable();

            GUIViewProxy.positionChanged += OnGUIViewPositionChanged;
            HostViewProxy.actualViewChanged += OnHostViewActualViewChanged;
            Tutorial.tutorialPagesModified += OnTutorialPagesModified;

            // test for page completion state changes (rather than criteria completion/invalidation directly)
            // so that page completion state will be up-to-date
            TutorialPage.criteriaCompletionStateTested += OnTutorialPageCriteriaCompletionStateTested;
            TutorialPage.tutorialPageMaskingSettingsChanged += OnTutorialPageMaskingSettingsChanged;
            TutorialPage.tutorialPageNonMaskingSettingsChanged += OnTutorialPageNonMaskingSettingsChanged;
            EditorApplication.playModeStateChanged -= TrackPlayModeChanging;
            EditorApplication.playModeStateChanged += TrackPlayModeChanging;

            SetUpTutorial();

            maskingEnabled = true;
            root.Add(footerBar);
            readme = FindReadme();
            EditorCoroutineUtility.StartCoroutineOwnerless(DelayedOnEnable());
        }

        void ExitClicked(MouseUpEvent mouseup)
        {
            SkipTutorial();
        }

        void SetIntroScreenVisible(bool visible)
        {
            if (visible)
            {
                ShowElement("TitleHeader");
                HideElement("TutorialActions");
                HideElement("Header");
                ShowElement("TutorialEmptyContents");
                // SHOW: tutorials
                // HIDE: tutorial steps
                HideElement("TutorialContainer");
                // Show card container
            }
            else
            {
                HideElement("TitleHeader");
                ShowElement("TutorialActions");
                VisualElement headerElement = rootVisualElement.Q("Header");
                ShowElement(headerElement);
                headerElement.Q<Label>("HeaderLabel").text = Localization.Tr(currentTutorial.tutorialTitle);
                headerElement.Q<Label>("StepCount").text = $"{currentTutorial.currentPageIndex + 1} / {currentTutorial.m_Pages.count}";
                headerElement.Q("Close").RegisterCallback<MouseUpEvent>(ExitClicked);
                //HideElement("TutorialImage");
                HideElement("TutorialEmptyContents");
                ShowElement("TutorialContainer");
                //ShowElement("VideoBox");
                // Hide card container
            }
            rootVisualElement.Q<Button>("PreviousButton").text = Localization.Tr(m_BackButtonText);
            rootVisualElement.Q<Button>("NextButton").text = Localization.Tr(m_NextButtonText);
        }

        void ShowElement(string name) => ShowElement(rootVisualElement.Q(name));
        void HideElement(string name) => HideElement(rootVisualElement.Q(name));

        static void ShowElement(VisualElement elem) => SetElementVisible(elem, true);
        static void HideElement(VisualElement elem) => SetElementVisible(elem, false);

        static void SetElementVisible(VisualElement elem, bool visible)
        {
            elem.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnDisable()
        {
            if (!m_PlayModeChanging)
            {
                AnalyticsHelper.TutorialEnded(TutorialConclusion.Quit);
            }

            Criterion.criterionCompleted -= UpdateNextButton;

            ClearTutorialListener();

            Tutorial.tutorialPagesModified -= OnTutorialPagesModified;
            TutorialPage.criteriaCompletionStateTested -= OnTutorialPageCriteriaCompletionStateTested;
            TutorialPage.tutorialPageMaskingSettingsChanged -= OnTutorialPageMaskingSettingsChanged;
            TutorialPage.tutorialPageNonMaskingSettingsChanged -= OnTutorialPageNonMaskingSettingsChanged;
            GUIViewProxy.positionChanged -= OnGUIViewPositionChanged;
            HostViewProxy.actualViewChanged -= OnHostViewActualViewChanged;

            videoPlaybackManager.OnDisable();

            ApplyMaskingSettings(false);

            // Play mode might trigger layout change (maximize on play) and closing of this window also.

            if (showTabClosedDialog && !TutorialManager.IsLoadingLayout && !m_PlayModeChanging)
            {
                // Without delayed call the Inspector appears completely black
                EditorApplication.delayCall += delegate
                {
                    EditorUtility.DisplayDialog(k_TabClosedDialogTitle.text, k_TabClosedDialogText.text, k_PromptOk.text);
                };
            }
        }

        void OnDestroy()
        {
            // TODO SkipTutorial();
        }

        void WindowForParagraph()
        {
            foreach (var p in m_Paragraphs)
            {
                p.SetWindow(instance);
            }
        }

        void OnHostViewActualViewChanged()
        {
            if (TutorialManager.IsLoadingLayout) { return; }
            // do not mask immediately in case unmasked GUIView doesn't exist yet
            // TODO disabled for now in order to get Welcome dialog masking working
            //QueueMaskUpdate();
        }

        void QueueMaskUpdate()
        {
            EditorApplication.update -= ApplyQueuedMask;
            EditorApplication.update += ApplyQueuedMask;
        }

        void OnTutorialPageCriteriaCompletionStateTested(TutorialPage sender)
        {
            if (currentTutorial == null || currentTutorial.currentPage != sender) { return; }

            foreach (var paragraph in m_Paragraphs)
            {
                paragraph.ResetState();
            }

            if (sender.allCriteriaAreSatisfied && sender.autoAdvanceOnComplete && !sender.hasMovedToNextPage)
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(GoToNextPageAfterDelay());
                return;
            }

            ApplyMaskingSettings(true);
        }

        IEnumerator GoToNextPageAfterDelay()
        {
            //TODO WaitForSecondsRealtime();
            float seconds = 0.5f;
            while (seconds > 0f)
            {
                seconds -= Time.deltaTime;
                yield return null;
            }
            if (currentTutorial.TryGoToNextPage())
            {
                UpdateNextButton();
                yield break;
            }
            ApplyMaskingSettings(true);
        }

        void SkipTutorial()
        {
            if (currentTutorial == null) { return; }

            switch (currentTutorial.skipTutorialBehavior)
            {
                case Tutorial.SkipTutorialBehavior.SameAsExitBehavior: ExitTutorial(false); break;
                case Tutorial.SkipTutorialBehavior.SkipToLastPage: currentTutorial.SkipToLastPage(); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        void ExitTutorial(bool completed)
        {
            switch (currentTutorial.exitBehavior)
            {
                case Tutorial.ExitBehavior.ShowHomeWindow:
                    if (completed)
                    {
                        HomeWindowProxy.ShowTutorials();
                    }
                    else if (
                        !IsInProgress() ||
                        EditorUtility.DisplayDialog(k_HomePromptTitle.text, k_HomePromptText.text, k_PromptYes.text, k_PromptNo.text))
                    {
                        HomeWindowProxy.ShowTutorials();
                        GUIUtility.ExitGUI();
                    }
                    return; // Return to avoid selecting asset on exit
                case Tutorial.ExitBehavior.CloseWindow:
                    // New behaviour: exiting resets and nullifies the current tutorial and shows the project's tutorials.
                    if (completed)
                    {
                        SetTutorial(null, false);
                        ResetTutorial();
                        TutorialManager.instance.RestoreOriginalState();
                    }
                    else
                    // TODO experimenting with UX that never shows the exit dialog, can be removed for good if deemed good.
                    //    if (!IsInProgress()
                    //    || EditorUtility.DisplayDialog(k_ExitPromptTitle.text, k_ExitPromptText.text, k_PromptYes.text, k_PromptNo.text))
                    {
                        SetTutorial(null, false);
                        ResetTutorial();
                        TutorialManager.instance.RestoreOriginalState();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // TODO new behaviour testing: assetSelectedOnExit was originally used for selecting
            // Readme but this is not required anymore as the TutorialWindow contains Readme's functionality.
            //if (currentTutorial?.assetSelectedOnExit != null)
            //    Selection.activeObject = currentTutorial.assetSelectedOnExit;

            //SaveTutorialStates();
        }

        void OnTutorialInitiated()
        {
            if (!currentTutorial) { return; }

            AnalyticsHelper.TutorialStarted(currentTutorial);
            GenesisHelper.LogTutorialStarted(currentTutorial.lessonId);
            CreateTutorialViews();
        }

        void OnTutorialCompleted(bool exitTutorial)
        {
            if (!currentTutorial) { return; }

            AnalyticsHelper.TutorialEnded(TutorialConclusion.Completed);
            GenesisHelper.LogTutorialEnded(currentTutorial.lessonId);
            MarkTutorialCompleted(currentTutorial.lessonId, currentTutorial.completed);

            if (!exitTutorial) { return; }
            ExitTutorial(currentTutorial.completed);
        }

        internal void CreateTutorialViews()
        {
            if (currentTutorial == null) return; // HACK
            m_AllParagraphs.Clear();
            foreach (var page in currentTutorial.pages)
            {
                if (page == null) { continue; }

                var instructionIndex = 0;
                foreach (var paragraph in page.paragraphs)
                {
                    if (paragraph.type == ParagraphType.Instruction)
                    {
                        ++instructionIndex;
                    }
                    m_AllParagraphs.Add(new TutorialParagraphView(paragraph, instance, styles.OrderedListDelimiter, styles.UnorderedListBullet, instructionIndex));
                }
            }
        }

        List<TutorialParagraphView> GetCurrentParagraph()
        {
            if (m_Indexes == null || m_Indexes.Length != currentTutorial.pageCount)
            {
                // Update page to paragraph index
                m_Indexes = new int[currentTutorial.pageCount];
                var pageIndex = 0;
                var paragraphIndex = 0;
                foreach (var page in currentTutorial.pages)
                {
                    m_Indexes[pageIndex++] = paragraphIndex;
                    if (page != null)
                        paragraphIndex += page.paragraphs.Count();
                }
            }

            List<TutorialParagraphView> tmp = new List<TutorialParagraphView>();
            if (m_Indexes.Length > 0)
            {
                var endIndex = currentTutorial.currentPageIndex + 1 > currentTutorial.pageCount - 1 ? m_AllParagraphs.Count : m_Indexes[currentTutorial.currentPageIndex + 1];
                for (int i = m_Indexes[currentTutorial.currentPageIndex]; i < endIndex; i++)
                {
                    tmp.Add(m_AllParagraphs[i]);
                }
            }
            return tmp;
        }

        // TODO 'page' and 'index' unused
        internal void PrepareNewPage(TutorialPage page = null, int index = 0)
        {
            if (currentTutorial == null) return;
            if (!m_AllParagraphs.Any())
            {
                CreateTutorialViews();
            }
            m_Paragraphs.Clear();

            if (currentTutorial.currentPage == null)
            {
                m_NextButtonText = string.Empty;
            }
            else
            {
                m_NextButtonText = IsLastPage()
                    ? currentTutorial.currentPage.doneButton
                    : currentTutorial.currentPage.nextButton;
            }
            m_BackButtonText = IsFirstPage() ? "All Tutorials" : "Back";

            m_Paragraphs = GetCurrentParagraph();

            m_Paragraphs.TrimExcess();

            WindowForParagraph();
            ShowCurrentTutorialContent(); // HACK
        }

        internal void ForceInititalizeTutorialAndPage()
        {
            m_FarthestPageCompleted = -1;

            CreateTutorialViews();
            PrepareNewPage();
        }

        static void OpenLoadTutorialDialog()
        {
            string assetPath = EditorUtility.OpenFilePanel("Load a Tutorial", "Assets", "asset");
            if (string.IsNullOrEmpty(assetPath)) { return; }
            assetPath = string.Format("Assets{0}", assetPath.Substring(Application.dataPath.Length));
            TutorialManager.instance.StartTutorial(AssetDatabase.LoadAssetAtPath<Tutorial>(assetPath));
            GUIUtility.ExitGUI();
        }

        bool IsLastPage() { return currentTutorial != null && currentTutorial.pageCount - 1 <= currentTutorial.currentPageIndex; }

        bool IsFirstPage() { return currentTutorial != null && currentTutorial.currentPageIndex == 0; }

        // Returns true if some real progress has been done (criteria on some page finished).
        bool IsInProgress()
        {
            return currentTutorial
                ?.pages.Any(pg => pg.paragraphs.Any(p => p.criteria.Any() && pg.allCriteriaAreSatisfied))
                ?? false;
        }

        void ClearTutorialListener()
        {
            if (currentTutorial == null) { return; }

            currentTutorial.tutorialInitiated -= OnTutorialInitiated;
            currentTutorial.tutorialCompleted -= OnTutorialCompleted;
            currentTutorial.pageInitiated -= OnShowPage;
            currentTutorial.StopTutorial();
        }

        internal void SetTutorial(Tutorial tutorial, bool reload)
        {
            ClearTutorialListener();

            currentTutorial = tutorial;
            if (currentTutorial != null)
            {
                if (reload)
                {
                    currentTutorial.ResetProgress();
                }
                m_AllParagraphs.Clear();
                m_Paragraphs.Clear();
            }

            ApplyMaskingSettings(currentTutorial != null);

            SetUpTutorial();
        }

        void SetUpTutorial()
        {
            // bail out if this instance no longer exists such as when e.g., loading a new window layout
            if (this == null || currentTutorial == null || currentTutorial.currentPage == null) { return; }

            if (currentTutorial.currentPage != null)
            {
                currentTutorial.currentPage.Initiate();
            }

            currentTutorial.tutorialInitiated += OnTutorialInitiated;
            currentTutorial.tutorialCompleted += OnTutorialCompleted;
            currentTutorial.pageInitiated += OnShowPage;

            if (m_AllParagraphs.Any())
            {
                PrepareNewPage();
                return;
            }
            ForceInititalizeTutorialAndPage();
        }

        void ApplyQueuedMask()
        {
            if (IsParentNull()) { return; }

            EditorApplication.update -= ApplyQueuedMask;
            ApplyMaskingSettings(true);
        }

        IEnumerator DelayedOnEnable()
        {
            yield return null;
    
            do
            {
                yield return null;
                videoBoxElement = rootVisualElement.Q("TutorialMediaContainer");
            } while (videoBoxElement == null);


            if (currentTutorial == null)
            {
                if (videoBoxElement != null )
                {
                    UIElementsUtils.Hide(videoBoxElement);
                }
            }
            videoPlaybackManager.OnEnable();
        }

        void OnGuiToolbar()
        {
            SetIntroScreenVisible(currentTutorial == null);
            if (s_AuthoringMode)
                ToolbarGUI();
        }

        void OnPreviousButtonClicked()
        {
            if (IsFirstPage())
            {
                SkipTutorial();
            }
            else
            {
                currentTutorial.GoToPreviousPage();
                UpdateNextButton();
            }
        }

        void OnNextButtonClicked()
        {
            if (currentTutorial)
                currentTutorial.TryGoToNextPage();

            UpdateNextButton();
            ShowCurrentTutorialContent();
        }

        // Resets the contents of this window. Use this before saving layouts for tutorials.
        internal void Reset()
        {
            m_AllParagraphs.Clear();
            SetTutorial(null, true);
            readme = null;
        }

        void ToolbarGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

            bool Button(string text)
            {
                return GUILayout.Button(text, EditorStyles.toolbarButton, GUILayout.MaxWidth(s_AuthoringModeToolbarButtonWidth));
            }

            using (new EditorGUI.DisabledScope(currentTutorial == null))
            {
                if (Button("Select Tutorial"))
                {
                    Selection.activeObject = currentTutorial;
                }

                using (new EditorGUI.DisabledScope(currentTutorial?.currentPage == null))
                {
                    if (Button("Select Page"))
                    {
                        Selection.activeObject = currentTutorial.currentPage;
                    }
                }

                if (Button("Skip To End"))
                {
                    currentTutorial.SkipToLastPage();
                }
            }

            GUILayout.FlexibleSpace();

            if (Button("Run Startup Code"))
            {
                UserStartupCode.RunStartupCode();
            }

            using (new EditorGUI.DisabledScope(currentTutorial == null))
            {
                EditorGUI.BeginChangeCheck();
                maskingEnabled = GUILayout.Toggle(
                    maskingEnabled, "Preview Masking", EditorStyles.toolbarButton,
                    GUILayout.MaxWidth(s_AuthoringModeToolbarButtonWidth)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    ApplyMaskingSettings(true);
                    GUIUtility.ExitGUI();
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void OnTutorialPagesModified(Tutorial sender)
        {
            if (sender == null || currentTutorial == null || currentTutorial != sender) { return; }

            CreateTutorialViews();

            ApplyMaskingSettings(true);
        }

        void OnTutorialPageMaskingSettingsChanged(TutorialPage sender)
        {
            if (sender == null || currentTutorial == null || currentTutorial.currentPage != sender) { return; }

            ApplyMaskingSettings(true);
        }

        void OnTutorialPageNonMaskingSettingsChanged(TutorialPage sender)
        {
            if (sender == null || currentTutorial == null || currentTutorial.currentPage != sender) { return; }
            Repaint();
        }

        void OnShowPage(TutorialPage page, int index)
        {
            page.RaiseOnBeforePageShownEvent();
            m_FarthestPageCompleted = Mathf.Max(m_FarthestPageCompleted, index - 1);
            ApplyMaskingSettings(true);

            AnalyticsHelper.PageShown(page, index);
            PrepareNewPage();

            videoPlaybackManager.ClearCache();
            page.RaiseOnAfterPageShownEvent();
        }

        void OnGUIViewPositionChanged(UnityObject sender)
        {
            if (TutorialManager.IsLoadingLayout || sender.GetType().Name == "TooltipView") { return; }

            ApplyMaskingSettings(true);
        }

        void ApplyMaskingSettings(bool applyMask)
        {
            // TODO IsParentNull() probably not needed anymore as TutorialWindow is always parented in the current design & layout.
            if (!applyMask || !maskingEnabled || currentTutorial == null
                || currentTutorial.currentPage == null || IsParentNull() || TutorialManager.IsLoadingLayout)
            {
                MaskingManager.Unmask();
                InternalEditorUtility.RepaintAllViews();
                return;
            }

            MaskingSettings maskingSettings = currentTutorial.currentPage.currentMaskingSettings;

            try
            {
                if (maskingSettings == null || !maskingSettings.enabled)
                {
                    MaskingManager.Unmask();
                }
                else
                {
                    bool foundAncestorProperty;
                    var unmaskedViews = UnmaskedView.GetViewsAndRects(maskingSettings.unmaskedViews, out foundAncestorProperty);
                    if (foundAncestorProperty)
                    {
                        // Keep updating mask when target property is not unfolded
                        QueueMaskUpdate();
                    }

                    if (currentTutorial.currentPageIndex <= m_FarthestPageCompleted)
                    {
                        unmaskedViews = new UnmaskedView.MaskData();
                    }

                    UnmaskedView.MaskData highlightedViews;

                    // if the current page contains no instructions, assume unmasked views should be highlighted because they are called out in narrative text
                    if (unmaskedViews.Count > 0 && !currentTutorial.currentPage.paragraphs.Any(p => p.type == ParagraphType.Instruction))
                    {
                        highlightedViews = (UnmaskedView.MaskData)unmaskedViews.Clone();
                    }
                    else if (canMoveToNextPage) // otherwise, if the current page is completed, highlight this window
                    {
                        highlightedViews = new UnmaskedView.MaskData();
                        highlightedViews.AddParentFullyUnmasked(this);
                    }
                    else // otherwise, highlight manually specified control rects if there are any
                    {
                        var unmaskedControls = new List<GUIControlSelector>();
                        var unmaskedViewsWithControlsSpecified =
                            maskingSettings.unmaskedViews.Where(v => v.GetUnmaskedControls(unmaskedControls) > 0).ToArray();
                        // if there are no manually specified control rects, highlight all unmasked views
                        highlightedViews = UnmaskedView.GetViewsAndRects(
                            unmaskedViewsWithControlsSpecified.Length == 0 ?
                            maskingSettings.unmaskedViews : unmaskedViewsWithControlsSpecified
                        );
                    }

                    // ensure tutorial window's HostView and tooltips are not masked
                    unmaskedViews.AddParentFullyUnmasked(this);
                    unmaskedViews.AddTooltipViews();

                    // tooltip views should not be highlighted
                    highlightedViews.RemoveTooltipViews();

                    MaskingManager.Mask(
                        unmaskedViews,
                        styles == null ? Color.magenta * new Color(1f, 1f, 1f, 0.8f) : styles.MaskingColor,
                        highlightedViews,
                        styles == null ? Color.cyan * new Color(1f, 1f, 1f, 0.8f) : styles.HighlightColor,
                        styles == null ? new Color(1, 1, 1, 0.5f) : styles.BlockedInteractionColor,
                        styles == null ? 3f : styles.HighlightThickness
                    );
                }
            }
            catch (ArgumentException e)
            {
                if (s_AuthoringMode)
                    Debug.LogException(e, currentTutorial.currentPage);
                else
                    Console.WriteLine(StackTraceUtility.ExtractStringFromException(e));

                MaskingManager.Unmask();
            }
            finally
            {
                InternalEditorUtility.RepaintAllViews();
            }
        }

        void ResetTutorialOnDelegate(PlayModeStateChange playmodeChange)
        {
            switch (playmodeChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    EditorApplication.playModeStateChanged -= ResetTutorialOnDelegate;
                    ResetTutorial();
                    break;
            }
        }

        internal void ResetTutorial()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.playModeStateChanged += ResetTutorialOnDelegate;
                EditorApplication.isPlaying = false;
                return;
            }
            else if (!EditorApplication.isPlaying)
            {
                m_FarthestPageCompleted = -1;
                TutorialManager.instance.ResetTutorial();
            }
        }

        // Returns Readme iff one Readme exists in the project.
        public static TutorialContainer FindReadme()
        {
            var ids = AssetDatabase.FindAssets($"t:{typeof(TutorialContainer).FullName}");
            return ids.Length == 1
                ? (TutorialContainer)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]))
                : null;
        }

        float checkLanguageTick = 0f;

        void Update()
        {
            checkLanguageTick += Time.deltaTime;
            if (checkLanguageTick >= 1f)
            {
                checkLanguageTick = 0f;
                if (EditorPrefs.GetInt("EditorLanguage") != currentEditorLanguage)
                {
                    currentEditorLanguage = EditorPrefs.GetInt("EditorLanguage");
                    if (currentTutorial != null)
                    {
                        ShowCurrentTutorialContent();
                    }
                    else
                    {
                        ExitTutorial(false);
                    }
                }
            }
        }

        internal void MarkAllTutorialsUncompleted()
        {
            Cards.ToList().ForEach(s => MarkTutorialCompleted(s.TutorialId, false));
            // TODO Refresh the cards
        }

        bool DoneFetchingTutorialStates = false;

        // Fetches statuses from the web API
        internal void FetchTutorialStates()
        {
            DoneFetchingTutorialStates = false;
            GenesisHelper.GetAllTutorials((tutorials) =>
            {
                tutorials.ForEach(t => MarkTutorialCompleted(t.lessonId, t.status == "Finished"));
                DoneFetchingTutorialStates = true;
            });
        }

        void MarkTutorialCompleted(string lessonId, bool completed)
        {
            var sections = readme?.Sections ?? new TutorialContainer.Section[0];
            var section = Array.Find(sections, s => s.TutorialId == lessonId);
            if (section != null)
            {
                section.TutorialCompleted = completed;
                section.SaveState();
            }
        }
    }
}
