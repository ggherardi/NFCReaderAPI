using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.NDEF
{
    /// <summary>
    /// Tag, Length, Value block template 
    /// Reference:  NFCForum-Type-2-Tag_1.1 specifications, chapter 2.3 TLV blocks, pag. 9
    /// Example of TLV with NDEFMessage as Text Record Type Definition:
    /// {0x03 - 0x0C - [0xD1 - 0x01 - 0x08 - 0x54 - (0x02 - 0x65 - 0x6E - (0x61 - 0x62 - 0x63 - 0x64 - 0x65))]}
    /// - TLV headers
    /// 0x03: Tag byte (0x03 for NDEF Message)
    /// 0x0C: Length byte of the Record (0x0C = 12)
    /// - NDEFMessage headers
    /// 0xD1: FLAG byte of the Record (0xD1 = 209, meaning: MB = 1, ME = 1, CF = 0, SR = 1, IL = 0, TNF = 001 (well-known type))  
    /// 0x01: Type Length byte (0x01 = well-known type)
    /// 0x08: Payload Length (0x08 = 8, including Text Record Type header)
    /// 0x54: Type Identifier (0x54 = "T", indicating Text Record Type)
    /// - Record Type headers
    /// 0x02: Flag byte (0x02 = 2, meaning: UTF8/16 = 0 (UTF8), Bit6 = 0 (Reserved), Bit5_0 = 00010 (Language Code with 2 bytes))
    /// 0x65: Language Code byte 1 (0x65 = "e")
    /// 0x6E: Language Code byte 2 (0x6E = "n") so the Language is "en" (English)
    /// - Payload
    /// 0x61, 0x62, 0x63, 0x64, 0x65: Actual text Payload ("abcde")
    /// </summary>
    public abstract class TLVBlock
    {
        protected byte _tagByte;
        protected byte[] _lengthBytes = new byte[] { };
        protected byte[] _valueBytes = new byte[] { };

        /// <summary>
        /// Tag Field Value byte
        /// Reference: NFCForum-Type-2-Tag_1.1 specifications, chapter 2.3 TLV blocks, pag. 10, Table 2: Defined TLV blocks
        /// </summary>
        public abstract byte TagByte { get; }
        public virtual byte[] LengthBytes { get => _lengthBytes; protected set => _lengthBytes = value; }
        public virtual byte[] ValueBytes { get => _valueBytes; protected set => _valueBytes = value; }
        public virtual int Length { get; set; }        

        public virtual byte[] GetFormattedBlock()
        {
            return (new byte[] { TagByte }).Concat(LengthBytes).Concat(ValueBytes).ToArray();
        }

        /// <summary>
        /// The Length field for a TLV block can be either 0, 1 or 3 bytes long.
        /// Reference: NFCForum-Type-2-Tag_1.1 specifications, chapter 2.3 TLV block, pag. 9
        /// </summary>
        /// <param name="valueLength"></param>
        /// <returns></returns>
        public static byte[] GetValueLengthInBytes(int valueLength)
        {
            byte[] valueLengthInBytes = BitConverter.GetBytes(valueLength);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(valueLengthInBytes);
            }
            if (valueLength < 255)
            {
                valueLengthInBytes = new byte[] { valueLengthInBytes[3] };
            }
            else
            {
                valueLengthInBytes = new byte[] { 0xFF, valueLengthInBytes[2], valueLengthInBytes[3] };
            }
            return valueLengthInBytes;
        }

        /// <summary>
        /// Retrieves the integer inside the Length byte(s) (1-3, varying on the length) of the TLV block
        /// </summary>
        /// <param name="lengthBytes"></param>
        /// <returns></returns>
        public static int GetLengthFromBytes(byte[] lengthBytes)
        {
            byte[] valueBytes = new byte[2];
            // If the first byte is 0xFF, it means that the length is in the 2 following bytes
            if (lengthBytes[0] == 0xFF)
            {
                // Copies the 2 bytes following 0xFF inside another array
                Array.Copy(lengthBytes, 1, valueBytes, 0, 2);
            }
            else
            {
                // Copies the single byte in another array
                valueBytes[1] = lengthBytes[0];
            }
            // Calculate the integer by adding the byte(s)
            return (valueBytes[0] > 0 ? valueBytes[0] + 255 : 0) + valueBytes[1];
        }
    }  
}
