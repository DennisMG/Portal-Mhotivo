using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhotivo.Data.Entities
{
    public class PersonalMessage
    {
        public PersonalMessage()
        {
            CreationDate = DateTime.Now;
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public virtual User UserSender { get; set; }
        public virtual User RecipientUser { get; set; }
        public DateTime CreationDate { get; set; }
        
    }
}
