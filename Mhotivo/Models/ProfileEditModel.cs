using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Mhotivo.Models
{
    public class ProfileEditModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Debe ingresar el nombre completo")]
        [Display(Name = "Nombre Completo")]
        public string FullName { get; set; }

        public Byte[] Photo { get; set; }

        [Required(ErrorMessage = "Debe ingresar una descripcion")]
        [Display(Name = "Descripción")]
        [AllowHtml]
        public string Description { get; set; }

        [Display(Name = "Foto Perfil")]
        public HttpPostedFileBase FilePicture { get; set; }
    }
}