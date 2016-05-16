using System;
using AutoMapper;
using Mhotivo.Data.Entities;
using Mhotivo.Interface.Interfaces;
using Mhotivo.Logic.ViewMessage;
using Mhotivo.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Mhotivo.App_Data;
using Mhotivo.Authorizations;
using PagedList;

namespace Mhotivo.Controllers
{
    public class HomeworkController : Controller
    {

        private readonly IAcademicCourseRepository _academicCourseRepository;
        private readonly ISessionManagementService _sessionManagement;
        private readonly IHomeworkRepository _homeworkRepository;
        private readonly ViewMessageLogic _viewMessageLogic;
        public long TeacherId = -1;

        public HomeworkController(IHomeworkRepository homeworkRepository,
            IAcademicCourseRepository academicCourseRepository, ISessionManagementService sessionManagement)
        {
            _homeworkRepository = homeworkRepository;
            _viewMessageLogic = new ViewMessageLogic(this);
            _academicCourseRepository = academicCourseRepository;
            _sessionManagement = sessionManagement;
        }

        [AuthorizeTeacher]
        public ActionResult Index(int ?page)
        {
            _viewMessageLogic.SetViewMessageIfExist();
            TeacherId = GetTeacherId();
            var allAcademicYearsDetails = GetAllAcademicYearsDetail(TeacherId).Select( x => x.Id);

            IEnumerable<Homework> allHomeworks = _homeworkRepository.GetAllHomeworks().Where(x => allAcademicYearsDetails.Any( y =>  x.AcademicCourse != null && y == x.AcademicCourse.Id));
           
            
            IEnumerable<HomeworkDisplayModel> allHomeworkDisplaysModel =
                allHomeworks.Select(Mapper.Map<Homework, HomeworkDisplayModel>).ToList();
            const int pageSize = 10;
            var pageNumber = (page ?? 1);
            return View(allHomeworkDisplaysModel.ToPagedList(pageNumber, pageSize));
        }

        private long GetTeacherId()
        {
            return Convert.ToInt64(_sessionManagement.GetUserLoggedId());
        }

        // GET: /Homework/Create
        [AuthorizeTeacher]
        public ActionResult Create()
        {
            var teacherId = GetTeacherId();
            var courses =
                _academicCourseRepository.Filter(
                    x => x.AcademicGrade.AcademicYear.IsActive && x.Teacher != null && x.Teacher.Id == teacherId)
                    .Select(x => x.Course);
            ViewBag.course = new SelectList(courses, "Id", "Name");
            ViewBag.Years = DateTimeController.GetYears();
            ViewBag.Months = DateTimeController.GetMonths();
            ViewBag.Days = DateTimeController.GetDaysForMonthAndYearStatic(1, DateTime.UtcNow.Year);
            var modelRegister = new HomeworkRegisterModel { Year = ((KeyValuePair<int, int>)((SelectList)ViewBag.Years).SelectedValue).Value };
            return View(modelRegister);
        }

        private IEnumerable<AcademicCourse> GetAllAcademicYearsDetail(long id)
        {
            IEnumerable<AcademicCourse> allAcademicYearsDetail =
                _academicCourseRepository.Filter(x => x.Teacher != null && x.Teacher.User.Id == id);
            return allAcademicYearsDetail;
        }

        [HttpPost]
        [AuthorizeTeacher]
        public ActionResult Create(HomeworkRegisterModel registerModelHomework)
        {
         
            var toCreate = Mapper.Map<Homework>(registerModelHomework);
            var teacherId = GetTeacherId();
            toCreate.AcademicCourse = _academicCourseRepository.Filter(x => x.Teacher != null && x.Teacher.User.Id == teacherId && x.Course.Id == registerModelHomework.Course).FirstOrDefault();
            _homeworkRepository.Create(toCreate);
            const string title = "Tarea agregada";
            string content = "La tarea " + toCreate.Title + " ha sido agregado exitosamente.";
            _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
            return RedirectToAction("Index");
        }

        // GET: /Homework/Edit/5
        [AuthorizeTeacher]
        public ActionResult Edit(long id)
        {
            Homework thisHomework = _homeworkRepository.GetById(id);
            var homework = Mapper.Map<HomeworkEditModel>(thisHomework);
            var teacherId = GetTeacherId();
            var detalleAnhosAcademicosActivos = _academicCourseRepository.GetAllAcademicYearDetails().ToList().FindAll(x => x.AcademicGrade.AcademicYear.IsActive);
            var detallesFilteredByTeacher = detalleAnhosAcademicosActivos.FindAll(x => x.Teacher != null && x.Teacher.Id == teacherId);
            var query = detallesFilteredByTeacher.Select(detail => detail.Course).ToList();
            ViewBag.course = new SelectList(query, "Id", "Name");
            ViewBag.Years = DateTimeController.GetYears();
            ViewBag.Months = DateTimeController.GetMonths();
            ViewBag.Days = DateTimeController.GetDaysForMonthAndYearStatic(1, DateTime.UtcNow.Year);
            return View("Edit", homework);
        }

        // POST: /Homework/Edit/5
        [HttpPost]
        [AuthorizeTeacher]
        public ActionResult Edit(HomeworkEditModel modelHomework)
        {
            Homework myStudent = _homeworkRepository.GetById(modelHomework.Id);
            Mapper.Map(modelHomework, myStudent);
            _homeworkRepository.Update(myStudent);
            const string title = "Tarea Actualizada";
            var content = "La tarea " + modelHomework.Title + " ha sido actualizada exitosamente.";
            _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
            return RedirectToAction("Index");
        }

        // GET: /Homework/Delete/5
        [AuthorizeTeacher]
        public ActionResult Delete(long id)
        {
            Homework homework = _homeworkRepository.Delete(id);
            const string title = "Tarea Eliminada";
            string content = "La tarea: " + homework.Title + " ha sido eliminada exitosamente.";
            _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
            return RedirectToAction("Index");
        }

        // POST: /Homework/Delete/5
        [HttpPost]
        [AuthorizeTeacher]
        public ActionResult Delete(long id, FormCollection collection)
        {
            var homework = _homeworkRepository.Delete(id);
            const string title = "Tarea Eliminada";
            string content = "La tarea: " + homework.Title + " ha sido eliminada exitosamente.";
            _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
            return RedirectToAction("Index");
        }
    }
}