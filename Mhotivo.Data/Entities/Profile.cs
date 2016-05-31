using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhotivo.Data.Entities
{
    public class Profile
    {
        public Profile()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string FullName { get; set; }
        public Byte[] Photo { get; set; }
        public string Description { get; set; }
        
    }
}
