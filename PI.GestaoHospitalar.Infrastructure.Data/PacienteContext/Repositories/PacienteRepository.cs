using PI.GestaoHospitalar.Core.Data;
using PI.GestaoHospitalar.Core.DbConnection;
using PI.GestaoHospitalar.Core.Helpers;
using PI.GestaoHospitalar.Core.Kafka;
using PI.GestaoHospitalar.Core.Results;
using AutoMapper;
using Dommel;
using PI.GestaoHospitalar.Domain.PacienteContext.Config.Kafka;
using PI.GestaoHospitalar.Domain.PacienteContext.Entities;
using PI.GestaoHospitalar.Domain.PacienteContext.Repositories;
using PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.InfraObjects.Entities.PacienteEntity;
using PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PI.GestaoHospitalar.Domain.PacienteContext.HubConfig;

namespace PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.Repositories
{
    public class PacienteRepository : BaseRepository, IPacienteRepository
    {
        private readonly PacienteQuery _query;
        private readonly ProducerWrapper _producerWrapper;
        private readonly IHubContext<PacienteHub> _pacienteHub;

        public PacienteRepository(IMapper mapper,
            DbConnectionCore dbConnectionCore, IHubContext<PacienteHub> pacienteHub) : base(dbConnectionCore, mapper)
        {
            _query = new PacienteQuery();
            _pacienteHub = pacienteHub;
        }

        public async Task<IResult> Get()
        {
            return await _dbConnectionCore.BeginConnection(async connection =>
            {
                 try
                 {
                     var data = await connection.GetAllAsync<PacienteUpdateData>();

                    var entities = _mapper.Map<List<Paciente>>(data);

                    return new Result(200, "Entidades enviadas para integração", entities);
                 }
                 catch (Exception ex)
                 {
                     return new Result(500, $"Erro", ex.Message);
                 }
             });
        }

        public async Task<IResult> Integrar(Paciente entity)
        {
            await _pacienteHub.Clients.All.SendAsync("pacienteChanges", entity);

            return new Result(200, $"Paciente atualziado com sucesso.", new { Ok = true });
        }

        public async Task<IResult> Salvar(Paciente entity)
        {
            return await CreateOrUpdate<Paciente, PacienteInsertData, PacienteUpdateData>(entity, s => s.NumeroPaciente == entity.NumeroPaciente);
        }
    }
}
