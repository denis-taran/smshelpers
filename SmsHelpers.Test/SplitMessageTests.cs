using System.Linq;
using Xunit;

namespace Texting.Tests
{
    public class SplitMessageTests : TestBase
    {
        [Theory]
        [InlineData(HighSurrogateChars60 + "🐳🐳🐳🐳🐳🐳", "🐳🐳🐳")]
        [InlineData(HighSurrogateChars60 + "🐳🐳🐳    🐳🐳🐳🐳", "   🐳🐳🐳🐳")]
        [InlineData(HighSurrogateChars60 + "🐳🐳1    🐳🐳🐳🐳", "  🐳🐳🐳🐳")]
        [InlineData(Gsm7BitBaseChars40 + "01234567890123456789🐳  1234567890", "1234567890")]
        [InlineData(Gsm7BitBaseChars40 + "🐳  " + Gsm7BitGoogleLink60 + " abc", Gsm7BitGoogleLink60 + " abc")]
        [InlineData(Gsm7BitBaseChars150 + "123ABCDEFGHIKL", "ABCDEFGHIKL")]
        public void SplitMessage_TwoPartsTest(string message, string expectedSecondMessage)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(message);

            Assert.Equal(2, splitted.Parts.Count);
            Assert.Equal(expectedSecondMessage, splitted.Parts[1].Content);
        }

        [Theory]
        [InlineData(Gsm7BitBaseChars150 + "0123456789")]
        [InlineData(Gsm7BitBaseChars150 + "€€€€€")]
        [InlineData(Gsm7BitBaseChars90 + Gsm7BitGoogleLink60)]
        [InlineData(Gsm7BitGoogleLink60)]
        public void SplitMessage_SinglePart7BitGsmTest(string message)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(message);

            Assert.Single(splitted.Parts);
            Assert.Equal(message, splitted.Parts[0].Content);
        }

        [Theory]
        [InlineData(Gsm7BitBaseChars60 + "юююююююююю")]
        [InlineData(Gsm7BitBaseChars60 + " ююююююююю")]
        public void SplitMessage_SinglePartUnicodeTest(string message)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(message);

            Assert.Single(splitted.Parts);
            Assert.Equal(message, splitted.Parts[0].Content);
        }

        [Theory]
        [InlineData(Gsm7BitBaseChars60 + "ююююююююююю", "юююю")]
        //[InlineData(Gsm7BitBaseChars60 + " юююююююююю", "юююююююююю")]
        public void SplitMessage_TwoPartsUnicodeTest(string message, string expectedSecondPart)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(message);

            Assert.True(splitted.Parts.Count == 2);
            Assert.Equal(expectedSecondPart, splitted.Parts[1].Content);
        }

        [Theory]
        [InlineData(HighSurrogateChars60 + HighSurrogateChars70 + HighSurrogateChars70 + HighSurrogateChars70 + HighSurrogateChars70)]
        [InlineData(HighSurrogateChars60 + HighSurrogateChars70 + " " + Gsm7BitGoogleLink60 + HighSurrogateChars70 + " " + HighSurrogateChars70)]
        [InlineData(HighSurrogateChars70 + " " + HighSurrogateChars70)]
        [InlineData(Gsm7BitGoogleLink60)]
        [InlineData(Gsm7BitBaseChars90 + Gsm7BitBaseChars90 + Gsm7BitGoogleLink60)]
        [InlineData(Gsm7BitBaseChars150 + Gsm7BitBaseChars150 + Gsm7BitBaseChars150 + Gsm7BitBaseChars150)]
        [InlineData(Gsm7BitBaseChars150 + Gsm7BitBaseChars150 + Gsm7BitBaseChars150 + Gsm7BitBaseChars150 + "🐳")]
        [InlineData(Gsm7BitBaseChars90 + " " + Gsm7BitBaseChars90 + " " + Gsm7BitGoogleLink60)]
        public void SplitMessage_MultipartEqual(string text)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(text);
            var combined = string.Concat(splitted.Parts.Select(part => part.Content));

            Assert.Equal(combined, text);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void SplitMessage_Empty(string text)
        {
            var result = SmsHelpers.SplitMessageWithWordWrap(text);
            Assert.Empty(result.Parts);
        }

        [Fact]
        public void SplitMessage_RandomStringsTest()
        {
            for (var i = 0; i < 10000; i++)
            {
                var randomNum = RandomNum.Next(1, 400);
                var randomStr = GenerateRandomUnicodeString(randomNum);

                var splitted = SmsHelpers.SplitMessageWithWordWrap(randomStr);
                var combined = string.Concat(splitted.Parts.Select(part => part.Content));

                Assert.Equal(combined, randomStr);
            }
        }

        [Theory]
        [InlineData(Gsm7BitBaseChars150 + "123~ABCDEFGH", "~ABCDEFGH")]
        [InlineData(Gsm7BitBaseChars150 + "12~ABCDEFGH", "~ABCDEFGH")]
        [InlineData(Gsm7BitBaseChars150 + "1~ABCDEFGHIJ", "ABCDEFGHIJ")]
        [InlineData(Gsm7BitBaseChars150 + "€ 12345678", "12345678")]
        [InlineData(Gsm7BitBaseChars150 + "01 12345678", "12345678")]
        [InlineData(Gsm7BitBaseChars90 + Gsm7BitBaseChars20 + " " + Gsm7BitGoogleLink60, Gsm7BitGoogleLink60)]
        public void SplitMessage_GsmCharactersExtensionTwoPartTest(string message, string expectedSecondPart)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(message);

            Assert.True(splitted.Parts.Count == 2);
            Assert.Equal(expectedSecondPart, splitted.Parts[1].Content);
        }

        [Theory]
        [InlineData(Gsm7BitBaseChars150 + "12~" + Gsm7BitBaseChars150 + "0~", "~")]
        [InlineData(Gsm7BitBaseChars150 + "1~" + Gsm7BitBaseChars150 + "01~", "~")]
        [InlineData(Gsm7BitBaseChars150 + Gsm7BitBaseChars150 + "01234~", "~")]
        public void SplitMessage_GsmCharactersExtensionThreePartTest(string message, string expectedThirdPart)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(message);

            Assert.True(splitted.Parts.Count == 3);
            Assert.Equal(expectedThirdPart, splitted.Parts[2].Content);
        }

        [Theory]
        [InlineData(Gsm7BitBaseChars90 + "~" + Gsm7BitBaseChars40, null)]
        [InlineData(Gsm7BitBaseChars150 + "€ €123456", "€123456")]
        [InlineData(Gsm7BitBaseChars150 + "€ €12345", null)]
        [InlineData(Gsm7BitBaseChars150 + "€ € € € €", "€ € € €")]
        public void SplitMessage_GsmCharactersExtensionTests(string message, string expectedSecondPart)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(message);

            if (expectedSecondPart == null)
            {
                Assert.Single(splitted.Parts);
                Assert.Equal(message, splitted.Parts[0].Content);
            }
            else
            {
                Assert.True(splitted.Parts.Count == 2);
                Assert.Equal(expectedSecondPart, splitted.Parts[1].Content);
            }
        }

        [Theory]
        [InlineData(Gsm7BitBaseChars150 + " www.abc.com", "www.abc.com")]
        [InlineData(Gsm7BitBaseChars150 + " http://www.abc.com", "http://www.abc.com")]
        [InlineData(Gsm7BitBaseChars150 + " https://www.abc.com", "https://www.abc.com")]
        public void SplitMessage_UrlSplitTests(string message, string expectedSecondPart)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(message);

            Assert.True(splitted.Parts.Count == 2);
            Assert.Equal(expectedSecondPart, splitted.Parts[1].Content);
        }

        [Theory]
        [InlineData(Gsm7BitBaseChars150 + " 0123-234-23-23", "0123-234-23-23")]
        [InlineData(Gsm7BitBaseChars150 + " +1123-234-23-23", "+1123-234-23-23")]
        [InlineData(Gsm7BitBaseChars150 + " +11232342323", "+11232342323")]
        public void SplitMessage_PhoneSplitTests(string message, string expectedSecondPart)
        {
            var splitted = SmsHelpers.SplitMessageWithWordWrap(message);

            Assert.True(splitted.Parts.Count == 2);
            Assert.Equal(expectedSecondPart, splitted.Parts[1].Content);
        }

        [Fact]
        public void SplitMessage_MultipleLinksTest()
        {
            const string sms = Gsm7BitGoogleLink60 + " " + Gsm7BitBaseChars150 + " " + Gsm7BitGoogleLink60 + " " +
                               Gsm7BitBaseChars150 + " " + Gsm7BitGoogleLink60;

            var splitted = SmsHelpers.SplitMessageWithWordWrap(sms);

            Assert.True(splitted.Parts.Count == 5);
            Assert.Equal(SmsEncoding.Gsm7Bit, splitted.Encoding);

            Assert.Equal(Gsm7BitGoogleLink60 + " ", splitted.Parts[0].Content);
            Assert.Equal(Gsm7BitBaseChars150 + " ", splitted.Parts[1].Content);
            Assert.Equal(Gsm7BitGoogleLink60 + " ", splitted.Parts[2].Content);
            Assert.Equal(Gsm7BitBaseChars150 + " ", splitted.Parts[3].Content);
            Assert.Equal(Gsm7BitGoogleLink60, splitted.Parts[4].Content);
            
            Assert.Equal(61, splitted.Parts[0].Length);
            Assert.Equal(151, splitted.Parts[1].Length);
            Assert.Equal(61, splitted.Parts[2].Length);
            Assert.Equal(151, splitted.Parts[3].Length);
            Assert.Equal(60, splitted.Parts[4].Length);
        }
    }
}
