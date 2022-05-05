using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace PI.GestaoHospitalar.Core.Controller
{
    public abstract class ControllerBaseCore : ControllerBase
    {
        /// <summary>
        /// Utilizado para Retorno em POST e demais verbos, Para Retorno GET utilize ReturnProcessStatusCodeGET
        /// </summary>
        /// <param name="statusCode"> Codigo do Status Code</param>
        /// <param name="message"> Mensagem</param>
        /// <param name="parameters"> Parâmetros</param>
        /// <param name="headers"> Informações do Header HTTP</param>
        /// <returns></returns>
        [NonAction]
        public async Task<object> ReturnProcessStatusCodeAsync(int statusCode, string message, object parameters, IList<KeyValuePair<string, string>> headers = null)
        {
            if (headers != null)
            {
                headers.ToList().ForEach(x => Response.Headers.Add(x.Key, x.Value));
            }
            return statusCode switch
            {
                200 => await Task.Run(() => Ok(parameters)),
                201 => await Task.Run(() => Created("", parameters)),
                202 => await Task.Run(() => Accepted()),
                204 => await Task.Run(() => NoContent()),
                404 => await Task.Run(() => NotFound(message)),
                409 => await Task.Run(() => Conflict(message)),
                422 => await Task.Run(() => UnprocessableEntity(parameters)),
                500 => await Task.Run(() => Problem(string.Format("{0}\n{1}", message, parameters.ToString()))),
                _ => await Task.Run(() => Problem(string.Format("Erro ou retorno sem tratamento:{0}\n{1}\n{1}", statusCode, message, parameters.ToString()))),
            };
        }
        /// <summary>
        /// Utilizado para Retorno  ActionResult GET, para POST ou outro Verbo utilize ReturnProcessStatusCodeAsync
        /// </summary>
        /// <param name="statusCode"> Codigo do Status Code</param>
        /// <param name="message"> Mensagem</param>
        /// <param name="parameters"> Parâmetros</param>
        /// <param name="headers"> Informações do Header HTTP</param>
        /// <returns></returns>
        [NonAction]
        public ActionResult ReturnProcessStatusCodeGET(int statusCode, string message, object parameters, IList<KeyValuePair<string, string>> headers = null)
        {
            if (headers != null)
            {
                headers.ToList().ForEach(x => Response.Headers.Add(x.Key, x.Value));
            }
            return statusCode switch
            {
                200 => Ok(parameters),
                201 => Created("", parameters),
                202 => Accepted(),
                204 => NoContent(),
                404 => NotFound(message),
                409 => Conflict(message),
                422 => UnprocessableEntity(parameters),
                500 => Problem(string.Format("{0}\n{1}", message, parameters.ToString())),
                _ => Problem(string.Format("Erro ou retorno sem tratamento:{0}\n{1}\n{1}", statusCode, message, parameters.ToString())),
            };
        }
    }
}
