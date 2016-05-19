using System.ComponentModel.DataAnnotations;

namespace Mhotivo.Models
{
    public class PersonalMessageDisplayModel
    {
        public long Id { get; set; }

        [Display(Name = "Asunto")]
        public string Title { get; set; }

        [Display(Name = "Mensaje")]
        public string Message { get; set; }

        [Display(Name = "Remitente")]
        public string NotificationCreator { get; set; }

        [Display(Name = "Receptor")]
        public string Receiver { get; set; }

        [Display(Name = "Fecha de Creacion")]
        public string CreationDate { get; set; }
        
    }
}