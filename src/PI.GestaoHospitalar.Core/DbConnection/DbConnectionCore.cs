using PI.GestaoHospitalar.Core.Connection;
using PI.GestaoHospitalar.Core.ExceptionCore;
using PI.GestaoHospitalar.Core.Results;
using Microsoft.AspNetCore.Http;
using System;
using System.Data;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Core.DbConnection
{
    public abstract class DbConnectionCore : IDbConnectionCore
    {
        public DbConnectionCore()
        {
        }

        protected DbConnectionCore(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }
        public string DbIntegracaoSAP { get; set; }

        public virtual async Task<IResult> BeginConnection(Func<IDbConnection, Task<IResult>> action)
        {
            return await Try(async () =>
            {
                using IDbConnection connection = GetConnection();

                connection.Open();

                IResult result = await Try(async () => await action(connection));

                if (connection != null)
                {
                    connection.Close();
                }

                return result;
            });
        }

        protected abstract IDbConnection GetConnection();

        public async Task<IResult> BeginTransaction(IDbConnection connection, Func<IDbTransaction, Task<IResult>> action) =>
            await BeginTransaction(connection, action, IsolationLevel.Serializable);

        public async Task<IResult> BeginTransaction(IDbConnection connection, Func<IDbTransaction, Task<IResult>> action, IsolationLevel isolationLevel)
        {
            return await Try(async () =>
            {
                using IDbTransaction transaction = connection.BeginTransaction(isolationLevel);

                IResult result = await Try(async () => await action(transaction));

                if (result.StatusCode >= 200 && result.StatusCode < 300) //sucesso
                    transaction.Commit();
                else
                    transaction.Rollback();

                return result;
            });
        }

        private static async Task<IResult> Try(Func<Task<IResult>> action)
        {
            try
            {
                return await action();
            }
            catch (BaseException baseException)
            {
                return new Result(baseException.StatusCode, baseException.Message, baseException.ToDetailedString());
            }
            catch (Exception exception)
            {
                return new Result(500, "Erro interno do Servidor de Banco de Dados", exception.ToDetailedString());
            }
        }
    }
}