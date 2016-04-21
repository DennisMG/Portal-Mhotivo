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
            request.AddParameter("html", "<html>Estimado " +  user.Name  + ", <br>Se ha creado una notificacion en la cual usted ha sido incluido. Favor revisar el portal </html>");
            request.Method = Method.POST;
            client.Execute(request);
           
        }
    }
}
