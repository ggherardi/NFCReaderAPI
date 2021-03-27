using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.Cards
{
    public class Ntag215 : NFCCard
    {
        #region Properties
        public override int MaxWritableBlocks { get => 4; protected set => MaxWritableBlocks = value; }
        public override int MaxReadableBytes { get => 16; protected set => MaxReadableBytes = value; }
        public override int UserConfigurationStartingPage { get => 0x83; protected set => UserConfigurationStartingPage = value; }
        public override int LastUserDataMemoryPage { get => 0x81; protected set => LastUserDataMemoryPage = value; }
        public override int FirstUserDataMemoryPage { get => 0x4; protected set => FirstUserDataMemoryPage = value; }        
        #endregion

        #region Commands
        /// <summary>
        /// Input | Cmd: 0x1B (1 byte) - Pwd: {password} (4 bytes)
        /// Response | {PACK} (2 bytes)
        /// References
        /// - Command NTAG213/215/216, chapter: 10.7. PWD_AUTH, pag. 46
        /// - Memory configuration for authentication and locks, chapter 8.5.7., Configuration Pages, pag. 18 
        /// - Programming authentication and locks, chapter 8.8.1., Programming of PWD and PACK, pag. 30
        /// Step should be as follow: 
        /// 1) Write PWD at memory page 133 (0x85) and PACK at memory page 134 (0x86) 
        /// 2) Set the AUTH0 byte at memory page 131 (0x83) to cover all memory 
        /// 3) Lock the PWD bytes by setting the static LOCK bytes at memory page 2 (0x02) 
        /// </summary>
        private readonly Lazy<Ntag215Command> _PWD_AUTH = new Lazy<Ntag215Command>(() =>
        {
            Ntag215Command command = new Ntag215Command()
            {
                CommandBytes = new byte[] { 0x1B, 0x00, 0x00, 0x00, 0x00 },
                Response = new NFCCommandResponse() { MinBufferLength = 2 }
            };
            return command;
        });
        private Ntag215Command PWD_AUTH { get => _PWD_AUTH.Value; }

        /// <summary>
        /// Input | Cmd: 0x60 (1 byte)
        /// Response | 0x00 (1 byte), {vendorID} (1 byte), {productType} (1 byte), {productSubtype} (1 byte), {majorProductVersion} (1 byte), {minorProductVersion} (1 byte), {storageSize} (1 byte), {procotolType} (1 byte)
        /// Reference: NTAG213/215/216, chapter: 10.1. GET_VERSION, pag. 34
        /// </summary>
        private readonly Lazy<Ntag215Command> _GET_VERSION = new Lazy<Ntag215Command>(() =>
        {
            Ntag215Command command = new Ntag215Command()
            {
                CommandBytes = new byte[] { 0x60 },
                Response = new NFCCommandResponse() { HeaderBytes = new byte[] { 0x00 }, MinBufferLength = 8 }
            };
            command.ExtractPayload = (responseBuffer) =>
            {
                byte[] payloadBytes = new byte[responseBuffer.Length];
                byte storageSizeByte = responseBuffer[6];
                string cardType = string.Empty;

                if (responseBuffer[0] != command.Response.HeaderBytes[0])
                {
                    command.Response.SetCommandStatus(NFCCommandStatus.Status.HeaderMismatch);
                }
                Array.Copy(responseBuffer, 1, payloadBytes, 0, responseBuffer.Length - 1);

                switch (storageSizeByte)
                {
                    case 0x0F:
                        cardType = "NTAG213";
                        break;
                    case 0x11:
                        cardType = "NTAG215";
                        break;
                    case 0x13:
                        cardType = "NTAG216";
                        break;
                    default: break;
                }

                return new NFCPayload(payloadBytes, cardType);
            };
            return command;
        });
        private Ntag215Command GET_VERSION { get => _GET_VERSION.Value; }

        /// <summary>
        /// Input | Cmd: 0xA2 (1 byte), {pageAddress} (1 byte), {data} (4 bytes)
        /// Reference: NTAG213/215/216, chapter: 10.4. WRITE, pag. 41
        /// </summary>
        private readonly Lazy<Ntag215Command> _WRITE = new Lazy<Ntag215Command>(() =>
        {
            Ntag215Command command = new Ntag215Command()
            {
                CommandBytes = new byte[] { 0x60, 0x00, 0x00, 0x00, 0x00, 0x00 }
            };
            return command;
        });
        public Ntag215Command WRITE { get => _WRITE.Value; }        
        #endregion

        #region Constructors
        public Ntag215(IntPtr hCard) : base(hCard) { }

        public Ntag215() : base() { }
        #endregion

        #region Abstraction implementation
        protected override NFCCommand Get_PWD_AUTH_Command(string password)
        {
            Ntag215Command command = new Ntag215Command(PWD_AUTH);
            byte[] passwordBytes = Encoding.Default.GetBytes(password);
            for(int i = 1; i <= 4; i++)
            {
                command.CommandBytes.SetValue(passwordBytes[i - 1], i);
            }            
            return command;
        }

        protected override NFCCommand Get_GET_VERSION_Command()
        {
            Ntag215Command get_version = GET_VERSION;
            return get_version;
        }
        #endregion

        #region Security commands
        public static byte[] GetDefaultSecuritySetupBytes(string password, string pack, int firstPageAddressToProtect, Ntag215AuthConfig.AccessByte.CFGLCK userConfigLock, Ntag215AuthConfig.AccessByte.PROT protectionMode, int authenticationRetryLimit)
        {
            Ntag215AuthConfig.MirrorByte mirrorByte = new Ntag215AuthConfig.MirrorByte(Ntag215AuthConfig.MirrorByte.MIRROR_CONF.NoMirror, Ntag215AuthConfig.MirrorByte.MIRROR_BYTE.First, Ntag215AuthConfig.MirrorByte.STRG_MOD_EN.Enabled);
            Ntag215AuthConfig.AccessByte accessByte = new Ntag215AuthConfig.AccessByte(protectionMode, userConfigLock, Ntag215AuthConfig.AccessByte.NFC_CNT_EN.Disabled, Ntag215AuthConfig.AccessByte.NFC_CNT_PWD_PROT.Disabled, authenticationRetryLimit);
            Ntag215AuthConfig auth = new Ntag215AuthConfig(mirrorByte, 0, firstPageAddressToProtect, accessByte, password, pack);
            return auth.Bytes;
        }

        public static byte[] GetDefaultSecuritySetupBytes(string password, string pack)
        {
            return GetDefaultSecuritySetupBytes(password, pack, 4, Ntag215AuthConfig.AccessByte.CFGLCK.UserConfigOpen, Ntag215AuthConfig.AccessByte.PROT.ReadWriteProtected, 0);
        }
        #endregion
    }

    public class Ntag215Command : NFCCommand
    {
        public Ntag215Command(Ntag215Command commandToClone) : base(commandToClone) { }
        public Ntag215Command() : base() { }
    }

    public class Ntag215AuthConfig
    {
        public class MirrorByte
        {
            public byte Byte { get; private set; }

            public enum MIRROR_CONF
            {
                NoMirror = 0x00,
                UIDMirror = 0x40,
                NFCCounterMirror = 0x80,
                UIDAndNFCCounterMirror = 0xC0
            }

            public enum MIRROR_BYTE
            {
                First = 0x00,
                Second = 0x10,
                Third = 0x20,
                Fourth = 0x30
            }

            public enum STRG_MOD_EN
            {
                Disabled = 0x00,
                Enabled = 0x04
            }

            public MirrorByte(MIRROR_CONF asciiMirrorConfig, MIRROR_BYTE mirrorBytePosition, STRG_MOD_EN strongModulation)
            {
                Byte = (byte)(0x00 | (int)asciiMirrorConfig | (int)mirrorBytePosition | (int)strongModulation);
            }
        }

        public class AccessByte
        {
            public byte Byte { get; private set; }

            public enum PROT
            {
                WriteProtected = 0x00,
                ReadWriteProtected = 0x80
            }

            public enum CFGLCK
            {
                UserConfigOpen = 0x00,
                PermanentlyLockUserConfig = 0x40
            }

            public enum NFC_CNT_EN
            {
                Disabled = 0x00,
                Enabled = 0x10
            }

            public enum NFC_CNT_PWD_PROT
            {
                Disabled = 0x00,
                Enabled = 0x08
            }

            public AccessByte(PROT protectionMode, CFGLCK userConfigLock, NFC_CNT_EN nfcCounter, NFC_CNT_PWD_PROT nfcCounterPasswordProtection, int maxPasswordAttempts)
            {
                Byte = (byte)(0x00 | (int)protectionMode | (int)userConfigLock | (int)nfcCounter | (int)nfcCounterPasswordProtection | maxPasswordAttempts.Clamp(0, 7));
            }
        }

        public byte[] Bytes { get; private set; }

        public Ntag215AuthConfig(MirrorByte mirrorByte, int mirrorPage, int firstProtectedPageNumber, AccessByte accessByte, string password, string pack)
        {
            Bytes = new byte[] { mirrorByte.Byte, 0x00, (byte)mirrorPage, (byte)firstProtectedPageNumber, accessByte.Byte, 0x00, 0x00, 0x00 };
            Bytes = Bytes.Concat(Encoding.UTF8.GetBytes(password)).Concat(Encoding.ASCII.GetBytes(pack)).Concat(new byte[] { 0x00, 0x00 }).ToArray();
        }
    }
}
