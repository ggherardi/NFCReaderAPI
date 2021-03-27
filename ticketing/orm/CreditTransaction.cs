using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCTicketing
{
    public class CreditTransaction
    {
        public byte[] CardId { get; set; }
        public string Location { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
