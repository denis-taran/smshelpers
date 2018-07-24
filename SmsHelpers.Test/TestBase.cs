using System;
using System.Globalization;
using System.Text;

namespace Texting.Tests
{
    public class TestBase
    {
        protected const string HighSurrogateChars70 = "🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳";
        protected const string HighSurrogateChars60 = "🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳🐳";
        protected const string Gsm7BitBaseChars20 = "01234567890123456789";
        protected const string Gsm7BitBaseChars40 = "0123456789012345678901234567890123456789";
        protected const string Gsm7BitBaseChars60 = "012345678901234567890123456789012345678901234567890123456789";
        protected const string Gsm7BitBaseChars150 = "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
        protected const string Gsm7BitBaseChars90 = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
        protected const string Gsm7BitGoogleLink60 = "https://www.google.com/search?s=a&gs_l=gbb-ab.3..00.1365.188";

        protected static readonly Random RandomNum = new Random();

        protected ISmsHelpers SmsHelpers { get; }

        protected TestBase()
        {
            SmsHelpers = new SmsHelpers();
        }

        protected static string GenerateRandomUnicodeString(int length)
        {
            var result = new StringBuilder();

            do
            {
                var ch = (char)RandomNum.Next(short.MaxValue);
                var category = char.GetUnicodeCategory(ch);

                if (category != UnicodeCategory.OtherNotAssigned && ch != '\n')
                {
                    result.Append(ch);
                }

            } while (result.Length < length);

            return result.ToString();
        }
    }
}
