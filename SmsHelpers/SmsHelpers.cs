using System;
using System.Linq;

namespace Sms.Helpers
{
    /// <inheritdoc />
    public class SmsHelpers : ISmsHelpers
    {
        // standard GSM characters
        private static readonly char[] GsmCharacters = {
            '@', '£', '$', '¥', 'è', 'é', 'ù', 'ì', 'ò', 'Ç', '\n', 'Ø', 'ø', '\r', 'Å', 'å',
            'Δ', '_', 'Φ', 'Γ', 'Λ', 'Ω', 'Π', 'Ψ', 'Σ', 'Θ', 'Ξ', '\u001b', 'Æ', 'æ', 'ß', 'É',
            ' ', '!', '\'', '#', '¤', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '<', '=', '>', '?',
            '¡', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
            'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'Ä', 'Ö', 'Ñ', 'Ü', '§',
            '¿', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o',
            'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'ä', 'ö', 'ñ', 'ü', 'à'
        };

        // GSM characters entensions - these characters must be prefixed with special escape character
        private static readonly char[] GsmCharactersExtension = {
            '\f', '^', '{', '}', '\\', '[', '~', ']', '|', '€'
        };

        // combined list of standard and extended GSM characters
        private static readonly char[] AllGsmCharacters = GsmCharacters.Concat(GsmCharactersExtension).ToArray();

        /// <summary>
        ///   Count the number of SMS parts
        /// </summary>
        /// <param name="content">Text message</param>
        /// <remarks>This method must only be called with GSM characters. Extended GSM characters are also allowed.</remarks>
        /// <exception cref="ArgumentException">If the provided string contains non-GSM characters.</exception>
        /// <exception cref="ArgumentNullException">If the provided string contains is null.</exception>
        /// <returns></returns>
        private static int CountNonUnicodeParts(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var counter = 0;

            foreach(var c in content)
            {
                if (GsmCharacters.Contains(c))
                {
                    counter++;
                }
                else if (GsmCharactersExtension.Contains(c))
                {
                    counter = counter + 2;
                }
                else
                {
                    throw new ArgumentException("This method cannot be called with unicode characters.");
                }
            }

            if (counter <= 160)
            {
                return 1;
            }

            return (int)Math.Ceiling(counter / 153m);
        }

        /// <inheritdoc />
        public SmsEncoding GetEncoding(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            foreach(var c in text)
            {
                if (!AllGsmCharacters.Contains(c))
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

            if (text.Length <= 70)
            {
                return 1;
            }

            switch (GetEncoding(text))
            {
                case SmsEncoding.Gsm7Bit:
                    return CountNonUnicodeParts(text);
                case SmsEncoding.GsmUnicode:
                    return (int)Math.Ceiling(text.Length / 67.0);
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
    }
}
