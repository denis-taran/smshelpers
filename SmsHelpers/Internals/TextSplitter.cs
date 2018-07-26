using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Texting.Internals
{
    internal static class TextSplitter
    {
        /// <summary>
        ///   Regex to match links in text
        /// </summary>
        private static readonly Regex LinkRegex = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        ///   Split text to individual words
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private static List<TextBlock> SplitWords(string original)
        {
            var result = new List<TextBlock>();

            var word = "";
            foreach (var c in original)
            {
                if (char.IsPunctuation(c) || char.IsWhiteSpace(c))
                {
                    if (!string.IsNullOrEmpty(word))
                    {
                        result.Add(new TextBlock(word));
                        word = "";
                    }
                    result.Add(new TextBlock(c.ToString()));
                }
                else
                {
                    word = word + c;
                }
            }

            if (!string.IsNullOrEmpty(word))
            {
                result.Add(new TextBlock(word));
            }

            return result;
        }

        private static List<TextBlock> SplitLinks(string original)
        {
            var result = new List<TextBlock>();

            var matches = LinkRegex.Matches(original);
            var prevIndex = 0;
            if (matches.Count <= 0)
            {
                return new List<TextBlock> { new TextBlock(original) };
            }

            foreach (Match match in matches)
            {
                var pre = original.Substring(prevIndex, match.Index - prevIndex);
                if (!string.IsNullOrEmpty(pre))
                {
                    result.Add(new TextBlock(pre));
                }
                result.Add(new TextBlock(match.Value, true));
                prevIndex = match.Index + match.Length;
            }

            result.Add(new TextBlock(original.Substring(prevIndex, original.Length - prevIndex)));

            return result;
        }

        /// <summary>
        ///   Split the provided text to individual parts like words, links etc.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static List<TextBlock> Split(string text, SmsEncoding encoding)
        {
            return SplitLinks(text)
                .SelectMany(b => b.IsLink ? new List<TextBlock> { b } : SplitWords(b.Content))
                .Select(t => ToTextBlock(t.Content, encoding))
                .ToList();
        }

        /// <summary>
        ///   Convert text to <see cref="TextBlock"/> and calculate SMS length for that block
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        private static TextBlock ToTextBlock(string text, SmsEncoding encoding)
        {
            return new TextBlock
            {
                Content = text,
                Length = encoding == SmsEncoding.GsmUnicode
                    ? text.Length
                    : text.Select(SmsInternalHelper.GetGsmCharLength).Sum()
            };
        }
    }
}
