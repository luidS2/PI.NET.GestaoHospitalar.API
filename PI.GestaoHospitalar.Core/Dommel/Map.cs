using Dapper.FluentMap.Dommel.Mapping;

namespace PI.GestaoHospitalar.Core.Dommel
{
    public class Map<TEntity, TBase> : DommelEntityMap<TEntity>
        where TBase : class
        where TEntity : class
    {
        public Map()
        {
            ToTable($"{typeof(TBase).Namespace.Replace(".", "_")}_{typeof(TBase).Name}");
        }
    }
}
