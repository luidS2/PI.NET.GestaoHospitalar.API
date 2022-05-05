namespace PI.GestaoHospitalar.Core.Kafka
{
    public abstract class KafkaConfig
    {
        public string Servidor { get; set; }
        public int TimeOut { get; set; }
        public string TopicoLogs { get; set; }

        public virtual string TopicoNome { get; set; }
        public virtual string GrupoNome { get;  set; }

        public virtual string TopicoErro { get; set; }
    }
}
