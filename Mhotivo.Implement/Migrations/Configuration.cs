using System;
using System.Collections.Generic;
using Mhotivo.Data.Entities;
using Mhotivo.Implement.Context;
using Mhotivo.Implement.Repositories;
using Mhotivo.Interface.Interfaces;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Mhotivo.Implement.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<MhotivoContext>
    {
        private IRoleRepository _roleRepository;
        private IUserRepository _userRepository;
        private ITeacherRepository _teacherRepository;
        private ITutorRepository _tutorRepository;
        private IPeopleWithUserRepository _peopleWithUserRepository;
        private IPrivilegeRepository _privilegeRepository;

        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(MhotivoContext context)
        {
            if (context.Users.Any())
                return;
            _roleRepository = new RoleRepository(context);
            _userRepository = new UserRepository(context);
            _teacherRepository = new TeacherRepository(context);
            _tutorRepository = new TutorRepository(context);
            _peopleWithUserRepository = new PeopleWithUserRepository(context);
            _privilegeRepository = new PrivilegeRepository(context);

            var allRoles = new List<Role>();
            
            var tRole = _roleRepository.Create(new Role { Name = "Administrador", Id = 0 });
            _privilegeRepository.Create(new Privilege { Id = 0, Description = "Privilegio de nivel Administrador", Name = "Administrador", Roles = new List<Role> { tRole } });
            allRoles.Add(tRole);

            tRole = _roleRepository.Create(new Role { Name = "Tutor", Id = 1 });
            _privilegeRepository.Create(new Privilege { Id = 1, Description = "Privilegio de nivel Padre", Name = "Padre", Roles = new List<Role> { tRole } });
            allRoles.Add(tRole);

            tRole = _roleRepository.Create(new Role { Name = "Maestro", Id = 2 });
            _privilegeRepository.Create(new Privilege { Id = 1, Description = "Privilegio de nivel Maestro", Name = "Maestro", Roles = new List<Role> { tRole } });
            allRoles.Add(tRole);

            tRole = _roleRepository.Create(new Role { Name = "Director", Id = 3 });
            _privilegeRepository.Create(new Privilege { Id = 1, Description = "Privilegio de nivel Director", Name = "Director", Roles = new List<Role> { tRole } });
            allRoles.Add(tRole);

            _privilegeRepository.Create(new Privilege { Id = 1, Description = "Privilegio de Login", Name = "Login", Roles = allRoles});

            var adminPeople = new PeopleWithUser
            {
                Address = "",
                BirthDate = DateTime.UtcNow,
                City = "",
                FirstName = "Rectora",
                IsActive = true,
                IdNumber = "0000-0000-00000",
                LastName = "",
                MyGender = Gender.Masculino,
                Photo = null,
                State = ""
            };
            adminPeople.FullName = adminPeople.FirstName + "" + adminPeople.LastName;
            adminPeople = _peopleWithUserRepository.Create(adminPeople);
            var admin = new User
            {
                Email = "admin@mhotivo.org",
                Password = "password",
                IsActive = true,
                UserOwner = adminPeople,
                Role = _roleRepository.Filter(x => x.Name == "Administrador").FirstOrDefault()
            };
            admin = _userRepository.Create(admin);
            adminPeople.User = admin;
            _peopleWithUserRepository.Update(adminPeople);
            DebuggingSeeder(); //MAKE SURE TO COMMENT THIS LINE BEFORE DEPLOYING.
        }

        private void DebuggingSeeder()
        {
            var generTeacher = new Teacher
            {
                Address = "Jardines del Valle, 4 calle, 1 etapa",
                BirthDate = new DateTime(1993, 3, 8),
                City = "San Pedro Sula",
                IsActive = true,
                FirstName = "Alejandro",
                LastName = "Diaz",
                FullName = "Alejandro Diaz",
                IdNumber = "0501-1993-08405",
                MyGender = Gender.Masculino,
                State = "Cortes",
            };
            generTeacher = _teacherRepository.Create(generTeacher);
            var genericTeacher = new User
            {
                UserOwner = generTeacher,
                Email = "ajdiaz@unitec.edu",
                Password = "password",
                IsActive = true,
                Name = "Alejandro Diaz",
                Role = _roleRepository.Filter(x => x.Name == "Maestro").FirstOrDefault(),
                IsUsingDefaultPassword = false
            };
            genericTeacher = _userRepository.Create(genericTeacher);
            generTeacher.User = genericTeacher;
           _teacherRepository.Update(generTeacher);
        
            
          
            var generTutor = new Tutor
            {
                IdNumber = "0501-1956-03145",
                FirstName = "Erick",
                LastName = "Caballero",
                FullName = "Erick Caballero",
                IsActive = true,
                MyGender = Gender.Femenino,
                BirthDate = new DateTime(1956, 11, 23),
                Parentage = Parentage.Mother,
                City = "San Pedro Sula",
                State = "Cortes",
                Address = "Jardines del Valle, 4 Calle, 1 Etapa, #9D",
            };
            generTutor = _tutorRepository.Create(generTutor);
            var genericTutor = new User
            {
                UserOwner = generTutor,
                Email = "erickdcb10@gmail.com",
                Password = "password",
                IsActive = true,
                Name =  "Erick",
                Role = _roleRepository.Filter(x => x.Name == "Tutor").FirstOrDefault()
            };
            genericTutor = _userRepository.Create(genericTutor);
            generTutor.User = genericTutor;
            _tutorRepository.Update(generTutor);

            var director = new PeopleWithUser
            {
                Address = "",
                BirthDate = DateTime.UtcNow,
                City = "",
                FirstName = "Director Generico",
                IsActive = true,
                IdNumber = "0000-0000-00000",
                LastName = "",
                MyGender = Gender.Masculino,
                Photo = null,
                State = "",
            };
            director.FullName = director.FirstName + "" + director.LastName;
            director = _peopleWithUserRepository.Create(director);
            var dir = new User
            {
                Email = "director@mhotivo.org",
                Password = "password",
                IsActive = true,
                UserOwner = director,
                Role = _roleRepository.Filter(x => x.Name == "Director").FirstOrDefault()
            };
            dir = _userRepository.Create(dir);
            director.User = dir;
            _peopleWithUserRepository.Update(director);
        }
    }
}
