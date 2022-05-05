using PI.GestaoHospitalar.Core.Domain;
using PI.GestaoHospitalar.Core.Results;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Core.Data
{
    public interface IBaseRepository
    {
        Task<IResult> BulkCopy(DataTable table, string destinationTableName, List<SqlBulkCopyColumnMapping> columnMappings, string tempTable = null, IDbTransaction transaction = null);
        Task<IResult> BulkInsert<T>(List<T> lista, string tableName, string tableTempName, IDbTransaction transaction = null);
        Task<IResult> CreateOrUpdate<TEntity, TInsert, TUpdate>(TEntity entity, Expression<Func<TInsert, bool>> predicate, IDbTransaction transaction = null)
            where TEntity : Entity
            where TInsert : Entity
            where TUpdate : Entity;
        Task<IResult> Execute(string sql, object param = null, int commandTimeout = 30, IDbTransaction transaction = null);
        DataTable ExecuteDataTable(string sql, IDbTransaction transaction = null);
        Task<IResult> QueryAsync<T>(string sql, object param = null, bool first = false, int commandTimeout = 30, IDbTransaction transaction = null);
    }
}
