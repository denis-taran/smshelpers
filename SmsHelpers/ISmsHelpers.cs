
using System.Collections.Generic;

namespace Texting
{
    /// <summary>
    ///   Utilities to work with text messages
    /// </summary>
    public interface ISmsHelpers
    {
        /// <summary>
        ///   Detect character set that will be used to send the provided text message
        /// </summary>
        /// <param name="text">Text message content</param>
        /// <exception cref="ArgumentNullException">If you pass a `null` value to this method.</exception>
        /// <returns></returns>
        SmsEncoding GetEncoding(string text);

        /// <summary>
        ///   Get number of text message parts that will be used to send your combined message.
        /// </summary>
        /// <param name="text">Text message content</param>
        /// <exception cref="ArgumentNullException">This exception will be thrown if you pass a `null` value.</exception>
        /// <returns></returns>
        int CountSmsParts(string text);

        /// <summary>
        ///   Perform the recommended new-line normalizations for the provided SMS.
        /// </summary>
        /// <param name="text">Text message to normalize</param>
        /// <remarks>Before sending a text message (SMS), it is recommended to normalize new line characters and only send \r without \n</remarks>
        /// <returns>New string with normalized new line character.</returns>
        string NormalizeNewLines(string text);

        /// <summary>
        ///   Split the provided message to separate SMS parts. When possible, word wrap will be used to avoid splitting words.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        List<string> SplitMessageWithWordWrap(string text);
    }
}
