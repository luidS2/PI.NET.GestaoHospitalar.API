using PI.GestaoHospitalar.Core.Services;
using PI.GestaoHospitalar.Domain.PacienteContext.Commands;
using PI.GestaoHospitalar.Domain.PacienteContext.Config.Kafka;
using PI.GestaoHospitalar.Domain.PacienteContext.Dtos;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;

namespace PI.GestaoHospitalar.API.Services
{
    public class Processing : BackGroundServiceProcessing
    {
        private readonly PacienteKafkaConfig _PacienteKafkaConfig;
        private IConfiguration Configuration { get; }
        public Processing(
            IConfiguration configuration,
            PacienteKafkaConfig PacienteKafkaConfig,
            IMediator mediator
            ) : base(configuration, typeof(Processing).Assembly.GetName().Name, mediator, Environment.MachineName)
        {
            _PacienteKafkaConfig = PacienteKafkaConfig;
            Configuration = configuration;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = new List<Task>();

            tasks.AddRange(new List<Task>()
            {
                Task.Run(() => {
                    startPacienteEventObservation();
                })
            });

        }
        private void startPacienteEventObservation()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(Configuration.GetSection("UrlRabbit").Value)
                };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare("queue_hospital",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) =>
                {
                    var body = e.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(message);

                    PacienteDTO dto = JsonConvert.DeserializeObject<PacienteDTO>(message);
                    _mediator.Send(new SalvarPacienteCommand(dto));
                };

                channel.BasicConsume("queue_hospital", true, consumer);
                Console.WriteLine("Consumer started");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Consumer error on start conection");
            }
        }
    }
}