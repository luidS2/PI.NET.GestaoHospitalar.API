using PI.GestaoHospitalar.Core.ExceptionCore;
using prmToolkit.NotificationPattern;
using System.Collections.Generic;
using System.Linq;

namespace PI.GestaoHospitalar.Core.Handlers
{
    public class BaseHandler : Notifiable
    {
        public void AdicionarNotificacoes(params IEnumerable<Notifiable>[] objects)
        {
            AddNotifications(objects);

            var retorno = (from notification in Notifications
                           select new
                           {
                               Messagem = notification.Message
                           }).ToArray();
            if (IsInvalid())
            {                
                throw new BaseException(retorno.ToString(), 422);
            }
        }

        public void AdicionarNotificacoes(params Notifiable[] objects)
        {
            AddNotifications(objects);

            var retorno = (from notification in Notifications
                           select new
                           {
                               Messagem = notification.Message
                           }).ToArray();
            if (IsInvalid())
            {
                throw new BaseException(retorno.ToString(), 422);
            }
        }
    }
}
