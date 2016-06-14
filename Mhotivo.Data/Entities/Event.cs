using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mhotivo.Data.Entities
{
    public class Event
    {
        public Event()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Title { get; set; }
        public Byte[] Photo { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan FinishTime { get; set; }
        public string Place { get; set; }
        public string Description { get; set; }
    }
}