using PI.GestaoHospitalar.Core.DbConnection;
using PI.GestaoHospitalar.Core.Domain;
using PI.GestaoHospitalar.Core.ExceptionCore;
using PI.GestaoHospitalar.Core.Helpers;
using PI.GestaoHospitalar.Core.Results;
using PI.GestaoHospitalar.Core.Util;
using AutoMapper;
using Dapper;
using Dommel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Core.Data
{
    public class BaseRepository : IBaseRepository
    {
        protected readonly DbConnectionCore _dbConnectionCore;
        protected readonly IMapper _mapper;
        public BaseRepository(DbConnectionCore dbConnectionCore)
        {
            _dbConnectionCore = dbConnectionCore;
        }
        public BaseRepository(DbConnectionCore dbConnectionCore, IMapper mapper) : this(dbConnectionCore)
        {
            _mapper = mapper;
        }

        public async Task<IResult> Execute(string sql, object param = null, int commandTimeout = 30, IDbTransaction transaction = null)
        {
            try
            {
                if (Equals(transaction, null))
                {
                    return await _dbConnectionCore.BeginConnection(async connection =>
                    {
                        return await _dbConnectionCore.BeginTransaction(connection, async transaction =>
                        {
                            return await ExecuteTransactionScope(sql, param, commandTimeout, transaction);
                        });
                    });
                }
                else
                {
                    return await ExecuteTransactionScope(sql, param, commandTimeout, transaction);
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(ex.Message, 500);
            }
        }

        private async Task<IResult> ExecuteTransactionScope(string sql, object param, int commandTimeout, IDbTransaction transaction)
        {
            try
            {
                await transaction.Connection.ExecuteAsync(sql, param, transaction, commandTimeout);
                return new Result(200, $"Processo concluído.", new { Ok = true });
            }
            catch (Exception ex)
            {
                return new Result(500, $"Erro", ex.Message);
            }
        }

        public async Task<IResult> QueryAsync<T>(string sql, object param = null, bool first = false, int commandTimeout = 30, IDbTransaction transaction = null)
        {
            try
            {
                if (Equals(transaction, null))
                {
                    return await _dbConnectionCore.BeginConnection(async connection =>
                    {
                        return await _dbConnectionCore.BeginTransaction(connection, async transaction =>
                        {
                            return await QueryAsyncTransactionScope<T>(sql, param, first, commandTimeout, transaction);

                        }, IsolationLevel.ReadUncommitted);
                    });
                }
                else
                {
                    return await QueryAsyncTransactionScope<T>(sql, param, first, commandTimeout, transaction);
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(ex.Message, 500);
            }
        }

        private async Task<IResult> QueryAsyncTransactionScope<T>(string sql, object param, bool first, int commandTimeout, IDbTransaction transaction)
        {
            try
            {
                var data = await transaction.Connection.QueryAsync<T>(sql, param, commandTimeout: commandTimeout, transaction: transaction);
                if (first)
                    return new Result(200, $"Processo concluído.", data.FirstOrDefault());
                else
                    return new Result(200, $"Processo concluído.", data);
            }
            catch (Exception ex)
            {
                return new Result(500, $"Erro", ex.Message);
            }
        }

        public DataTable ExecuteDataTable(string sql, IDbTransaction transaction = null)
        {
            try
            {
                if (Equals(transaction, null))
                {
                    using SqlConnection connection = new SqlConnection(_dbConnectionCore.ConnectionString);
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    return ExecuteDataTableTransactionScope(sql, transaction);
                }
                else
                {
                    return ExecuteDataTableTransactionScope(sql, transaction);
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(ex.Message, 500);
            }
        }

        private DataTable ExecuteDataTableTransactionScope(string sql, IDbTransaction transaction)
        {
            var table = new DataTable();
            using (SqlCommand command = new SqlCommand(sql, (SqlConnection)transaction.Connection, (SqlTransaction)transaction))
            {
                command.CommandTimeout = 0;
                using SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(table);
            }
            return table;
        }

        private async Task<IResult> CreateOrUpdateTransactionScope<TEntity, TInsert, TUpdate>(TEntity entity, Expression<Func<TInsert, bool>> predicate, IDbTransaction transaction)
            where TEntity : Entity
            where TInsert : Entity
            where TUpdate : Entity
        {
            try
            {
                var result = await transaction.Connection.FirstOrDefaultAsync(predicate, transaction);
                if (result != null)
                {
                    var data = _mapper.Map<TUpdate>(entity);
                    data.Id = result.Id;
                    await transaction.Connection.UpdateAsync(data, transaction);
                }
                else
                {
                    var data = _mapper.Map<TInsert>(entity);
                    await transaction.Connection.InsertAsync(data, transaction);
                }

                return new Result(201, "", entity);
            }
            catch (Exception ex)
            {
                return new Result(500, $"Erro", ex.Message);
            }
        }

        public async Task<IResult> CreateOrUpdate<TEntity, TInsert, TUpdate>(TEntity entity, Expression<Func<TInsert, bool>> predicate, IDbTransaction transaction = null)
            where TEntity : Entity
            where TInsert : Entity
            where TUpdate : Entity
        {
            try
            {
                if (Equals(transaction, null))
                {
                    return await _dbConnectionCore.BeginConnection(async connection =>
                    {
                        return await _dbConnectionCore.BeginTransaction(connection, async transaction =>
                        {
                            return await CreateOrUpdateTransactionScope<TEntity, TInsert, TUpdate>(entity, predicate, transaction);
                        });
                    });
                }
                else
                {
                    return await CreateOrUpdateTransactionScope<TEntity, TInsert, TUpdate>(entity, predicate, transaction);
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(ex.Message, 500);
            }
        }

        public async Task<IResult> BulkInsert<T>(List<T> lista, string tableName, string tableTempName, IDbTransaction transaction = null)
        {
            List<SqlBulkCopyColumnMapping> lst = new List<SqlBulkCopyColumnMapping>();

            DataTable dt;
            
            if (lista.First() is IDynamicMetaObjectProvider)
            {
                var json = await JsonHelper.SerializeAsync(lista);
                dt = await JsonHelper.DeserializeAsync<DataTable>(json);
            }
            else
            {
                dt = lista.ToList().ToDataTable();
            }

            var dtSource = ExecuteDataTable($@"SELECT TOP 1 * FROM {$"{tableName}"} WITH (NOLOCK)", transaction); 

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string destinationColumnName = dt.Columns[i].ToString();
                if (dtSource.Columns.Contains(destinationColumnName))
                {
                    int sourceColumnIndex = dtSource.Columns.IndexOf(destinationColumnName);
                    string sourceColumnName = dtSource.Columns[sourceColumnIndex].ToString();
                    lst.Add(new SqlBulkCopyColumnMapping(sourceColumnName, sourceColumnName));
                }
            }

            var result = await BulkCopy(dt, tableName, lst, tableTempName, transaction);
            result.Parameters = lista;
            return result;
        }

        public async Task<IResult> BulkCopy(DataTable table, string destinationTableName, List<SqlBulkCopyColumnMapping> columnMappings, string tempTable = null, IDbTransaction transaction = null)
        {            
            try
            {
                if (Equals(transaction, null))
                {
                    return await _dbConnectionCore.BeginConnection(async connection =>
                    {
                        return await _dbConnectionCore.BeginTransaction(connection, async transaction =>
                        {
                            return await BulkCopyTransactionScope(table, destinationTableName, columnMappings, tempTable, transaction);
                        });
                    });
                }
                else
                {
                    return await BulkCopyTransactionScope(table, destinationTableName, columnMappings, tempTable, transaction);
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(ex.Message, 500);
            }
        }

        private async Task<IResult> BulkCopyTransactionScope(DataTable table, string destinationTableName, List<SqlBulkCopyColumnMapping> columnMappings, string tempTable, IDbTransaction transaction)
        {
            SqlBulkCopy bulkCopy = null;
            try
            {
                if (tempTable != null)
                    await Execute($@"SELECT TOP 0 * INTO {tempTable} FROM {destinationTableName} WITH (NOLOCK)", transaction: transaction, commandTimeout: 0);

                var options = SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.CheckConstraints;
                bulkCopy = new SqlBulkCopy((SqlConnection)transaction.Connection, options, (SqlTransaction)transaction);

                if (columnMappings != null)
                {
                    foreach (var item in columnMappings)
                    {
                        bulkCopy.ColumnMappings.Add(item);
                    }
                }
                                
                bulkCopy.DestinationTableName = tempTable ?? destinationTableName;
                bulkCopy.BatchSize = table.Rows.Count;
                bulkCopy.BulkCopyTimeout = 0;
                bulkCopy.EnableStreaming = true;
                await bulkCopy.WriteToServerAsync(table);

                return new Result(200, $"Processo concluido.", new { Ok = true });
            }
            catch (Exception ex)
            {
                return new Result(500, GetBulkCopyColumnException(ex, bulkCopy), null);
            }
        }

        private string GetBulkCopyColumnException(Exception ex, SqlBulkCopy bulkcopy)

        {
            string message;
            if (ex.Message.Contains("Received an invalid column length from the bcp client for colid"))
            {
                string pattern = @"\d+";
                Match match = Regex.Match(ex.Message.ToString(), pattern);
                var index = Convert.ToInt32(match.Value) - 1;

                FieldInfo fi = typeof(SqlBulkCopy).GetField("_sortedColumnMappings", BindingFlags.NonPublic | BindingFlags.Instance);
                var sortedColumns = fi.GetValue(bulkcopy);
                var items = (object[])sortedColumns.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sortedColumns);

                FieldInfo itemdata = items[index].GetType().GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance);
                var metadata = itemdata.GetValue(items[index]);
                var column = metadata.GetType().GetField("column", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);
                var length = metadata.GetType().GetField("length", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);
                message = (string.Format("Column: {0} contains data with a length greater than: {1}", column, length));
            }
            else
            {
                message = ex.ToString();
            }

            return message;
        }
    }
}
