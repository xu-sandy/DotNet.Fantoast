using FCake.Bll.Services;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FCake.Web.Controllers
{
    public class CooperationController : Controller
    {
        CooperationService cs = new CooperationService();

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SubmitCooperation(Cooperation entity)
        {
            var result = cs.SubmitCooperation(entity);
            return Json(result);
        }
    }
}
