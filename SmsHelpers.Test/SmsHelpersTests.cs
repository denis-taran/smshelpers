using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit;

namespace Texting.Tests
{
    public class SmsHelpersTests
    {
        private const string HighSurrogateChars70 = "🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳";
        private const string HighSurrogateChars60 = "🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳";
        private const string Gsm7BitBaseChars20 = "01234567890123456789";
        private const string Gsm7BitBaseChars40 = "0123456789012345678901234567890123456789";
        private const string Gsm7BitBaseChars150 = "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
        private const string Gsm7BitBaseChars90 = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
        private const string Gsm7BitGoogleLink60 = "https://www.google.com/search?s=a&gs_l=gbb-ab.3..00.1365.188";

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
            SmsHelpers = new SmsHelpers();
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

        [Fact]
        public void GetCharset_ThrowAnException()
        {
            Assert.Throws<ArgumentNullException>(() => SmsHelpers.GetEncoding(null))

            ;
        }

        [Theory]
        [InlineData("", SmsEncoding.Gsm7Bit)]
        [InlineData("a", SmsEncoding.Gsm7Bit)]
        [InlineData("≀", SmsEncoding.GsmUnicode)]
        [InlineData("a≀", SmsEncoding.GsmUnicode)]
        public void GetCharset_DetectEncoding(string text, SmsEncoding expectedEncoding)
        {
            var encoding = SmsHelpers.GetEncoding(text);
            Assert.Equal(encoding, expectedEncoding);
        }

        [Fact]
        public void GetCharset_DetectEncodingGsmCharacters()
        {
            foreach (var c in GsmCharacters)
            {
                var encoding = SmsHelpers.GetEncoding(c.ToString());
                Assert.Equal(SmsEncoding.Gsm7Bit, encoding);
            }

            foreach (var c in GsmCharactersExtension)
            {
                var encoding = SmsHelpers.GetEncoding(c.ToString());
                Assert.Equal(SmsEncoding.Gsm7Bit, encoding);
            }
        }

        [Fact]
        public void NormalizeNewLines_ReturnsNull()
        {
            Assert.Null(SmsHelpers.NormalizeNewLines(null));
        }

        [Theory]
        [InlineData("a\ra", "a\ra")]
        [InlineData("a\r\na", "a\ra")]
        [InlineData("a\n\ra", "a\ra")]
        [InlineData("a\r\ra", "a\r\ra")]
        [InlineData("a\r\n\r\na", "a\r\ra")]
        [InlineData("a\n\r\n\ra", "a\r\ra")]
        public void NormalizeNewLines_Test(string before, string expected)
        {
            var result = SmsHelpers.NormalizeNewLines(before);
            Assert.Equal(result, expected);
        }

        [Fact]
        public void CountSmsParts_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SmsHelpers.CountSmsParts(null));
        }

        [Theory]
        [InlineData("", 1)]
        [InlineData("1", 1)]
        [InlineData(@"Dear abcde,

This is a reminder that you have an appointment on 24/04/2011 10:30 with ABCABCAB ABCAC at eeBBBBBBBB Court, 4444444444444444444444444444444444444444 444.

If you are unable to keep this appointment, please call 44444'444444, 44444444444444444444 at 444444444444444 to reschedule your appointment at your earliest convenience.

We appreciate your business and look forward to seeing you soon!

Warm Regards,
4444444444444444444444444444444444", 3)]
        [InlineData(@"Hi 444444, this is a quick reminder that you have an
appointment with 444444444444444444 on 4/44/2015 1:54 PM.

If you would like to reschedule this appointment please call/ text 444444444444.

Have a great day!

444444444444444444444444
44444444444444444444444444444444444444444444444444444444
444444444444", 3)]
        public void CountSmsParts_Test(string sms, int expectedSmsParts)
        {
            var normalized = SmsHelpers.NormalizeNewLines(sms);
            Assert.True(SmsHelpers.CountSmsParts(normalized) == expectedSmsParts);
        }

        [Fact]
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

        [Fact]
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

                Assert.Equal(parts, expectedNumOfParts);
            }
        }

        [Theory]
        [InlineData('≀')]
        [InlineData('←')]
        public void CountSmsParts_UnicodeCharTest(char c)
        {
            var message = new string(c, 69);
            Assert.Equal(1, SmsHelpers.CountSmsParts(message));

            message = new string(c, 70);
            Assert.Equal(1, SmsHelpers.CountSmsParts(message));

            message = new string(c, 71);
            Assert.Equal(2, SmsHelpers.CountSmsParts(message));

            message = new string(c, 134);
            Assert.Equal(2, SmsHelpers.CountSmsParts(message));

            message = new string(c, 135);
            Assert.Equal(3, SmsHelpers.CountSmsParts(message));
        }

        [Theory]
        [InlineData(HighSurrogateChars70, 1)]
        [InlineData(HighSurrogateChars70 + "🐳", 2)]
        [InlineData(HighSurrogateChars70 + "1", 2)]
        [InlineData(HighSurrogateChars70 + HighSurrogateChars60 + "🐳🐳", 2)]
        [InlineData(HighSurrogateChars70 + HighSurrogateChars60 + "🐳🐳🐳", 3)]
        [InlineData(HighSurrogateChars70 + HighSurrogateChars60 + "🐳🐳1", 3)]
        [InlineData(HighSurrogateChars70 + HighSurrogateChars70 + HighSurrogateChars70 + HighSurrogateChars70 + HighSurrogateChars70, 6)]
        public void CountSmsParts_HighSurrogateTest(string content, int expectedLength)
        {
            var length = SmsHelpers.CountSmsParts(content);
            Assert.Equal(expectedLength, length);
        }

        [Theory]
        [InlineData(HighSurrogateChars60 + "🐳🐳🐳🐳🐳🐳", "🐳")]
        [InlineData(HighSurrogateChars60 + "🐳🐳🐳    🐳🐳🐳🐳", "🐳🐳🐳🐳")]
        [InlineData(HighSurrogateChars60 + "🐳🐳1    🐳🐳🐳🐳", "🐳🐳🐳🐳")]
        [InlineData(Gsm7BitBaseChars40 + "01234567890123456789🐳  1234567890", "1234567890")]
        [InlineData(Gsm7BitBaseChars40 + "🐳  " + Gsm7BitGoogleLink60 + " abc", Gsm7BitGoogleLink60 + " abc")]
        public void SplitMessage_TwoPartsTest(string message, string expectedSecondMessage)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(message);

            Assert.Equal(2, splitted.Count);
            Assert.Equal(expectedSecondMessage, splitted[1]);
        }

        [Theory]
        [InlineData(Gsm7BitBaseChars150 + "€ 12345678", "12345678")]
        [InlineData(Gsm7BitBaseChars150 + "01 12345678", "12345678")]
        [InlineData(Gsm7BitBaseChars90 + Gsm7BitBaseChars20 + " " + Gsm7BitGoogleLink60, Gsm7BitGoogleLink60)]
        public void SplitMessage_TwoParts7BitGsmTest(string message, string expectedSecondMessage)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(message);

            Assert.Equal(2, splitted.Count);
            Assert.Equal(expectedSecondMessage, splitted[1]);
        }

        [Theory]
        [InlineData(Gsm7BitBaseChars150 + "0123456789")]
        [InlineData(Gsm7BitBaseChars150 + "€€€€€")]
        [InlineData(Gsm7BitBaseChars90 + Gsm7BitGoogleLink60)]
        [InlineData(Gsm7BitGoogleLink60)]
        public void SplitMessage_SinglePart7BitGsmTest(string message)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(message);

            Assert.Single(splitted);
            Assert.Equal(message, splitted[0]);
        }

        [Theory]
        [InlineData(HighSurrogateChars60 + HighSurrogateChars70 + HighSurrogateChars70 + HighSurrogateChars70 + HighSurrogateChars70)]
        [InlineData(HighSurrogateChars60 + HighSurrogateChars70 + " " + Gsm7BitGoogleLink60 + HighSurrogateChars70 + " " + HighSurrogateChars70)]
        [InlineData(HighSurrogateChars70 + " " + HighSurrogateChars70)]
        [InlineData(Gsm7BitGoogleLink60)]
        [InlineData(Gsm7BitBaseChars90 + Gsm7BitBaseChars90 + Gsm7BitGoogleLink60)]
        [InlineData(Gsm7BitBaseChars90 + " " + Gsm7BitBaseChars90 + " " + Gsm7BitGoogleLink60)]
        public void SplitMessage_MultipartEqual(string text)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(text);
            var combined = string.Concat(splitted);

            var equal = combined.Equals(text);
            Assert.Equal(combined, text);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void SplitMessage_Empty(string text)
        {
            var result = SmsHelpers.SplitMessageWithWordWrap(text);
            Assert.Empty(result);
        }
    }
}
