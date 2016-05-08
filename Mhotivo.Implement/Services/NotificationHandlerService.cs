using System.Collections.Generic;
using System.Linq;
using Mhotivo.Data.Entities;
using Mhotivo.Interface.Interfaces;

namespace Mhotivo.Implement.Services
{
    public class NotificationHandlerService : INotificationHandlerService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IStudentRepository _suStudentRepository;
        private readonly IAcademicGradeRepository _academicGradeRepository;
        private readonly IAcademicCourseRepository _academicCourseRepository;
        private readonly IUserRepository _userRepository;

        public string message =
            ",<br><br>Se le notifica que ha recibido un nuevo mensaje en el portal, porfavor ingrese a:<br><br> http://padres.mhotivo.tk/Account/Login <br><br>Atentamente,<br><br>Fundación Mhotivo <br><p style=\"font-size:12px;line-height:16px;font-family:Helvetica,Arial,sans-serif;color:#999;\"> Por favor, NO responda a este mensaje, es un envío automático</p></html>";
        public NotificationHandlerService(INotificationRepository notificationRepository, IStudentRepository suStudentRepository, 
            IAcademicGradeRepository academicGradeRepository, IAcademicCourseRepository academicCourseRepository, IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _suStudentRepository = suStudentRepository;
            _academicGradeRepository = academicGradeRepository;
            _academicCourseRepository = academicCourseRepository;
            _userRepository = userRepository;
        }

        public List<Notification> GetAllActiveOfType(NotificationType type)
        {
            return _notificationRepository.Filter(x => x.NotificationType == type && x.AcademicYear.IsActive).ToList();
        }

        public List<Notification> GetAllOfTypeAndYear(NotificationType type, long yearId)
        {
            return _notificationRepository.Filter(x => x.NotificationType == type && x.AcademicYear.Id == yearId).ToList();
        }

        public void SendNotification(Notification notification)
        {
            if (notification.Sent)
                return;
            switch (notification.NotificationType)
            {
                case NotificationType.General:
                    var allGrades = _academicGradeRepository.Filter(x => x.AcademicYear.Id == notification.AcademicYear.Id).ToList();
                    foreach (var grade in allGrades)
                    {
                        SendToStudents(grade.Students, notification);
                    }
                    notification.Sent = true;
                    _notificationRepository.Update(notification);
                    break;
                case NotificationType.EducationLevel:
                    var gradesForLevel =
                        _academicGradeRepository.Filter(
                            x => x.Grade.EducationLevel.Id == notification.DestinationId &&
                                 x.AcademicYear.Id == notification.AcademicYear.Id).ToList();
                    foreach (var grade in gradesForLevel)
                    {
                        SendToStudents(grade.Students, notification);
                    }
                    notification.Sent = true;
                    _notificationRepository.Update(notification);
                    break;
                case NotificationType.Grade:
                    var grades =
                        _academicGradeRepository.Filter(x => x.Grade.Id == notification.DestinationId &&
                                                                 x.AcademicYear.Id == notification.AcademicYear.Id).ToList();
                    foreach (var grade in grades)
                    {
                        SendToStudents(grade.Students, notification);
                    }
                    notification.Sent = true;
                    _notificationRepository.Update(notification);
                    break;
                case NotificationType.Section:
                    var singleGrade =
                        _academicGradeRepository.Filter(x => x.Id == notification.DestinationId &&
                                                                 x.AcademicYear.Id == notification.AcademicYear.Id).FirstOrDefault();
                    if (singleGrade != null)
                    {
                        SendToStudents(singleGrade.Students, notification);
                        notification.Sent = true;
                        _notificationRepository.Update(notification);
                    }
                    break;
                case NotificationType.Course:
                    var course = _academicCourseRepository.Filter(x => x.Id == notification.DestinationId &&
                                                                           x.AcademicGrade.AcademicYear.Id ==
                                                                           notification.AcademicYear.Id).FirstOrDefault();
                    if (course != null)
                    {
                        SendToStudents(course.AcademicGrade.Students, notification);
                        notification.Sent = true;
                        _notificationRepository.Update(notification);
                    }
                    break;
                case NotificationType.Student:
                    var singleStudent = _suStudentRepository.Filter(x => x.Id == notification.DestinationId).FirstOrDefault();
                    if (singleStudent != null)
                    {
                        SendToStudent(singleStudent, notification);
                        notification.Sent = true;
                        _notificationRepository.Update(notification);
                    }
                    break;
            }
        }

        private void SendToStudent(Student student, Notification notification)
        {
            if (student.Tutor1 != null && !student.Tutor1.User.Notifications.Contains(notification))
            {
                var user = student.Tutor1.User;
                user.Notifications.Add(notification);
                notification.RecipientUsers.Add(user);
                _userRepository.Update(user);
                _notificationRepository.Update(notification);
                if (notification.SendEmail)
                    MailgunEmailService.SendEmailToUser(student.Tutor1.User, message);
            }
            if (student.Tutor2 != null && !student.Tutor2.User.Notifications.Contains(notification))
            {
                var user = student.Tutor2.User;
                user.Notifications.Add(notification);
                notification.RecipientUsers.Add(user);
                _userRepository.Update(user);
                _notificationRepository.Update(notification);
                if (notification.SendEmail)
                    MailgunEmailService.SendEmailToUser(student.Tutor2.User, message);
            }
        }

        private void SendToStudents(IEnumerable<Student> students, Notification notification)
        {
            foreach (var student in students.ToList())
            {
                SendToStudent(student, notification);
            }
        }

        public void SendAllPending()
        {
            var notifications =
                _notificationRepository.Filter(x => x.Approved && !x.Sent && x.AcademicYear.IsActive);
            foreach (var notification in notifications.ToList())
            {
                SendNotification(notification);
            }
        }
    }
}
