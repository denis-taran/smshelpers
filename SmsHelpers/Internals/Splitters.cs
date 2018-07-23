using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Texting.Internals
{
    internal static class Splitters
    {
        private static readonly Regex LinkRegex = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        ///   Split text to punctuations and words
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static List<TextBlock> SplitString(string original)
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

        public static List<TextBlock> SplitLinks(string original)
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
                var pre = original.Substring(prevIndex, match.Index);
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
    }
}
