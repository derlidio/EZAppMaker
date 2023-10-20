/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Text;
using System.Globalization;

namespace EZAppMaker.Support
{
    public static class EZText
    {

        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string Mask(string text, string mask)
        {
            text = EmptyIfNull(text);
            mask = EmptyIfNull(mask);

            string copy = RemoveDiacritics(text).ToUpper();

            if (string.IsNullOrWhiteSpace(copy)) return null;

            int i = 0;

            string[] allowed = { "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "0123456789", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", "0123456789ABCDEF" };

            while ((i < text.Length) && (i < mask.Length))
            {
                string m = mask.Substring(i, 1);
                string t = copy.Substring(i, 1);

                int test = "X9*H".IndexOf(m);

                if (test == -1)
                {
                    if (m != t)
                    {
                        text = text.Insert(i, m);
                        copy = copy.Insert(i, m);
                    }
                    i++;
                    continue;
                }

                if (allowed[test].IndexOf(t) == -1)
                {
                    text = text.Remove(i, 1);
                    copy = copy.Remove(i, 1);
                }
                else
                {
                    i++;
                }
            }

            if (text.Length > mask.Length)
            {
                text = text.Substring(0, mask.Length);
            }

            return text;
        }

        public static string RemoveMask(string text, string mask)
        {
            if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrEmpty(mask))
            {
                int start = text.Length > mask.Length ? mask.Length : text.Length;

                for (int i = start - 1; i >= 0; i--)
                {
                    if ("X9*H".IndexOf(mask.Substring(i, 1)) == -1)
                    {
                        text = text.Remove(i, 1);
                    }
                }
            }

            return text;
        }

        public static string EmptyIfNull(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";

            return str;
        }
    }
}