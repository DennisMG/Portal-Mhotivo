using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Mhotivo.Models
{
    public class SliderRegisterModel
    {
        [Display(Name = "Foto Perfil")]
        public byte[] Photo { get; set; }

        [DataType(DataType.Upload)]
        public HttpPostedFileBase UploadPhoto { get; set; }
    }
}