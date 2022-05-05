using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace PI.GestaoHospitalar.Core.Helpers
{
    public class EmailHelper : IEmailHelper
    {
        private readonly IConfiguration _configuracao;        
        public EmailHelper(IConfiguration configuracao)
        {
            _configuracao = configuracao;            
        }
        public void Enviar(string emailTo, string assunto, string mensagem, string pathAttachment)
        {
            try
            {
                string SmtpHost = _configuracao.GetSection("ProjetoIntegrado:SmtpHost").Get<string>();
                string SystemMailAddress = _configuracao.GetSection("SystemMailAddress").Get<string>();
                mensagem = mensagem.Replace("\n", "<br />");
                using SmtpClient smtpClient = new SmtpClient(SmtpHost);
                using MailMessage email = new MailMessage
                {
                    From = new MailAddress(SystemMailAddress)
                };

                var listaEmailTo = emailTo.Split(';');
                for (int i = 0; i < listaEmailTo.Count(); i++)
                {
                    if (!string.IsNullOrEmpty(listaEmailTo[i]))
                    {
                        email.To.Add(new MailAddress(listaEmailTo[i]));
                    }
                }

                if (!string.IsNullOrEmpty(pathAttachment))
                {
                    email.Attachments.Add(new Attachment(pathAttachment));
                }
                email.Subject = assunto;
                email.Body = string.Format(TemplateEmail(), mensagem);
                email.IsBodyHtml = true;
                email.Priority = MailPriority.High;
                smtpClient.Send(email);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string TemplateEmail()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<div id=':3un' class='a3s' style='overflow: hidden;'>");
            sb.AppendLine("	<table width='100%' border='0' align='center' cellpadding='30' cellspacing='0' bgcolor='#195690'>");
            sb.AppendLine("		<tbody>");
            sb.AppendLine("			<tr>");
            sb.AppendLine("				<td>");
            sb.AppendLine("					<table width='1024' border='0' align='center'>");
            sb.AppendLine("						<tbody>");
            sb.AppendLine("							<tr>");
            sb.AppendLine("								<td width='212px'>");
            sb.AppendLine("									<img src='http://projetoIntegrado.vteximg.com.br/arquivos/qd.logo.png' width='212px' height ='63' border='0' alt='ProjetoIntegrado Logo' style='display: block'></td>");
            sb.AppendLine("								<td width='892' align='center'>");
            sb.AppendLine("									<p style='font-family: Helvetica, Arial, sans-serif; font-weight: bold; font-size: 18px; color: #ffffff; text-align: center'></span></p>");
            sb.AppendLine("								</td>");
            sb.AppendLine("							</tr>");
            sb.AppendLine("							<tr>");
            sb.AppendLine("								<td colspan='2'>&nbsp;</td>");
            sb.AppendLine("							</tr>");
            sb.AppendLine("							<tr>");
            sb.AppendLine("								<td colspan='2' bgcolor='#F3FBFF'>");
            sb.AppendLine("									<p style='margin-left: 30px; margin-right: 30px; font-family: Helvetica, Arial, sans-serif; font-size: 14px; text-align: justify; color: #2E8AE6'>");
            sb.AppendLine("										<br>");
            sb.AppendLine("									<div style='font-family: Helvetica, Arial, sans-serif; font-size: 14px; text-align: justify; color: #2E8AE6; margin-left: 5px;margin-right: 5px;'>");
            sb.AppendLine("										{0}");
            sb.AppendLine("									</div>");
            sb.AppendLine("									<br>");
            sb.AppendLine("									<br>	");
            sb.AppendLine("									<br>");
            sb.AppendLine("								</td>");
            sb.AppendLine("							</tr>");
            sb.AppendLine("						</tbody>");
            sb.AppendLine("					</table>");
            sb.AppendLine("					<p style='margin-left: 30px; margin-right: 30px; font-family: Helvetica, Arial, sans-serif; font-size: 10px; text-align: center; color: #ffffff'>");
            sb.AppendLine("						Esta &eacute; uma mensagem autom&aacute;tica. N&atilde;o &eacute; nescess&aacute;rio respond&ecirc;-la.<br>");
            sb.AppendLine("					</p>");
            sb.AppendLine("				</td>");
            sb.AppendLine("			</tr>");
            sb.AppendLine("		</tbody>");
            sb.AppendLine("	</table>");
            sb.AppendLine("</div>");

            return sb.ToString();
        }
    }
}
