using CSharp.NFC.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.NDEF
{
    public class NDEFStreamReader
    {
        private NDEFMessage _NDEFMessage { get; set; }
        private NFCReader _reader { get; set; }
        private int _remainingBytes { get; set; }

        public NDEFStreamReader(NFCReader reader, NDEFMessage message)
        {
            _reader = reader;
            _NDEFMessage = message;
            _remainingBytes = _NDEFMessage.ContentPayloadLength;
        }

        public NDEFStreamReader() { }

        /// <summary>
        /// Read payload bytes using the length in the NDEF Header. Throws an exception if no TLV Terminator is found at the end of the payload.
        /// </summary>
        /// <returns></returns>
        public NDEFOperation ReadBytes()
        {
            NDEFOperation ndefOperation = new NDEFOperation();
            int i = 1;
            while (_remainingBytes > 0)
            {
                NFCOperation nfcOperation = _reader.ReadBlocks((byte)(4 * i));
                byte[] bytesToRead = nfcOperation.ReaderCommand.Payload.PayloadBytes;
                if (i == 1)
                {                 
                    bytesToRead = bytesToRead.Skip(_NDEFMessage.TotalHeaderLength).ToArray();                    
                }
                if (_remainingBytes - bytesToRead.Length < 0)
                {
                    if(bytesToRead[_remainingBytes] != new Terminator().TagByte)
                    {
                        throw new Exception("No TLV Terminator block found at the end of the payload.");
                    }
                    bytesToRead = bytesToRead.Take(_remainingBytes).ToArray();
                }
                _remainingBytes -= bytesToRead.Length;                
                _NDEFMessage.Record.RecordType.AddTextToPayload(bytesToRead);
                ndefOperation.Operations.Add(nfcOperation);
                i++;
            }            
            return ndefOperation;
        }
    }
}
