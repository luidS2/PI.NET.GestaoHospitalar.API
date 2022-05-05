using System.Collections.Generic;

namespace PI.GestaoHospitalar.Core.Results
{
    public class Result : IResult
    {
        /// <summary>
        /// Resultado para execução de Comandos
        /// </summary>
        /// </summary>
        /// <param name="statusCode">O código de status que representa o retorno.</param>
        /// <param name="message">A mensagem de retorno.</param>
        /// <param name="parameters">Opcional.Uma matriz de atributos usada para gerar uma mensagem de erro diferente e / ou localizada para o cliente.</param>
        public Result(int statusCode, string message, object parameters)
        {
            StatusCode = statusCode;
            Message = message;
            Parameters = parameters;
        }
        /// <summary>
        /// Resultado para execução de Comandos
        /// </summary>
        /// <param name="statusCode">O código de status que representa o retorno.</param>
        /// <param name="message">A mensagem de retorno.</param>
        /// <param name="parameters">Opcional.Uma matriz de atributos usada para gerar uma mensagem de erro diferente e / ou localizada para o cliente.</param>
        /// <param name="headers">Cabeçalho da Requisição Http</param>
        public Result(int statusCode, string message, object parameters, IList<KeyValuePair<string, string>> headers)
        {
            StatusCode = statusCode;
            Message = message;
            Parameters = parameters;
            Headers = headers;
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object Parameters { get; set; }
        public IList<KeyValuePair<string, string>> Headers { get; set; }
    }
}
