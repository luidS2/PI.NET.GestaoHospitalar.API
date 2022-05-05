using PI.GestaoHospitalar.Core.Results;
using System;
using System.Data;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Core.Connection
{
    interface IDbConnectionCore
    {
        Task<IResult> BeginConnection(Func<IDbConnection, Task<IResult>> action);

        Task<IResult> BeginTransaction(IDbConnection connection, Func<IDbTransaction, Task<IResult>> action, IsolationLevel isolationLevel);

        Task<IResult> BeginTransaction(IDbConnection connection, Func<IDbTransaction, Task<IResult>> action);

    }
}
