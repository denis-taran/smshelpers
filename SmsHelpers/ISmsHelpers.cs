using System;

namespace Sms.Helpers
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
    }
}
