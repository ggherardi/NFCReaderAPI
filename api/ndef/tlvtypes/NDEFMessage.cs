using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.NDEF
{

    /// <summary>
    /// NDEFMessage Tag Length Value Block
    /// Reference:  NFCForum-Type-2-Tag_1.1 specifications, chapter 2.3.4 NDEF Message TLV, pag. 12
    /// </summary>
    public class NDEFMessage : TLVBlock
    {
        /// <summary>
        /// Tag Field Value byte for "NDEF Message TLV"        
        /// </summary>
        public override byte TagByte { get => 0x03; }
        public NDEFRecord Record { get; set; }
        public int TotalHeaderLength { get; set; }        
        public int ContentPayloadLength { get; set; }

        public NDEFMessage() { }

        public NDEFMessage(byte[] bytes, NDEFRecordType.Types type) 
        {
            switch(type)
            {
                case NDEFRecordType.Types.Text:
                    RTDText rtdText = new RTDText(bytes, RTDText.EnglishLanguage);
                    NDEFRecord record = new NDEFRecord(rtdText);                    
                    ValueBytes = record.GetBytes();
                    LengthBytes = GetValueLengthInBytes(ValueBytes.Length);
                    Record = record;
                    break;
                default: break;
            }
        }

        public NDEFMessage(string text, NDEFRecordType.Types type) : this(Encoding.ASCII.GetBytes(text), type) { }

        public static NDEFMessage GetNDEFMessageFromBytes(byte[] bytes)
        {
            if(bytes.Length == 0)
            {
                throw new Exception("The provided bytes buffer is empty");
            }
            int bytesReadToSkip = 1;
            NDEFMessage message = new NDEFMessage();
            if (bytes[0] == message.TagByte)
            {                
                message.Length = TLVBlock.GetLengthFromBytes(bytes.Skip(bytesReadToSkip).Take(3).ToArray());
                bool isLongMessage = message.Length > 255;
                message.LengthBytes = isLongMessage ? bytes.Skip(bytesReadToSkip).Take(3).ToArray() : bytes.Skip(bytesReadToSkip).Take(1).ToArray();
                bytesReadToSkip += isLongMessage ? 3 : 1;
                if (message.Length > 0)
                {
                    NDEFRecord record = new NDEFRecord();
                    NDEFRecordFlag recordFlag = NDEFRecordFlag.GetNDEFRecordFlagFromByte(bytes.Skip(bytesReadToSkip++).First());
                    record.RecordFlag = recordFlag;
                    record.FlagField = recordFlag.GetByte();
                    bool hasId = (record.FlagField & (int)NDEFRecordFlag.IDLength.True) == (int)NDEFRecordFlag.IDLength.True;
                    bool isShortRecord = (record.FlagField & (int)NDEFRecordFlag.ShortRecord.True) == (int)NDEFRecordFlag.ShortRecord.True;
                    record.TypeLengthField = bytes.Skip(bytesReadToSkip++).First();
                    int payloadBytesCount = isShortRecord ? 1 : 4;
                    record.PayloadLengthField = bytes.Skip(bytesReadToSkip).Take(payloadBytesCount).ToArray();
                    bytesReadToSkip += payloadBytesCount;
                    if (hasId)
                    {
                        record.IDLengthField = bytes[bytesReadToSkip++];
                        record.TypeIdentifierField = bytes[bytesReadToSkip++];
                        record.IDField = bytes[bytesReadToSkip++];
                    }
                    else
                    {
                        record.TypeIdentifierField = bytes[bytesReadToSkip++];
                    }                    
                    NDEFRecordType type = NDEFRecordType.GetNDEFRecordTypeWithTypeIdentifier(record.TypeIdentifierField);                    
                    type.BuildRecordFromBytes(bytes.Skip(bytesReadToSkip).Take(type.HeaderLength).ToArray());                    
                    record.RecordType = type;
                    message.TotalHeaderLength = bytesReadToSkip += type.HeaderLength;
                    message.Record = record;

                    byte[] payloadLengthClone = record.PayloadLengthField.Take(record.PayloadLengthField.Length).ToArray();
                    while(payloadLengthClone.Length < 4)
                    {
                        payloadLengthClone = payloadLengthClone.Concat(new byte[] { 0 }).ToArray();
                    }
                    message.ContentPayloadLength = BitConverter.ToInt32(payloadLengthClone, 0) - record.RecordType.HeaderLength;
                }
            }
            else
            {
                // No NDEF Message found
            }
            return message;
        }

        public override byte[] GetFormattedBlock()
        {
            byte[] tlvBlock = new byte[] { TagByte };
            return tlvBlock.Concat(LengthBytes).Concat(ValueBytes).Concat(new Terminator().GetFormattedBlock()).ToArray();
        }
    }
}
