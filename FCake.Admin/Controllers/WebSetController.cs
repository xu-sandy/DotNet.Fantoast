using FCake.Admin.Helper;
using FCake.Admin.Models;
using FCake.Bll;
using FCake.Bll.Services;
using FCake.Core.MvcCommon;
using FCake.Domain.Entities;
using FCake.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace FCake.Admin.Controllers
{
    public class WebSetController : BaseController
    {
        private readonly BaseService _baseSvc = new BaseService();
        /// <summary>
        /// 幻灯片设定
        /// </summary>
        /// <returns></returns>
        #region
        public ActionResult Slide()
        {
            return View();
        }
        [HttpPost]
        [CheckPermission(controlName = "WebSet", actionName = "slide", permissionCode = "view")]
        public ActionResult GetSlides()
        {
            var slides = new SlideService().GetSlides<SlideVM>();
            return Json(new { total = slides.Count(), rows = slides });
        }
        [CheckPermission(controlName = "WebSet", actionName = "slide", permissionCode = "add")]
        public ActionResult CreateSlide(string id)
        {
            var slide = this._baseSvc.Find<Slide>(id);
            return View(slide);
        }

        [HttpPost]
        [CheckPermission(controlName = "WebSet", actionName = "slide", permissionCode = "add")]
        public ActionResult CreateSlide(Slide model, string id, string url, SlideStatus status, int? apply)
        {
            var result = new SlideService().SaveSlide(model,UserCache.CurrentUser.Id);
            return Json(result);
        }

        [HttpPost]
        [ValidateInput(false)]
        [CheckPermission(controlName = "WebSet", actionName = "slide", permissionCode = "edit")]
        public ActionResult UploadSlide(string id)
        {
            try
            {
                string name = Request.Files[0].FileName;

                //格式
                Regex r = new Regex(@"\.(jpg|jpeg|gif|png)$");
                if (r.IsMatch(name) == false)
                    throw new Exception("上传文件格式不正确( jpg jpeg gif png )");

                Session["SlideMsg"] = new SlideService().SaveSlidePicture(id, Request.Files[0], UserCache.CurrentUser.Id);
            }
            catch (Exception ex)
            {
                Session["SlideMsg"] = new { validate = false, msg = ex.Message };
            }
            return Json("", "text/html");
        }

        [HttpPost]
        //[CheckPermission(controlName = "WebSet", actionName = "GetSlideMsg", permissionCode = "view")]
        public ActionResult GetSlideMsg()
        {
            var result = Session["SlideMsg"];
            Session["SlideMsg"] = null;
            return Json(result);
        }
        [CheckPermission(controlName = "WebSet", actionName = "MsgTemplate", permissionCode = "view")]
        public ActionResult MsgTemplate()
        {
            return View();
        }
        [HttpPost]
        [CheckPermission(controlName = "WebSet", actionName = "MsgTemplate", permissionCode = "view")]
        public ActionResult GetMsgTemplate()
        {
            var result = new MsgTemplateService().GetAllMsgTemp();
            return Json(result.ToList());
        }
        [CheckPermission(controlName = "WebSet", actionName = "MsgTemplate", permissionCode = "edit")]
        public ActionResult UpdateMsgTemplate(string id, string content)
        {
            var result = new MsgTemplateService().UpdateMsgTemplate(id, content);
            return Json(result);
        }
        [CheckPermission(controlName = "WebSet", actionName = "MsgTemplate", permissionCode = "view")]
        public ActionResult GetContentByCategory(string Category)
        {
            var result = new MsgTemplateService().GetMsgTempByCategory(Category);
            return Json(result);
        }

        #endregion

        #region 营业管理设置
        private readonly SysConfigService _sysConfigSvc = new SysConfigService();
        [CheckPermission(controlName = "WebSet", actionName = "BusinessManage", permissionCode = "view")]
        public ActionResult BusinessManage()
        {
            var model = new BusinessManageModel();
            var sales = SysConfigsCache.GetSalesConfig();
            model.TempEarlyDistributionTime = sales.TempEarlyDistributionTime;
            model.ProductionHours = sales.ProductionHours;
            //生产时长
            return View(model);
        }
        [HttpPost]
        [CheckPermission(controlName = "WebSet", actionName = "BusinessManage", permissionCode = "publish")]
        public JsonResult PublishBusinessManage(string type, string val)
        {
            var result = _sysConfigSvc.PublishBusinessManage(type, val);
            return new JsonNetResult(result);
        }
        #endregion
    }

}
