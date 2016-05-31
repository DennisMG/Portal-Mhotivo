using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Mhotivo.Models
{
    public class EventRegisterModel
    {
        [Required(ErrorMessage = "Debe Ingresar una descripción")]
        [AllowHtml]
        public string Description { get; set; }

        [Required(ErrorMessage = "Debe Ingresar Titulo")]
        [Display(Name = "Titulo")]
        public string Title { get; set; }

        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }
        [DataType(DataType.Time)]
        public TimeSpan FinishTime { get; set; }

        public string Place { get; set; }

        [Display(Name = "Foto Perfil")]
        public byte[] Photo { get; set; }

        [DataType(DataType.Upload)]
        public HttpPostedFileBase UploadPhoto { get; set; }
    }
}