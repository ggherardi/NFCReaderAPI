using NFCTicketing.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCTicketing
{
    public class ValidationManager
    {
        private DateTime _timestamp;
        private EncryptableSmartTicket _ticket;
        private IValidationStorage _storage;
        private string _location;

        public ValidationManager(EncryptableSmartTicket ticket, IValidationStorage storage, string location)
        {
            _ticket = ticket;
            _storage = storage;
            _location = location;
        }

        /// <summary>
        /// Start Main BL, check flowchart.png
        /// </summary>
        public EncryptableSmartTicket ValidateTicket()
        {
            _timestamp = DateTime.Now;
            _ticket.UsageTimestamp = _timestamp;
            if (_ticket.SessionValidation == null)
            {
                ResetTicketValidation();
            }
            else
            {
                TimeSpan timeSinceFirstValidation = _timestamp - (DateTime)_ticket.SessionValidation;
                if (timeSinceFirstValidation.TotalMinutes < _ticket.Type.DurationInMinutes)
                {
                    ManageValidTicket();
                }
                else if (_ticket.Type.NextTicketUpgrade == null || timeSinceFirstValidation.TotalMinutes > _ticket.Type.NextTicketUpgrade.DurationInMinutes)
                {
                    // Ticket is expired for both the current ticket type and the upgraded ticket type
                    ResetTicketValidation();
                }
                else
                {
                    if ((_timestamp - (DateTime)_ticket.CurrentValidation).TotalMinutes < SmartTicketType.BIT.DurationInMinutes)
                    {
                        ManageValidTicket();
                    }
                    else
                    {
                        if (_ticket.Type.NextTicketUpgrade != null && _ticket.SessionExpense + SmartTicketType.BIT.Cost >= _ticket.Type.NextTicketUpgrade.Cost)
                        {
                            // Upgrade the ticket since it would be more cost efficient than buying a new base ticket
                            UpgradeTicket();
                        }
                        else
                        {
                            ValidateBaseTicket();
                        }
                    }
                }
            }
            RegisterTicketUpdate();
            return _ticket;
        }

        private void ResetTicketValidation()
        {
            _ticket.SessionExpense = 0;
            ChargeTicket(SmartTicketType.BIT.Cost);
            _ticket.Type = SmartTicketType.BIT;
            _ticket.CurrentValidation = _timestamp;
            _ticket.SessionValidation = _timestamp;
            RegisterValidation();
        }

        private void UpgradeTicket()
        {
            decimal upgradeCost = _ticket.Type.NextTicketUpgrade.Cost - (decimal)_ticket.SessionExpense;
            ChargeTicket(upgradeCost);
            _ticket.Type = (SmartTicketType)_ticket.Type.NextTicketUpgrade;
        }

        private void ManageValidTicket()
        {
            TimeSpan timeSinceLastValidation = _timestamp - (DateTime)_ticket.CurrentValidation;
            if (timeSinceLastValidation.TotalMinutes > SmartTicketType.BIT.DurationInMinutes)
            {
                _ticket.CurrentValidation = _timestamp;
                RegisterValidation();
            }
            else
            {
                // I need to add a different type of object to return, so I can add messages: i.e. I could add here a message that says that the ticket didn't need to be validated!
            }
        }

        private void ValidateBaseTicket()
        {
            ChargeTicket(SmartTicketType.BIT.Cost);
            _ticket.CurrentValidation = _timestamp;
            RegisterValidation();
        }

        private void ChargeTicket(decimal amount)
        {
            if (_ticket.Credit - amount < 0)
            {
                throw new Exception($"Credito non sufficiente per l'operazione (Richiesti: {amount} €. Credito disponibile: {Math.Round(_ticket.Credit, 2)} €");
            }
            _ticket.Credit -= amount;
            _ticket.SessionExpense += amount;
        }

        private void RegisterValidation()
        {
            string encryptedTicketHash = Encoding.Unicode.GetString(TicketEncryption.EncryptTicket(_ticket, TicketEncryption.GetPaddedIV(_ticket.CardID)));            
            _storage.RegisterValidation(new ValidationEntity() { CardId = _ticket.CardID, Location = _location, Time = _timestamp, EncryptedTicketHash = encryptedTicketHash });
        }

        private void RegisterTicketUpdate()
        {
            _storage.RegisterTicketUpdate(_ticket);
        }

        private void RegisterTransaction(decimal amount)
        {
            _storage.RegisterTransaction(new CreditTransaction() { CardId = _ticket.CardID, Location = _location, Date = DateTime.Now, Amount = amount });
        }
        // End Main BL
    }
}
