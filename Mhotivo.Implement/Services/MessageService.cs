using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mhotivo.Data.Entities;

namespace Mhotivo.Implement.Services
{
    public class MessageService
    {
        public static string ConstruirMensaje(Role role, string nombreNotificacion)
        {
            if (role.Name.Equals("Tutor"))
            {
                return EmailBaseMessage + "que se ha hecho un nuevo comentario en la notificación \"" + nombreNotificacion + "\" " + ParentUrlPage;
            }
            return EmailBaseMessage + "que se ha hecho un nuevo comentario en la notificación \"" + nombreNotificacion + "\" "  + AdministrativeUrlPage;
        }

        public static string NotificarTarea(string title)
        {
            return EmailBaseMessage + "ha asignado una nueva tarea en el portal con el titulo "+"\""+ title + "\" " + ParentUrlPage;
        }
        public static string ConstruirMensaje(Role role)
        {
            if (role.Name.Equals("Tutor"))
            {
                return EmailBaseMessage + NotificationMessage + ParentUrlPage;
            }
            return EmailBaseMessage + NotificationMessage + AdministrativeUrlPage;
        }

        public static string NotificationMessage = "ha recibido una nueva notificacion en el portal ";
        public static string EmailBaseMessage = ",<br><br>Se le comunica que ";
        public static string AdministrativeUrlPage = "porfavor ingrese a:<br><br> http://www.mhotivo.tk/Account/Login?ReturnUrl=%2f <br><br>Atentamente,<br><br>Fundación Mhotivo <br><p style=\"font-size:12px;line-height:16px;font-family:Helvetica,Arial,sans-serif;color:#999;\"> Por favor, NO responda a este mensaje, es un envío automático</p></html>";
        public static string ParentUrlPage = "porfavor ingrese a:<br><br> http://padres.mhotivo.tk/Account/Login <br><br>Atentamente,<br><br>Fundación Mhotivo <br><p style=\"font-size:12px;line-height:16px;font-family:Helvetica,Arial,sans-serif;color:#999;\"> Por favor, NO responda a este mensaje, es un envío automático</p></html>";
    }
}
