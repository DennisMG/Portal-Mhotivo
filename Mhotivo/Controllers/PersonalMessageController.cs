using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Mhotivo.Authorizations;
using Mhotivo.Data.Entities;
using Mhotivo.Interface.Interfaces;
using Mhotivo.Logic.ViewMessage;
using Mhotivo.Models;
using PagedList;

namespace Mhotivo.Controllers
{
    public class PersonalMessageController : Controller
    {
        private readonly IAcademicGradeRepository _academicGradeRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ISessionManagementService _sessionManagement;
        private readonly IUserRepository _userRepository;
        private readonly ViewMessageLogic _viewMessageLogic;

        public PersonalMessageController(ISessionManagementService sessionManagement, IUserRepository userRepository,
            INotificationRepository notificationRepository, 
            IAcademicGradeRepository academicGradeRepository)
        {
            _sessionManagement = sessionManagement;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
           _academicGradeRepository = academicGradeRepository;
           _viewMessageLogic = new ViewMessageLogic(this);
        }

        // GET: /PersonalMessage/
        [AuthorizeNewUser]
        public ActionResult Index(string searchName, int? page)
        {
            _viewMessageLogic.SetViewMessageIfExist();
            var loggedUser =
                _userRepository.GetById(Convert.ToInt64(_sessionManagement.GetUserLoggedId()));

            var roleName = loggedUser.Role.Name;
            var notifications = new List<Notification>();
            if (roleName.Equals("Administrador"))
            {
                notifications =
                    _notificationRepository.Filter(personal => personal.NotificationType == NotificationType.Personal)
                        .OrderByDescending(x => x.CreationDate).ToList();
            }
            else if (roleName.Equals("Director"))
            {
                var academicGrade = _academicGradeRepository.Filter(
                    x =>
                        x.Grade.EducationLevel.Director != null &&
                        x.Grade.EducationLevel.Director.Id == loggedUser.Id && x.SectionTeacher != null).ToList();
                var teachersIds = academicGrade.Select( x => x.SectionTeacher.User.Id).Distinct().ToList();
                notifications =
                    _notificationRepository.Filter(personal => personal.NotificationType == NotificationType.Personal
                                                               &&
                                                               teachersIds.Any(
                                                                   x => personal.To != null &&
                                                                       personal.To.User.Id == x)).ToList();
            }
            else
            {
                notifications =
                    _notificationRepository.Filter(personal => personal.NotificationType == NotificationType.Personal
                                                               && personal.To != null &&
                                                               personal.To.User.Email == loggedUser.Email)
                        .OrderByDescending(x => x.CreationDate)
                        .ToList();
            }


            if (!string.IsNullOrWhiteSpace(searchName))
                notifications = notifications.ToList().FindAll(x => x.Title.Contains(searchName));
            ViewBag.RoleName = roleName;
            var notificationsModel = notifications.Select(Mapper.Map<PersonalMessageDisplayModel>);
            const int pageSize = 10;
            var pageNumber = page ?? 1;
            return View(notificationsModel.ToPagedList(pageNumber, pageSize));
        }
    }
}