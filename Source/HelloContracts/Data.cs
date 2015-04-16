using System;
using ProtoBuf;

namespace HelloContracts
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class Data
    {
        public Guid Id { get; set; }
    }
}
