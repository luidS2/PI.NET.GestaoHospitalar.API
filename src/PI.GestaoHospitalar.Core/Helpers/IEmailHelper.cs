namespace PI.GestaoHospitalar.Core.Helpers
{
    public interface IEmailHelper
    {
        void Enviar(string emailTo, string assunto, string mensagem, string pathAttachment);
    }
}
