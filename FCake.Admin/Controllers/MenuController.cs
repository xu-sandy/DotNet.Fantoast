using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Admin.Models;

namespace FCake.Admin.Controllers
{
    public class MenuController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
