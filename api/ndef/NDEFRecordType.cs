using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC.NDEF
{
    /// <summary>
    /// Abstract class used as template for all the Record Types
    /// Reference: NFC Record Type Definition (RTD) - Technical Specifications
    /// </summary>
    public abstract class NDEFRecordType
    {
        public abstract int TypeIdentifier { get; }
        public abstract int TypeLength { get; }
        public abstract int HeaderLength { get; }

        public enum Types
        {
            Text,
            URI
        }

        public abstract byte[] GetBytes();

        public abstract void AddTextToPayload(byte[] bytes);

        public abstract void BuildRecordFromBytes(byte[] bytes);

        public abstract NDEFPayload GetPayload();

        public abstract override string ToString();

        public static NDEFRecordType GetNDEFRecordTypeWithTypeIdentifier(int typeIdentifier)
        {
            NDEFRecordType type = null;
            IEnumerable<Type> recordTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(NDEFRecordType)));
            PropertyInfo typeIdentifierProperty = typeof(NDEFRecordType).GetProperties().Where(p => p.Name == "TypeIdentifier").FirstOrDefault();
            foreach (Type recordType in recordTypes)
            {
                type = Activator.CreateInstance(recordType) as NDEFRecordType;
                if (type != null && (int)typeIdentifierProperty.GetValue(type) == typeIdentifier)
                {                    
                    return type;
                }
            }
            return type;
        }
    }    
}
