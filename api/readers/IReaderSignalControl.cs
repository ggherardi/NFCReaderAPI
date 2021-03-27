using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.Readers
{
    public interface IReaderSignalControl
    {
        byte[] GetErrorSignalCommand();
        byte[] GetSuccessSignalCommand();
        uint GetControlCode();
    }
}
