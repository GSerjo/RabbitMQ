using System;
using System.Runtime.Serialization;

namespace HelloContracts
{
    [DataContract]
    public sealed class DataRequest
    {
        [DataMember]
        public Guid Id { get; set; }
    }
}
