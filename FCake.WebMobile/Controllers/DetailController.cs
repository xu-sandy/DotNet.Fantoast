using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Domain;
using FCake.Bll;

namespace FCake.WebMobile.Controllers
{
    public class DetailController : Controller
    {
        //
        // GET: /Detail/
        ProductService ps = new ProductService();
        public ActionResult Index(string id)
        {
            var data = ps.GetProductById(id);
            if (data != null)
            {
                return View(data);
            }
            //找不到产品跳转到列表页
           return RedirectToAction("Index","Product");
        }

    }
}
