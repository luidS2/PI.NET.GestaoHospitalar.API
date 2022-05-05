using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PI.GestaoHospitalar.API
{
    /// <summary>
    /// Inicializador do Cors
    /// </summary>
    public static class CorsStartup
    {
        /// <summary>
        /// Adiciona cors customizado.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuracao"></param>
        public static void AddCorsCustomizado(this IServiceCollection services, IConfiguration configuracao)
        {
            var origins = configuracao.GetSection("Cors:AllowOrigins").Get<string[]>();

            services.AddCors(o => o.AddPolicy("policy", builder =>
            {
                builder.WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }));
        }

        /// <summary>
        /// Adiciona utilização do cors customizado
        /// </summary>
        /// <param name="app"></param>
        public static void UseCorsCustomizado(this IApplicationBuilder app)
        {
            app.UseCors("policy");
        }
    }
}
