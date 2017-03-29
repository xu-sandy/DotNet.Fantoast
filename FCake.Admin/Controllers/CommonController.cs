using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using FCake.Admin.Models;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.IO;
using FCake.Domain;
using FCake.Domain.Entities;
using FCake.Bll;
using System.Web.Script.Serialization;
using System.Linq.Expressions;
using FCake.Core.Common;
using System.Xml.Linq;
using FCake.Admin.Helper;

namespace FCake.Admin.Controllers
{
    public class CommonController : BaseController
    {
        private EFDbContext context = new EFDbContext();
        CommonService svc = new CommonService();

        [CheckPermission(needMenuID = true, checkRegex = true)]
        [HttpPost]
        public ActionResult GetData(int? page, int? rows, string type, string order = "")
        {
            var t = Type.GetType(type);

            if (t == null)
            {
                return Json("");
            }

            var t1 = typeof(DbSet<>).MakeGenericType(t);

            var ctype = typeof(EFDbContext);

            dynamic result = "";

            foreach (var x in ctype.GetProperties())
            {
                if (t1.IsAssignableFrom(x.PropertyType))
                {
                    result = x.GetValue(context, null);
                    result = new CommonService().RemoveDelete(result);
                    break;
                }
            }

            int p = page ?? 1;
            int r = rows ?? 1;

            var    datarows = page == null ? (result as IQueryable<dynamic>) : (result as IQueryable<dynamic>).OrderBy(order).Skip((p - 1) * r).Take(r);
            return Json(new
            {
                total = (result as IQueryable<dynamic>).Count(),
                rows = datarows
            });
        }


        [HttpPost]
        [CheckPermission(isRelease = true)]
        public ActionResult GetTreeData(string type, int? p, string doname = "ToTree", string special = "")
        {
            var t = Type.GetType(type);
            var t1 = typeof(DbSet<>).MakeGenericType(t);
            var t2 = typeof(IQueryable<>).MakeGenericType(t);

            var ctype = typeof(EFDbContext);

            dynamic result = "";

            foreach (var x in ctype.GetProperties())
            {
                if (t1.IsAssignableFrom(x.PropertyType))
                {
                    result = x.GetValue(context, null);
                    result=new CommonService().RemoveDelete(result);
                    break;
                }
            }

            var f = typeof(MenuService);
            var mi = f.GetMethod(doname, new Type[] { typeof(IQueryable<>).MakeGenericType(t), typeof(int) });

            if (mi == null)
                throw new Exception("类" + type + "查无" + doname + "方法");

            result = mi.Invoke(new MenuService(), new object[] { result, p ?? 0 });

            if (special.IsNullOrTrimEmpty() == false)
            {
                mi = f.GetMethod(special, new Type[] { typeof(List<Dictionary<string, object>>),typeof(bool),typeof(string) });
                if (mi == null)
                    throw new Exception("类" + type + "查无" + special + "方法");
                result = mi.Invoke(new MenuService(), new object[] { result,UserCache.CurrentUser.IsAdmin,UserCache.CurrentUser.Id });
            }

            return Json(result);
        }

        [HttpPost]
        [CheckPermission(isRelease = true)]
        public ActionResult GetSelectData(string type, string value, string text)
        {
            var t = Type.GetType(type);
            var t1 = typeof(DbSet<>).MakeGenericType(t);

            var ctype = typeof(EFDbContext);

            dynamic result = "";

            foreach (var x in ctype.GetProperties())
            {
                if (t1.IsAssignableFrom(x.PropertyType))
                {
                    result = x.GetValue(context, null);
                    break;
                }
            }

            var tp = t.GetProperty(text);
            var vp = t.GetProperty(value);

            List<Dictionary<string, object>> dics = new List<Dictionary<string, object>>();

            //增加空选项
            Dictionary<string, object> temp = new Dictionary<string, object>();
            temp.Add("text", "----请选择---");
            temp.Add("value", "");
            dics.Add(temp);


            foreach (var x in result as IQueryable<dynamic>)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("text", tp == null || tp.GetValue(x, null) == null ? "" : tp.GetValue(x, null));
                dic.Add("value", vp == null || vp.GetValue(x, null) == null ? "" : vp.GetValue(x, null));
                dics.Add(dic);
            }

            return Json(dics);
        }

        [HttpPost]
        [ValidateInput(false)]
        [CheckPermission(needMenuID = true, checkRegex = true)]
        public ActionResult SaveData(string type, FormCollection c)
        {
            try
            {
                var t = Type.GetType(type);
                var data = t.Assembly.CreateInstance(t.FullName);

                data.CopyProperty(c.ToDic());

                var f = typeof(CommonService);
                var mi = f.GetMethods().First(a => a.IsGenericMethod && a.Name == "Save").MakeGenericMethod(t);
                if (mi == null)
                    throw new Exception("类" + type + "查无Save方法");

                string msg = mi.Invoke(new CommonService(), new object[] { data,context,UserCache.CurrentUser,true }).ToString();
                return Json(new { validate = msg.IsNullOrTrimEmpty(), msg = msg, data = data });
            }
            catch (Exception e)
            {
                return Json(new { validate = false, msg = e.Message });
            }
        }

        [HttpPost]
        [CheckPermission(needMenuID = true, checkRegex = true)]
        public ActionResult DeleteData(string type, FormCollection c)
        {
            try
            {
                var t = Type.GetType(type);
                var data = t.Assembly.CreateInstance(t.FullName);

                data.CopyProperty(c.ToDic());

                var f = typeof(CommonService);
                var mi = f.GetMethods().First(a => a.IsGenericMethod && a.Name == "Delete").MakeGenericMethod(t);
                if (mi == null)
                    throw new Exception("类" + type + "查无Delete方法");

                string msg = mi.Invoke(new CommonService(), new object[] { data, context, UserCache.CurrentUser, true }).ToString();
                return Json(new { validate = msg.IsNullOrTrimEmpty(), msg = msg });

            }
            catch (Exception e)
            {
                return Json(new { validate = false, msg = e.Message });
            }
        }

        /// <summary>
        /// 获取类型参数
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckPermission(isRelease = true)]
        public ActionResult GetTypeMsg(string type)
        {
            var t = Type.GetType(type);

            return Json(new
            {
                name = t.Name,
                fullname = t.FullName,
                columns = t.GetPropertyList()
            });
        }

        [CheckPermission(isRelease = true)]
        public ActionResult ErrorJson()
        {
            dynamic d = Session["ErrorJson"];
            Session["ErrorJson"] = null;
            return Json(d, JsonRequestBehavior.AllowGet);
        }

        [CheckPermission(isRelease = true)]
        public ActionResult ErrorCheckJson()
        {
            dynamic d = Session["ErrorCheckJson"];
            Session["ErrorCheckJson"] = null;
            return Json(d, JsonRequestBehavior.AllowGet);
        }

        [CheckPermission(isRelease = true)]
        public ActionResult Error()
        {
            ViewBag.msg = Session["ErrorMsg"];
            Session["ErrorMsg"] = null;
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        [CheckPermission(needMenuID = true, permissionCode = "upload")]
        public ActionResult Upload(string linkid, string filetype)
        {
            try
            {
                Session["SlideMsg"] = new
                {
                    validate = true,
                    data = new BaseFileService().SaveBaseFiles(Request.Files, linkid, filetype, UserCache.CurrentUser.Id)
                };
                return Json("", "text/html");
            }
            catch (Exception e)
            {
                Session["SlideMsg"] = new { validate = false, msg = e.Message };

                return Json("", "text/html");
            }
        }
        [CheckPermission(isRelease = true)]
        public ActionResult GetEnum(string code)
        {
            var type = Type.GetType(code);
            List<EasyuiCombo> result = new List<EasyuiCombo>();
            foreach (var item in Enum.GetValues(type))
            {
                result.Add(new EasyuiCombo() { id = ((int)item).ToString(), text = item.ToString(), desc = item.ToString() });
            }
            return Json(result);
        }

        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult GetPosition(string position,string value)
        {
            //取出地址
            var dics = AddressService.GetPositions(position,value);
            return Json(dics);
        }
    }
}
