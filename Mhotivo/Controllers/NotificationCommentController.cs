using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using Mhotivo.Authorizations;
using Mhotivo.Data.Entities;
using Mhotivo.Implement.Repositories;
using Mhotivo.Implement.Services;
using Mhotivo.Interface.Interfaces;
using Mhotivo.Models;

namespace Mhotivo.Controllers
{
    public class NotificationCommentController : Controller
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationCommentRepository _notificationCommentRepository;
        private readonly IUserRepository _userRepository;
      

        public NotificationCommentController(INotificationRepository notificationRepository, 
            INotificationCommentRepository notificationCommentRepository,
            IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _notificationCommentRepository = notificationCommentRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        [AuthorizeNewUser]
        public ActionResult Add(long id)
        {
          return PartialView(new NotificationCommentRegisterModel() { Notification = id});
        }

        [HttpPost]
        [AuthorizeNewUser]
        public ActionResult Add(NotificationCommentRegisterModel notificationCommentRegister)
        {

            var loggedUserEmail = System.Web.HttpContext.Current.Session["loggedUserEmail"].ToString();
            var loggedUser = _userRepository.Filter(y => y.Email == loggedUserEmail).FirstOrDefault();
            var selectedNotification = _notificationRepository.GetById(notificationCommentRegister.Notification);
            selectedNotification.NotificationComments.Add(new NotificationComment
            {
                CommentText = notificationCommentRegister.CommentText,
                Commenter = loggedUser
            });
            _notificationRepository.Update(selectedNotification);

            var users = selectedNotification.RecipientUsers.ToList();
            foreach (var user in users)
            {
                if(!user.Email.Equals(loggedUserEmail))
                    MailgunEmailService.SendEmailToUser(user, MessageService.ConstruirMensaje(user.Role, selectedNotification.Title) );
            }
            var notificationId = notificationCommentRegister.Notification;
            return RedirectToAction("Index", new { notificationId });
        }

        [AuthorizeNewUser]
        public ActionResult Index(long notificationId)
        {
            var notification = _notificationRepository.GetById(notificationId);
            var commentsForNotifications = notification.NotificationComments.Select(Mapper.Map<NotificationCommentDisplayModel>);
            ViewBag.NotificationId = notificationId;
            return View(commentsForNotifications);
        }
        [AuthorizeNewUser]
        public ActionResult Delete(long notificationId, long commentId)
        {
            var comments = _notificationCommentRepository.GetById(commentId);
            _notificationCommentRepository.Delete(comments);
            return RedirectToAction("Index",new { notificationId });
        }

    }
}
