using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.Controllers
{
    public class PN532 : NFCController
    {
        /// <summary>
        /// Input | Class: 0xD4 (1 byte) - Instruction: 0x42 (1 byte) - Data out [] (n bytes)
        /// Response | 0xD5 (1 byte), 0x43 (1 byte), Status (1 byte) (errors: PN532 User Manual, chapter 7.1. Error handling, pag. 67), DataOut[] (array of raw data)
        /// In order to send a direct transmit to the card, one should a combination of commands, i.e.: _reader.DirectTransmitCommand(payload:{_controller.ModuleDataExchangeCommand(DataOut:{_card.PWD_AUTH(value:{password})})})
        /// Reference: PN532 User Manual, chapter 7.3.9. InCommunicateThru, pag. 136
        /// </summary>
        private readonly PN532Command _inCommunicateThru = new PN532Command()
        {
            CommandBytes = new byte[] { 0xD4, 0x42 },
            Response = new NFCCommandResponse() { HeaderBytes = new byte[] { 0xD5, 0x43, }, MinBufferLength = 3 }
        };

        /// <summary>
        /// Input | Class: 0xD4 (1 byte) - Instruction: 0x40 (1 byte) - Tg: {target} (1 byte) - Data out [] (n bytes)
        /// Response | 0xD5 (1 byte), 0x41 (1 byte), Status (1 byte) (errors: PN532 User Manual, chapter 7.1. Error handling, pag. 67), DataOut[] (array of raw data)
        /// More robust than InCommunicateThru, it can be used to WRITE, READ, FAST_READ and other MIFARE commands (see pag. 130)
        /// Reference: PN532 User Manual, chapter 7.3.8. InDataExchange, pag. 127
        /// </summary>
        private readonly PN532Command _inDataExchange = new PN532Command()
        {
            CommandBytes = new byte[] { 0xD4, 0x40, 0x00 },
            Response = new NFCCommandResponse() { HeaderBytes = new byte[] { 0xD5, 0x41, }, MinBufferLength = 3 }
        };

        /// <summary>
        /// Controller errors
        /// Reference: UM0801-03 (PN532 User Manual), chapter 8.1. Error Handling, pag. 50
        /// </summary>
        public enum Status
        {
            [Description("Operation successful")]
            Success = 0x00,
            [Description("Time Out, the target has not answered")]
            TimeOut = 0x01,
            [Description("A CRC Error has been detected by the CIU")]
            CRCError = 0x02,
            [Description("A Parity error has been detected by the CIU")]
            PairtyError = 0x03,
            [Description("During an anti-collision/select operation an erroneous Bit Count has been detected")]
            BitCountError = 0x04,
            [Description("Framing error during MIFARE operation")]
            MIFAREFramingError = 0x05,
            [Description("An abnormal bit-collision has been detected during bit wise anti-collision at 106 kbps")]
            BitCollision = 0x06,
            [Description("Communication buffer size insufficient")]
            CommunicationBufferSizeInsufficient = 0x07,
            [Description("RF Buffer overflow has been detected by the CIU")]
            RFBufferOverflow = 0x09,
            [Description("In active communication mode, the RF field has not been switched on in time by the counterpart")]
            LateRFFieldSwitching = 0x0A,
            [Description("RF Protocol Error")]
            RFProtocolError = 0x0B,
            [Description("Temperature error: the interal temperature sensor has detected overhating")]
            OverheatingError = 0x0D,
            [Description("Internal buffer overflow")]
            InternalBufferOverflow = 0x0E,
            [Description("Invalid parameter (range, format, ...)")]
            InvalidParameter = 0x10,
            [Description("DEP Protocol: the PN532 configured in target mode does not support the command received from the initiator")]
            CommandNotSupported = 0x12,
            [Description("DEP Protocol, MIFARE or ISO/IEC14443-4: the data format does not match to the specification")]
            DataFormatError = 0x13,
            [Description("MIFARE: authentication error")]
            MIFAREAuthError = 0x14,
            [Description("Target or Initiator does not support NFC Secure")]
            NFCSecureNotSupported = 0x18,
            [Description("I2C bus line is Busy. A TDA transaction is on going")]
            BusLineBusy = 0x19,
            [Description("ISO/IEC14443-4: UID Check byte is wrong")]
            UIDCheckByteError = 0x23,
            [Description("DEP Protocol: invalid device state, the system is in a state which does not allow operation")]
            InvalidDeviceState = 0x25,
            [Description("Operation not allowed in this configuration (host controller interface)")]
            OperationNotAllowed = 0x26,
            [Description("This command is not acceptable due to the current context of the PN553")]
            CommandNotAcceptable = 0x27,
            [Description("The PN553 configured as target has been released by the initiator")]
            DeviceReleasedError = 0x29,
            [Description("PN553 and ISO/IEC14443-3B only: the ID of the card does not match, meaning that the expected card has been exchanged with another one")]
            CardIDMismatch = 0x2A,
            [Description("PN553 and ISO/IEC14443-3B only: the card previously activated has disappeared")]
            CardDisappearedError = 0x2B,
            [Description("Mismatch between the NFCID3 initiator and the NFCID3 target in DEP 212/424 kbps passive")]
            InitiatorMismatch = 0x2C,
            [Description("An over-current event has been detected")]
            OverCurrentError = 0x2D,
            [Description("NAD missing in DEP frame")]
            NADMissingError = 0x2E
        }

        protected override NFCCommand Get_DataExchangeCommand()
        {
            return new PN532Command(_inDataExchange);
        }

        protected override NFCCommand Get_InCommunicateThruCommand()
        {
            return new PN532Command(_inCommunicateThru);
        }
    }

    public class PN532Command : NFCCommand
    {
        public PN532Command(PN532Command commandToClone) : base(commandToClone) 
        {
            ExtractPayload = _extractPayload;
        }

        public PN532Command() : base() { }

        private NFCPayload _extractPayload(byte[] responseBuffer)
        {
            byte[] payload = new byte[responseBuffer.Length];
            bool isHeaderCorrect = true;
            for(int i = 0; i < Response.HeaderBytes.Length; i++)
            {
                if(responseBuffer[i] != Response.HeaderBytes[i])
                {
                    isHeaderCorrect = false;
                }
            }
            if(isHeaderCorrect)
            {
                int responseStatusByte = responseBuffer[2];
                PN532.Status responseStatus = (PN532.Status)responseStatusByte;
                if(responseStatusByte == 0)
                {
                    Response.SetCommandSuccessful(responseStatusByte, Utility.GetEnumDescription(responseStatus));
                }
                else
                {
                    Response.SetCommandFailure(responseStatusByte, Utility.GetEnumDescription(responseStatus));
                }
                Array.Copy(responseBuffer, 3, payload, 0, responseBuffer.Length - 3);
            }
            else
            {
                Response.SetCommandFailure((int)NFCCommandStatus.Status.HeaderMismatch, Utility.GetEnumDescription(NFCCommandStatus.Status.HeaderMismatch));
            }
            return new NFCPayload(payload);
        }
    }
}
