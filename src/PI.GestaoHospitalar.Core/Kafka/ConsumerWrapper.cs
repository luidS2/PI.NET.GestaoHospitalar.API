using PI.GestaoHospitalar.Core.ExceptionCore;
using Confluent.Kafka;
using System;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Core.Kafka
{
    public class ConsumerWrapper:IDisposable
    {
        private readonly KafkaConfig _kafkaConfig;
        private readonly ConsumerConfig _config;
        private readonly ClientConfig _clientConfig;
        private readonly IConsumer<Ignore, string> _consumer;

        public ConsumerWrapper(KafkaConfig kafkaConfig)
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
            };
            _consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
            _consumer.Subscribe(_kafkaConfig.TopicoNome);
        }
        public ConsumerWrapper(KafkaConfig kafkaConfig, string idAssinante) : 
            this(kafkaConfig)
        {
            _config.GroupId = $"{_kafkaConfig.GrupoNome}_{idAssinante}";
        }

        public async Task<string> ReadMessage()
        {
            try
            {
                var message = await Task.Run(() => _consumer.Consume(TimeSpan.FromSeconds(_kafkaConfig.TimeOut / 1000)));
                if (message != null)
                {
                    return message.Message.Value;
                }
                else
                {
                    return "";
                }

            }
            catch(ConsumeException ce)
            {
                Console.WriteLine(ce.ToDetailedString()) ;
                return "";
            }
            catch (Exception ex)
            {
                 throw ex;
            }            
        }

        public void Dispose()
        {
            _consumer.Close();
        }
    }
}