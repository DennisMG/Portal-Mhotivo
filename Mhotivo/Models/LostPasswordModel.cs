using System.ComponentModel.DataAnnotations;

namespace Mhotivo.Models
{
    public class LostPasswordModel
    {
        [Required(ErrorMessage = "Debe Ingresar su correo electr�nico")]
        [Display(Name = "Correo Electr�nico ")]
        public string Email { get; set; }
    }
}