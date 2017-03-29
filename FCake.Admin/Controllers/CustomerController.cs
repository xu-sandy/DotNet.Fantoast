using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FCake.Admin.Models;
using FCake.Domain;
using FCake.Admin.Helper;
using FCake.Bll;
using FCake.Domain.Entities;
using FCake.Domain.Common;

namespace FCake.Admin.Controllers
{
    public class CustomerController : BaseController
    {
        //
        // GET: /Customer/
        CustomerAddressService cas = new CustomerAddressService();
        private readonly CustomersService _customersService = new CustomersService();
        public ActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// 根据条件查询客户信息
        /// </summary>
        /// <param name="isVip">是否vip</param>
        /// <param name="sex">性别</param>
        /// <param name="type">客户类型</param>
        /// <param name="beginage">生日开始时间</param>
        /// <param name="endage">生日结束时间</param>
        /// <param name="validdateBegin">注册开始</param>
        /// <param name="validdateEnd">注册结束</param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Customer", actionName = "Index", permissionCode = "view")]
        public ActionResult GetUsers(string fullName, string phone, int memberLevelVal, int orderby, int page = 1, int rows = 10)
        {
            int count = 0;
            var customers = _customersService.GetCustomers(fullName, phone, memberLevelVal, orderby, out count, page, rows);
            GridJsonModel djson = new GridJsonModel(count, customers);
            return Json(djson);
        }

        /// <summary>
        /// 根据用户ID，查询用户所有使用地址
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Customer", actionName = "Index", permissionCode = "view")]
        [HttpPost]
        public string getCustomerAddress(string customerId, int isdef)
        {

            var address = new List<FCake.Domain.Entities.CustomerAddress>();
            if (customerId.Trim() != "" && customerId != null)
            {
                address = cas.GetCustomerAddressesById(customerId, isdef);
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(address);
        }
        /// <summary>
        /// 根据地址ID获取用户默认地址
        /// </summary>
        /// <param name="addressId">地址ID</param>
        /// <returns></returns>
        [CheckPermission(controlName = "Customer", actionName = "Index", permissionCode = "view")]
        public string getCustomerDefAddress(string addressId)
        {
            if (addressId != null && addressId != "")
            {
                var address = cas.GetCustomerDefAddress(addressId);
                return address.ToString();
            }
            return "";
        }
        /// <summary>
        /// 根据客户Id查询所有使用地址
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Customer", actionName = "Index", permissionCode = "view")]
        public ActionResult GetCustomerAddByCustomerId(string customerId)
        {
            if (customerId != null)
            {
                var data = Bll.CustomersService.pro.GetCustomerAddByCustomerId(customerId);
                var addressModel = (from add in data select new EasyuiCombo() { id = add.Id, text = add.Address, desc = "" }).ToList();
                return Json(addressModel);
            }
            else

                return View();

        }

        /// <summary>
        /// 根据用户ID获取相关送货区域
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Customer", actionName = "Index", permissionCode = "view")]
        public ActionResult GetAreaByCustomerId(string customerId)
        {
            if (customerId != null)
            {
                var data = Bll.CustomersService.pro.GetCustomerAddByCustomerId(customerId);
                var areaModel = (from a in data select new EasyuiCombo() { id = a.Id, text = a.Area, desc = "" }).ToList();
                return Json(areaModel);
            }
            else
                return View();
        }
        /// <summary>
        /// 根据用户ID获取历史收货人
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Customer", actionName = "Index", permissionCode = "view")]
        public ActionResult GetReceiverByCustomerId(string customerId)
        {
            if (customerId != null)
            {
                var data = Bll.CustomersService.pro.GetCustomerAddByCustomerId(customerId);
                var recevierModel = (from a in data select new EasyuiCombo() { id = a.Id, text = a.Receiver, desc = "" }).ToList();
                return Json(recevierModel);
            }
            else
                return View();
        }

        [CheckPermission(controlName = "Customer", actionName = "Index", permissionCode = "view")]
        public ActionResult CustomerAddress(string customerAddressId)
        {
            if (customerAddressId != null)
            {
                if (customerAddressId.Trim() != "")
                {
                    var temp = Bll.CustomersService.pro.CustomerAddress(customerAddressId);

                    return View(temp);
                }
            }
            return View();
        }

        //客户管理页面
        [CheckPermission(isRelease = true)]
        public ActionResult Management()
        {
            return View();
        }

        /// <summary>
        /// 客户管理搜索定位 by cloud
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Customer", actionName = "Management", permissionCode = "view")]
        public ActionResult CustomerLocate(string condition, string status, int page = 1, int rows = 30)
        {
            condition = HttpUtility.UrlDecode(condition.Trim());
            dynamic result;
            int totalCount = 0;
            result = Bll.CustomersService.pro.CustomerLocate(condition, status, out totalCount, page, rows);
            return Json(new { total = totalCount, rows = result });
        }

        /// <summary>
        /// 客户购物历史之客户信息 by cloud
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Customer", actionName = "Management", permissionCode = "view")]
        public ActionResult CustomerDetails(string customerId, string orderNo)
        {
            var csvc = new CustomersService();
            string province, city, area, address;
            if (!string.IsNullOrEmpty(orderNo))
            {
                ViewBag.orderid = orderNo;
            }
            var data = csvc.GetCustomerInfoById(customerId, out province, out city, out area, out address);
            ViewBag.Province = province;
            ViewBag.City = city;
            ViewBag.Area = area;
            ViewBag.Address = address;
            return View(data);
        }

        //客户资料编辑页面 by cloud
        [CheckPermission(controlName = "Customer", actionName = "Management", permissionCode = "edit")]
        public ActionResult CustomerEdit(string customerId)
        {
            string province, city, area, address;
            var data = new CustomersService().GetCustomerInfoById(customerId, out province, out city, out area, out address);
            ViewBag.Province = province;
            ViewBag.City = city;
            ViewBag.Area = area;
            ViewBag.Address = address;
            return View(data);
        }

        //客户资料保存按钮事件 by cloud
        [CheckPermission(controlName = "Customer", actionName = "Management", permissionCode = "edit")]
        public ActionResult SaveCustomerInfo(VCustomer customer)
        {
            string userId = UserCache.CurrentUser.Id;
            return Json(new CustomersService().SaveCustomerInfo(customer, userId));
        }
        /// <summary>
        /// 获取用户信息，根据id
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        [HttpPost]
        public ActionResult GetMemberByMemberId(string memberId)
        {
            return Json(new CustomersService().GetMemberByMemberId(memberId));
        }
        /// <summary>
        /// 获取会员的成长值记录
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        public ActionResult GetMemberGrowthValueLogById(string memberId, int page = 1, int rows = 15)
        {
            PageInfo pageInfo = new PageInfo() { Page = page, Rows = rows };
            if (memberId == null || memberId == "")
            {
                return Json(new { total = 0, rows = "" });
            }
            int totalCount = 0;
            var result = new CustomersService().GetMemberGrowthValueLogById(memberId, pageInfo, out totalCount);
            return Json(new { total = totalCount, rows = result });
        }
        /// <summary>
        /// 获取会员积分的交易记录
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        public ActionResult GetMemberIntegralLogById(string memberId, int page = 1, int rows = 15)
        {
            PageInfo pageInfo = new PageInfo() { Page = page, Rows = rows };
            if (memberId == null || memberId == "")
            {
                return Json(new { total = 0, rows = "" });
            }
            int totalCount = 0;
            var result = new CustomersService().GetMemberIntegralLogById(memberId, pageInfo, out totalCount);
            return Json(new { total = totalCount, rows = result });
        }
        /// <summary>
        /// 统计会员数量
        /// </summary>
        /// <returns></returns>
        [CheckPermission(isRelease = true)]
        public ActionResult GetCustomersCount(int type, string data)
        {
            return Json(new CustomersService().GetCustomersCount(type, data));
        }
        /// <summary>
        /// 客户的优惠信息（代金卡，优惠券，交易记录）
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [CheckPermission(controlName = "Customer", actionName = "Management", permissionCode = "view")]
        public ActionResult FavorableInfo(string Id)
        {
            ViewBag.Id = Id;
            return View();
        }
    }
}
