using System;
using UnityEditor;
using UnityEngine;

namespace Unity.InteractiveTutorials
{
    public class TutorialContainer : ScriptableObject
    {
        public event Action Modified;

        public Texture2D HeaderBackground;

        public LocalizableString Title;

        public LocalizableString ProjectName;

        public LocalizableString Description;

        [Tooltip("Can be used to override the default layout specified by the Tutorial Framework.")]
        public UnityEngine.Object ProjectLayout;

        public Section[] Sections = {};

        public string ProjectLayoutPath =>
            ProjectLayout != null ? AssetDatabase.GetAssetPath(ProjectLayout) : k_DefaultLayoutPath;

        // The default layout used when a project is started for the first time.
        // TODO IET unit test the the file exist.
        // TODO IET unit test the the layout contains TutorialWindow.
        // TODO Should be in TutorialProjectSettings and/or UserStartupCode instead.
        const string k_DefaultLayoutPath =
            "Packages/com.unity.learn.iet-framework/Framework/Interactive Tutorials/Layouts/DefaultLayout.wlt";

        [Serializable]
        public class Section
        {
            public int OrderInView;

            public LocalizableString Heading;

            public LocalizableString Text;

            // TODO Rename
            [Tooltip("Used as content type metadata for external references/URLs")]
            public string LinkText;

            [Tooltip("Setting the URL will take precedence and make the card act as a link card instead of a tutorial card")]
            public string Url;

            [Tooltip("Use for Unity Connect auto-login, shortened URLs do not work")]
            public bool AuthorizedUrl;

            public Texture2D Image;

            [SerializeField]
            Tutorial Tutorial = null;

            public bool TutorialCompleted { get; set; }

            public bool IsTutorial => Url.IsNullOrEmpty();

            public string TutorialId => Tutorial?.lessonId.AsEmptyIfNull();

            public string SessionStateKey => $"Unity.InteractiveTutorials.lesson{TutorialId}";

            public void StartTutorial()
            {
                TutorialManager.instance.StartTutorial(Tutorial);
            }

            public void OpenUrl()
            {
                if (string.IsNullOrEmpty(Url))
                    return;

                if (AuthorizedUrl && UnityConnectProxy.loggedIn)
                    UnityConnectProxy.OpenAuthorizedURLInWebBrowser(Url);
                else
                    Application.OpenURL(Url);

                AnalyticsHelper.SendExternalReferenceEvent(Url, Heading.Untranslated, LinkText, Tutorial?.lessonId);
            }

            // returns true if the state was found from EditorPrefs
            public bool LoadState()
            {
                const string nonexisting = "NONEXISTING";
                var state = SessionState.GetString(SessionStateKey, nonexisting);
                if (state == "")
                {
                    TutorialCompleted = false;
                }
                else if (state == "Finished")
                {
                    TutorialCompleted = true;
                }
                return state != nonexisting;
            }

            public void SaveState()
            {
                SessionState.SetString(SessionStateKey, TutorialCompleted ? "Finished" : "");
            }
        }

        void OnValidate()
        {
            SortSections();
            for (int i = 0; i < Sections.Length; ++i)
            {
                Sections[i].OrderInView = i * 2;
            }
        }

        void SortSections()
        {
            Array.Sort(Sections, (x, y) => x.OrderInView.CompareTo(y.OrderInView));
        }

        public void LoadTutorialProjectLayout()
        {
            TutorialManager.LoadWindowLayoutWorkingCopy(ProjectLayoutPath);
        }

        public void RaiseModifiedEvent()
        {
            Modified?.Invoke();
        }
    }
}
