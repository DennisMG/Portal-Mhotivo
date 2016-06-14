using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Mhotivo.Data.Entities;
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
        private static ISliderRepository _sliderRepository;

        public HomeController(ISecurityService securityService, IProfileRepository profileRepository
            , IEventRepository eventRepository
            , ISliderRepository sliderRepository)
        {
            _securityService = securityService;
            _profileRepository = profileRepository;
            _eventRepository = eventRepository;
            _sliderRepository = sliderRepository;
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


        public static List<Slider> GetAllSliderPhotos()
        {
            var sliderPhotos = _sliderRepository.GetAllSliderPhotos().ToList();
            return sliderPhotos;
        }
    }
}
