using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.NDEF
{

    /// <summary>
    /// Terminator Tag Length Value Block
    /// Reference:  NFCForum-Type-2-Tag_1.1 specifications, chapter 2.3.6 Terminator TLV, pag. 13
    /// </summary>
    public class Terminator : TLVBlock
    {
        /// <summary>
        /// Tag Field Value byte for "Terminator TLV"        
        /// </summary>
        public override byte TagByte { get => 0xFE; }
    }
}
