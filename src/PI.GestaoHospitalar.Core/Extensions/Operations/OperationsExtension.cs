using PI.GestaoHospitalar.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PI.GestaoHospitalar.Core.Extensions.Operations
{
    public static class OperationsExtension
    {
        /// <summary>
        /// Obtém apenas os registros mais novos removendo dados duplicados baseand-se no predicado informado.
        /// </summary>
        /// <value>predicato de função com as chaves do registro, exemplo: produtoDeposito => new {produtoDeposito.CodigoProduto, produtoDeposito.CodigoDeposito}</value>
        public static IEnumerable<TDto> GetOnlyLastRegisters<TDto, TResult>(
            this IEnumerable<TDto> list, Func<TDto, TResult> predicate) where TDto : CdcDto
        {
            return list.GroupBy(predicate)
                       .Select(grp => grp.FirstOrDefault(x => x.DataEntregaPacote == grp.Max(y => y.DataEntregaPacote)))
                       .ToList();
        }
    }
}
