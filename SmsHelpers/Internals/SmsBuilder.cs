using System;
using System.Collections.Generic;

namespace Texting.Internals
{
    internal class SmsBuilder
    {
        public List<SmsPart> Parts = new List<SmsPart>();
        private string _currentPart = "";
        private int _currentPartLength;
        private readonly SmsEncoding _smsEncoding;

        public SmsBuilder(List<TextBlock> blocks, SmsEncoding encoding)
        {
            _smsEncoding = encoding;

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
                if ((block.Length + _currentPartLength) > lengthLimit && block.Length <= lengthLimit)
                {
                    MoveCurrentToNewPart();
                }

                AddContentRecursive(block.Content, lengthLimit);

                var moreThanOnePart = Parts.Count > 1 || Parts.Count == 1 && _currentPartLength > 0;

                if (lengthLimit == SmsConstants.GsmLengthLimitSinglePart && moreThanOnePart)
                {
                    Clear();
                    AddGsm(blocks, (int)SmsConstants.GsmLengthLimitMultipart);
                    return;
                }
            }

            if (_currentPartLength > 0)
            {
                MoveCurrentToNewPart();
            }
        }

        private void AddUnicode(List<TextBlock> blocks, int lengthLimit = SmsConstants.UnicodeLengthLimitSinglePart)
        {
            foreach (var block in blocks)
            {
                // need to move the current block to the next sms part, because it will not fit
                if ((block.Length + _currentPartLength) > lengthLimit && block.Length <= lengthLimit)
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

            if (_currentPartLength > 0)
            {
                MoveCurrentToNewPart();
            }
        }

        private void Clear()
        {
            Parts = new List<SmsPart>();
            _currentPartLength = 0;
            _currentPart = "";
        }

        private void MoveCurrentToNewPart()
        {
            var part = new SmsPart
            {
                Length = _currentPartLength,
                Content = _currentPart
            };

            Parts.Add(part);

            _currentPart = "";
            _currentPartLength = 0;
        }

        private void AddContentRecursive(string content, int lengthLimit)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            var charsLeft = lengthLimit - _currentPartLength;

            if (charsLeft <= 0)
            {
                MoveCurrentToNewPart();
                charsLeft = lengthLimit;
            }

            var charCounter = 0;
            var lengthCounter = 0;
            int index;
            var lastSurrogatePos = 0;

            for (index = 0; index < content.Length && lengthCounter <= charsLeft; index++)
            {
                var c = content[index];

                if (_smsEncoding == SmsEncoding.Gsm7Bit)
                {
                    var charLen = SmsInternalHelper.GetGsmCharLength(c);

                    if (charLen == 2)
                    {
                        lastSurrogatePos = index;
                        if (charsLeft == 1)
                        {
                            MoveCurrentToNewPart();
                            charsLeft = lengthLimit;
                        }
                    }

                    if (lengthCounter + charLen > charsLeft)
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

            _currentPart += content.Substring(0, charCounter);
            _currentPartLength += lengthCounter;

            var lastCharLen = 1;

            if (lastSurrogatePos == charCounter || lastSurrogatePos == (charCounter - 2))
            {
                lastCharLen = 2;
            }

            if (lastSurrogatePos == 0)
            {
                lastCharLen = 0;
            }

            AddContentRecursive(
                _smsEncoding == SmsEncoding.Gsm7Bit
                    ? content.Substring(charCounter, content.Length - charCounter)
                    : content.Substring(charCounter - lastCharLen, content.Length - charCounter), lengthLimit);
        }
    }
}
