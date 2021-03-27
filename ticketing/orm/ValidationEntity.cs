using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCTicketing
{
    public class ValidationEntity
    {
        public byte[] CardId { get; set; }
        public string Location { get; set; }
        public DateTime Time { get; set; }        
        public string EncryptedTicketHash { get; set; }
    }
}
