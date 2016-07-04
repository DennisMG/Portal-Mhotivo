using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Mhotivo.Authorizations;
using Mhotivo.Data.Entities;
using Mhotivo.Interface.Interfaces;
using Mhotivo.Logic.ViewMessage;
using Mhotivo.Models;
using PagedList;
using System.Web.Helpers;

namespace Mhotivo.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventRepository _eventRepository;
        private readonly ViewMessageLogic _viewMessageLogic;

        public EventController(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
            _viewMessageLogic = new ViewMessageLogic(this);
        }


        /// GET: /Event/
        [AuthorizeAdminDirector]
        public ActionResult Index(int? page)
        {
            _viewMessageLogic.SetViewMessageIfExist();
            var events = _eventRepository.GetAllEvents().ToList();
            if (!events.Any()) return View(new List<EventDisplayModel>().ToPagedList(1, 10));
            var eventsModel = Enumerable.ToList(events.Select(Mapper.Map<Event, EventDisplayModel>));
            var numberPage = page ?? 1;
            return View(eventsModel.ToPagedList(numberPage, 10));
        }


        [HttpGet]
        [AuthorizeAdminDirector]
        public ActionResult Add()
        {
            return View("Create", new EventRegisterModel());
        }

        [HttpPost]
        [AuthorizeAdminDirector]
        public ActionResult Add(EventRegisterModel eventRegistered)
        {
            var title = "";
            string content;
            var @event = Mapper.Map<EventRegisterModel, Event>(eventRegistered);
            try
            {
                if (eventRegistered.UploadPhoto != null)
                {
                    WebImage img = new WebImage(eventRegistered.UploadPhoto.InputStream);
                    if (img.Width > 200 || img.Height > 200)
                    {
                        img.Resize(200, 200);
                    }

                    @event.Photo = img.GetBytes();
                }               
            }
            catch (Exception)
            {
                title = "Error!";
                content = "Formato de Imagen Incorrecto";
                _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.ErrorMessage);
                return RedirectToAction("Add");
            }
            var query =
                _eventRepository.Filter(e => e.Title == @event.Title);
            if (query.Any())
            {
                title = "Error!";
                content = "El Evento ya existe.";
                _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.ErrorMessage);
                return RedirectToAction("Index");
            }
         
            var @eventCreated = _eventRepository.Create(@event);
            title = "Evento Agregado";
            content = @eventCreated.Title + " evento ha sido guardado exitosamente.";
            _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AuthorizeAdminDirector]
        public ActionResult Delete(long id)
        {
            const string title = "Evento ha sido Eliminado";
            var @event = _eventRepository.Delete(id);
            var content = @event.Title + " ha sido eliminado exitosamente.";
            _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
            return RedirectToAction("Index");
        }
    }
}