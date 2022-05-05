using Confluent.Kafka;
using System;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Core.Kafka
{
    public class ConsumerWrapperNew : IDisposable
    {
        private readonly KafkaConfig _kafkaConfig;
        private readonly ConsumerConfig _config;
        private readonly ClientConfig _clientConfig;
        private readonly IConsumer<Ignore, string> _consumer;

        public ConsumerWrapperNew(KafkaConfig kafkaConfig)
        {
            _kafkaConfig = kafkaConfig;
            _clientConfig = new ClientConfig()
            {
                BootstrapServers = kafkaConfig.Servidor,
                MessageMaxBytes = 15728640,
            };
            _config = new ConsumerConfig(_clientConfig)
            {
                GroupId = _kafkaConfig.GrupoNome,                
                AutoOffsetReset = AutoOffsetReset.Earliest,
                AllowAutoCreateTopics = true,

                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
                MaxPollIntervalMs = 300000,
            };
            _consumer = new ConsumerBuilder<Ignore, string>(_config)
                .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
                .Build();
            _consumer.Subscribe(_kafkaConfig.TopicoNome);
        }
        public ConsumerWrapperNew(KafkaConfig kafkaConfig, string idAssinante) : 
            this(kafkaConfig)
        {
            _config.GroupId = $"{_kafkaConfig.GrupoNome}_{idAssinante}";
        }

        public async Task<string> ReadMessage()
        {
            try
            {
                var message = await Task.Run(() =>  _consumer.Consume(
                            TimeSpan.FromMilliseconds(_config.MaxPollIntervalMs - 1000 ?? 250000)));
                if (message != null)
                {
                    await Task.Run(() => _consumer.Commit(message));
                    await Task.Run(() => _consumer.StoreOffset(message));
                    return message.Message.Value;

                }
                else
                {
                    return "";
                }
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Consume error: {ex.ToString()} | Tópico:{_kafkaConfig.TopicoNome}|GroupId:{_kafkaConfig.GrupoNome}");
                throw new Exception($"Falha na leitura:Tópico:{_kafkaConfig.TopicoNome}|GroupId:{_kafkaConfig.GrupoNome}", ex);
            }            
        }

        public void Dispose()
        {
            _consumer.Close();
        }
    }
}