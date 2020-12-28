using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using ProtoBuf;

namespace Test
{
    class PBHelper
    {
        public static byte[] Serialize<T>(T t)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize(ms, t);
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] bytes) {
            using (MemoryStream ms = new MemoryStream(bytes)) {
                return Serializer.Deserialize<T>(ms);
            }
        }

        public static byte[] Serialize_PB3<T>(T t) {
            return (t as IMessage).ToByteArray();
        }

        public static T Deserialize_PB3<T>(byte[] bytes) where T:IMessage<T>, new()
        {
            MessageParser<T> messageParser = new MessageParser<T>(() => new T());
            return messageParser.ParseFrom(bytes);
        }
    }
}
