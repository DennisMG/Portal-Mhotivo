using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Mhotivo.Data.Entities;
using RestSharp;
using RestSharp.Authenticators;

namespace Mhotivo.Implement.Services
{
    public class MailgunEmailService
    {
        public static void SendEmailToUsers(List<User> userList, Notification notification)
        {
            foreach (var user in userList)
            {
                SendEmailToUser(user, notification);
            }
        }

        public static void SendEmailToUser(User user, Notification notification)
        {
            if (String.IsNullOrWhiteSpace(user.Email))
                return;

            RestClient client = new RestClient
            {
                BaseUrl = new Uri("https://api.mailgun.net/v3"),
                Authenticator = new HttpBasicAuthenticator("api",
                    "key-b6702d7b76a3bc3b0bf4b23085557e9b")
            };
            RestRequest request = new RestRequest();
            request.AddParameter("domain",
                                "sandboxce1b0c48aca04155828bb554357ae733.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Fundacion Mhotivo <mhotivo@sandboxce1b0c48aca04155828bb554357ae733.mailgun.org>");
            request.AddParameter("to",user.Name + " " +"<" + user.Email + ">");
            request.AddParameter("subject", notification.Title);
            request.AddParameter("html", "<html>Estimado/a " +  user.Name  + ", <br>Se le notifica que ha recibido un nuevo mensaje en el portal, porfavor ingrese a:<br> http://padres.mhotivo.tk/Account/Login <br> Atentamente,<br> Fundación Mhotivo <br> <p style=\"font-size:12px;line-height:16px;font-family:Helvetica,Arial,sans-serif;color:#999;\"> Por favor, NO responda a este mensaje, es un envío automático</p></html>");
            request.Method = Method.POST;
            client.Execute(request);
           
        }
    }
}
