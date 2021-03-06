﻿using System;
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
    public class SliderController : Controller
    {
        private readonly ISliderRepository _sliderRepository;
        private readonly ViewMessageLogic _viewMessageLogic;
        //
        // GET: /Slider/
        public SliderController(ISliderRepository sliderRepository)
        {
            _sliderRepository = sliderRepository;
            _viewMessageLogic = new ViewMessageLogic(this);
        }

        public ActionResult Index(int? page)
        {
            _viewMessageLogic.SetViewMessageIfExist();
            var sliderPhotos = _sliderRepository.GetAllSliderPhotos().ToList();
            if (!sliderPhotos.Any()) return View(new List<SliderDisplayModel>().ToPagedList(1, 10));
            var slidersPhotosModel = Enumerable.ToList(sliderPhotos.Select(Mapper.Map<Slider, SliderDisplayModel>));
            var numberPage = page ?? 1;
            return View(slidersPhotosModel.ToPagedList(numberPage, 10));
        }

        [HttpGet]
        [AuthorizeAdminDirector]
        public ActionResult Add()
        {
            return PartialView("Create", new SliderRegisterModel());
        }

        [HttpPost]
        [AuthorizeAdminDirector]
        public ActionResult Add(SliderRegisterModel photoRegistered)
        {
            var title = "";
            string content;
            var sliderPhoto = Mapper.Map<SliderRegisterModel, Slider>(photoRegistered);
            try
            {
                if (photoRegistered.UploadPhoto != null)
                {
                    WebImage img = new WebImage(photoRegistered.UploadPhoto.InputStream);
                    if (img.Width > 3500 || img.Height > 1750)
                    {
                        img.Resize(3500, 1750);
                    }

                    sliderPhoto.Photo = img.GetBytes();
                }
            }
            catch (Exception)
            {
                title = "Error!";
                content = "Formato de Imagen Incorrecto";
                _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.ErrorMessage);
                return RedirectToAction("Add");
            }

            _sliderRepository.Create(sliderPhoto);
            title = "Foto Agregada";
            content = "Ha sido guardado exitosamente.";
            _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [AuthorizeAdminDirector]
        public ActionResult Delete(long id)
        {
            const string title = "La foto se ha eliminado exitosamente";
            _sliderRepository.Delete(id);
            var content = "";
            _viewMessageLogic.SetNewMessage(title, content, ViewMessageType.SuccessMessage);
            return RedirectToAction("Index");
        }
    }
}