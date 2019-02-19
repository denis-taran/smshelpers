using System;
using System.Linq;

namespace Texting.Internals
{
    internal static class SmsInternalHelper
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
            'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'ä', 'ö', 'ñ', 'ü', 'à',
            '"'
        };

        // GSM characters entensions - these characters must be prefixed with special escape character
        private static readonly char[] GsmCharactersExtension = {
            '\f', '^', '{', '}', '\\', '[', '~', ']', '|', '€'
        };

        // combined list of standard and extended GSM characters
        public static readonly char[] AllGsmCharacters = GsmCharacters.Concat(GsmCharactersExtension).ToArray();

        /// <summary>
        ///   The method will calculate how many GSM characters is required to send a single character
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int GetGsmCharLength(char c)
        {
            if (GsmCharacters.Contains(c))
            {
                return 1;
            }
            else if (GsmCharactersExtension.Contains(c))
            {
                return 2;
            }
            else
            {
                throw new ArgumentException("This method cannot be called with unicode characters.");
            }
        }

        /// <summary>
        ///   Count the number of SMS parts
        /// </summary>
        /// <param name="content">Text message</param>
        /// <remarks>This method must only be called with GSM characters. Extended GSM characters are also allowed.</remarks>
        /// <exception cref="ArgumentException">If the provided string contains non-GSM characters.</exception>
        /// <exception cref="ArgumentNullException">If the provided string contains is null.</exception>
        /// <returns></returns>
        public static int CountNonUnicodeParts(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var counter = 0;

            foreach (var c in content)
            {
                counter = counter + GetGsmCharLength(c);
            }

            if (counter <= SmsConstants.GsmLengthLimitSinglePart)
            {
                return 1;
            }

            return (int)Math.Ceiling(counter / SmsConstants.GsmLengthLimitMultipart);
        }
    }
}
