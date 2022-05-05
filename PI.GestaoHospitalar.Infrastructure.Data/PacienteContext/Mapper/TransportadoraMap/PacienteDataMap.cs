using PI.GestaoHospitalar.Core.Dommel;
using PI.GestaoHospitalar.Domain.PacienteContext.Entities;
using PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.InfraObjects.Entities.PacienteEntity;

namespace PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.Mapper.PacienteMap
{
    public class PacienteDataMap<TEntity> : Map<TEntity, Paciente>
        where TEntity : PacienteData
    {
        public PacienteDataMap()
        {
            Map(t => t.Nome);
            Map(t => t.PressaoDiastolica);
            Map(t => t.PressaoSistolica);
            Map(t => t.NumeroPaciente);
        }
    }
}