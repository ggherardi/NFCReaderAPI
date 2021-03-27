using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.NDEF
{
    /// <summary>
    /// NDEF Record format
    /// Reference: NFC Data Exchange Format (NDEF), chapter 3.2, pag. 14
    /// </summary>
    public class NDEFRecord
    {

        private bool _isShortRecord;
        private bool _hasID;

        // Flag field: (MB, ME, CF, SR, IL, TNF)
        public NDEFRecordFlag RecordFlag { get; set; }
        public byte FlagField { get; set; }
        public byte TypeLengthField { get; set; }
        public byte[] PayloadLengthField { get; set; }
        public byte IDLengthField { get; set; }
        public byte TypeIdentifierField { get; set; }
        public byte IDField { get; set; }
        public NDEFRecordType RecordType { get; set; }
        public byte[] NDEFRecordPayloadBytes { get; set; }

        public NDEFRecord(NDEFRecordType recordType, NDEFRecordFlag flag, int idLength, int id, bool chunk)
        {
            // Add management for chunked record, not required right now
            // Getting NDEFRecordType byte
            RecordType = recordType;
            NDEFRecordPayloadBytes = RecordType.GetBytes();

            // Determining payload length byte/bytes
            byte[] payloadLength = BitConverter.GetBytes(NDEFRecordPayloadBytes.Length);
            _isShortRecord = NDEFRecordPayloadBytes.Length <= 255;
            int numberOfPayloadLengthFields = _isShortRecord ? 1 : 4;
            PayloadLengthField = _isShortRecord ? new byte[numberOfPayloadLengthFields] : new byte[numberOfPayloadLengthFields];            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(payloadLength);
            }
            int j = 3;            
            for (int i = 0; i < numberOfPayloadLengthFields; i++)
            {
                PayloadLengthField[i] = payloadLength[j--];
            }

            // Setting Type Length and Type bytes
            TypeLengthField = (byte)RecordType.TypeLength;
            TypeIdentifierField = (byte)RecordType.TypeIdentifier;

            // Setting ID Length and ID bytes only if the flag has IL field = 1
            _hasID = flag.IDLengthBit == NDEFRecordFlag.IDLength.True;
            if (_hasID)
            {
                IDLengthField = (byte)idLength;
                IDField = (byte)id;
            }

            // Setting Flag field bits
            RecordFlag = flag;
            if(RecordFlag.ShortRecordBit == NDEFRecordFlag.ShortRecord.True && !_isShortRecord)
            {
                RecordFlag.ShortRecordBit = 0;
            }
            FlagField = flag.GetByte();
        }

        public NDEFRecord(NDEFRecordType recordType, NDEFRecordFlag flag, bool chunked = false) : this(recordType, flag, 0, 0, chunked) { }

        public NDEFRecord(NDEFRecordType recordType, bool chunked = false) : this(recordType, new NDEFRecordFlag(NDEFRecordFlag.MessageBegin.True, NDEFRecordFlag.MessageEnd.True, chunked ? NDEFRecordFlag.Chunk.True : NDEFRecordFlag.Chunk.False, NDEFRecordFlag.ShortRecord.True, NDEFRecordFlag.IDLength.False, NDEFRecordFlag.TypeNameFormat.NFCForumWellKnownType), chunked) { }

        public NDEFRecord() { }

        public byte[] GetBytes()
        {
            List<byte> record = new List<byte>();
            record.Add(FlagField);
            record.Add(TypeLengthField);
            record.AddRange(PayloadLengthField);
            if (_hasID)
            {
                record.Add(IDLengthField);
            }
            record.Add(TypeIdentifierField);
            if (_hasID)
            {
                record.Add(IDField);
            }
            record.AddRange(NDEFRecordPayloadBytes);
            return record.ToArray();
        }
    }

    /// <summary>
    /// Class used to set all the bits required in the flag
    /// Reference: NFC Data Exchange Format (NDEF) Technical Specifications, chapters 3.2.1 - 3.2.6, pag. 14 - 16
    /// </summary>
    public class NDEFRecordFlag
    {
        public MessageBegin MessageBeginBit { get; set; }
        public MessageEnd MessageEndBit { get; set; }
        public Chunk ChunkBit { get; set; }
        public ShortRecord ShortRecordBit { get; set; }
        public IDLength IDLengthBit { get; set; }
        public TypeNameFormat TNFBits { get; set; }

        public NDEFRecordFlag(MessageBegin messageBegin, MessageEnd messageEnd, Chunk chunk, ShortRecord shortRecord, IDLength idLength, TypeNameFormat tnf)
        {
            MessageBeginBit = messageBegin;
            MessageEndBit = messageEnd;
            ChunkBit = chunk;
            ShortRecordBit = shortRecord;
            IDLengthBit = idLength;
            TNFBits = tnf;
        }

        public NDEFRecordFlag() { }

        public static NDEFRecordFlag GetNDEFRecordFlagFromByte (byte flagByte)
        {
            NDEFRecordFlag recordFlag = new NDEFRecordFlag();           
            recordFlag.MessageBeginBit = (MessageBegin)(flagByte & (int)MessageBegin.True);
            recordFlag.MessageEndBit = (MessageEnd)(flagByte & (int)MessageEnd.True);
            recordFlag.ChunkBit = (Chunk)(flagByte & (int)Chunk.True);
            recordFlag.ShortRecordBit = (ShortRecord)(flagByte & (int)ShortRecord.True);
            recordFlag.IDLengthBit = (IDLength)(flagByte & (int)IDLength.True);
            recordFlag.TNFBits = (TypeNameFormat)(flagByte & (int)TypeNameFormat.Reserved);
            return recordFlag;
        }

        public byte GetByte()
        {
            return (byte)(0 | (int)MessageBeginBit | (int)MessageEndBit | (int)ChunkBit | (int)ShortRecordBit | (int)IDLengthBit | (int)TNFBits);
        }

        public enum MessageBegin
        {
            True = 0x80,
            False = 0x00
        }

        public enum MessageEnd
        {
            True = 0x40,
            False = 0x00
        }

        public enum Chunk
        {
            True = 0x20,
            False = 0x00
        }

        public enum ShortRecord
        {
            True = 0x10,
            False = 0x00
        }

        public enum IDLength
        {
            True = 0x08,
            False = 0x00
        }

        public enum TypeNameFormat
        {
            Empty = 0x00,
            NFCForumWellKnownType = 0x01,
            MediaType = 0x02,
            AbsoluteURI = 0x03,
            NFCForumExternalType = 0x04,
            Unknown = 0x05,
            Unchanged = 0x06,
            Reserved = 0x07
        }
    }
}
