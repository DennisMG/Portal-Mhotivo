using System.Web.Mvc;

namespace Mhotivo.Models
{
    public class EventDisplayModel
    {
        public string Title { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        public string Place { get; set; }
        public string EventDate { get; set; }
        public string ScheduleTime { get; set; }
        public long Id { get; set; }
        public byte[] Photo { get; set; }
    }
}