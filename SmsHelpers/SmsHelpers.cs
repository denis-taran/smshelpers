using System;
using System.Collections.Generic;
using System.Linq;
using Texting.Internals;

namespace Texting
{
    /// <inheritdoc />
    public class SmsHelpers : ISmsHelpers
    {
        /// <inheritdoc />
        public SmsEncoding GetEncoding(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            foreach(var c in text)
            {
                if (!SmsInternalHelper.AllGsmCharacters.Contains(c))
                {
                    return SmsEncoding.GsmUnicode;
                }
            }

            return SmsEncoding.Gsm7Bit;
        }

        /// <inheritdoc />
        public int CountSmsParts(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (text.Length <= SmsConstants.UnicodeLengthLimitSinglePart)
            {
                return 1;
            }

            switch (GetEncoding(text))
            {
                case SmsEncoding.Gsm7Bit:
                    return SmsInternalHelper.CountNonUnicodeParts(text);
                case SmsEncoding.GsmUnicode:
                    return (int)Math.Ceiling(text.Length / SmsConstants.UnicodeLengthLimitMultipart);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public string NormalizeNewLines(string text)
        {
            if (text == null)
                return null;

            text = text.Replace("\r\n", "\r");
            text = text.Replace("\n\r", "\r");
            text = text.Replace("\n", "\r");

            return text;
        }

        /// <inheritdoc />
        public List<string> SplitMessageWithWordWrap(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new List<string>();
            }

            var encoding = GetEncoding(text);
            var splitted = Splitters.SplitLinks(text).SelectMany(b => b.IsLink ? new List<TextBlock> { b } : Splitters.SplitString(b.Content));
            var blocks = splitted.Select(t => ToTextBlock(t.Content, encoding)).ToList();
            var builder = new SmsBuilder(blocks, encoding);
            return builder.Parts.Select(n => n.Content).ToList();
        }

        private static TextBlock ToTextBlock(string text, SmsEncoding encoding)
        {
            var result = new TextBlock
            {
                Content = text,
                Length = encoding == SmsEncoding.GsmUnicode
                    ? text.Length
                    : text.Select(SmsInternalHelper.GetGsmCharLength).Sum()
            };

            return result;
        }
    }
}
