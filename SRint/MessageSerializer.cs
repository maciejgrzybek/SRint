using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRint
{
    namespace Serialization
    {
        class MessageSerializer
        {
            public static byte[] Serialize(protobuf.Message message)
            {
                byte[] b = null;
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize<protobuf.Message>(ms, message);
                    b = new byte[ms.Position];
                    var fullB = ms.GetBuffer();
                    Array.Copy(fullB, b, b.Length);
                }
                return b;
            }

            public static protobuf.Message Deserialize(byte[] serializationBytes)
            {
                protobuf.Message m = null;
                using (var ms = new MemoryStream(serializationBytes))
                {
                    m = Serializer.Deserialize<protobuf.Message>(ms);
                }
                return m;
            }
        }
    }
}
