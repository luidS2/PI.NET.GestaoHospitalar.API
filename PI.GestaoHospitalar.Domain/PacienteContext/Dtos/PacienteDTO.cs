using PI.GestaoHospitalar.Core.Domain;
using System;
using System.Text.Json.Serialization;

namespace PI.GestaoHospitalar.Domain.PacienteContext.Dtos
{
    public class PacienteDTO : CdcDto
    {
        public int NumeroPaciente { get; set; }
        public int PressaoSistolica { get; set; }
        public int PressaoDiastolica { get; set; }
        public string Nome { get; set; }
    }
}
