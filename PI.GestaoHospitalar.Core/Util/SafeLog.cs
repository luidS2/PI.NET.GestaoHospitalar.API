using PI.GestaoHospitalar.Core.ExceptionCore;
using PI.GestaoHospitalar.Core.Helpers;
using PI.GestaoHospitalar.Core.Kafka;
using System;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Core.Util
{
    public static class SafeLog
    {

        public async static Task WriteLogMessage(KafkaConfig kafkaConfig, KafkaDados dados)
        {
            try
            {
                await WriteMessage(kafkaConfig, await JsonHelper.SerializeAsync(dados), kafkaConfig.TopicoLogs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToDetailedString());
            }
        }

        public async static Task WriteMessage(KafkaConfig kafkaConfig, string message, string topicName)
        {
            try
            {
                var producer = new ProducerWrapper(kafkaConfig);

                await producer.WriteMessageAsync(topicName, message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async static Task WriteLogMessage(KafkaConfig kafkaConfig, string nameBackEnd, Exception ex)
        {
            var dados = new KafkaDados
            {
                NameBackEnd = nameBackEnd,
                StatusCode = 555,
                StackTrace = ex.ToDetailedString()
            };
            await WriteLogMessage(kafkaConfig, dados);
        }
        public async static Task WriteLogMessage(KafkaConfig kafkaConfig, string nameBackEnd, string localMachineName, Exception ex)
        {
            var dados = new KafkaDados
            {
                NameBackEnd = nameBackEnd,
                StatusCode = 555,
                LocalMachineName = localMachineName,
                StackTrace = ex.ToDetailedString()
            };
            await WriteLogMessage(kafkaConfig, dados);
        }
    }
}
