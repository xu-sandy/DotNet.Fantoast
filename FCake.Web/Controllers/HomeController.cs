using FCake.Bll;
using FCake.Bll.Services;
using FCake.Domain.Entities;
using FCake.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Core.Common;
using FCake.Core.MvcCommon;
using System.Text.RegularExpressions;

namespace FCake.Web.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Home/
        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ActiveCenter()
        {
            return View();
        }

        public ActionResult AboutUs()
        {
            return View();
        }

        public ActionResult ContactUs()
        {
            return View();
        }

        public ActionResult OrderHelp()
        {
            return View();
        }
        [Authorize]
        public ActionResult Cooperation()
        {
            var coopSvc = new CooperationService();
            var result = coopSvc.FindByCustomerId(CurrentMember.MemberId.ToString());
            if (result.Status != 0)
            {
                result = new Cooperation();
            }
            return View(result);
        }
        [HttpPost]
        public ActionResult SubmitCooperation(Cooperation entity)
        {
            var result = new OpResult();
            var userId = CurrentMember.MemberId.ToString();
            result = new CooperationService().SubmitCooperation(entity, userId);
            return Json(result);
        }


        public ActionResult Error()
        {
            return View("Error");
        }
    }
}
