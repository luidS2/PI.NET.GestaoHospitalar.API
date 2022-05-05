using PI.GestaoHospitalar.Core.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace PI.GestaoHospitalar.Domain.PacienteContext.Config.Kafka
{
    public class PacienteKafkaConfig : KafkaConfig
    {
        public override string TopicoNome => "CDC.COSMOS.dbo.Paciente";
        public override string GrupoNome => "PI.GestaoHospitalar.API.Paciente.Group";
        public override string TopicoErro => "PI.GestaoHospitalar.API.Paciente.Erros";
    }
}
