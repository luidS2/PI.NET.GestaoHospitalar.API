using PI.GestaoHospitalar.Domain.PacienteContext.Dtos;
using MediatR;
using System.Collections.Generic;

namespace PI.GestaoHospitalar.Domain.PacienteContext.Commands
{
    public class SalvarPacienteCommand : IRequest<object>
    {
        public SalvarPacienteCommand(PacienteDTO dto)
        {
            PacienteDto = dto;
        }
        public PacienteDTO PacienteDto { get; }
    }
}
