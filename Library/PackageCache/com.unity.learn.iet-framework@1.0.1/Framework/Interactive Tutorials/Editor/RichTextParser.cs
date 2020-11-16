using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.InteractiveTutorials
{
    public static class RichTextParser
    {
        /// <summary>
        /// Transforms HTML tags to word element labels with different styles to enable rich text.
        /// </summary>
        /// <param name="htmlText"></param>
        /// <param name="targetContainer">
        /// The following need to set for the container's style:
        /// flex-direction: row;
        /// flex-wrap: wrap;
        /// </param>
        public static void RichTextToVisualElements(string htmlText, VisualElement targetContainer)
        {
            // TODO should translation be a responsibility of the caller of this function instead
            htmlText = Localization.Tr(htmlText);
            VisualElementStyleSheetSet style = targetContainer.styleSheets;
            targetContainer.Clear();
            bool boldOn = false; // <b> sets this on </b> sets off
            bool italicOn = false; // <i> </i>
            bool linkOn = false;
            string linkURL = "";
            bool firstLine = true;
            bool lastLineHadText = false;
            // start streaming text per word to elements while retaining current style for each word block
            string[] lines = htmlText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                string[] words = line.Split(new[] { " " }, StringSplitOptions.None);

                if (!firstLine && !lastLineHadText)
                {
                    AddParagraphToElement(targetContainer);
                }
                if (!firstLine && lastLineHadText)
                {
                    AddLinebreakToElement(targetContainer);
                    //AddParagraphToElement(targetContainer);
                    lastLineHadText = false;
                }

                foreach (string word in words)
                {
                    if (word == "" || word == " " || word == "   ") continue;
                    lastLineHadText = true;
                    string strippedWord = word;
                    bool removeBold = false;
                    bool removeItalic = false;
                    bool addParagraph = false;
                    bool removeLink = false;

                    if (strippedWord.Contains("<b>"))
                    {
                        strippedWord = strippedWord.Replace("<b>", "");
                        boldOn = true;
                    }
                    if (strippedWord.Contains("<i>"))
                    {
                        strippedWord = strippedWord.Replace("<i>", "");
                        italicOn = true;
                    }
                    if (strippedWord.Contains("<a"))
                    {
                        strippedWord = strippedWord.Replace("<a", "");
                        linkOn = true;
                    }
                    if (linkOn && strippedWord.Contains("href="))
                    {
                        strippedWord = strippedWord.Replace("href=", "");
                        int linkFrom = strippedWord.IndexOf("\"", StringComparison.Ordinal) + 1;
                        int linkTo = strippedWord.LastIndexOf("\"", StringComparison.Ordinal);
                        // TODO handle invalid values
                        linkURL = strippedWord.Substring(linkFrom, linkTo - linkFrom);
                        strippedWord = strippedWord.Substring(linkTo + 2, (strippedWord.Length - 2) - linkTo);
                        strippedWord.Replace("\">", "");
                    }
                    if (strippedWord.Contains("</a>"))
                    {
                        strippedWord = strippedWord.Replace("</a>", "");
                        // TODO </a>text -> also text part is still blue. Parse - for now we can take care when authoring.
                        removeLink = true;
                    }
                    if (strippedWord.Contains("<br/>"))
                    {
                        strippedWord = strippedWord.Replace("<br/>", "");
                        addParagraph = true;
                    }
                    if (strippedWord.Contains("</b>"))
                    {
                        strippedWord = strippedWord.Replace("</b>", "");
                        removeBold = true;
                    }
                    if (strippedWord.Contains("</i>"))
                    {
                        strippedWord = strippedWord.Replace("</i>", "");
                        removeItalic = true;
                    }
                    if (boldOn)
                    {
                        Label wordLabel = new Label(strippedWord);
                        wordLabel.style.color = Color.black;
                        wordLabel.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
                        targetContainer.Add(wordLabel);
                    }
                    else if (italicOn)
                    {
                        Label wordLabel = new Label(strippedWord);
                        wordLabel.style.color = Color.black;
                        wordLabel.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Italic);
                        targetContainer.Add(wordLabel);
                    }
                    else if (addParagraph)
                    {
                        AddParagraphToElement(targetContainer);
                    }
                    else if (linkOn && !string.IsNullOrEmpty(linkURL))
                    {
                        Label newLabel = new Label(strippedWord);

                        newLabel.style.color = Color.blue;
                        newLabel.style.borderBottomWidth = 1f;
                        newLabel.style.borderBottomColor = Color.blue;
                        newLabel.tooltip = linkURL;
                        newLabel.RegisterCallback<MouseUpEvent, string>(
                            (evt, linkurl) =>
                            {
                                // Supporting only hyperlinks to Unity's websites.
                                // The user needs be be logged in in order the hyperlink to work.
                                UnityConnectProxy.OpenAuthorizedURLInWebBrowser(linkurl);
                            },
                            linkURL
                        );

                        targetContainer.Add(newLabel);
                    }
                    else
                    {
                        Label newlabel = new Label(strippedWord);
                        newlabel.style.color = Color.black;
                        targetContainer.Add(newlabel);
                    }
                    if (removeBold) boldOn = false;
                    if (removeItalic) italicOn = false;
                    if (removeLink)
                    {
                        linkOn = false;
                        linkURL = "";
                    }
                }
                firstLine = false;
            }
        }

        static void AddLinebreakToElement(VisualElement elementTo)
        {
            Label wordLabel = new Label(" ");
            wordLabel.style.color = Color.black;
            wordLabel.style.flexDirection = FlexDirection.Row;
            wordLabel.style.flexGrow = 1f;
            wordLabel.style.width = 3000f;
            wordLabel.style.height = 0f;
            elementTo.Add(wordLabel);
        }

        static void AddParagraphToElement(VisualElement elementTo)
        {
            Label wordLabel = new Label(" ");
            wordLabel.style.color = Color.black;
            wordLabel.style.flexDirection = FlexDirection.Row;
            wordLabel.style.flexGrow = 1f;
            wordLabel.style.width = 3000f;
            elementTo.Add(wordLabel);
        }
    }
}
