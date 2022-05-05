using PI.GestaoHospitalar.Core.Domain;
using System;

namespace PI.GestaoHospitalar.Domain.PacienteContext.Entities
{
    public class Paciente : Entity, IAggregateRoot
    {
        public int NumeroPaciente { get; private set; }
        public int PressaoSistolica { get; private set; }
        public int PressaoDiastolica { get; private set; }
        public string Nome { get; private set; }

        public Paciente(int numeroPaciente, int pressaoSistolica, int pressaoDiastolica, string nome)
        {
            NumeroPaciente = numeroPaciente;
            PressaoSistolica = pressaoSistolica;
            PressaoDiastolica = pressaoDiastolica;
            Nome = nome;
        }
        public Paciente(Guid id, int numeroPaciente, int pressaoSistolica, int pressaoDiastolica, string nome)
        {
            Id = id;
            NumeroPaciente = numeroPaciente;
            PressaoSistolica = pressaoSistolica;
            PressaoDiastolica = pressaoDiastolica;
            Nome = nome;
        }
    }
}