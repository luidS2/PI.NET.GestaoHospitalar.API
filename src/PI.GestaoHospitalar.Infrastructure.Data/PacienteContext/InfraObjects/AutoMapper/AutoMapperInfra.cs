using AutoMapper;
using PI.GestaoHospitalar.Domain.PacienteContext.Entities;
using PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.InfraObjects.Entities.PacienteEntity;

namespace PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.InfraObjects.AutoMapper
{
    public class AutoMapperInfra : Profile
    {
        public AutoMapperInfra()
        {
            #region Paciente
            CreateMap<Paciente, PacienteInsertData>();
            CreateMap<Paciente, PacienteUpdateData>();
            #endregion
        }
    }
}
