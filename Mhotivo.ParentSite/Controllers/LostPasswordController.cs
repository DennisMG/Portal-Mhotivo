using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mhotivo.ParentSite.Controllers
{
    public class LostPasswordController : Controller
    {
        public LostPasswordController()
        {
            
        }
        //
        // GET: /LostPassword/
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

    }
}
