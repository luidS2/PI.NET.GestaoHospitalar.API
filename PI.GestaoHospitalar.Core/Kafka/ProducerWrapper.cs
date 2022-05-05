using Confluent.Kafka;
using System;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Core.Kafka
{
    public class ProducerWrapper
    {
        private readonly KafkaConfig _kafkaConfig;
        private readonly ProducerConfig _config;
        private readonly ClientConfig _clientConfig;

        public ProducerWrapper(KafkaConfig kafkaConfig)
        {
            _clientConfig = new ClientConfig()
            {
                BootstrapServers = kafkaConfig.Servidor,
                MessageMaxBytes = 1000000000,
            };
            _config = new ProducerConfig(_clientConfig)
            {                
                MessageTimeoutMs = kafkaConfig.TimeOut,                
                CompressionType = CompressionType.Gzip
            };
            _kafkaConfig = kafkaConfig;

        }
        /// <summary>
        /// Escreve a mensagem no Topico do Kafka
        /// </summary>
        /// <param name="message">Messagem a escrever</param>
        /// <param name="TopicoErro">true para gravar no topico de Erro, false para topico normal</param>
        public void WriteMessage(string message, bool topicoErro = false)
        {
            string topico;
            if (topicoErro)
            {
                topico = _kafkaConfig.TopicoErro;
            }
            else
            {
                topico = _kafkaConfig.TopicoNome;
            }
            using (var producer = new ProducerBuilder<Null, string>(_config).Build())
            {
                try
                {
                    var sendResult = producer
                                        .ProduceAsync(topico, new Message<Null, string> { Value = message })
                                        .GetAwaiter()
                                        .GetResult();

                    //return $"Mensagem '{sendResult.Value}' de '{sendResult.TopicPartitionOffset}'";
                }
                catch (ProduceException<Null, string> ex)
                {
                    throw new Exception($"Falha na entrega: {ex.Error.Reason}", ex);
                }
            }
        }
        /// <summary>
        /// Escreve a mensagem no Topico do Kafka assíncrono
        /// </summary>
        /// <param name="message">Messagem a escrever</param>
        /// <param name="TopicoErro">true para gravar no topico de Erro, false para topico normal</param>
        public async Task WriteMessageAsync(string message, bool topicoErro = false)
        {
            await Task.Run(() => WriteMessage(message, topicoErro));
        }

        /// <summary>
        /// Escreve a mensagem no Topico do Kafka
        /// </summary>
        /// <param name="topic">Nome do tópico a escrever</param>
        /// <param name="message">Messagem a escrever</param>
        /// <returns></returns>
        public async Task WriteMessageAsync(string topic, string message)
        {
            using var producer = new ProducerBuilder<Null, string>(_config).Build();
            try
            {
                _ = await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });

            }
            catch (ProduceException<Null, string> ex)
            {
                throw new Exception($"Falha na entrega: {ex.Error.Reason}", ex);
            }
        }
    }
}
