using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Mhotivo.Interface.Interfaces;
using Mhotivo.ParentSite.Models;

namespace Mhotivo.ParentSite.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ISecurityService _securityService;
        private readonly IProfileRepository _profileRepository;
        private readonly IEventRepository _eventRepository;

        public HomeController(ISecurityService securityService, IProfileRepository profileRepository, IEventRepository eventRepository)
        {
            _securityService = securityService;
            _profileRepository = profileRepository;
            _eventRepository = eventRepository;
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            var todayDate = DateTime.Now;
            var orderedEvents = _eventRepository.Filter( x => x.EventDate.CompareTo(todayDate) > 0 || x.EventDate.CompareTo(todayDate.Date) == 0)
                .OrderBy(x => x.EventDate).ToList();

            var amountEvents = orderedEvents.Count;
            
            if (amountEvents > 4)
            {
                orderedEvents = orderedEvents.GetRange(0, 4);
            }

            var homeDisplayModel = new HomeDisplayModel
            {
                EventDisplayModels = orderedEvents.Select(n => new EventDisplayModel
                {
                    Id = n.Id,
                    Photo = n.Photo,
                    Description = n.Description,
                    EventDate = n.EventDate.ToString("dd MMM", new CultureInfo("es-ES")).ToUpper(),
                    Place = n.Place,
                    Title = n.Title,
                    ScheduleTime = n.StartTime + " - " + n.FinishTime
                }),
                ProfileDisplayModels = _profileRepository.GetAllProfiles().Select(n => new ProfileDisplayModel
                {
                    Id = n.Id,
                    Description = n.Description,
                    FullName = n.FullName,
                    Photo = n.Photo
                })
            };

            return View(homeDisplayModel);
        }

        public ActionResult GetUserLoggedName()
        {
            var userName = _securityService.GetUserLoggedName();
            return Content(userName);
        }
    }
}
