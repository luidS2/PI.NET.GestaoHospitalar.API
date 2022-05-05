using AutoMapper;
using PI.GestaoHospitalar.Domain.PacienteContext.Dtos;
using PI.GestaoHospitalar.Domain.PacienteContext.Entities;

namespace PI.GestaoHospitalar.Domain.PacienteContext.Mapping
{
    public class PacienteProfile : Profile
    {
        public PacienteProfile()
        {
            CreateMap<Paciente, PacienteDTO>().ReverseMap();
        }
    }
}
