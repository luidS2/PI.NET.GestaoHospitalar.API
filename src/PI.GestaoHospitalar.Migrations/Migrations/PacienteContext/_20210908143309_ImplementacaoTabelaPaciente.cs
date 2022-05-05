using FluentMigrator;
using PI.GestaoHospitalar.Domain.PacienteContext.Entities;
using System;

namespace PI.GestaoHospitalar.Migrations.Migrations.PacienteContext
{
    [Migration(20210908143309, "Implementacao Tabela Paciente")]

    public class _20210908143309_ImplementacaoTabelaPaciente : Migration
    {
        public override void Down()
        {
            throw new NotImplementedException();
        }

        public override void Up()
        {
            Create.Table($"{typeof(Paciente).Namespace.Replace(".", "_")}_{typeof(Paciente).Name}")
                .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("Nome").AsString().NotNullable()
                .WithColumn("NumeroPaciente").AsInt32().NotNullable()
                .WithColumn("PressaoSistolica").AsInt32().NotNullable()
                .WithColumn("PressaoDiastolica").AsInt32().NotNullable();
        }
    }
}
