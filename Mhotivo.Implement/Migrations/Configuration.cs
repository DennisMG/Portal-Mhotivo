using System;
using System.Collections.Generic;
using Mhotivo.Data.Entities;
using System.Data.Entity.Migrations;
using System.Linq;
using Mhotivo.Implement.Context;
using Mhotivo.Implement.Repositories;
using Mhotivo.Interface.Interfaces;

namespace Mhotivo.Implement.Migrations
{
    public class Configuration : DbMigrationsConfiguration<MhotivoContext>
    {
        private IPasswordGenerationService _passwordGenerationService;
        private IAreaRepository _areaRepository;
        private IGradeRepository _gradeRepository;
        private ICourseRepository _courseRepository;
        private IPensumRepository _pensumRepository;
        private IAcademicYearRepository _academicYearRepository;
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(MhotivoContext context)
        {
            _passwordGenerationService = new PreloadedPasswordsGenerationService(context);
            _areaRepository = new AreaRepository(context);
            _gradeRepository = new GradeRepository(context);
            _courseRepository = new CourseRepository(context, _areaRepository);
            _pensumRepository = new PensumRepository(context);
            _academicYearRepository = new AcademicYearRepository(context);
            context.Roles.AddOrUpdate(new Role { Description = "Administrador", Name = "Administrador" });
            context.Roles.AddOrUpdate(new Role { Description = "Padre", Name = "Padre" });
            context.Roles.AddOrUpdate(new Role { Description = "Maestro", Name = "Maestro" });
            context.SaveChanges();
            var admin = new User
            {
                DisplayName = "Administrador",
                Email = "admin@mhotivo.edu",
                Password = "password",
                IsActive = true,
                Roles = new List<Role> {context.Roles.First()}
            };
            admin.EncryptPassword();
            context.Users.AddOrUpdate(admin);
            context.SaveChanges();
            context.NotificationTypes.AddOrUpdate(new NotificationType { Id = 1, Description = "General" });
            context.NotificationTypes.AddOrUpdate(new NotificationType { Id = 2, Description = "Area" });
            context.NotificationTypes.AddOrUpdate(new NotificationType { Id = 3, Description = "Grado" });
            context.NotificationTypes.AddOrUpdate(new NotificationType { Id = 4, Description = "Personal" });
            context.SaveChanges();
            DebuggingSeeder(context);
            context.SaveChanges();
        }

        private void DebuggingSeeder(MhotivoContext context)
        {
            var passwordsList = new List<string>
            {
                "arbol",
                "madera",
                "republica",
                "imperio",
                "crucio",
                "excursion",
                "pitonisa",
                "ventilador",
                "oraculo",
                "sanguineo"
            };
            _passwordGenerationService.AddPasswordsToTable(passwordsList);
            _areaRepository.Create(new Area { Name = "Prescolar" });
            _areaRepository.Create(new Area { Name = "Primaria" });
            _areaRepository.Create(new Area { Name = "Secundaria" });
            _gradeRepository.Create(new Grade {EducationLevel = _areaRepository.GetById(1).Name, Name = "Kinder"});
            _gradeRepository.Create(new Grade { EducationLevel = _areaRepository.GetById(1).Name, Name = "Preparatoria" });
            _gradeRepository.Create(new Grade { EducationLevel = _areaRepository.GetById(2).Name, Name = "Primero" });
            _gradeRepository.Create(new Grade { EducationLevel = _areaRepository.GetById(2).Name, Name = "Segundo" });
            _gradeRepository.Create(new Grade { EducationLevel = _areaRepository.GetById(2).Name, Name = "Tercero" });
            _gradeRepository.Create(new Grade { EducationLevel = _areaRepository.GetById(2).Name, Name = "Cuarto" });
            _gradeRepository.Create(new Grade { EducationLevel = _areaRepository.GetById(2).Name, Name = "Quinto" });
            _gradeRepository.Create(new Grade { EducationLevel = _areaRepository.GetById(2).Name, Name = "Sexto" });
            _gradeRepository.Create(new Grade { EducationLevel = _areaRepository.GetById(3).Name, Name = "Septimo" });
            _gradeRepository.Create(new Grade { EducationLevel = _areaRepository.GetById(3).Name, Name = "Octavo" });
            _gradeRepository.Create(new Grade { EducationLevel = _areaRepository.GetById(3).Name, Name = "Noveno" });
            _gradeRepository.Create(new Grade { EducationLevel = _areaRepository.GetById(3).Name, Name = "Decimo" });
            _gradeRepository.Create(new Grade { EducationLevel = _areaRepository.GetById(3).Name, Name = "Onceavo" });
            _courseRepository.Create(new Course {Area = _areaRepository.GetById(1), Name = "English"});
            _courseRepository.Create(new Course { Area = _areaRepository.GetById(1), Name = "Math" });
            _courseRepository.Create(new Course { Area = _areaRepository.GetById(2), Name = "English" });
            _courseRepository.Create(new Course { Area = _areaRepository.GetById(2), Name = "Math" });
            _courseRepository.Create(new Course { Area = _areaRepository.GetById(2), Name = "Science" });
            _courseRepository.Create(new Course { Area = _areaRepository.GetById(2), Name = "Espaniol" });
            _courseRepository.Create(new Course { Area = _areaRepository.GetById(2), Name = "Estudios Sociales" });
            _courseRepository.Create(new Course { Area = _areaRepository.GetById(3), Name = "Algebra" });
            _courseRepository.Create(new Course { Area = _areaRepository.GetById(3), Name = "Geometry" });
            _courseRepository.Create(new Course { Area = _areaRepository.GetById(3), Name = "Physics" });
            _courseRepository.Create(new Course { Area = _areaRepository.GetById(3), Name = "Biology" });
            _courseRepository.Create(new Course { Area = _areaRepository.GetById(3), Name = "Physical Education" });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(1), Grade = _gradeRepository.GetById(1) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(1), Grade = _gradeRepository.GetById(2) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(2), Grade = _gradeRepository.GetById(2) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(2), Grade = _gradeRepository.GetById(1) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(3), Grade = _gradeRepository.GetById(3) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(3), Grade = _gradeRepository.GetById(4) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(4), Grade = _gradeRepository.GetById(5) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(5), Grade = _gradeRepository.GetById(6) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(6), Grade = _gradeRepository.GetById(7) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(7), Grade = _gradeRepository.GetById(8) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(8), Grade = _gradeRepository.GetById(9) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(12), Grade = _gradeRepository.GetById(10) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(9), Grade = _gradeRepository.GetById(11) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(10), Grade = _gradeRepository.GetById(12) });
            _pensumRepository.Create(new Pensum { Course = _courseRepository.GetById(11), Grade = _gradeRepository.GetById(13) });
            for (int i = 1; i <= 13; i++)
            {
                _academicYearRepository.Create(new AcademicYear {Approved = true, Grade = _gradeRepository.GetById(i), IsActive = true, Section = "A", Year = new DateTime(2015, 1, 1)});
                _academicYearRepository.Create(new AcademicYear { Approved = true, Grade = _gradeRepository.GetById(i), IsActive = true, Section = "B", Year = new DateTime(2015, 1, 1) });
                _academicYearRepository.Create(new AcademicYear { Approved = true, Grade = _gradeRepository.GetById(i), IsActive = true, Section = "C", Year = new DateTime(2015, 1, 1) });
            }
            var genericTeacher = new User
            {
                DisplayName = "Maestro Generico",
                Email = "teacher@mhotivo.edu",
                Password = "password",
                IsActive = true,
                Roles = new List<Role> { context.Roles.Find(3) }
            };
            genericTeacher.EncryptPassword();
            var genericParent = new User
            {
                DisplayName = "Padre Generico",
                Email = "padre@mhotivo.edu",
                Password = "password",
                IsActive = true,
                Roles = new List<Role> { context.Roles.Find(2) }
            };
            genericParent.EncryptPassword();
            context.Users.AddOrUpdate(genericTeacher);
            context.Users.AddOrUpdate(genericParent);
            context.SaveChanges();
            var maestroDefault = context.Teachers.FirstOrDefault(x => x.FullName == "Maestro Generico");
            if (maestroDefault == null)
            {
                context.Teachers.AddOrUpdate(new Teacher { IdNumber = "0000000000000", FirstName = "Maestro", LastName = "Generico", FullName = "Maestro Generico", Disable = false, Gender = true, MyUser = genericTeacher });
            }
            var padreDefault = context.Parents.FirstOrDefault(x => x.FullName == "Padre Generico");
            if (padreDefault == null)
            {
                context.Parents.AddOrUpdate(new Parent { IdNumber = "1234567890", FirstName = "Padre", LastName = "Generico", FullName = "Padre Generico", Disable = false, MyUser = genericParent});
            }
            context.SaveChanges();
        }
    }
}
