using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace PI.GestaoHospitalar.Domain.PacienteContext.Queries
{
    public class GetPacientesQuery : IRequest<object>
    {
        public GetPacientesQuery()
        {
        }
    }
}
