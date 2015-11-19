﻿ using System;
using System.ComponentModel.DataAnnotations;

namespace Mhotivo.Models
{
    public class AcademicCourseDisplayModel
    {
        public long Id { get; set; }

        [Display(Name = "Horario")]
        public string Schedule { get; set; }

        [Display(Name = "Curso")]
        public string Course { get; set; }

        [Display(Name = "Maestro/a")]
        public string Teacher { get; set; }
    }

    public class AcademicCourseRegisterModel
    {
        [Required(ErrorMessage = "Debe Ingresar un Horario")]
        [Display(Name = "Horario")]
        public string Schedule { get; set; }

        [Required(ErrorMessage = "Debe Ingresar un Curso")]
        [Display(Name = "Curso")]
        public long Course { get; set; }

        [Required(ErrorMessage = "Debe Ingresar una Maestro")]
        [Display(Name = "Maestro")]
        public long Teacher { get; set; }

        [Display(Name = "Id Año Academico")]
        public long AcademicGrade { get; set; }
    }

    public class AcademicCourseEditModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Debe Ingresar un Horario")]
        [Display(Name = "Horario")]
        public TimeSpan Schedule { get; set; }

        [Required(ErrorMessage = "Debe Ingresar un Curso")]
        [Display(Name = "Curso")]
        public long Course { get; set; }

        [Required(ErrorMessage = "Debe Ingresar una Maestro")]
        [Display(Name = "Maestro/a")]
        public long Teacher { get; set; }
    }
}