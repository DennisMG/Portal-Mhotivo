using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mhotivo.Data.Entities;
using Mhotivo.Implement.Services;
using Mhotivo.Interface.Interfaces;
using Mhotivo.ParentSite.Authorization;
using Mhotivo.ParentSite.Models;

namespace Mhotivo.ParentSite.Controllers
{
    public class MessageToTeacherController : Controller
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly ITutorRepository _tutorRepository;
        private readonly INotificationRepository _notificationRepository;

        private readonly IAcademicYearRepository _academicYearRepository;
        private readonly IUserRepository _userRepository;

        public MessageToTeacherController(ITeacherRepository teacherRepository,
            ITutorRepository tutorRepository, 
            INotificationRepository notificationRepository, 
            IAcademicYearRepository academicYearRepository,
            IUserRepository userRepository)
        {
            _teacherRepository = teacherRepository;
            _tutorRepository = tutorRepository;
            _notificationRepository = notificationRepository;
            _academicYearRepository = academicYearRepository;
            _userRepository = userRepository;
        }

        //
        // GET: /MessageToTeacher/
        [VerifyEmail]
        public ActionResult Index()
        {
            var allTeachers = _teacherRepository.GetAllTeachers().ToList();
            var allTeachersModel = new List<TeacherModel>();
            foreach (var teacher in allTeachers)
            {

                allTeachersModel.Add(new TeacherModel()
                {
                    Name = teacher.FullName,
                    Email = teacher.User.Email
                });
            }
            return View(new Tuple<IEnumerable<TeacherModel>, MessageToTeacherModel>(allTeachersModel,null));
        }
        [AuthorizeNewUser]
        public ActionResult SendNewMessage([Bind(Prefix = "Item2")] MessageToTeacherModel model)
        {
            if (HttpContext.Session != null)
            {
                var loggedUserEmail = System.Web.HttpContext.Current.Session["loggedUserEmail"].ToString();
                var  loggedTutor = _tutorRepository.Filter(y => y.User.Email == loggedUserEmail).FirstOrDefault();
                var teacher = _teacherRepository.Filter(x => x.User.Email == model.To).ToList().FirstOrDefault();

                var newNotification = new Notification(model.Subject, model.Message, loggedTutor, teacher,
                    NotificationType.Personal, _academicYearRepository.GetCurrentAcademicYear())
                {
                    Approved = true,
                    SendEmail = true
                };
                _notificationRepository.Create(newNotification);


                if (teacher == null) return RedirectToAction("Index");
                MailgunEmailService.SendEmailToUser(teacher.User, MessageService.ConstruirMensaje(teacher.User.Role));
                ViewBag.Message = "Mensaje Enviado!";
            }
            else
            {
                ViewBag.Message = "Mensaje No Enviado!";
            }
            return RedirectToAction("Index");
        }
    }
}
