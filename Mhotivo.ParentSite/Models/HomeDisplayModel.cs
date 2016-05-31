using System.Collections.Generic;

namespace Mhotivo.ParentSite.Models
{
    public class HomeDisplayModel
    {
        public IEnumerable<ProfileDisplayModel> ProfileDisplayModels { get; set; }
        public IEnumerable<EventDisplayModel> EventDisplayModels { get; set; }
    }
}