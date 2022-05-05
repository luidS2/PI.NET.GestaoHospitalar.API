using PI.GestaoHospitalar.Core.Domain;
using System;

namespace PI.GestaoHospitalar.Core.Data
{
    public interface IRepository<T> : IDisposable where T : IAggregateRoot
    {
        //IUnitOfWork UnitOfWork { get; }
    }
}

