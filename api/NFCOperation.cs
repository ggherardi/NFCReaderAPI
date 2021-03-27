using CSharp.NFC.NDEF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC
{
    public class NFCOperation
    {
        private readonly NFCCommand _readerCommand;
        private readonly NFCCommand _controllerCommand;
        private readonly NFCCommand _cardCommand;
        private readonly byte[] _wrappedCommand;
        private readonly string _wrappedCommandAsHex;

        private NFCOperationType OperationType { get; set; }

        public byte[] WrappedCommand { get => _wrappedCommand; }
        public string WrappedCommandAsHex { get => _wrappedCommandAsHex; }
        public byte[] ResponseBuffer { get; set; }
        public string ResponseAsHexString { get; private set; }
        public string ResponsePayloadAsHexString { get; private set; }
        public byte[] ResponsePayloadBuffer { get; private set; }
        public string ResponsePayloadText { get; private set; }
        public NFCCommand ReaderCommand { get => _readerCommand; }
        public NFCCommand ControllerCommand { get => _controllerCommand; }
        public NFCCommand CardCommand { get => _cardCommand; }
        public int Status { get; set; }        

        public enum NFCOperationType
        {
            ReaderOperation,
            CardOperation
        }

        public NFCOperation(NFCCommand readerCommand, NFCCommand controllerCommand, NFCCommand cardCommand, byte[] wrappedCommand, NFCOperationType operationType = NFCOperationType.CardOperation )
        {
            _readerCommand = readerCommand;
            _controllerCommand = controllerCommand;
            _cardCommand = cardCommand;
            _wrappedCommand = wrappedCommand;
            _wrappedCommandAsHex = BitConverter.ToString(_wrappedCommand);
            OperationType = operationType;
        }

        public NFCOperation(NFCCommand readerCommand) : this(readerCommand, null, null, readerCommand.CommandBytes, NFCOperationType.ReaderOperation) { }

        public NFCOperation() { }

        public void ElaborateResponse()
        {
            NFCPayload readerPayload = null;
            NFCPayload controllerPayload = null;
            NFCPayload cardPayload = null;
            ResponseBuffer = Utility.TrimTrailingZeros(ResponseBuffer);
            if (_readerCommand != null)
            {
                readerPayload = _readerCommand.ExtractPayload(ResponseBuffer);
                _readerCommand.Payload = readerPayload;
                if (OperationType == NFCOperationType.ReaderOperation)
                {
                    ResponsePayloadBuffer = readerPayload.PayloadBytes;
                    ResponsePayloadText = readerPayload.PayloadText;
                }
            }            
            if(_controllerCommand != null)
            {
                controllerPayload = _controllerCommand.ExtractPayload(readerPayload.PayloadBytes);
                _controllerCommand.Payload = controllerPayload;
            }
            if (_cardCommand != null)
            {
                cardPayload = _cardCommand.ExtractPayload(controllerPayload.PayloadBytes);
                _cardCommand.Payload = cardPayload;
                if (OperationType == NFCOperationType.CardOperation)
                {
                    ResponsePayloadBuffer = cardPayload.PayloadBytes;
                    ResponsePayloadText = cardPayload.PayloadText;
                }
            }
            ResponseAsHexString = Utility.GetByteArrayAsHexString(ResponseBuffer);
            ResponsePayloadAsHexString = BitConverter.ToString(ResponsePayloadBuffer);
        }
    }

    public class NFCPayload
    {
        public byte[] PayloadBytes { get; set; }
        public string PayloadText { get; set; }

        public NFCPayload(byte[] payload, string payloadText)
        {
            PayloadBytes = payload;
            PayloadText = payloadText;
        }

        public NFCPayload(byte[] payload) : this(payload, string.Empty) { }

        public NFCPayload() : this(new byte[] { }) { }
    }
}
