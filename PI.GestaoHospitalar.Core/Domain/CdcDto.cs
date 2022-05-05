using System;
using System.Text.Json.Serialization;

namespace PI.GestaoHospitalar.Core.Domain
{
    public class CdcDto
    {
        [JsonPropertyName("op")]
        public string Operacao { get; set; }
        [JsonPropertyName("updatedate")]
        public DateTime DataEntregaPacote { get; set; }
    }
}
