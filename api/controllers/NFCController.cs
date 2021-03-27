using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.Controllers
{
    public abstract class NFCController
    {
        protected abstract NFCCommand Get_DataExchangeCommand();        
        protected abstract NFCCommand Get_InCommunicateThruCommand();        

        public NFCCommand GetDataExchangeCommand()
        {
            return Get_DataExchangeCommand();
        }              
        
        public NFCCommand GetInCommunicateThruCommand()
        {
            return Get_InCommunicateThruCommand();
        }
    }
}
