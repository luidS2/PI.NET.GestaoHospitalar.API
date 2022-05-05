using PI.GestaoHospitalar.Core.Results;
using PI.GestaoHospitalar.Domain.PacienteContext.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Domain.PacienteContext.Repositories
{
    public interface IPacienteRepository
    {
        Task<IResult> Salvar(Paciente entity);
        Task<IResult> Integrar(Paciente entity);
        Task<IResult> Get();
    }
}
