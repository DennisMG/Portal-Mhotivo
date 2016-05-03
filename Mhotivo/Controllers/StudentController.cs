﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;
using System.Web.WebPages;
using Mhotivo.Implement.Utils;
using Mhotivo.Interface.Interfaces;
using Mhotivo.Data.Entities;
using Mhotivo.Logic.ViewMessage;
using Mhotivo.Models;
using AutoMapper;
using Mhotivo.Authorizations;
using PagedList;

namespace Mhotivo.Controllers
{
    public class StudentController : Controller
    {
        private IAcademicGradeRepository _academicGradeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISessionManagementService _sessionManagementService;
        private readonly IContactInformationRepository _contactInformationRepository;
        private readonly ITutorRepository _tutorRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ViewMessageLogic _viewMessageLogic;

        public StudentController(IStudentRepository studentRepository, ITutorRepository tutorRepository,
            IContactInformationRepository contactInformationRepository, IAcademicGradeRepository academicGradeRepository, IUserRepository userRepository, ISessionManagementService sessionManagementService)
        {
            _academicGradeRepository = academicGradeRepository;
            _userRepository = userRepository;
            _sessionManagementService = sessionManagementService;
            _studentRepository = studentRepository;
            _tutorRepository = tutorRepository;
            _contactInformationRepository = contactInformationRepository;
            _viewMessageLogic = new ViewMessageLogic(this);
        }

        [AuthorizeAdminDirector]
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            _viewMessageLogic.SetViewMessageIfExist();
            var allStudents = _studentRepository.GetAllStudents();
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            if (!String.IsNullOrWhiteSpace(searchString))
            {
                allStudents = _studentRepository.Filter(x => x.FullName.Contains(searchString) || x.AccountNumber.Contains(searchString)).ToList();
            }
            var allStudentDisplaysModel = allStudents.Select(Mapper.Map<Student, StudentDisplayModel>).ToList();
            ViewBag.CurrentFilter = searchString;
            switch (sortOrder)
            {
                case "name_desc":
                    allStudentDisplaysModel = allStudentDisplaysModel.OrderByDescending(s => s.FullName).ToList();
                    break;
                default:  // Name ascending 
                    allStudentDisplaysModel = allStudentDisplaysModel.OrderBy(s => s.FullName).ToList();
                    break;
            }
            const int pageSize = 10;
            var pageNumber = (page ?? 1);
            return View(allStudentDisplaysModel.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        [AuthorizeAdminDirector]
        public ActionResult ContactEdit(long id)
        {
            var thisContactInformation = _contactInformationRepository.GetById(id);
            var contactInformation = Mapper.Map<ContactInformationEditModel>(thisContactInformation);
            contactInformation.Controller = "Student";
            return View("ContactEdit", contactInformation);
        }

        [HttpGet]
        [AuthorizeAdminDirector]
        public ActionResult Edit(long id)
        {
            var student = _studentRepository.GetById(id);
            var studentModel = Mapper.Map<Student, StudentEditModel>(student);
            ViewBag.Tutor1Id = new SelectList(_tutorRepository.Query(x => x).OrderBy(x => x.FullName), "Id", "FullName",
                studentModel.Tutor1);
            var items = ((Gender[])Enum.GetValues(typeof(Gender))).Select(c => new SelectListItem
            {
                Text = c.GetEnumDescription(),
                Value = c.ToString("D")
            }).ToList();

            ViewBag.Genders = new List<SelectListItem>(items);
            ViewBag.Years = DateTimeController.GetYears();
            ViewBag.Months = DateTimeController.GetMonths();
            ViewBag.Days = DateTimeController.GetDaysForMonthAndYearStatic(1, DateTime.UtcNow.Year);
            return View("Edit", studentModel);
        }

        [HttpPost]
        [AuthorizeAdminDirector]
        public ActionResult Edit(StudentEditModel modelStudent)
        {
            var validImageTypes = new []
            {
                "image/gif",
                "image/jpeg",
                "image/pjpeg",
                "image/png"
            };
            if (modelStudent.FilePicture != null && modelStudent.FilePicture.ContentLength > 0)
            {
                if (!validImageTypes.Contains(modelStudent.FilePicture.ContentType))
                {
                    ModelState.AddModelError("FilePicture", "Por favor seleccione entre una imagen GIF, JPG o PNG");
                }
            }
            if (ModelState.IsValid)
            {
                if (
                    _studentRepository.Filter(x => (x.IdNumber.Equals(modelStudent.IdNumber) || x.AccountNumber.Equals(modelStudent.AccountNumber)) && x.Id != modelStudent.Id)
                        .Any())
                {
                    const string title = "Error!";
                    const string content = "Ya existe un estudiante con ese número de identidad o de cuenta.";
                    _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.ErrorMessage);
                    return RedirectToAction("Index");
                }
                try
                {
                    if (modelStudent.FilePicture != null)
                    {
                        using (var binaryReader = new BinaryReader(modelStudent.FilePicture.InputStream))
                        {
                            modelStudent.Photo = binaryReader.ReadBytes(modelStudent.FilePicture.ContentLength);
                        }
                    }
                    var myStudent = _studentRepository.GetById(modelStudent.Id);
                    Mapper.Map(modelStudent, myStudent);
                    _studentRepository.Update(myStudent);
                    const string title = "Estudiante Actualizado";
                    var content = "El estudiante " + myStudent.FullName + " ha sido actualizado exitosamente.";
                    _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
                    return RedirectToAction("Index");
                }
                catch
                {
                    ViewBag.Tutor1Id = new SelectList(_tutorRepository.Query(x => x).OrderBy(x => x.FullName), "Id", "FullName",
                        modelStudent.Tutor1);
                    var items = ((Gender[])Enum.GetValues(typeof(Gender))).Select(c => new SelectListItem
                    {
                        Text = c.GetEnumDescription(),
                        Value = c.ToString("D")
                    }).ToList();

                    ViewBag.Genders = new List<SelectListItem>(items);
                    ViewBag.Years = DateTimeController.GetYears();
                    ViewBag.Months = DateTimeController.GetMonths();
                    ViewBag.Days = DateTimeController.GetDaysForMonthAndYearStatic(1, DateTime.UtcNow.Year);
                    return View(modelStudent);
                }
            }
            ViewBag.Tutor1Id = new SelectList(_tutorRepository.Query(x => x).OrderBy(x => x.FullName), "Id", "FullName",
                modelStudent.Tutor1);
            var items2 = ((Gender[])Enum.GetValues(typeof(Gender))).Select(c => new SelectListItem
            {
                Text = c.GetEnumDescription(),
                Value = c.ToString("D")
            }).ToList();

            ViewBag.Genders = new List<SelectListItem>(items2);
            ViewBag.Years = DateTimeController.GetYears();
            ViewBag.Months = DateTimeController.GetMonths();
            ViewBag.Days = DateTimeController.GetDaysForMonthAndYearStatic(1, DateTime.UtcNow.Year);
            return View(modelStudent);
        }

        [HttpPost]
        [AuthorizeAdminDirector]
        public ActionResult Delete(long id)
        {
            var stu = _studentRepository.GetById(id);
            if (stu.MyGrade != null)
            {
                const string title2 = "Error";
                const string content2 = "El estudiante esta actualmente matriculado y no puese ser eliminado.";
                _viewMessageLogic.SetNewMessage(title2, content2, ViewMessageType.ErrorMessage);
                return RedirectToAction("Index");
            }
            var student = _studentRepository.Delete(id);
            const string title = "Estudiante Eliminado";
            var content = "El estudiante " + student.FullName + " ha sido eliminado exitosamente.";
            _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [AuthorizeAdminDirector]
        public ActionResult ContactAdd(long id)
        {
            var model = new ContactInformationRegisterModel
            {
                People = id,
                Controller = "Student"
            };
            return View("ContactAdd", model);
        }

        [HttpGet]
        [AuthorizeAdminDirector]
        public ActionResult Add()
        {
            ViewBag.Tutor1Id = new SelectList(_tutorRepository.Query(x => x).OrderBy(x => x.FullName), "Id", "FullName");
            var items = ((Gender[])Enum.GetValues(typeof(Gender))).Select(c => new SelectListItem
            {
                Text = c.GetEnumDescription(),
                Value = c.ToString("D")
            }).ToList();

            ViewBag.Genders = new List<SelectListItem>(items);
            ViewBag.Years = DateTimeController.GetYears();
            ViewBag.Months = DateTimeController.GetMonths();
            ViewBag.Days = DateTimeController.GetDaysForMonthAndYearStatic(1, DateTime.UtcNow.Year);
            return View("Create", new StudentRegisterModel { Year = ((KeyValuePair<int, int>)((SelectList)ViewBag.Years).SelectedValue).Value });
        }

        [HttpPost]
        [AuthorizeAdminDirector]
        public ActionResult Add(StudentRegisterModel modelStudent)
        {
            var studentModel = Mapper.Map<StudentRegisterModel, Student>(modelStudent);
            if (
                _studentRepository.Filter(
                    x => x.AccountNumber.Equals(modelStudent.AccountNumber) || x.IdNumber.Equals(modelStudent.IdNumber))
                    .Any())
            {
                _viewMessageLogic.SetNewMessage("Dato Inválido", "Ya existe un estudiante con ese número de Identidad o de cuenta", ViewMessageType.ErrorMessage);
                return RedirectToAction("Index");
            }
            _studentRepository.Create(studentModel);
            const string title = "Estudiante Agregado";
            var content = "El estudiante " + studentModel.FullName + " ha sido agregado exitosamente.";
            _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [AuthorizeAdminDirector]
        public ActionResult Details(long id)
        {
            _viewMessageLogic.SetViewMessageIfExist();
            var student = _studentRepository.GetById(id);
            var studentModel = Mapper.Map<Student, StudentDisplayModel>(student);
            return View("Details", studentModel);
        }

        [HttpGet]
        [AuthorizeAdminDirector]
        public ActionResult StudentByGrade(string gradeSection, int? page, long gradeId)
        {
          
            _viewMessageLogic.SetViewMessageIfExist();
            ViewBag.gradeId = gradeId;
            ViewBag.gradeSection = gradeSection;
          
            var user = _userRepository.GetById(Convert.ToInt64(_sessionManagementService.GetUserLoggedId()));
            var isDirector = user.Role.Name.Equals("Director");
            var grades = isDirector
                ? _academicGradeRepository.Filter(
                    x =>
                        x.AcademicYear.IsActive && x.Grade.EducationLevel.Director != null &&
                        x.Grade.EducationLevel.Director.Id == user.Id).ToList()
                : _academicGradeRepository.Filter(x => x.AcademicYear.IsActive && x.Grade.Id == gradeId).ToList();

            var sections = grades.ToList().Select(grade => grade.Section).ToList();

            var listSelectedSections = new List<SelectListItem> { new SelectListItem { Selected = true, Value = "N/A", Text = "Sin Filtro" } };
            for (int i = 0; i < sections.Count; i++)
            {
                listSelectedSections.Add(new SelectListItem { Value = sections[i], Text = sections[i] });
            }
            ViewBag.Sections = listSelectedSections;
            if (gradeSection != null && !gradeSection.IsEmpty() && !gradeSection.Equals("N/A"))
            {
                grades = grades.FindAll( x => x.Section == gradeSection);
            }
            if (!grades.Any())
                return View();
            var model = new List<EnrollDisplayModel>();
            foreach (var academicGrade in grades)
            {
                model.AddRange(academicGrade.Students.Select(n => new EnrollDisplayModel
                {
                    AcademicGradeId = academicGrade.Id,
                    StudentId = n.Id,
                    FullName = n.FullName,
                    Photo = n.Photo,
                    MyGender = n.MyGender.ToString("G"),
                    AccountNumber = n.AccountNumber,
                    Grade = academicGrade.Grade.Name,
                    Section = academicGrade.Section
                }));
            }
            const int pageSize = 10;
            var pageNumber = (page ?? 1);
            return View(model.ToPagedList(pageNumber, pageSize));
        }

        
    }
}