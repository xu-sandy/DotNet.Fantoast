using FCake.Admin.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FCake.Admin.Areas.AreaPad.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /AreaPad/Home/

        public ActionResult Index()
        {
            return View();
        }

    }
}
