using System;
using System.IO;
using Nelibur.Sword.DataStructures;
using Nelibur.Sword.Extensions;
using ProtoBuf;

namespace Core
{
    public sealed class DataSerializer
    {
        public Option<TData> FromProto<TData>(byte[] value)
        {
            try
            {
                using (var stream = new MemoryStream(value))
                {
                    var item = Serializer.Deserialize<TData>(stream);
                    return item.ToOption();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Option<TData>.Empty;
            }
        }

        public Option<byte[]> ToProto(object value)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, value);
                    return stream.ToArray().ToOption();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Option<byte[]>.Empty;
            }
        }
    }
}
