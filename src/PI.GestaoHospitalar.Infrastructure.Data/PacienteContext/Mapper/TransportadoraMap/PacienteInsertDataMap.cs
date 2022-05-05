using Dapper.FluentMap.Dommel.Mapping;
using PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.InfraObjects.Entities.PacienteEntity;

namespace PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.Mapper.PacienteMap
{
    public class PacienteInsertDataMap : PacienteDataMap<PacienteInsertData>
    {
        public PacienteInsertDataMap()
        {
            Map(t => t.Id).ToColumn("Id");
        }
    }
}
