using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;
using AutoMapper;
using Mhotivo.Authorizations;
using Mhotivo.Implement.Utils;
using Mhotivo.Logic.ViewMessage;
using Mhotivo.Models;
using Mhotivo.Data.Entities;
using Mhotivo.Implement.Services;
using Mhotivo.Interface.Interfaces;
using Microsoft.Ajax.Utilities;
using Microsoft.Office.Interop.Excel;
using PagedList;


namespace Mhotivo.Controllers
{
    [AuthorizeNewUser]
    public class NotificationController : Controller
    {
        private readonly ViewMessageLogic _viewMessageLogic;
        private readonly ISessionManagementService _sessionManagement;
        private readonly IGradeRepository _gradeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IPeopleRepository _peopleRepository;
        private readonly ITutorRepository _tutorRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IAcademicYearRepository _academicYearRepository;
        private readonly IAcademicGradeRepository _academicGradeRepository;
        private readonly IAcademicCourseRepository _academicCourseRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IEducationLevelRepository _areaReporsitory;
        private readonly INotificationHandlerService _notificationHandlerService;
        private readonly IEducationLevelRepository _educationLevelRepository;

        public NotificationController(ISessionManagementService sessionManagement, IUserRepository userRepository,
            INotificationRepository notificationRepository, IPeopleRepository peopleRepository,
            ITeacherRepository teacherRepository,
            IAcademicCourseRepository academicCourseRepository, IStudentRepository studentRepository,
            ITutorRepository tutorRepository, IGradeRepository gradeRepository,
            IAcademicYearRepository academicYearRepository,
            IEducationLevelRepository areaReporsitory, INotificationHandlerService notificationHandlerService, IAcademicGradeRepository academicGradeRepository, IEducationLevelRepository educationLevelRepository)
        {
            _sessionManagement = sessionManagement;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _peopleRepository = peopleRepository;
            _teacherRepository = teacherRepository;
            _academicCourseRepository = academicCourseRepository;
            _tutorRepository = tutorRepository;
            _studentRepository = studentRepository;
            _gradeRepository = gradeRepository;
            _academicYearRepository = academicYearRepository;
            _areaReporsitory = areaReporsitory;
            _notificationHandlerService = notificationHandlerService;
            _academicGradeRepository = academicGradeRepository;
            _educationLevelRepository = educationLevelRepository;
            _viewMessageLogic = new ViewMessageLogic(this);
        }
        [AuthorizeNewUser]
        public ActionResult Index(string searchName, int? page)
        {
            _viewMessageLogic.SetViewMessageIfExist();
            var user =
                _userRepository.GetById(Convert.ToInt64(_sessionManagement.GetUserLoggedId()));
            var isDirector = user.Role.Name.Equals("Director");
            var isAdmin = user.Role.Name.Equals("Administrador");
            var notifications = new List<Notification>();
            if (isAdmin)
            {
                notifications = _notificationRepository.Filter( personal => personal.NotificationType != NotificationType.Personal).OrderByDescending( x => x.CreationDate).ToList();
            }
            else if (isDirector)
            {
                notifications.AddRange(
                    _notificationRepository.Filter(x => x.NotificationType == NotificationType.General));
                var educationLevel = _educationLevelRepository.Filter(x => x.Director != null && x.Director.Id == user.Id).FirstOrDefault();
                if (educationLevel != null)
                {
                    notifications.AddRange(
                    _notificationRepository.Filter(
                        x =>
                            x.NotificationType == NotificationType.EducationLevel &&
                            x.DestinationId == educationLevel.Id));
                    var gradeIds = educationLevel.Grades.Select(x => x.Id);
                    notifications.AddRange(_notificationRepository.Filter(x => x.NotificationType == NotificationType.Grade && gradeIds.Contains(x.DestinationId)));
                    var academicGradeIds =
                        _academicGradeRepository.Filter(x => gradeIds.Contains(x.Grade.Id)).Select(x => x.Id);
                    notifications.AddRange(_notificationRepository.Filter(x => x.NotificationType == NotificationType.Section && academicGradeIds.Contains(x.DestinationId)));
                    var courseIds = _academicCourseRepository.Filter(x => academicGradeIds.Contains(x.AcademicGrade.Id)).Select(x => x.Id);
                    notifications.AddRange(_notificationRepository.Filter(x => x.NotificationType == NotificationType.Course && courseIds.Contains(x.DestinationId)));
                    var tutorIds =
                        _studentRepository.Filter(x => x.MyGrade != null && academicGradeIds.Contains(x.MyGrade.Id))
                            .Select(x => x.Tutor1.Id);
                    notifications.AddRange(_notificationRepository.Filter(x => x.NotificationType == NotificationType.Student && tutorIds.Contains(x.DestinationId)));
                }
            }else
                notifications = _notificationRepository.Filter(x => x.NotificationCreator.Id == user.UserOwner.Id).ToList();
            if (!string.IsNullOrWhiteSpace(searchName))
                notifications = notifications.ToList().FindAll(x => x.Title.Contains(searchName));

            var notificationsModel = notifications.Select(Mapper.Map<NotificationDisplayModel>);
            const int pageSize = 10;
            var pageNumber = (page ?? 1);
            return View(notificationsModel.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        [AuthorizeNewUser]
        public ActionResult Add()
        {
            var notification = new NotificationRegisterModel();
            var items = ((NotificationType[])Enum.GetValues(typeof(NotificationType))).Select(c => new SelectListItem
            {
                Text = c.GetEnumDescription(),
                Value = c.ToString("D")
            }).ToList();

            var user =
                _userRepository.GetById(Convert.ToInt64(_sessionManagement.GetUserLoggedId()));
            var roleName = user.Role.Name;
            items = items.FindAll(x => x.Text != "Personal");
            if (roleName.Equals("Director"))
            {
                items = items.FindAll(x => x.Text != "General");
            }else if (roleName.Equals("Maestro"))
            {
                items = items.FindAll(x => x.Text != "General" && x.Text != "Nivel de Educación");
            }
            ViewBag.NotificationTypes = new List<SelectListItem>(items);
            var list = new List<SelectListItem> { new SelectListItem { Value = "-1", Text = "N/A" } };
            ViewBag.List1 = new List<SelectListItem>(list);
            ViewBag.List2 = new List<SelectListItem>(list);
            ViewBag.DestinationList = new List<SelectListItem>(list);
            return View("Add", notification);
        }

        [HttpPost]
        [AuthorizeNewUser]
        public ActionResult Add(NotificationRegisterModel eventNotification)
        {
            eventNotification.NotificationCreator = _userRepository.GetById(Convert.ToInt64(_sessionManagement.GetUserLoggedId())).UserOwner.Id;
            eventNotification.AcademicYear = _academicYearRepository.GetCurrentAcademicYear().Id;
            var notificationIdentity = Mapper.Map<Notification>(eventNotification);
            var approved = _sessionManagement.GetUserLoggedRole().Equals("Administrador");
            notificationIdentity.Approved = approved;
            notificationIdentity = _notificationRepository.Create(notificationIdentity);
            var users = _userRepository.Filter(x => x.Role.Name == "Administrador");
            if (!approved)
            {
                foreach (var user in users)
                {
                    MailgunEmailService.SendEmailToUser(user, MessageService.ApproveMessage());
                }
            }
            _notificationHandlerService.SendAllPending();
            const string title = "Notificación Agregada";
            var content = "La notificacion " + notificationIdentity.Title + " ha sido agregada exitosamente.";
            _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
            return RedirectToAction("Index");
        }
        [AuthorizeNewUser]
        public ActionResult Edit(long id)
        {
            var toEdit =
                _notificationRepository.GetById(id);
            var toEditModel = Mapper.Map<NotificationEditModel>(toEdit);
            var items = ((NotificationType[])Enum.GetValues(typeof(NotificationType))).Select(c => new SelectListItem
            {
                Text = c.GetEnumDescription(),
                Value = c.ToString("D")
            }).ToList();
            ViewBag.NotificationTypes = new List<SelectListItem>(items);
            var list = new List<SelectListItem> { new SelectListItem { Value = "-1", Text = "N/A" } };
            ViewBag.List1 = new List<SelectListItem>(list);
            ViewBag.List2 = new List<SelectListItem>(list);
            ViewBag.DestinationList = new List<SelectListItem>(list);
            return View(toEditModel);
        }

        [HttpPost]
        [AuthorizeNewUser]
        public ActionResult Edit(long id, NotificationEditModel eventNotificationEdit)
        {
            try
            {
                var toEdit = _notificationRepository.GetById(eventNotificationEdit.Id);
                toEdit = Mapper.Map(eventNotificationEdit, toEdit);
                _notificationRepository.Update(toEdit);
                _viewMessageLogic.SetNewMessage("Notificación Editada", "La notificación fue editada exitosamente.",
                    ViewMessageType.SuccessMessage);
            }
            catch
            {
                _viewMessageLogic.SetNewMessage("Error en edición",
                    "La notificación no pudo ser editada correctamente, por favor intente nuevamente.",
                    ViewMessageType.ErrorMessage);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AuthorizeNewUser]
        public ActionResult Delete(long id)
        {
            _notificationRepository.Delete(id);
            return RedirectToAction("Index");
        }

        [AuthorizeAdminDirector]
        public ActionResult Approve(int? page)
        {
            _viewMessageLogic.SetViewMessageIfExist();
            var user =
                _userRepository.GetById(Convert.ToInt64(_sessionManagement.GetUserLoggedId()));
            var isDirector = user.Role.Name.Equals("Director");
            var notifications = new List<Notification>();
            if (isDirector)
            {
                notifications.AddRange(
                    _notificationRepository.Filter(x => x.NotificationType == NotificationType.General && !x.Approved));
                var educationLevel =
                    _educationLevelRepository.Filter(x => x.Director != null && x.Director.Id == user.Id)
                        .FirstOrDefault();
                if (educationLevel != null)
                {
                    notifications.AddRange(
                        _notificationRepository.Filter(
                            x =>
                                x.NotificationType == NotificationType.EducationLevel &&
                                x.DestinationId == educationLevel.Id && !x.Approved));
                    var gradeIds = educationLevel.Grades.Select(x => x.Id);
                    notifications.AddRange(
                        _notificationRepository.Filter(
                            x =>
                                x.NotificationType == NotificationType.Grade && gradeIds.Contains(x.DestinationId) &&
                                !x.Approved));
                    var academicGradeIds =
                        _academicGradeRepository.Filter(x => gradeIds.Contains(x.Grade.Id)).Select(x => x.Id);
                    notifications.AddRange(
                        _notificationRepository.Filter(
                            x =>
                                x.NotificationType == NotificationType.Section &&
                                academicGradeIds.Contains(x.DestinationId) && !x.Approved));
                    var courseIds =
                        _academicCourseRepository.Filter(x => academicGradeIds.Contains(x.AcademicGrade.Id))
                            .Select(x => x.Id);
                    notifications.AddRange(
                        _notificationRepository.Filter(
                            x =>
                                x.NotificationType == NotificationType.Course && courseIds.Contains(x.DestinationId) &&
                                !x.Approved));
                    var tutorIds =
                        _studentRepository.Filter(x => x.MyGrade != null && academicGradeIds.Contains(x.MyGrade.Id))
                            .Select(x => x.Tutor1.Id);
                    notifications.AddRange(
                        _notificationRepository.Filter(
                            x =>
                                x.NotificationType == NotificationType.Student && tutorIds.Contains(x.DestinationId) &&
                                !x.Approved));
                }
            }
            else
            {
                notifications = _notificationRepository
                .Filter(x => x.Approved == false).ToList();
            }
            var notificationsModel = notifications.OrderByDescending(i => i.CreationDate).Select(Mapper.Map<NotificationDisplayModel>);
            const int pageSize = 10;
            var pageNumber = (page ?? 1);
            return View("Approve", notificationsModel.ToPagedList(pageNumber,pageSize));
        }

        [HttpPost]
        [AuthorizeAdminDirector]
        public ActionResult Approve(long id)
        {
            try
            {
                var toApprove = _notificationRepository.GetById(id);
                if (toApprove != null)
                {
                    toApprove.Approved = true;
                    _notificationHandlerService.SendNotification(toApprove);
                    _notificationRepository.Update(toApprove);
                    _viewMessageLogic.SetNewMessage("Notificación Aprobada",
                        "La notificación fue aprobada exitosamente.",
                        ViewMessageType.SuccessMessage);
                }
            }
            catch
            {
                _viewMessageLogic.SetNewMessage("Error en aprobacion",
                    "La notificación no pudo ser aprobada correctamente, por favor intente nuevamente.",
                    ViewMessageType.ErrorMessage);
            }
            return RedirectToAction("Approve");
        }

        private NotificationSelectListsModel LoadEducationLevels(NotificationRegisterModel model, NotificationSelectListsModel toReturn)
        {
            var user =
                _userRepository.GetById(Convert.ToInt64(_sessionManagement.GetUserLoggedId()));
            var isDirector = user.Role.Name.Equals("Director");
            toReturn.EducationLevels = new SelectList(isDirector ? _areaReporsitory.Filter(x => x.Director != null && x.Director.Id == user.Id) : _areaReporsitory.GetAllAreas(), "Id", "Name");
            return toReturn;
        }

        private NotificationSelectListsModel LoadGrades(NotificationRegisterModel model, NotificationSelectListsModel toReturn)
        {
            var user =
                _userRepository.GetById(Convert.ToInt64(_sessionManagement.GetUserLoggedId()));
            var userRoleName = user.Role.Name;

             toReturn.Grades = new SelectList(GetGradesByRole(userRoleName, user), "Id", "Name");
                  
            return toReturn;
        }

        private IEnumerable<Grade> GetGradesByRole(string userRoleName, User user)
        {
            IEnumerable<Grade> grades = _gradeRepository.GetAllGrade();
            if (userRoleName.Equals("Director"))
            {
                grades = grades.Where(
                    x => x.EducationLevel.Director != null && x.EducationLevel.Director.Id == user.Id);
            }
            else if (userRoleName.Equals("Maestro"))
            {
                var gradesList = _academicGradeRepository.Filter(
                    x => x.SectionTeacher != null && x.SectionTeacher.User.Id == user.Id)
                    .Select(y => y.Grade).ToList();
                gradesList.AddRange(_academicCourseRepository.Filter(x => x.Teacher != null && x.Teacher.User.Id == user.Id)
                .Select(y => y.AcademicGrade.Grade)
                .ToList());

                grades = gradesList.Distinct();
            }
            return grades;
        }

        private List<AcademicGrade> GetAcademicGradesByTeacherCourse(User user)
        {
            return _academicCourseRepository.Filter(x => x.Teacher != null && x.Teacher.User.Id == user.Id)
                .Select(y => y.AcademicGrade)
                .ToList();
        }

        private List<AcademicGrade> GetAcademicGradesByDirectorCourse(User user)
        {
            return _academicCourseRepository.Filter(x => x.AcademicGrade.Grade.EducationLevel.Director != null && x.AcademicGrade.Grade.EducationLevel
            .Director.Id == user.Id)
                .Select(y => y.AcademicGrade)
                .ToList();
        }

        private List<AcademicGrade> GetAcademicGradesByAdmin()
        {
            return _academicCourseRepository.GetAllAcademicYearDetails().Select( x => x.AcademicGrade).ToList();
        }



        private NotificationSelectListsModel LoadAcademicGrades(NotificationRegisterModel model, NotificationSelectListsModel toReturn)
        {
            var list = new List<SelectListItem> { new SelectListItem { Value = "-1", Text = "N/A" } };
            var user =
                _userRepository.GetById(Convert.ToInt64(_sessionManagement.GetUserLoggedId()));

            var items = GetGradesByRole(user.Role.Name, user).ToList();
            toReturn.Grades = new SelectList(items, "Id", "Name");
            if (items.Any())
            {
                var first = items.First();
                List<AcademicGrade> sList = GetSectionByRole(first.Id, user);
               
                toReturn.AcademicGrades =
                    new SelectList(sList
                        , "Id", "Section");
            }
            else
            {
                toReturn.AcademicGrades = new SelectList(list, "Value", "Text");
            }
            return toReturn;
        }

        private List<AcademicGrade> GetSectionByRole(long gradeId, User user)
        {
            var roleName = user.Role.Name;
            if (roleName.Equals("Maestro"))
            {
               return  _academicGradeRepository
                    .Filter(x => x.Grade.Id == gradeId && x.SectionTeacher.User.Id == user.Id)
                    .ToList();
            }
            return _academicGradeRepository.Filter(x => x.Grade.Id == gradeId).ToList();
        }

      

        private NotificationSelectListsModel LoadAcademicCourses(NotificationRegisterModel model, NotificationSelectListsModel toReturn)
        {
            var list = new List<SelectListItem> { new SelectListItem { Value = "-1", Text = "N/A" } };
            var user =
                _userRepository.GetById(Convert.ToInt64(_sessionManagement.GetUserLoggedId()));
            var roleName = user.Role.Name;
            List<AcademicGrade> academicGrades;
            if (roleName.Equals("Director"))
            {
                academicGrades = GetAcademicGradesByDirectorCourse(user);
            }else if (roleName.Equals("Maestro"))
            {
                academicGrades = GetAcademicGradesByTeacherCourse(user);
            }
            else
            {
                academicGrades = GetAcademicGradesByAdmin();
            }
            var grades = academicGrades.Select(x => x.Grade).Distinct().ToList();
            toReturn.Grades = new SelectList(grades, "Id", "Name");
            if (academicGrades.Any())
            {
                var first = grades.First();
                var sList = academicGrades.Where(x => x.Grade.Id == first.Id).ToList();
                toReturn.AcademicGrades =
                    new SelectList(
                        sList, "Id", "Section");
                if (sList.Any())
                {
                    List<AcademicCourse> sList2;
                    var first2 = sList.First();
                    if (roleName.Equals("Maestro"))
                    {
                        sList2 = _academicCourseRepository.Filter(
                      x => x.AcademicGrade.Id == first2.Id && x.Teacher != null && x.Teacher.User.Id == user.Id).ToList();
                    }
                    else
                    {
                        sList2 = _academicCourseRepository.Filter(
                      x => x.AcademicGrade.Id == first2.Id).ToList();
                    }
                  
                    toReturn.AcademicCourses = new SelectList(
                        sList2, "Id", "Course.Name");
                }
                else
                {
                    toReturn.AcademicCourses = new SelectList(list, "Value", "Text");
                }
            }
            else
            {
                toReturn.AcademicGrades = new SelectList(list, "Value", "Text");
            }
            return toReturn;
        }

        private NotificationSelectListsModel LoadStudents(NotificationRegisterModel model, NotificationSelectListsModel toReturn)
        {
            var list = new List<SelectListItem> { new SelectListItem { Value = "-1", Text = "N/A" } };
            var user =
                _userRepository.GetById(Convert.ToInt64(_sessionManagement.GetUserLoggedId()));
            var roleName = user.Role.Name;
           
            var items = GetGradesByRole(roleName, user).ToList();
            toReturn.Grades = new SelectList(items, "Id", "Name");
            if (items.Any())
            {
                var first = items.First();
                List<AcademicGrade> sList;
                if (roleName.Equals("Maestro"))
                {

                    sList  =_academicGradeRepository
                             .Filter(x => x.Grade.Id == first.Id && x.SectionTeacher.User.Id == user.Id)
                             .ToList();
                    sList.AddRange( _academicCourseRepository.Filter( x => x.Teacher != null && x.Teacher.User.Id == user.Id).Select( y => y.AcademicGrade).ToList());
                 
                }
                else
                {
                    sList = _academicGradeRepository.Filter(
                  x => x.Grade.Id == first.Id).ToList();
                }
             
                toReturn.AcademicGrades =
                    new SelectList(
                        sList.DistinctBy(x => x.Section), "Id", "Section");
                if (sList.Any())
                {
                    var first2 = sList.First();
                    var sList2 = _studentRepository.Filter(
                        x => x.MyGrade.Id == first2.Id).ToList();
                    toReturn.Personals = new SelectList(
                        sList2, "Id", "FullName");
                }
                else
                {
                    toReturn.Personals = new SelectList(list, "Value", "Text");
                }
            }
            else
            {
                toReturn.AcademicGrades = new SelectList(list, "Value", "Text");
            }
            return toReturn;
        }

        private NotificationSelectListsModel LoadAcademicGradesFromList1(NotificationRegisterModel model, NotificationSelectListsModel toReturn)
        {
            var user =
               _userRepository.GetById(Convert.ToInt64(_sessionManagement.GetUserLoggedId()));
            var roleName = user.Role.Name;
            List<AcademicGrade> sList;
            if (roleName.Equals("Maestro"))
            {
                sList =
                    _academicGradeRepository.Filter(x => x.SectionTeacher != null && x.SectionTeacher.User.Id == user.Id && x.Grade.Id == model.Id1)
                        .ToList();
            }
            else
            {
                sList  = _academicGradeRepository.Filter(
                    x => x.Grade.Id == model.Id1).ToList();
            }
          
            toReturn.AcademicGrades =
                new SelectList(
                    sList, "Id", "Section");
            return toReturn;
        }

        private NotificationSelectListsModel LoadAcademicCoursesFromList1(NotificationRegisterModel model, NotificationSelectListsModel toReturn)
        {
            var list = new List<SelectListItem> { new SelectListItem { Value = "-1", Text = "N/A" } };
            var user =
               _userRepository.GetById(Convert.ToInt64(_sessionManagement.GetUserLoggedId()));
            List<AcademicGrade> sList;
            var roleName = user.Role.Name;
            if (roleName.Equals("Maestro"))
            {
                sList =
                    _academicCourseRepository.Filter(
                        x => x.Teacher != null && x.Teacher.User.Id == user.Id && x.AcademicGrade.Grade.Id == model.Id1)
                        .Select(y => y.AcademicGrade).ToList();
            }
            else
            {
                sList = _academicGradeRepository.Filter(
                   x => x.Grade.Id == model.Id1).ToList();
            }            
            
            toReturn.AcademicGrades =
                new SelectList(
                    sList, "Id", "Section");
            if (sList.Any())
            {
                var first2 = sList.First();
                List<AcademicCourse> sList2;
                if (roleName.Equals("Maestro"))
                {
                    sList2 = _academicCourseRepository
                        .Filter( x => x.AcademicGrade.Id == first2.Id && x.Teacher != null && x.Teacher.User.Id == user.Id)
                        .ToList();
                }
                else
                {
                    sList2 = _academicCourseRepository.Filter(
                     x => x.AcademicGrade.Id == first2.Id).ToList();
                }
                
                toReturn.AcademicCourses = new SelectList(
                    sList2, "Id", "Course.Name");
            }
            else
            {
                toReturn.AcademicCourses = new SelectList(list, "Value", "Text");
            }
            return toReturn;
        }

        private NotificationSelectListsModel LoadStudentsFromList1(NotificationRegisterModel model, NotificationSelectListsModel toReturn)
        {
            var list = new List<SelectListItem> { new SelectListItem { Value = "-1", Text = "N/A" } };
            var user =
               _userRepository.GetById(Convert.ToInt64(_sessionManagement.GetUserLoggedId()));
            var roleName = user.Role.Name;
            List<AcademicGrade> sList;
            if (roleName.Equals("Maestro"))
            {

                sList = _academicGradeRepository
                         .Filter(x => x.Grade.Id == model.Id1 && x.SectionTeacher.User.Id == user.Id)
                         .ToList();
                sList.AddRange(_academicCourseRepository.Filter(x => x.Teacher != null && x.Teacher.User.Id == user.Id).Select(y => y.AcademicGrade).ToList());

            }
            else
            {
                sList = _academicGradeRepository.Filter(
              x => x.Grade.Id == model.Id1).ToList();
            }
           
            toReturn.AcademicGrades =
                new SelectList(
                    sList.DistinctBy(x => x.Section), "Id", "Section");
            if (sList.Any())
            {
                var first2 = sList.First();
                var sList2 = _studentRepository.Filter(
                    x => x.MyGrade.Id == first2.Id).ToList();
                toReturn.Personals = new SelectList(
                    sList2, "Id", "FullName");
            }
            else
            {
                toReturn.Personals = new SelectList(list, "Value", "Text");
            }
            return toReturn;
        }

        private NotificationSelectListsModel LoadAcademicCoursesFromList2(NotificationRegisterModel model, NotificationSelectListsModel toReturn)
        {
            var sList = _academicCourseRepository.Filter(
                    x => x.AcademicGrade.Id == model.Id2).ToList();
            toReturn.AcademicCourses =
                new SelectList(
                    sList, "Id", "Course.Name");
            return toReturn;
        }

        private NotificationSelectListsModel LoadStudentsFromList2(NotificationRegisterModel model, NotificationSelectListsModel toReturn)
        {
            var sList = _studentRepository.Filter(
                    x => x.MyGrade.Id == model.Id2).ToList();
            toReturn.Personals =
                new SelectList(
                    sList, "Id", "FullName");
            return toReturn;
        }

        public JsonResult LoadFromNotificationTypeList(NotificationRegisterModel registerModel)
        {
            var list = new List<SelectListItem> {new SelectListItem {Value = "-1", Text = "N/A"}};
            var selectListsModel = new NotificationSelectListsModel
            {
                EducationLevels = new SelectList(list, "Value", "Text"),
                Grades = new SelectList(list, "Value", "Text"),
                AcademicGrades = new SelectList(list, "Value", "Text"),
                AcademicCourses = new SelectList(list, "Value", "Text"),
                Personals = new SelectList(list, "Value", "Text")
            };
            var dict = new Dictionary<NotificationType, Func<NotificationRegisterModel, NotificationSelectListsModel, NotificationSelectListsModel>>
            {
                {NotificationType.General, LoadEducationLevels},
                {NotificationType.EducationLevel, LoadEducationLevels},
                {NotificationType.Grade, LoadGrades},
                {NotificationType.Section, LoadAcademicGrades},
                {NotificationType.Course, LoadAcademicCourses},
                {NotificationType.Student, LoadStudents}
            };
            selectListsModel = dict[registerModel.NotificationType](registerModel, selectListsModel);
            return Json(selectListsModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadFromList1(NotificationRegisterModel registerModel)
        {
            var list = new List<SelectListItem> { new SelectListItem { Value = "-1", Text = "N/A" } };
            var selectListsModel = new NotificationSelectListsModel
            {
                EducationLevels = new SelectList(list, "Value", "Text"),
                Grades = new SelectList(list, "Value", "Text"),
                AcademicGrades = new SelectList(list, "Value", "Text"),
                AcademicCourses = new SelectList(list, "Value", "Text"),
                Personals = new SelectList(list, "Value", "Text")
            };
            var dict = new Dictionary<NotificationType, Func<NotificationRegisterModel, NotificationSelectListsModel, NotificationSelectListsModel>>
            {
                {NotificationType.Section, LoadAcademicGradesFromList1},
                {NotificationType.Course, LoadAcademicCoursesFromList1},
                {NotificationType.Student, LoadStudentsFromList1}
            };
            selectListsModel = dict[registerModel.NotificationType](registerModel, selectListsModel);
            return Json(selectListsModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadFromList2(NotificationRegisterModel registerModel)
        {
            var list = new List<SelectListItem> { new SelectListItem { Value = "-1", Text = "N/A" } };
            var selectListsModel = new NotificationSelectListsModel
            {
                EducationLevels = new SelectList(list, "Value", "Text"),
                Grades = new SelectList(list, "Value", "Text"),
                AcademicGrades = new SelectList(list, "Value", "Text"),
                AcademicCourses = new SelectList(list, "Value", "Text"),
                Personals = new SelectList(list, "Value", "Text")
            };
            var dict = new Dictionary<NotificationType, Func<NotificationRegisterModel, NotificationSelectListsModel, NotificationSelectListsModel>>
            {
                {NotificationType.Course, LoadAcademicCoursesFromList2},
                {NotificationType.Student, LoadStudentsFromList2}
            };
            selectListsModel = dict[registerModel.NotificationType](registerModel, selectListsModel);
            return Json(selectListsModel, JsonRequestBehavior.AllowGet);
        }
    }
}