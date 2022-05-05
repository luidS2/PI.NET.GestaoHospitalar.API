using PI.GestaoHospitalar.Core.ExceptionCore;
using PI.GestaoHospitalar.Core.Handlers;
using PI.GestaoHospitalar.Core.Results;
using AutoMapper;
using PI.GestaoHospitalar.Domain.PacienteContext.Commands;
using PI.GestaoHospitalar.Domain.PacienteContext.Entities;
using PI.GestaoHospitalar.Domain.PacienteContext.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PI.GestaoHospitalar.Domain.PacienteContext.Queries;
using PI.GestaoHospitalar.Domain.PacienteContext.Dtos;
using Microsoft.Extensions.Logging;

namespace PI.GestaoHospitalar.Domain.PacienteContext.Handlers
{
    public class PacienteHandler : BaseHandler,
        IRequestHandler<SalvarPacienteCommand, object>,
        IRequestHandler<GetPacientesQuery, object>
    {
        private readonly IPacienteRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<PacienteHandler> _logger;

        public PacienteHandler(IPacienteRepository repository, IMapper mapper, ILogger<PacienteHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<object> Handle(SalvarPacienteCommand command, CancellationToken cancellationToken)
        {
            try
            {
                IResult result = null;
                _logger.LogInformation("SalvarPacienteCommand - iniciado");

                var entities = _mapper.Map<Paciente>(command.PacienteDto);
                AdicionarNotificacoes(entities);

                result = await _repository.Salvar(entities);
                if (result.StatusCode == 201)
                    _ = _repository.Integrar(entities);


                _logger.LogInformation("SalvarPacienteCommand - finalziado");
                return result;
            }
            catch (BaseException ex)
            {
                return new Result(ex.StatusCode, "", ex.Message);
            }
            catch (Exception ex)
            {
                return new Result(500, "Erro interno do Servidor", string.Format("{0}\n{1}\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
            }
        }

        public async Task<object> Handle(GetPacientesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _repository.Get();

                var retorno = _mapper.Map<List<PacienteDTO>>(result.Parameters as List<Paciente>).OrderByDescending(x => x.NumeroPaciente);

                return new Result(200, "Pedidos listados com suscesso", retorno);
            }
            catch (Exception ex)
            {
                return new Result(500, "Erro interno do Servidor", string.Format("{0}\n{1}\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
            }
        }
    }
}

