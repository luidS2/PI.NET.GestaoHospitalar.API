using PI.GestaoHospitalar.Core.Controller;
using PI.GestaoHospitalar.Core.Results;
using PI.GestaoHospitalar.Domain.PacienteContext.Commands;
using PI.GestaoHospitalar.Domain.PacienteContext.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using PI.GestaoHospitalar.Core.Helpers;
using Newtonsoft.Json;
using PI.GestaoHospitalar.Domain.PacienteContext.Queries;

namespace PI.GestaoHospitalar.API.Controllers
{
    [ApiController]
    [Route("pacientes")]
    public class PacienteController : ControllerBaseCore
    {
        private readonly IMediator _mediator;
        public PacienteController(IMediator mediator)
        {
            _mediator = mediator;
        }       
        [HttpPost]
        [Produces("application/json")]
        public async Task<object> Post(PacienteDTO paciente)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqps://riyivmoy:aSgqoDTgEN3BO6fbRwSzpABj9q2c6PBK@moose.rmq.cloudamqp.com/riyivmoy")
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare("queue_hospital",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            var count = 0;


            var message = new { Name = "Producer", Message = paciente };
            var body = Encoding.UTF8.GetBytes(JsonHelper.Serialize(paciente));

            channel.BasicPublish("", "queue_hospital", null, body);

            return await ReturnProcessStatusCodeAsync(201, "mensagem postada", "mensagem postada");
        }
        [HttpGet]
        [Produces("application/json")]
        public async Task<object> Get()
        {
            var result = await _mediator.Send(new GetPacientesQuery()) as IResult;
            return await Task.Run(() => ReturnProcessStatusCodeGET(result.StatusCode, result.Message, result.Parameters));
        }
    }
}
