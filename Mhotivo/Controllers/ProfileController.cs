using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Mhotivo.Authorizations;
using Mhotivo.Interface.Interfaces;
using Mhotivo.Logic.ViewMessage;
using Mhotivo.Models;
using PagedList;
using Profile = Mhotivo.Data.Entities.Profile;
using System.Web.Helpers;

namespace Mhotivo.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IProfileRepository _profileRepository;
        private readonly ViewMessageLogic _viewMessageLogic;

        public ProfileController(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
            _viewMessageLogic = new ViewMessageLogic(this);
        }

        //
        // GET: /Profile/

        public ActionResult Index(int? page)
        {
            _viewMessageLogic.SetViewMessageIfExist();
            var profiles = _profileRepository.GetAllProfiles().ToList();
            if (!profiles.Any()) return View(new List<ProfileDisplayModel>().ToPagedList(1, 10));
            var eventsModel = Enumerable.ToList(profiles.Select(Mapper.Map<Profile, ProfileDisplayModel>));
            var numberPage = page ?? 1;
            return View(eventsModel.ToPagedList(numberPage, 10));
        }

        [HttpGet]
        [AuthorizeAdminDirector]
        public ActionResult Add()
        {
            return View("Create", new ProfileRegisterModel());
        }

        [HttpPost]
        [AuthorizeAdminDirector]
        public ActionResult Add(ProfileRegisterModel profileRegistered)
        {
            var title = "";
            string content;
            var profile = Mapper.Map<ProfileRegisterModel, Profile>(profileRegistered);
            try
            {
                if (profileRegistered.UploadPhoto != null)
                {
                    WebImage img = new WebImage(profileRegistered.UploadPhoto.InputStream);
                    if (img.Width > 200 || img.Height > 200)
                    {
                        img.Resize(200, 200);
                    }

                    profile.Photo = img.GetBytes();
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
                _profileRepository.Filter(e => e.FullName == profile.FullName);
            if (query.Any())
            {
                title = "Error!";
                content = "El Perfil ya existe.";
                _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.ErrorMessage);
                return RedirectToAction("Index");
            }

            var profileCreated = _profileRepository.Create(profile);
            title = "Perfil Agregado";
            content = profileCreated.FullName + "ha sido guardado exitosamente.";
            _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AuthorizeAdminDirector]
        public ActionResult Delete(long id)
        {
            const string title = "El perfil ha sido Eliminado";
            var profile = _profileRepository.Delete(id);
            var content = profile.FullName + " ha sido eliminado exitosamente.";
            _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
            return RedirectToAction("Index");
        }


        [HttpGet]
        [AuthorizeAdminDirector]
        public ActionResult Edit(long id)
        {
            var profile = _profileRepository.GetById(id);
            var profileModel = Mapper.Map<Profile, ProfileEditModel>(profile);
            return View("Edit", profileModel);
        }

        [HttpPost]
        [AuthorizeAdminDirector]
        public ActionResult Edit(ProfileEditModel modelProfile)
        {
            var validImageTypes = new[]
            {
                "image/gif",
                "image/jpeg",
                "image/pjpeg",
                "image/png"
            };
            if (modelProfile.FilePicture != null && modelProfile.FilePicture.ContentLength > 0)
            {
                if (!validImageTypes.Contains(modelProfile.FilePicture.ContentType))
                {
                    ModelState.AddModelError("FilePicture", "Por favor seleccione entre una imagen GIF, JPG o PNG");
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (modelProfile.FilePicture != null)
                    {
                        WebImage img = new WebImage(modelProfile.FilePicture.InputStream);
                        if (img.Width > 200 || img.Height > 200)
                        {
                            img.Resize(200, 200);
                        }

                        modelProfile.Photo = img.GetBytes();
                    }
                    var profile = _profileRepository.GetById(modelProfile.Id);
                    Mapper.Map(modelProfile, profile);
                    _profileRepository.Update(profile);
                    const string title = "Perfil Actualizado";
                    var content = "El perfil de" + profile.FullName + " ha sido actualizado exitosamente.";
                    _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
                }
                catch (Exception e)
                {
                    const string title = "Error";
                    var content = "El perfil no se actualizó.";
                    _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.ErrorMessage);
                }
            }
            return RedirectToAction("Index");
        }
    }
}

