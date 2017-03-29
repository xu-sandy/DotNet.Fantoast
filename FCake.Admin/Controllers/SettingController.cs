using FCake.Bll.Services;
using FCake.Admin.Models;
using FCake.Domain.Common;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Core.MvcCommon;
using FCake.Admin.Helper;

namespace FCake.Admin.Controllers
{
    public class SettingController : BaseController
    {
        //
        // GET: /Setting/
        private SettingService svc = new SettingService();

        public ActionResult Index()
        {
            return View();
        }
        #region 数据字典类型
        public ActionResult Dictionary()
        {
            return View();
        }

        public ActionResult QueryDictionary(DictionaryType dictionary, int page = 1, int rows = 20)
        {
            var pageInfo = new PageInfo() { Page = page, Rows = rows };
            int totalCount = 0;
            var result = svc.FilterDictionary(dictionary, out totalCount, pageInfo);
            GridJsonModel djson = new GridJsonModel(totalCount, result);
            return Json(djson);
        }

        public ActionResult DictionaryEdit(string id)
        {
            var result = svc.GetDictionaryTypeById(id);
            return View(result);
        }
        public JsonResult SaveDictionaryEdit(DictionaryType dictionary)
        {
            var result = svc.SaveDictionaryType(dictionary, UserCache.CurrentUser.Id);
            return Json(result);
        }

        public ActionResult CheckDictionaryCode(string code)
        {
            if (svc.DictionaryCodeExsist(code))
                return JqRemoteValidation.Invalid("字典编号已存在");
            else return JqRemoteValidation.Valid();
        }
        #endregion

        #region 数据字典
        public ActionResult DictionaryData()
        {
            return View();
        }
        public ActionResult GetDictionaryTypeTree()
        {
            var totalCount = 0;
            var result = svc.FilterDictionary(new DictionaryType(), out totalCount, null);
            IList<EasyuiTree> treeList = new List<EasyuiTree>();
            EasyuiTree tree = new EasyuiTree() { id = "top_top", text = "数据字典类型" };
            treeList.Add(tree);
            var childs = (from dic in result
                          select new EasyuiTree()
                          {
                              id = dic.Code,
                              text = dic.Name,
                              url = "true"
                          }).ToList();
            tree.children = childs;
            return Json(treeList);
        }
        public ActionResult DictionaryDataGrid(string dicCode)
        {
            ViewBag.dicCode = dicCode;
            return View();
        }
        public ActionResult GetDictionaryDataGrid(string dicCode, int page = 1, int rows = 20)
        {
            var totalCount = 0;
            var pageInfo = new PageInfo() { Page = page, Rows = rows };
            var result = svc.GetDictionaryDataByDicCode(dicCode, out totalCount, pageInfo);
            GridJsonModel djson = new GridJsonModel(totalCount, result);
            return Json(djson);
        }
        #endregion
    }
}
