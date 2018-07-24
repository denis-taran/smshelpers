using System.Collections.Generic;

namespace Texting
{
    public class SmsSplittingResult
    {
        /// <summary>
        ///   Encoding that must be used for sending SMS
        /// </summary>
        public SmsEncoding Encoding { get; set; }

        /// <summary>
        ///   Individual SMS parts after splitting
        /// </summary>
        public List<SmsPart> Parts { get; set; }
    }
}
