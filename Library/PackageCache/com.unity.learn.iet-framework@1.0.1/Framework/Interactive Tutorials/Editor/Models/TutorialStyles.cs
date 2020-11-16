using UnityEngine;

namespace Unity.InteractiveTutorials
{
    class TutorialStyles : ScriptableObject
    {
        public Color MaskingColor => m_MaskingColor;
        [SerializeField]
        Color m_MaskingColor = new Color32(0, 40, 53, 204);

        public Color HighlightColor => m_HighlightColor;
        [SerializeField]
        Color m_HighlightColor = new Color32(0, 198, 223, 255);

        public Color BlockedInteractionColor => m_BlockedInteractionColor;
        [SerializeField]
        Color m_BlockedInteractionColor = new Color(1, 1, 1, 0.5f);

        public float HighlightThickness => m_HighlightThickness;
        [SerializeField, Range(0f, 10f)]
        float m_HighlightThickness = 3f;

        [SerializeField, Range(0f, 10f)]
        float m_HighlightAnimationSpeed = 1.5f;

        [SerializeField, Range(0f, 10f)]
        float m_HighlightAnimationDelay = 5f;

        #region TODO Will be deprecated and deleted
        public string OrderedListDelimiter => m_OrderedListDelimiter;
        [SerializeField]
        string m_OrderedListDelimiter = ".";

        public string UnorderedListBullet => m_UnorderedListBullet;
        [SerializeField]
        string m_UnorderedListBullet = "\u2022";
        #endregion

        void OnEnable()
        {
            MaskingManager.highlightAnimationSpeed = m_HighlightAnimationSpeed;
            MaskingManager.highlightAnimationDelay = m_HighlightAnimationDelay;
        }

        void OnValidate()
        {
            MaskingManager.highlightAnimationSpeed = m_HighlightAnimationSpeed;
            MaskingManager.highlightAnimationDelay = m_HighlightAnimationDelay;
        }
    }

    #region TODO Will be deprecated and deleted
    class AllTutorialStyles
    {
        public static GUIStyle FindStyle(GUIStyle style, string name)
        {
            if (style == null)
            {
                style = GUI.skin.FindStyle(name);
            }
            return style;
        }

        static AllTutorialStyles()
        {
            headerLabel = FindStyle(headerLabel, s_HeaderStyleName);
            instructionLabel = FindStyle(instructionLabel, s_InstructionLabelStyleName);
            line = FindStyle(line, s_LineStyleName);
            listPrefix = FindStyle(listPrefix, s_ListPrefixStyleName);
            list = FindStyle(list, s_ListStyleName);
            progressLabel = FindStyle(progressLabel, s_ProgressLabelStyle);
            sectionTitleLabel = FindStyle(sectionTitleLabel, s_SectionTitleLabelStyle);
            theInBetweenText = FindStyle(theInBetweenText, s_TheInBetweenTextStyle);
            narrativeStyle = FindStyle(narrativeStyle, s_Narrative);
            switchTutorialStyle = FindStyle(switchTutorialStyle, s_SwitchTutorialStyleName);
            imageStyle = FindStyle(imageStyle, s_ImageStyle);
            videoStyle = FindStyle(videoStyle, s_VideoStyle);

            sectionTitleBackground = FindStyle(sectionTitleBackground, s_SectionTitleBackground);
            topBarBackground = FindStyle(topBarBackground, s_TopBarBackground);

            headerBGStyle = FindStyle(headerBGStyle, s_HeaderStyleBG);
            listBGStyle = FindStyle(listBGStyle, s_ListBG);
            theInBetweenTextNotActiveOrCompleted = FindStyle(theInBetweenTextNotActiveOrCompleted, s_TheInBetweenTextStyleNotActiveOrCompleted);
            completedElementBackground = FindStyle(completedElementBackground, s_CompletedElementBackgroundStyle);
            activeElementBackground = FindStyle(activeElementBackground, s_ActiveElementBackgroundStyle);
            inActiveElementBackground = FindStyle(inActiveElementBackground, s_InActiveElementBackgroundStyle);
            darkBackground = FindStyle(darkBackground, s_DarkBackground);
            bgTheInBetweenText = FindStyle(bgTheInBetweenText, s_BGTheInBetweenText);
            background = FindStyle(background, s_Background);

            footer = FindStyle(footer, s_Footer);
            paginationLabel = FindStyle(paginationLabel, s_PaginationLabel);
            nextButton = FindStyle(nextButton, s_NextButton);
            nextButtonDisabled = FindStyle(nextButtonDisabled, s_NextButtonDisabled);
            backButton = FindStyle(backButton, s_BackButton);
            backButtonDisabled = FindStyle(backButtonDisabled, s_BackButtonDisabled);
            progressBar = FindStyle(progressBar, s_ProgressBar);

            //iconButtonBack = FindStyle(,s_IconButtonBack);
            iconButtonReset = FindStyle(iconButtonReset, s_IconButtonReset);
            iconButtonHome = FindStyle(iconButtonHome, s_IconButtonHome);
            iconButtonClose = FindStyle(iconButtonClose, s_IconButtonClose);
            instructionLabelIconCompleted = FindStyle(instructionLabelIconCompleted, s_InstructionLabelIconStyleCompleted);
            instructionLabelIconNotCompleted = FindStyle(instructionLabelIconNotCompleted, s_InstructionLabelIconStyleNotCompleted);

            tutorialCard = FindStyle(tutorialCard, s_TutorialCard);
            linkCard = FindStyle(linkCard, s_LinkCard);

            welcomeDialogButton = FindStyle(welcomeDialogButton, s_WelcomeDialogButton);

            tooltip = FindStyle(tooltip, s_Tooltip);
        }

        private static readonly string s_InstructionLabelStyleName = "Instruction Label";
        private static readonly string s_Narrative = "NarrativeStyle";
        private static readonly string s_SwitchTutorialStyleName = "SwitchTutorialStyle";
        private static readonly string s_ImageStyle = "ImageStyle";
        private static readonly string s_VideoStyle = "VideoStyle";
        private static readonly string s_HeaderStyleName = "Header";
        private static readonly string s_LineStyleName = "Line";
        private static readonly string s_ListStyleName = "List";
        private static readonly string s_ListPrefixStyleName = "ListPrefix";
        private static readonly string s_ProgressLabelStyle = "Progress Label";
        private static readonly string s_SectionTitleLabelStyle = "Section Title Label";
        private static readonly string s_TheInBetweenTextStyle = "TheInBetweenText";

        private static readonly string s_Background = "Background";
        private static readonly string s_HeaderStyleBG = "SectionTitleBackground";
        private static readonly string s_ListBG = "ListBackground";
        private static readonly string s_SectionTitleBackground = "SectionTitleBackground";
        private static readonly string s_TopBarBackground = "TopBarBackground";
        private static readonly string s_DarkBackground = "DarkBackground";
        private static readonly string s_CompletedElementBackgroundStyle = "CompletedElementBackground";
        private static readonly string s_ActiveElementBackgroundStyle = "ActiveElementBackground";
        private static readonly string s_InActiveElementBackgroundStyle = "InActiveElementBackground";
        private static readonly string s_BGTheInBetweenText = "BGTheInBetweenText";
        private static readonly string s_TheInBetweenTextStyleNotActiveOrCompleted = "BGTheInBetweenTextNotActiveOrCompleted";

        private static readonly string s_Footer = "Footer";
        private static readonly string s_PaginationLabel = "PaginationLabel";
        private static readonly string s_NextButton = "NextButton";
        private static readonly string s_NextButtonDisabled = "NextButtonDisabled";
        private static readonly string s_BackButton = "BackButton";
        private static readonly string s_BackButtonDisabled = "BackButtonDisabled";
        private static readonly string s_ProgressBar = "ProgressBar";

        private static readonly string s_InstructionLabelIconStyleNotCompleted = "InstructionLabelIconNotCompleted";
        private static readonly string s_InstructionLabelIconStyleCompleted = "InstructionLabelIconCompleted";
        //private static readonly string s_IconButtonBack = "IconButtonBack";
        private static readonly string s_IconButtonReset = "IconButtonReset";
        private static readonly string s_IconButtonHome = "IconButtonHome";
        private static readonly string s_IconButtonClose = "IconButtonClose";

        private static readonly string s_TutorialCard = "TutorialCard";
        private static readonly string s_LinkCard = "LinkCard";

        private static readonly string s_WelcomeDialogButton = "WelcomeDialogButton";

        private static readonly string s_Tooltip = "Tooltip";

        public static GUIStyle narrativeStyle;
        public static GUIStyle switchTutorialStyle;
        public static GUIStyle imageStyle;
        public static GUIStyle videoStyle;
        public static GUIStyle theInBetweenText;
        public static GUIStyle headerLabel;
        public static GUIStyle instructionLabel;
        public static GUIStyle line;
        public static GUIStyle list;
        public static GUIStyle listPrefix;
        public static GUIStyle progressLabel;
        public static GUIStyle sectionTitleLabel;

        public static GUIStyle footer;
        public static GUIStyle paginationLabel;
        public static GUIStyle nextButton;
        public static GUIStyle nextButtonDisabled;
        public static GUIStyle backButton;
        public static GUIStyle backButtonDisabled;
        public static GUIStyle progressBar;

        public static GUIStyle background;
        public static GUIStyle sectionTitleBackground;
        public static GUIStyle topBarBackground;
        public static GUIStyle bgTheInBetweenText;
        public static GUIStyle completedElementBackground;
        public static GUIStyle activeElementBackground;
        public static GUIStyle inActiveElementBackground;
        public static GUIStyle darkBackground;
        public static GUIStyle theInBetweenTextNotActiveOrCompleted;
        public static GUIStyle headerBGStyle;
        public static GUIStyle listBGStyle;

        public static GUIStyle instructionLabelIconNotCompleted;
        public static GUIStyle instructionLabelIconCompleted;
        //public static GUIStyle iconButtonBack;
        public static GUIStyle iconButtonReset;
        public static GUIStyle iconButtonHome;
        public static GUIStyle iconButtonClose;

        public static GUIStyle tutorialCard;
        public static GUIStyle linkCard;

        public static GUIStyle welcomeDialogButton;

        public static GUIStyle tooltip;
    }

    /// <summary>
    /// An hack version of AllTutorialStyles that allows the main window to not blow up due to crazy
    /// static constructor initialization. Should be removed when migrating to UIElements
    /// </summary>
    public class AllStylesHACK
    {
        public static GUIStyle FindStyle(GUIStyle style, string name)
        {
            if (style == null)
            {
                style = GUI.skin.FindStyle(name);
            }
            return style;
        }

        public AllStylesHACK()
        {
            headerLabel = FindStyle(headerLabel, s_HeaderStyleName);
            instructionLabel = FindStyle(instructionLabel, s_InstructionLabelStyleName);
            line = FindStyle(line, s_LineStyleName);
            listPrefix = FindStyle(listPrefix, s_ListPrefixStyleName);
            list = FindStyle(list, s_ListStyleName);
            progressLabel = FindStyle(progressLabel, s_ProgressLabelStyle);
            sectionTitleLabel = FindStyle(sectionTitleLabel, s_SectionTitleLabelStyle);
            theInBetweenText = FindStyle(theInBetweenText, s_TheInBetweenTextStyle);
            narrativeStyle = FindStyle(narrativeStyle, s_Narrative);
            switchTutorialStyle = FindStyle(switchTutorialStyle, s_SwitchTutorialStyleName);
            imageStyle = FindStyle(imageStyle, s_ImageStyle);
            videoStyle = FindStyle(videoStyle, s_VideoStyle);

            sectionTitleBackground = FindStyle(sectionTitleBackground, s_SectionTitleBackground);
            topBarBackground = FindStyle(topBarBackground, s_TopBarBackground);

            headerBGStyle = FindStyle(headerBGStyle, s_HeaderStyleBG);
            listBGStyle = FindStyle(listBGStyle, s_ListBG);
            theInBetweenTextNotActiveOrCompleted = FindStyle(theInBetweenTextNotActiveOrCompleted, s_TheInBetweenTextStyleNotActiveOrCompleted);
            completedElementBackground = FindStyle(completedElementBackground, s_CompletedElementBackgroundStyle);
            activeElementBackground = FindStyle(activeElementBackground, s_ActiveElementBackgroundStyle);
            inActiveElementBackground = FindStyle(inActiveElementBackground, s_InActiveElementBackgroundStyle);
            darkBackground = FindStyle(darkBackground, s_DarkBackground);
            bgTheInBetweenText = FindStyle(bgTheInBetweenText, s_BGTheInBetweenText);
            background = FindStyle(background, s_Background);

            footer = FindStyle(footer, s_Footer);
            paginationLabel = FindStyle(paginationLabel, s_PaginationLabel);
            nextButton = FindStyle(nextButton, s_NextButton);
            nextButtonDisabled = FindStyle(nextButtonDisabled, s_NextButtonDisabled);
            backButton = FindStyle(backButton, s_BackButton);
            backButtonDisabled = FindStyle(backButtonDisabled, s_BackButtonDisabled);
            progressBar = FindStyle(progressBar, s_ProgressBar);

            //iconButtonBack = FindStyle(,s_IconButtonBack);
            iconButtonReset = FindStyle(iconButtonReset, s_IconButtonReset);
            iconButtonHome = FindStyle(iconButtonHome, s_IconButtonHome);
            iconButtonClose = FindStyle(iconButtonClose, s_IconButtonClose);
            instructionLabelIconCompleted = FindStyle(instructionLabelIconCompleted, s_InstructionLabelIconStyleCompleted);
            instructionLabelIconNotCompleted = FindStyle(instructionLabelIconNotCompleted, s_InstructionLabelIconStyleNotCompleted);

            tutorialCard = FindStyle(tutorialCard, s_TutorialCard);
            linkCard = FindStyle(linkCard, s_LinkCard);

            welcomeDialogButton = FindStyle(welcomeDialogButton, s_WelcomeDialogButton);

            tooltip = FindStyle(tooltip, s_Tooltip);
        }

        private static readonly string s_InstructionLabelStyleName = "Instruction Label";
        private static readonly string s_Narrative = "NarrativeStyle";
        private static readonly string s_SwitchTutorialStyleName = "SwitchTutorialStyle";
        private static readonly string s_ImageStyle = "ImageStyle";
        private static readonly string s_VideoStyle = "VideoStyle";
        private static readonly string s_HeaderStyleName = "Header";
        private static readonly string s_LineStyleName = "Line";
        private static readonly string s_ListStyleName = "List";
        private static readonly string s_ListPrefixStyleName = "ListPrefix";
        private static readonly string s_ProgressLabelStyle = "Progress Label";
        private static readonly string s_SectionTitleLabelStyle = "Section Title Label";
        private static readonly string s_TheInBetweenTextStyle = "TheInBetweenText";

        private static readonly string s_Background = "Background";
        private static readonly string s_HeaderStyleBG = "SectionTitleBackground";
        private static readonly string s_ListBG = "ListBackground";
        private static readonly string s_SectionTitleBackground = "SectionTitleBackground";
        private static readonly string s_TopBarBackground = "TopBarBackground";
        private static readonly string s_DarkBackground = "DarkBackground";
        private static readonly string s_CompletedElementBackgroundStyle = "CompletedElementBackground";
        private static readonly string s_ActiveElementBackgroundStyle = "ActiveElementBackground";
        private static readonly string s_InActiveElementBackgroundStyle = "InActiveElementBackground";
        private static readonly string s_BGTheInBetweenText = "BGTheInBetweenText";
        private static readonly string s_TheInBetweenTextStyleNotActiveOrCompleted = "BGTheInBetweenTextNotActiveOrCompleted";

        private static readonly string s_Footer = "Footer";
        private static readonly string s_PaginationLabel = "PaginationLabel";
        private static readonly string s_NextButton = "NextButton";
        private static readonly string s_NextButtonDisabled = "NextButtonDisabled";
        private static readonly string s_BackButton = "BackButton";
        private static readonly string s_BackButtonDisabled = "BackButtonDisabled";
        private static readonly string s_ProgressBar = "ProgressBar";

        private static readonly string s_InstructionLabelIconStyleNotCompleted = "InstructionLabelIconNotCompleted";
        private static readonly string s_InstructionLabelIconStyleCompleted = "InstructionLabelIconCompleted";
        //private static readonly string s_IconButtonBack = "IconButtonBack";
        private static readonly string s_IconButtonReset = "IconButtonReset";
        private static readonly string s_IconButtonHome = "IconButtonHome";
        private static readonly string s_IconButtonClose = "IconButtonClose";

        private static readonly string s_TutorialCard = "TutorialCard";
        private static readonly string s_LinkCard = "LinkCard";

        private static readonly string s_WelcomeDialogButton = "WelcomeDialogButton";

        private static readonly string s_Tooltip = "Tooltip";

        public GUIStyle narrativeStyle;
        public GUIStyle switchTutorialStyle;
        public GUIStyle imageStyle;
        public GUIStyle videoStyle;
        public GUIStyle theInBetweenText;
        public GUIStyle headerLabel;
        public GUIStyle instructionLabel;
        public GUIStyle line;
        public GUIStyle list;
        public GUIStyle listPrefix;
        public GUIStyle progressLabel;
        public GUIStyle sectionTitleLabel;

        public GUIStyle footer;
        public GUIStyle paginationLabel;
        public GUIStyle nextButton;
        public GUIStyle nextButtonDisabled;
        public GUIStyle backButton;
        public GUIStyle backButtonDisabled;
        public GUIStyle progressBar;

        public GUIStyle background;
        public GUIStyle sectionTitleBackground;
        public GUIStyle topBarBackground;
        public GUIStyle bgTheInBetweenText;
        public GUIStyle completedElementBackground;
        public GUIStyle activeElementBackground;
        public GUIStyle inActiveElementBackground;
        public GUIStyle darkBackground;
        public GUIStyle theInBetweenTextNotActiveOrCompleted;
        public GUIStyle headerBGStyle;
        public GUIStyle listBGStyle;

        public GUIStyle instructionLabelIconNotCompleted;
        public GUIStyle instructionLabelIconCompleted;
        //public GUIStyle iconButtonBack;
        public GUIStyle iconButtonReset;
        public GUIStyle iconButtonHome;
        public GUIStyle iconButtonClose;

        public GUIStyle tutorialCard;
        public GUIStyle linkCard;

        public GUIStyle welcomeDialogButton;

        public GUIStyle tooltip;
    }
    #endregion
}
