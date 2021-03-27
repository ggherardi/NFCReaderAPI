using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.NDEF
{
    public class NDEFOperation
    {
        public List<NFCOperation> Operations { get; set; }
        public NDEFMessage NDEFMessage { get; set; }

        public NDEFOperation()
        {
            Operations = new List<NFCOperation>();
            NDEFMessage = new NDEFMessage();
        }
    }
}
