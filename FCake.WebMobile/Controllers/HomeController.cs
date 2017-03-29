using FCake.Bll;
using FCake.Bll.Services;
using FCake.Domain.Entities;
using FCake.WebMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Core.Common;
using System.Text.RegularExpressions;
using FCake.Core.MvcCommon;

namespace FCake.WebMobile.Controllers
{
    public class HomeController : Controller
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
            //result.Successed = false;
            //string par = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            //Regex regex = new Regex(par);
            //Match match = regex.Match(entity.Email);
            //if (!match.Success)
            //{
            //    result.Message = "请输入正确的邮箱";
            //    return new JsonNetResult(result);
            //}
            //if (new Regex(@"^1[3|5|7|8|][0-9]{9}$").IsMatch(entity.Phone) == false)
            //{
            //    result.Message = "请输入正确的手机号!";
            //    return new JsonNetResult(result);
            //}
            result = new CooperationService().SubmitCooperation(entity, CurrentMember.MemberId.ToString());
            return new JsonNetResult(result);
        }

        public ActionResult Error()
        {
            return View("Error");
        }
    }
}
