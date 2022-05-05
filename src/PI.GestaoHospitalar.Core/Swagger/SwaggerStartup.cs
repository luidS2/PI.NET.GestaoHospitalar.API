using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace PI.GestaoHospitalar.Core.Swagger
{
    public static class SwaggerStartup
    {
        public static void AddSwaggerCustomizado(this IServiceCollection services, string title, string version, string description, string xmlPath)
        {
            services.AddSwaggerGen(x =>
            {
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    x.IncludeXmlComments(xmlPath);
                }
                x.SwaggerDoc(version,
                                    new OpenApiInfo
                                    {
                                        Title = title,
                                        Version = version,
                                        Description = description,
                                        Contact = new OpenApiContact
                                        {
                                            Name = "Arquitetura",
                                            Email = "arquitetura@projetoIntegrado.com.br"
                                        }
                                    });
            });
        }

        public static void UseSwaggerCustomizado(this IApplicationBuilder app, string SwaggerFile,string nameAPI)
        {
            app.UseSwagger(c =>
            {
                // c.SerializeAsV2 = true; // Para Swagger 2.0 descomentar aqui
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(SwaggerFile, nameAPI);
                c.RoutePrefix = "";
            });

        }
    }
}
