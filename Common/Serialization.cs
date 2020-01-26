using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System;
using MessagePack;

namespace WorkNet.Common
{
    public static class Serilization
    {
        public static byte[] SerializeToByteArray(this object obj)
        {
            if (obj == null)
            {
                return null;
            }
            var bin = MessagePackSerializer.Serialize(obj);
            Console.WriteLine(MessagePackSerializer.ConvertToJson(bin));
            return bin;
        }

        public static T Deserialize<T>(this byte[] byteArray) where T : class
        {
            if (byteArray == null)
            {
                return null;
            }
            return MessagePackSerializer.Deserialize<T>(byteArray);
        }
    }
}