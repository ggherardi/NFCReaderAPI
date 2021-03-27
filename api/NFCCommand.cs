using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC
{
    public class NFCCommand
    {
        private Func<byte[], NFCPayload> _extractPayload = (responseBuffer) => { return new NFCPayload(responseBuffer); };
        public virtual byte[] CommandBytes { get; set; }
        public NFCCommandResponse Response { get; set; }
        public NFCPayload Payload { get; set; }
        public Func<byte[], NFCPayload> ExtractPayload { get => _extractPayload; set => _extractPayload = value; }

        public NFCCommand(byte[] commandBytes, byte[] responseHeaderBytes, int minResponseBufferLength)
        {
            CommandBytes = commandBytes;
            Response = new NFCCommandResponse
            {
                HeaderBytes = responseHeaderBytes,
                MinBufferLength = minResponseBufferLength
            };
        }

        public NFCCommand(NFCCommand commandToClone) : this(commandToClone.CommandBytes, commandToClone.Response.HeaderBytes, commandToClone.Response.MinBufferLength) { }

        /// <summary>
        /// Initializes a new NFCCommand. If this constructor is used, it is mandatory to specify the Bytes and the NFCCommandResponse.
        /// </summary>
        public NFCCommand() : this(new byte[] { }, new byte[] { }, 0) { }

        public void ConcatBytesToCommand(byte[] bytesToAdd)
        {
            CommandBytes = CommandBytes.Concat(bytesToAdd).ToArray();
        }
    }

    public class NFCCommandResponse
    {
        public NFCCommandStatus CommandStatus { get; private set; }
        public byte[] HeaderBytes { get; set; }
        public int MinBufferLength { get; set; }

        public NFCCommandResponse()
        {
            CommandStatus = new NFCCommandStatus();
            HeaderBytes = new byte[] { };
            MinBufferLength = 0;
        }

        public void SetCommandStatus(int resultCode, string message, NFCCommandStatus.Status result)
        {
            CommandStatus = new NFCCommandStatus()
            {
                ResultCode = resultCode,
                Message = message,
                Result = result
            };
        }

        public void SetCommandStatus(NFCCommandStatus.Status result) { SetCommandStatus((int)result, Utility.GetEnumDescription(result), result); }

        public void SetCommandSuccessful(int resultCode, string message) { SetCommandStatus(resultCode, message, NFCCommandStatus.Status.Success);  }

        public void SetCommandSuccessful() { SetCommandStatus((int)NFCCommandStatus.Status.Success, Utility.GetEnumDescription(NFCCommandStatus.Status.Success), NFCCommandStatus.Status.Success); }

        public void SetCommandFailure(int resultCode, string message) { SetCommandStatus(resultCode, message, NFCCommandStatus.Status.Failure); }

        public void SetCommandFailure() { SetCommandStatus((int)NFCCommandStatus.Status.Failure, Utility.GetEnumDescription(NFCCommandStatus.Status.Failure), NFCCommandStatus.Status.Failure); }
    }

    public class NFCCommandStatus
    {
        public Status Result { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; }

        public enum Status
        {
            [Description("Unset")]
            Unset = -1,
            [Description("Operation successful")]
            Success = 0x00,
            [Description("Operation failure")]
            Failure = 0x01,
            [Description("Header mismatch in response buffer")]
            HeaderMismatch = 0x02,
            [Description("The command encountered an error")]
            GenericError = 0x03
        }
    }
}