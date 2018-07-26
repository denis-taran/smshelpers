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

            var lengthLimit = encoding == SmsEncoding.Gsm7Bit
                ? SmsConstants.GsmLengthLimitSinglePart
                : SmsConstants.UnicodeLengthLimitSinglePart;

            Add(blocks, lengthLimit);
        }

        private void Add(List<TextBlock> blocks, int lengthLimit)
        {
            foreach (var block in blocks)
            {
                // need to move the current block to the next sms part, because it will not fit
                if (block.Length + _currentPartLength > lengthLimit && block.Length <= lengthLimit)
                {
                    MoveCurrentToNewPart();
                }

                AddContent(block.Content, lengthLimit);

                switch (_smsEncoding)
                {
                    case SmsEncoding.GsmUnicode when lengthLimit == SmsConstants.UnicodeLengthLimitSinglePart && IsMultipart():
                        Clear();
                        Add(blocks, (int)SmsConstants.UnicodeLengthLimitMultipart);
                        return;
                    case SmsEncoding.Gsm7Bit when lengthLimit == SmsConstants.GsmLengthLimitSinglePart && IsMultipart():
                        Clear();
                        Add(blocks, (int)SmsConstants.GsmLengthLimitMultipart);
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

        private void AddContent(string content, int lengthLimit)
        {
            while (true)
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

                if (charsLeft == 1 && char.IsHighSurrogate(content[0]))
                {
                    MoveCurrentToNewPart();
                    charsLeft = lengthLimit;
                }

                var charCounter = 0;
                var lengthCounter = 0;

                foreach (var c in content)
                {
                    if (lengthCounter > charsLeft)
                    {
                        break;
                    }

                    if (_smsEncoding == SmsEncoding.Gsm7Bit)
                    {
                        var charLen = SmsInternalHelper.GetGsmCharLength(c);

                        if (charLen == 2)
                        {
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
                        var isHighSurrogate = char.IsHighSurrogate(c);

                        if (lengthCounter + (isHighSurrogate ? 1 : 0) >= charsLeft)
                        {
                            break;
                        }

                        lengthCounter++;
                        charCounter++;
                    }
                }

                _currentPart += content.Substring(0, charCounter);
                _currentPartLength += lengthCounter;

                content = content.Substring(charCounter, content.Length - charCounter);
            }
        }

        private bool IsMultipart()
        {
            return Parts.Count > 1 || Parts.Count == 1 && _currentPartLength > 0;
        }
    }
}
