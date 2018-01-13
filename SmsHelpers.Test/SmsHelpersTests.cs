using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sms.Helpers;

namespace SmsHelpers.Test
{
    [TestClass]
    public class SmsHelpersTests
    {
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

        private static readonly char[] GsmCharactersExtension = {
            '\f', '^', '{', '}', '\\', '[', '~', ']', '|', '€'
        };

        private ISmsHelpers SmsHelpers { get; }

        public SmsHelpersTests()
        {
            SmsHelpers = new Sms.Helpers.SmsHelpers();
        }

        private static string GenerateRandomUnicodeString(int length)
        {
            var result = new StringBuilder();
            var random = new Random();

            do
            {
                var ch = (char)random.Next(short.MaxValue);
                var category = char.GetUnicodeCategory(ch);

                if (category != UnicodeCategory.OtherNotAssigned && ch != '\n')
                {
                    result.Append(ch);
                }

            } while (result.Length < length);

            return result.ToString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCharset_ThrowAnException()
        {
            SmsHelpers.GetEncoding(null);
        }

        [TestMethod]
        [DataRow("", SmsEncoding.Gsm7Bit)]
        [DataRow("a", SmsEncoding.Gsm7Bit)]
        [DataRow("≀", SmsEncoding.GsmUnicode)]
        [DataRow("a≀", SmsEncoding.GsmUnicode)]
        public void GetCharset_DetectEncoding(string text, SmsEncoding expectedEncoding)
        {
            var encoding = SmsHelpers.GetEncoding(text);
            Assert.AreEqual(encoding, expectedEncoding);
        }

        [TestMethod]
        public void GetCharset_DetectEncodingGsmCharacters()
        {
            foreach (var c in GsmCharacters)
            {
                var encoding = SmsHelpers.GetEncoding(c.ToString());
                Assert.AreEqual(encoding, SmsEncoding.Gsm7Bit);
            }

            foreach (var c in GsmCharactersExtension)
            {
                var encoding = SmsHelpers.GetEncoding(c.ToString());
                Assert.AreEqual(encoding, SmsEncoding.Gsm7Bit);
            }
        }

        [TestMethod]
        public void NormalizeNewLines_ReturnsNull()
        {
            Assert.IsNull(SmsHelpers.NormalizeNewLines(null));
        }

        [TestMethod]
        [DataRow("a\ra", "a\ra")]
        [DataRow("a\r\na", "a\ra")]
        [DataRow("a\n\ra", "a\ra")]
        [DataRow("a\r\ra", "a\r\ra")]
        [DataRow("a\r\n\r\na", "a\r\ra")]
        [DataRow("a\n\r\n\ra", "a\r\ra")]
        public void NormalizeNewLines_Test(string before, string expected)
        {
            var result = SmsHelpers.NormalizeNewLines(before);
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CountSmsParts_ThrowArgumentNullException()
        {
            SmsHelpers.CountSmsParts(null);
        }

        [TestMethod]
        [DataRow("", 1)]
        [DataRow("1", 1)]
        [DataRow(@"Dear abcde,

This is a reminder that you have an appointment on 24/04/2011 10:30 with ABCABCAB ABCAC at eeBBBBBBBB Court, 4444444444444444444444444444444444444444 444.

If you are unable to keep this appointment, please call 44444'444444, 44444444444444444444 at 444444444444444 to reschedule your appointment at your earliest convenience.

We appreciate your business and look forward to seeing you soon!

Warm Regards,
4444444444444444444444444444444444", 3)]
        [DataRow(@"Hi 444444, this is a quick reminder that you have an
appointment with 444444444444444444 on 4/44/2015 1:54 PM.

If you would like to reschedule this appointment please call/ text 444444444444.

Have a great day!

444444444444444444444444
44444444444444444444444444444444444444444444444444444444
444444444444", 3)]
        public void CountSmsParts_Test(string sms, int expectedSmsParts)
        {
            var normalized = SmsHelpers.NormalizeNewLines(sms);
            Assert.IsTrue(SmsHelpers.CountSmsParts(normalized) == expectedSmsParts);
        }

        [TestMethod]
        public void CountSmsParts_NoExceptions()
        {
            for (var i = 0; i < 100; i++)
            {
                for (var j = 1; j < 700; j++)
                {
                    var str = GenerateRandomUnicodeString(j);
                    SmsHelpers.GetEncoding(str);
                    SmsHelpers.CountSmsParts(str);
                }
            }
        }

        [TestMethod]
        public void CountSmsParts_7BitTest()
        {
            for (var i = 1; i < 1100; i++)
            {
                var generated = new StringBuilder();

                for (var j = 0; j < i; j++)
                {
                    // append a random character
                    generated.Append(GsmCharacters.OrderBy(n => Guid.NewGuid()).First());
                }

                var expectedNumOfParts = i <= 160
                    ? 1
                    : (int)Math.Ceiling((decimal)i / 153);

                var parts = SmsHelpers.CountSmsParts(generated.ToString());

                Assert.AreEqual(parts, expectedNumOfParts);
            }
        }

        [TestMethod]
        [DataRow('≀')]
        [DataRow('←')]
        public void CountSmsParts_UnicodeCharTest(char c)
        {
            var message = new string(c, 69);
            Assert.AreEqual(SmsHelpers.CountSmsParts(message), 1);

            message = new string(c, 70);
            Assert.AreEqual(SmsHelpers.CountSmsParts(message), 1);

            message = new string(c, 71);
            Assert.AreEqual(SmsHelpers.CountSmsParts(message), 2);

            message = new string(c, 134);
            Assert.AreEqual(SmsHelpers.CountSmsParts(message), 2);

            message = new string(c, 135);
            Assert.AreEqual(SmsHelpers.CountSmsParts(message), 3);
        }
    }
}
