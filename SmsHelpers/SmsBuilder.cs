using System;
using System.Collections.Generic;

namespace Texting
{
    internal class SmsBuilder
    {
        public List<SmsPart> Parts = new List<SmsPart>();
        private string CurrentPart = "";
        private int CurrentPartLength = 0;
        private readonly SmsEncoding SmsEncoding;

        public SmsBuilder(List<TextBlock> blocks, SmsEncoding encoding)
        {
            SmsEncoding = encoding;

            switch (encoding)
            {
                case SmsEncoding.Gsm7Bit:
                    AddGsm(blocks);
                    break;
                case SmsEncoding.GsmUnicode:
                    AddUnicode(blocks);
                    break;
                default:
                    throw new Exception("Invalid SMS encoding");
            }
        }

        private void AddGsm(List<TextBlock> blocks, int lengthLimit = SmsConstants.GsmLengthLimitSinglePart)
        {
            foreach (var block in blocks)
            {
                // need to move the current block to the next sms part, because it will not fit
                if ((block.Length + CurrentPartLength) > lengthLimit && block.Length <= lengthLimit)
                {
                    MoveCurrentToNewPart();
                }

                AddContentRecursive(block.Content, lengthLimit);

                if (lengthLimit == SmsConstants.GsmLengthLimitSinglePart && Parts.Count > 1)
                {
                    Clear();
                    AddUnicode(blocks, (int)SmsConstants.GsmLengthLimitMultipart);
                    return;
                }
            }

            if (CurrentPartLength > 0)
            {
                MoveCurrentToNewPart();
            }
        }

        private void AddUnicode(List<TextBlock> blocks, int lengthLimit = SmsConstants.UnicodeLengthLimitSinglePart)
        {
            foreach (var block in blocks)
            {
                // need to move the current block to the next sms part, because it will not fit
                if ((block.Length + CurrentPartLength) > lengthLimit && block.Length <= lengthLimit)
                {
                    MoveCurrentToNewPart();
                }

                AddContentRecursive(block.Content, lengthLimit);

                if (lengthLimit == SmsConstants.UnicodeLengthLimitSinglePart && Parts.Count > 1)
                {
                    Clear();
                    AddUnicode(blocks, (int) SmsConstants.UnicodeLengthLimitMultipart);
                    return;
                }
            }

            if (CurrentPartLength > 0)
            {
                MoveCurrentToNewPart();
            }
        }

        private void Clear()
        {
            Parts = new List<SmsPart>();
            CurrentPartLength = 0;
            CurrentPart = "";
        }

        private void MoveCurrentToNewPart()
        {
            var part = new SmsPart
            {
                Length = CurrentPartLength,
                Content = CurrentPart
            };

            Parts.Add(part);

            CurrentPart = "";
            CurrentPartLength = 0;
        }

        private void AddContentRecursive(string content, int lengthLimit)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            var charsLeft = lengthLimit - CurrentPartLength;

            if (charsLeft <= 0)
            {
                MoveCurrentToNewPart();
                charsLeft = lengthLimit;
            }

            var charCounter = 0;
            var lengthCounter = 0;
            var index = 0;
            var lastSurrogatePos = 0;

            for (index = 0; index < content.Length && lengthCounter <= charsLeft; index++)
            {
                var c = content[index];

                if (SmsEncoding == SmsEncoding.Gsm7Bit)
                {
                    var charLen = SmsInternalHelper.GetGsmCharLength(c);
                    if ((lengthCounter + charLen) > charsLeft)
                    {
                        break;
                    }
                    lengthCounter += charLen;
                    charCounter++;
                }
                else
                {
                    var isSurrogate = char.IsHighSurrogate(c);

                    if (isSurrogate)
                    {
                        lastSurrogatePos = index;
                    }

                    // do not break high surrogates
                    if (isSurrogate && (lengthCounter + 1) > charsLeft)
                    {
                        break;
                    }

                    lengthCounter++;
                    charCounter++;
                }
            }

            CurrentPart += content.Substring(0, charCounter);
            CurrentPartLength += lengthCounter;

            var lastCharLen = 1;

            if (lastSurrogatePos == charCounter || lastSurrogatePos == (charCounter - 2))
            {
                lastCharLen = 2;
            }

            AddContentRecursive(content.Substring(charCounter - lastCharLen, content.Length - charCounter), lengthLimit);
        }
    }
}
