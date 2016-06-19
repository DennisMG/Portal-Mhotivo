using System.ComponentModel.DataAnnotations;

namespace Mhotivo.Models
{
    public class LostPasswordModel
    {
        [Required(ErrorMessage = "Debe Ingresar su correo electrónico")]
        [Display(Name = "Correo Electrónico ")]
        public string Email { get; set; }
    }
}