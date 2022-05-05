using PI.GestaoHospitalar.Core.DbConnection;
using PI.GestaoHospitalar.Core.Middleware;
using PI.GestaoHospitalar.Core.Services;
using PI.GestaoHospitalar.Core.Swagger;
using AutoMapper;
using Dapper.FluentMap;
using FluentMigrator.Runner;
using PI.GestaoHospitalar.API.Services;
using PI.GestaoHospitalar.Domain.PacienteContext.Config.Kafka;
using PI.GestaoHospitalar.Domain.PacienteContext.Repositories;
using PI.GestaoHospitalar.Infrastructure.Data.PacienteContext.Repositories;
using PI.GestaoHospitalar.Migrations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using PI.GestaoHospitalar.Domain.PacienteContext.Handlers;
using PI.GestaoHospitalar.Domain.PacienteContext.HubConfig;
using PI.GestaoHospitalar.Domain.PacienteContext.Mapping;

namespace PI.GestaoHospitalar.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            services.AddSwaggerCustomizado("GN - API de Gestão Hospitalar", "v1", "API de Gestão Hospitalar", xmlPath);


            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddControllers(options =>
            {
                options.OutputFormatters.RemoveType<StringOutputFormatter>();
            });

            var ConnectionSqlServer = new DbConnectionCoreSqlServer();
            Configuration.Bind("DbConnectionCoreSqlServer", ConnectionSqlServer);
            services.AddSingleton<DbConnectionCore>(ConnectionSqlServer);

            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfile(new Infrastructure.Data.PacienteContext.InfraObjects.AutoMapper.AutoMapperInfra());
                config.AddProfile(new PacienteProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            FluentMapper.Initialize(config =>
            {
                Infrastructure.Data.PacienteContext.Mapper.Initialize.Config(config);
            });

            services.AddSignalR();

            services.AddCorsCustomizado(Configuration);

            services
              .AddLogging(c => c.AddFluentMigratorConsole())
              .AddFluentMigratorCore()
              .ConfigureRunner(
              builder => builder
              .AddSqlServer2016()
              .WithGlobalConnectionString(ConnectionSqlServer.ConnectionString)
              .WithVersionTable(new VersionTable())
              .ScanIn(typeof(MigrationExtension).Assembly).For.All());

            services.AddControllers();

            #region Paciente
            services.AddTransient<IPacienteRepository, PacienteRepository>();
            services.AddMediatR(typeof(PacienteHandler).GetTypeInfo().Assembly); //Mediator 
            BackGroundServiceConfig(services);

            #endregion

            services.AddSingleton<Processing>();
            services.AddHostedService<BackgroundServiceStarter<Processing>>();
            Console.WriteLine("*********************************************************************");
            Console.WriteLine($"UrlAPI = {Configuration.GetSection("UrlAPI").Value}");
            Console.WriteLine($"KafkaConfig = {Configuration.GetSection("KafkaConfig:Servidor").Value}");
            Console.WriteLine("*********************************************************************");
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration iConfig)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwaggerCustomizado("swagger/v1/swagger.json", "GN - API Cadastros WMS");

            app.UseRouting();

            //app.UseAuthentication();

            app.UseAuthorization();

            //app.UseHttpsRedirection(); //atrapalha dando erro: connect ECONNREFUSED, por isso foi comentado

            app.UseCorsCustomizado();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireCors("policy");

                endpoints.MapHub<PacienteHub>("/hub/pacientes");
            });

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
            });

            app.Migrate();
        }
        private void BackGroundServiceConfig(IServiceCollection services)
        {
            var PacienteKafkaConfig = new PacienteKafkaConfig();
            Configuration.Bind("KafkaConfig", PacienteKafkaConfig);
            services.AddSingleton(PacienteKafkaConfig);

            services.AddSingleton<Processing>();
            services.AddHostedService<BackgroundServiceStarter<Processing>>();
        }
    }
}
