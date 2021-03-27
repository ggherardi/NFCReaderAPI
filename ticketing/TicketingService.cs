using CSharp.NFC;
using CSharp.NFC.NDEF;
using CSharp.NFC.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFCTicketing.Encryption;

namespace NFCTicketing
{
    public class TicketingService
    {
        private readonly string _password;
        private readonly byte[] _cardID;
        private readonly NFCReader _nfcReader;
        private readonly IValidatorLocation _location;
        private readonly IValidationStorage _storage;
        private ValidationManager _validationManager;
        private EncryptableSmartTicket _ticket;

        public EncryptableSmartTicket ConnectedTicket { get => _ticket; private set => _ticket = value; }

        public TicketingService(NFCReader ticketValidator, byte[] cardID, IValidatorLocation location, IValidationStorage storage, string password) 
        {
            _cardID = cardID;
            _nfcReader = ticketValidator;
            _location = location;
            _password = password;
            _storage = storage ?? new LocalDBStorage();
        }

        public TicketingService(NFCReader ticketValidator, byte[] cardID, IValidatorLocation location) : this(ticketValidator, cardID, location, null, string.Empty) { }

        public TicketingService(NFCReader ticketValidator, byte[] cardID, IValidatorLocation location, IValidationStorage storage) : this(ticketValidator, cardID, location, storage, string.Empty) { }

        public void ConnectTicket()
        {
            _ticket = ReadTicket();
        }

        /// <summary>
        /// Adds the specified amount to the credit of the connected ticket
        /// </summary>
        /// <param name="creditAmount"></param>
        public void AddCredit(decimal creditAmount)
        {
            _ticket.Credit += creditAmount;
            WriteTicket();
            _storage.RegisterTransaction(new CreditTransaction() { CardId = _ticket.CardID, Location = _location.GetLocation(), Date = DateTime.Now, Amount = creditAmount });
        }

        public void InitNewTicket()
        {
            _ticket = new EncryptableSmartTicket() { Credit = 0, TicketTypeName = SmartTicketType.BIT.Name, CurrentValidation = null, SessionValidation = null, SessionExpense = 0, UsageTimestamp = DateTime.Now, CardID = _cardID };
            WriteTicket();
        }

        /// <summary>
        /// Start Main BL, check flowchart.png
        /// </summary>
        public void ValidateTicket()
        {
            _validationManager = new ValidationManager(_ticket, _storage, _location.GetLocation());
            _ticket = _validationManager.ValidateTicket();            
        }

        public EncryptableSmartTicket ReadTicket()
        {
            NDEFPayload payload = _nfcReader.GetNDEFPayload();
            EncryptableSmartTicket ticket = TicketEncryption.DecryptTicket<EncryptableSmartTicket>(payload.Bytes, TicketEncryption.GetPaddedIV(_cardID));
            return ticket;
        }

        private List<NFCOperation> WriteTicket()
        {
            // Sample ticket
            //SmartTicket ticket = new SmartTicket() { Credit = 10.5, Type = SmartTicketType.BIT, CurrentValidation = DateTime.Now, SessionValidation = DateTime.Now.AddHours(-10), SessionExpense = 3.0, CardID = new byte[] { 0x04, 0x15, 0x91, 0x8A, 0xCB, 0x42, 0x20 } };
            byte[] encryptedTicketBytes = TicketEncryption.EncryptTicket(_ticket, TicketEncryption.GetPaddedIV(_cardID));
            List<NFCOperation> operations = _nfcReader.WriteTextNDEFMessage(encryptedTicketBytes, _password);
            return operations;
        }
    }
}
