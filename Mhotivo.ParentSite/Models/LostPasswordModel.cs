using System.ComponentModel.DataAnnotations;

namespace Mhotivo.ParentSite.Models
{
    public class LostPasswordModel
    {
        [Display(Name = "Email de usuario")]
        [Required(ErrorMessage = "Debe Ingresar Email de Usuario")]
        public string Email { get; set; } 
    }
}