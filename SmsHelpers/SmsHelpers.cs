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
        public SmsSplittingResult SplitMessageWithWordWrap(string text, bool concatenatedSms = true)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (text == string.Empty)
            {
                return new SmsSplittingResult
                {
                    Encoding = SmsEncoding.Gsm7Bit,
                    Parts = new List<SmsPart>()
                };
            }

            var encoding = GetEncoding(text);
            var blocks = TextSplitter.Split(text, encoding);
            var builder = new SmsBuilder(blocks, encoding, concatenatedSms);

            return new SmsSplittingResult
            {
                Encoding = encoding,
                Parts = builder.Parts
            };
        }
    }
}
