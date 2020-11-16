using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Unity.InteractiveTutorials
{
    // Good info on PO: http://pology.nedohodnik.net/doc/user/en_US/ch-poformat.html
    public static class POFileUtils
    {
        public static readonly Dictionary<SystemLanguage, string> SupportedLanguages = new Dictionary<SystemLanguage, string>
        {
            //{ SystemLanguage.English,  "en" },
            { SystemLanguage.Japanese, "ja" },
            { SystemLanguage.Korean, "ko" },
            { SystemLanguage.ChineseSimplified, "zh-hans" },
            { SystemLanguage.ChineseTraditional, "zh-hant" },
        };

        // https://www.gnu.org/software/trans-coord/manual/gnun/html_node/PO-Header.html
        // https://www.gnu.org/software/gettext/manual/html_node/Header-Entry.html
        // TODO Unit tests that the files provided with the package have this.
        // TODO check if we want to fill something more to the header
        // NOTE We don't have POTs so for POT-Creation-Date I just picked something.
        // TODO Value of Plural-Forms not probably true for all languages we support?
        public static string CreateHeader(string langCode, string name, string version) =>
            $@"
msgid """"
msgstr """"
""Project-Id-Version: {name}@{version} \n""
""Report-Msgid-Bugs-To: \n""
""Language-Team: #devs-localization\n""
""POT-Creation-Date: 2020-05-15 21:02+03:00\n""
""PO-Revision-Date: {DateTime.Now.ToString(DateTimeFormat)}\n""
""Language: {langCode}\n""
""MIME-Version: 1.0\n""
""Content-Type: text/plain; charset=UTF-8\n""
""Content-Transfer-Encoding: 8bit\n""
""Plural-Forms: nplurals=2; plural=(n != 1);\n""
""X-Generator: com.unity.learn.iet-framework.authoring\n""
";

        // Using the format given here https://www.gnu.org/software/trans-coord/manual/gnun/html_node/PO-Header.html
        public const string DateTimeFormat = "yyyy-MM-dd HH:mmK";

        // https://www.gnu.org/software/gettext/manual/html_node/PO-Files.html
        public class POEntry
        {
            public string TranslatorComments;           //  # translator-comments                   0
            public string ExtractedComments;            //  #. extracted-comments                   1
            public string Reference;                    //  #: reference                            2
            public string Flag;                         //  #, flag                                 3
            public string PreviousUntranslatedString;   //  #| msgid "previous-untranslated-string" 4
            public string UntranslatedString;           //  msgid "untranslated-string"             5
            public string TranslatedString;             //  msgstr "translated-string"              6

            public bool IsValid() => Reference.IsNotNullOrEmpty() && UntranslatedString.IsNotNullOrEmpty();

            public string Serialize()
            {
                return string.Format(
                    "{0}" +
                    "{1}" +
                    "{2}" +
                    "{3}" +
                    "{4}" +
                    "msgid \"{5}\"\n" +
                    "msgstr \"{6}\"",
                    TranslatorComments.IsNotNullOrEmpty() ? $"# {TranslatorComments}\n" : string.Empty,
                    ExtractedComments.IsNotNullOrEmpty() ? $"#. {ExtractedComments}\n" : string.Empty,
                    Reference.IsNotNullOrEmpty() ? $"#: {Reference}\n" : string.Empty,
                    Flag.IsNotNullOrEmpty() ? $"#, {Flag}\n" : string.Empty,
                    PreviousUntranslatedString.IsNotNullOrEmpty() ? $"#| {PreviousUntranslatedString}" : string.Empty,
                    UntranslatedString,
                    TranslatedString
                );
            }
        }


        public static List<POEntry> ReadPOFile(string filepath)
        {
            const string str = "msgstr ";
            const string id = "msgid ";
            const string previd = "#| msgstr ";
            const string flag = "#,";
            const string reference = "#:";
            const string ecomment = "#.";
            const string tcomment = "#";

            var ret = new List<POEntry>();
            try
            {
                using (var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                using (var streamReader = new StreamReader(fileStream, Utf8WithoutBom))
                {
                    var entry = new POEntry();
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.StartsWith(str))
                        {
                            entry.TranslatedString = line.Substring(str.Length);
                            entry.TranslatedString = entry.TranslatedString.Trim(new char[] {' ', '\"'});
                        }
                        if (line.StartsWith(id))
                        {
                            entry.UntranslatedString = line.Substring(id.Length);
                            entry.UntranslatedString = entry.UntranslatedString.Trim(new char[] { ' ', '\"' });
                        }
                        if (line.StartsWith(previd))
                        {
                            entry.PreviousUntranslatedString = line.Substring(previd.Length);
                            entry.PreviousUntranslatedString = entry.PreviousUntranslatedString.Trim(new char[] { ' ', '\"' });
                        }
                        if (line.StartsWith(flag)) entry.Flag = line.Substring(flag.Length).Trim();
                        if (line.StartsWith(reference)) entry.Reference = line.Substring(reference.Length).Trim();
                        if (line.StartsWith(ecomment)) entry.ExtractedComments = line.Substring(ecomment.Length).Trim();
                        if (line.StartsWith(tcomment)) entry.TranslatorComments = line.Substring(tcomment.Length).Trim();

                        if (line.IsNullOrWhitespace() && entry.IsValid())
                        {
                            ret.Add(entry);
                            entry = new POEntry();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return ret;
        }

        public static bool WritePOFile(string projectName, string projectVersion, string langCode, IEnumerable<POEntry> entries, string filepath)
        {
            const string controlCharacterPattern = @"\p{Cc}+";

            try
            {
                using (var sw = new StreamWriter(filepath, append: false, Utf8WithoutBom))
                {
                    sw.Write(CreateHeader(langCode, projectName, projectVersion));
                    // Editor's handling of PO files seems very finicky, an empty line after the header
                    // and before the first entry required.
                    sw.WriteLine();
                    foreach (var entry in entries)
                    {
                        if (Regex.IsMatch(entry.UntranslatedString, controlCharacterPattern))
                        {
                            Debug.LogWarning($"msgid for '{entry.Reference}' contains control characters, they will be removed ({filepath}).");
                            entry.UntranslatedString = Regex.Replace(entry.UntranslatedString, controlCharacterPattern, string.Empty);
                        }
                        sw.WriteLine(entry.Serialize() + "\n");
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        // Let's be very explicit about this, using e.g. System.Text.Encoding.UTF8 gives UTF-8 with BOM...
        static System.Text.UTF8Encoding Utf8WithoutBom => new System.Text.UTF8Encoding();
    }
}
