using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Mhotivo.Data.Entities;
using Mhotivo.Interface.Interfaces;
using Mhotivo.ParentSite.Authorization;
using Mhotivo.ParentSite.Models;
using Microsoft.Ajax.Utilities;

namespace Mhotivo.ParentSite.Controllers
{
    public class HomeworkController : Controller
    {
        //Bunch of unused repositories. Delete?
        public static IStudentRepository StudentRepository;
        public static ISecurityService SecurityService;
        public static List<long> StudentsId;

        public HomeworkController(IHomeworkRepository homeworkRepository,
            IAcademicCourseRepository academicCourseRepository, IAcademicYearRepository academicYearRepository, ICourseRepository courseRepository, IStudentRepository studentRepository,
            ISecurityService securityService, ITutorRepository tutorRepository)
        {
            StudentRepository = studentRepository;
            SecurityService = securityService;
        }
        [AuthorizeNewUser]
        public ActionResult Index(long student = -1)
        {
            var students = GetAllStudents(GetTutorId());
            var homeworks = new List<Homework>();
            if (student == -1)
            {
                foreach (var academicCourse in students.Where(student1 => student1.MyGrade != null).SelectMany(student1 => student1.MyGrade.CoursesDetails.ToList()))
                {
                    homeworks.AddRange(academicCourse.Homeworks);
                }
                homeworks = homeworks.Distinct().ToList();
            }
            else
            {
                var student1 = students.FirstOrDefault(x => x.Id == student);
                if (student1 != null && student1.MyGrade != null)
                {
                    foreach (var academicCourse in student1.MyGrade.CoursesDetails.Select(courses => courses))
                    {
                        homeworks.AddRange(academicCourse.Homeworks);
                    }
                }
            }
            var model = new HomeworksModel();
            foreach (var homework in homeworks)
            {
                if (homework.DeliverDate.Date > DateTime.UtcNow.Date)
                {
                    model.FutureHomeworks.Add(Mapper.Map<HomeworkModel>(homework));
                }
                else if (homework.DeliverDate.Date == DateTime.UtcNow.Date)
                {
                    model.CurrentHomeworks.Add(Mapper.Map<HomeworkModel>(homework));
                }
                else
                {
                    model.PastHomeworks.Add(Mapper.Map<HomeworkModel>(homework));
                }
            }
            return View(model);
        }

        private static List<long> GetAllStudentsId(IEnumerable<Student> students)
        {
            var studentsId = new List<long>();
            var enumerable = students as Student[] ?? students.ToArray();
            for (int i = 0; i < enumerable.Count(); i++)
            {
                studentsId.Add(enumerable.ElementAt(i).Id);
            }
            return studentsId;
        }

        public static IEnumerable<Student> GetAllStudents(long tutorId)
        {
            IEnumerable<Student> allStudents =
                StudentRepository.GetAllStudents().Where(x => x.Tutor1.Id.Equals(tutorId));
            return allStudents;
        }


        public static List<string> GetStudentName(long academicyearId)
        {
            //var enroll = GetEnrollsbyAcademicYear(academicyearId);
            //return enroll.Select(e => e.Student.FirstName).ToList();
            return null;
        }

        public static string GetStudenById(long studentId)
        {
            return StudentRepository.GetById(studentId).FirstName;
        }

        public static long GetTutorId()
        {
            var people = SecurityService.GetUserLoggedPeoples();
            long id = 0;
            foreach (var p in people)
            {
                if (p is Tutor)
                    id = p.Id;
            }
            return id;
        }
    }
}