using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using UnityEngine.Serialization;

namespace Unity.InteractiveTutorials
{
    public enum ParagraphType
    {
        Narrative,
        Instruction,
        SwitchTutorial,
        UnorderedList,
        OrderedList,
        Icons,
        Image,
        Video,
    }

    enum CompletionType
    {
        CompletedWhenAllAreTrue,    // TODO Simplify name, "All(True)"
        CompletedWhenAnyIsTrue      // TODO Simplify name, "Any(True)"
    }

    [Serializable]
    public class TutorialParagraph
    {
        public ParagraphType type { get { return m_Type; } }
        [SerializeField]
        internal ParagraphType m_Type;

        public string summary { get { return m_Summary; } set { m_Summary = value; } }
        [SerializeField, TextArea(1, 1)]
        string m_Summary = "";

        public string Description { get { return m_Description; } set { m_Description = value; } }
        [FormerlySerializedAs("m_description1")] // TODO we're breaking backwards-compatibility fully, this can be removed after the refactoring
        [SerializeField, TextArea(1, 8)]
        string m_Description;

        public string InstructionTitle { get { return m_InstructionBoxTitle; } set { m_InstructionBoxTitle = value; } }
        [FormerlySerializedAs("m_Text")] // TODO we're breaking backwards-compatibility fully, this can be removed after the refactoring
        [SerializeField, TextArea(1, 15)]
        string m_InstructionBoxTitle = "";

        public string InstructionText { get { return m_InstructionText; } set { m_InstructionText = value; } }
        [SerializeField, TextArea(1, 15)]
        string m_InstructionText = "";

        [SerializeField]
        internal string m_TutorialButtonText = "";

        [SerializeField]
        internal Tutorial m_Tutorial;

        public IEnumerable<InlineIcon> icons
        {
            get
            {
                m_Icons.GetItems(m_IconBuffer);
                return m_IconBuffer;
            }
        }
        [SerializeField]
        InlineIconCollection m_Icons = new InlineIconCollection();
        readonly List<InlineIcon> m_IconBuffer = new List<InlineIcon>();

        public Texture2D image { get { return m_Image; } set { m_Image = value; } }

        [SerializeField]
        Texture2D m_Image = null;

        public VideoClip video { get { return m_Video; } set { m_Video = value; } }
        [SerializeField]
        VideoClip m_Video = null;

        [SerializeField]
        internal CompletionType m_CriteriaCompletion = CompletionType.CompletedWhenAllAreTrue;

        [SerializeField] internal TypedCriterionCollection m_Criteria = new TypedCriterionCollection();
        readonly List<TypedCriterion> m_CriteriaBuffer = new List<TypedCriterion>();

        public IEnumerable<TypedCriterion> criteria
        {
            get
            {
                m_Criteria.GetItems(m_CriteriaBuffer);
                return m_CriteriaBuffer.ToArray();
            }
        }

        public MaskingSettings maskingSettings { get { return m_MaskingSettings; } }
        [SerializeField]
        MaskingSettings m_MaskingSettings = new MaskingSettings();

        public bool completed
        {
            get
            {
                bool allMandatory = m_CriteriaCompletion == CompletionType.CompletedWhenAllAreTrue;
                bool result = allMandatory;

                foreach (var typedCriterion in m_Criteria)
                {
                    var criterion = typedCriterion.criterion;
                    if (criterion != null)
                    {
                        if (!allMandatory && criterion.completed)
                        {
                            result = true;
                            break;
                        }

                        if (allMandatory && !criterion.completed)
                        {
                            result = false;
                            break;
                        }
                    }
                }

                return result;
            }
        }
    }

    [Serializable]
    public class TutorialParagraphCollection : CollectionWrapper<TutorialParagraph>
    {
        public TutorialParagraphCollection() : base()
        {
        }

        public TutorialParagraphCollection(IList<TutorialParagraph> items) : base(items)
        {
        }
    }
}
