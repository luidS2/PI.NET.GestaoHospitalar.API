using PI.GestaoHospitalar.Core.Connection;
using System.Data;
using System.Data.SqlClient;

namespace PI.GestaoHospitalar.Core.DbConnection
{
    public class DbConnectionCoreSqlServer : DbConnectionCore
    {
        protected override IDbConnection GetConnection() => new SqlConnection(ConnectionString);
    }
}
