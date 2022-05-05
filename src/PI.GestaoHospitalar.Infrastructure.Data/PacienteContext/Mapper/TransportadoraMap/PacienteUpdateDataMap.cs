using PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.InfraObjects.Entities.PacienteEntity;

namespace PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.Mapper.PacienteMap
{
    public class PacienteUpdateDataMap : PacienteDataMap<PacienteUpdateData>
    {
        public PacienteUpdateDataMap() : base()
        {
            Map(t => t.Id).ToColumn("Id").IsKey();
        }
    }
}
