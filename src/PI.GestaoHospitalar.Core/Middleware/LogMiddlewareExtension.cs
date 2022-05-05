using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace PI.GestaoHospitalar.Core.Middleware
{
    public static class LogMiddlewareExtension
    {
        public static IApplicationBuilder UseLogMiddleware(this IApplicationBuilder builder, string nameBackEnd, string servidorKafka, string topicoLogs, string localMachineName)
        {
            return builder.UseMiddleware<LogMiddleware>(nameBackEnd, servidorKafka, topicoLogs,localMachineName);
        }
    }
}
