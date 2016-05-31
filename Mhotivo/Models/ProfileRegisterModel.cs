using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Mhotivo.Models
{
    public class ProfileRegisterModel
    {
        [Required(ErrorMessage = "Debe Ingresar una descripción")]
        [AllowHtml]
        public string Description { get; set; }


        [Required(ErrorMessage = "Debe Ingresar un nombre")]
        public string FullName { get; set; }

     
        [Display(Name = "Foto Perfil")]
        public byte[] Photo { get; set; }

        [DataType(DataType.Upload)]
        public HttpPostedFileBase UploadPhoto { get; set; }
    }
}