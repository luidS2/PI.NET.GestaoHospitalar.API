using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;

namespace PI.GestaoHospitalar.Migrations
{
    public static class MigrationExtension
    {
        public static IApplicationBuilder Migrate(this IApplicationBuilder app)
        {
            Logger logger = LogManager.GetCurrentClassLogger();

            try
            {
                logger.Info("");
                using var scope = app.ApplicationServices.CreateScope();
                var runner = scope.ServiceProvider.GetService<IMigrationRunner>();
                runner.ListMigrations();
                runner.MigrateUp();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Erro ao realizar a migração");
            }

            return app;
        }
    }
}
