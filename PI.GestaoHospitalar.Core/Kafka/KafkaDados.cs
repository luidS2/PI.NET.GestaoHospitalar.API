using System;

namespace PI.GestaoHospitalar.Core.Kafka
{
    public struct KafkaDados
    {
        public string NameBackEnd { get; set; }
        public string AbsoluteUri { get; set; }
        public DateTime RequestTime { get; set; }
        public long Latency { get; set; }
        public string LocalMachineName { get; set; }
        public int StatusCode { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public string RequestPayload { get; set; }
        public string ResponsePayload { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public string Complement { get; set; }
        public string StackTrace { get; set; }
        public string TopicName { get; set; }
        public string GroupName { get; set; }
    }
}
