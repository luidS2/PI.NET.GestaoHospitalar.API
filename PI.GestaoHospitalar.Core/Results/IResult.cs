using System.Collections.Generic;

namespace PI.GestaoHospitalar.Core.Results
{
    /// <summary>
    /// StatusCode:	O código de status que representa o retorno.
    /// Message: A mensagem de retorno.
    /// parameters:  Opcional.Uma matriz de atributos usada para gerar uma mensagem de erro quando houver diferente e / ou localizada para o cliente.
    /// </summary>
    public interface IResult
    {
        int StatusCode { get; set; }
        string Message { get; set; }
        object Parameters { get; set; }
        IList<KeyValuePair<string, string>> Headers { get; set; }
    }
}
