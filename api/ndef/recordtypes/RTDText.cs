using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.NDEF
{
    public class RTDText : NDEFRecordType
    {
        public override int TypeIdentifier => 0x54; // Record type "T", Text
        public override int TypeLength => 0x01;
        public override int HeaderLength => 3;

        private int _flag = 0x00;
        private byte[] _language;
        private byte[] _textBytes;

        public static Language EnglishLanguage = new Language() { Code = new byte[] { 0x65, 0x6E }, Length = 2 };
        public static Language ItalianLanguage = new Language() { Code = new byte[] { 0x69, 0x74 }, Length = 2 };

        protected RTDText(byte[] textContentBytes, byte[] languageBytes, byte flagByte) 
        {            
            _textBytes = textContentBytes;
            _language = languageBytes;
            _flag = flagByte;
        }

        protected RTDText(string textContent, Language language, RTDTextFlag flag) : this(Encoding.ASCII.GetBytes(textContent), language.Code, flag.GetByte()) { }

        public RTDText(byte[] textContentBytes, Language language) : this(textContentBytes, language.Code, new RTDTextFlag(RTDTextFlag.LanguageEncoding.UTF8, language.Length).GetByte()) { }

        public RTDText(string textContent, Language language) : this(textContent, language, new RTDTextFlag(RTDTextFlag.LanguageEncoding.UTF8, language.Length)) { }

        public RTDText(byte[] textContentBytes) : this(textContentBytes, EnglishLanguage) { }

        public RTDText(string textContent) : this(textContent, EnglishLanguage) { }

        public RTDText() { }

        public override byte[] GetBytes()
        {
            byte[] rtdTextBytes = new byte[] { (byte)_flag, _language[0], _language[1] };
            return rtdTextBytes.Concat(_textBytes).ToArray();
        }

        public override void BuildRecordFromBytes(byte[] bytes)
        {
            _flag = bytes[0];
            _language = new byte[] { bytes[1], bytes[2] };
            _textBytes = new byte[] { };
        }

        public override void AddTextToPayload(byte[] bytes)
        {
            _textBytes = _textBytes.Concat(bytes).ToArray();
        }

        public override NDEFPayload GetPayload()
        {
            return new NDEFPayload()
            {
                Bytes = _textBytes,
                Text = this.ToString()
            };
        }

        public override string ToString()
        {
            string text = string.Empty;
            for(int i = 0; i < _textBytes.Length; i++)
            {
                text += $"{(char)_textBytes[i]}";
            }
            return text;
        }

        public enum TextEncoding
        {
            UTF8 = 0x00,
            UTF16 = 0x80
        }

        public class Language
        {
            public byte[] Code { get; set; }
            public int Length { get; set; }
        }
    }

    public class RTDTextFlag
    {
        public LanguageEncoding LanguageFormatBit { get; set; }
        public int LanguageCodeLength { get; set; }

        public RTDTextFlag(LanguageEncoding format, int languageCodeLength)
        {
            LanguageFormatBit = format;
            LanguageCodeLength = languageCodeLength.Clamp(0, 63);
        }

        public enum LanguageEncoding
        {
            UTF8 = 0x00,
            UTF16 = 0x80
        }

        public RTDTextFlag() { }

        public byte GetByte()
        {
            return (byte)(0 | (int)LanguageFormatBit | LanguageCodeLength);
        }
    }
}
