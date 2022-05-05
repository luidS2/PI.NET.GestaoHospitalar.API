using Dapper.FluentMap.Configuration;
using Dapper.FluentMap.Dommel;
using PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.Mapper.PacienteMap;

namespace PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.Mapper
{
    public static class Initialize
    {
        public static void Config(FluentMapConfiguration config)
        {
            #region Paciente
            config.AddMap(new PacienteInsertDataMap());
            config.AddMap(new PacienteUpdateDataMap());
            #endregion

            config.ForDommel();
        }
    }
}
