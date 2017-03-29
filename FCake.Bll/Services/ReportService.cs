using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCake.Domain.Entities;
using FCake.Domain;
using System.Data.SqlClient;
using System.Data;
using System.Web;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HPSF;
using System.Reflection;
using System.Collections;
using System.Data.Entity;
using FCake.Domain.Enums;
using FCake.Core.Common;
using FCake.Core.MvcCommon;
using FCake.Domain.WebModels;

namespace FCake.Bll.Services
{
    public class ReportService
    {
        EFDbContext context = new EFDbContext();
        /// <summary>
        /// 产品生产周期报表查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="beginTime"></param>
        /// <param name="endtime"></param>
        /// <returns></returns>
        public List<T> GetReportByCycTime<T>(DateTime? beginTime, DateTime? endtime)
        {
            if (beginTime == null || endtime == null)
            {
                if (beginTime == null)
                {
                    beginTime = Convert.ToDateTime("2015-01-01 00:00:00");
                }
                if (endtime == null)
                {
                    endtime = DateTime.Now;
                }
            }
            SqlParameter[] sqlparms = new SqlParameter[2];
            sqlparms[0] = new SqlParameter("@startTime", beginTime);
            sqlparms[1] = new SqlParameter("@closeTime", endtime);
            //sqlparms[2] = new SqlParameter("@type", type);
            var result = (from p in context.Database.SqlQuery<T>("EXEC dbo.ProcOrderCycleTime @startTime, @closeTime", sqlparms) select p).ToList();
            return result;

        }
        /// <summary>
        /// 产品生产周期报表导出
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endtime"></param>
        /// <returns></returns>
        public OpResult ExportCycTime(DateTime? beginTime, DateTime? endtime)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            if (beginTime == null || endtime == null)
            {
                if (beginTime == null)
                {
                    beginTime = Convert.ToDateTime("2015-01-01 00:00:00");
                }
                if (endtime == null)
                {
                    endtime = DateTime.Now;
                }
            }

            try
            {
                SqlParameter[] sqlparms = new SqlParameter[2];
                sqlparms[0] = new SqlParameter("@startTime", beginTime);
                sqlparms[1] = new SqlParameter("@closeTime", endtime);
                DataTable dt = context.Database.SqlQueryForDataTatable("EXEC dbo.ProcOrderCycleTime @startTime, @closeTime", sqlparms);
                dt.Columns.Remove("ID");
                dt.Columns.Remove("ProductID");
                dt.Columns.Remove("SubProductID");
                dt.Columns.Remove("OrderTime");
                dt.Columns.Remove("Sort");
                string[] fields = new string[] { "Gernre", "ProductName", "Num", "Size", "MakeTimeDiff" };
                string[] names = new string[] { "类型", "蛋糕名称", "数量", "规格", "生产时间" };
                FCake.Core.Common.ExportExcel excelExport = new ExportExcel();
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                int[] marger = { 0 };
                string fileUrl = excelExport.ToExcel(fileName, dt, fields, names, marger);
                return OpResult.Success("导出成功", null, fileUrl);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 下单时段报表查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="type"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<T> GetReportByOrderTime<T>(DateTime? beginTime, DateTime? endTime, int type, string param)
        {
            //下单时段的报表
            if (beginTime == null || endTime == null)
            {
                if (beginTime == null)
                {
                    beginTime = Convert.ToDateTime("2015-01-01 00:00:00");
                }
                if (endTime == null)
                {
                    endTime = DateTime.Now;
                }
            }
            SqlParameter[] sqlparms = new SqlParameter[4];
            sqlparms[0] = new SqlParameter("@startTime", beginTime);
            sqlparms[1] = new SqlParameter("@closeTime", endTime);
            sqlparms[2] = new SqlParameter("@type", type);
            sqlparms[3] = new SqlParameter("@param", param);
            var result = context.Database.SqlQuery<T>("EXEC dbo.ProcOrderTimeCount @startTime,@closeTime,@type,@param", sqlparms).ToList();
            return result;
        }


        /// <summary>
        /// 销售流量报表查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<T> GetSalesByPosition<T>(DateTime? beginTime, DateTime? endTime, int type)
        {
            if (beginTime == null || endTime == null)
            {
                if (beginTime == null)
                {
                    beginTime = Convert.ToDateTime("2015-01-01 00:00:00");
                }
                if (endTime == null)
                {
                    endTime = DateTime.Now;
                }
            }
            SqlParameter[] sqlparms = new SqlParameter[3];
            sqlparms[0] = new SqlParameter("@fromTime", beginTime);
            sqlparms[1] = new SqlParameter("@toTime", endTime);
            sqlparms[2] = new SqlParameter("@type", type);
            var result = context.Database.SqlQuery<T>("EXEC dbo.Proc_SalesReport @fromTime,@toTime,@type", sqlparms).ToList();
            return result;
        }
        /// <summary>
        /// 销售流量报表导出
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public OpResult ExportSales(DateTime? beginTime, DateTime? endTime, int type)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            if (beginTime == null || endTime == null)
            {
                if (beginTime == null)
                {
                    beginTime = Convert.ToDateTime("2015-01-01 00:00:00");
                }
                if (endTime == null)
                {
                    endTime = DateTime.Now;
                }
            }
            SqlParameter[] sqlparms = new SqlParameter[3];
            sqlparms[0] = new SqlParameter("@fromTime", beginTime);
            sqlparms[1] = new SqlParameter("@toTime", endTime);
            sqlparms[2] = new SqlParameter("@type", type);


            try
            {
                DataTable dt = context.Database.SqlQueryForDataTatable("EXEC dbo.Proc_SalesReport @fromTime,@toTime,@type", sqlparms);
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                string[] fields = null;
                string[] names = null;

                switch (type)
                {
                    case 1:
                        dt.Columns.Remove("GroupId");
                        dt.Columns.Remove("GroupName");
                        dt.Columns.Remove("Sorting");
                        dt.Columns.Remove("IsData");
                        dt.Columns.Remove("Id");
                        fields = new string[] { "Name", "Num", "TotalPrice" };
                        names = new string[] { "蛋糕类型", "数量", "金额" };
                        break;
                    case 2:
                        dt.Columns.Remove("Id");
                        dt.Columns.Remove("GroupId");
                        dt.Columns.Remove("Sorting");
                        dt.Columns.Remove("IsData");
                        fields = new string[] { "Name", "GroupName", "Num", "TotalPrice" };
                        names = new string[] { "蛋糕名称", "蛋糕类型", "数量", "金额" };
                        break;
                    case 3:
                        dt.Columns.Remove("GroupId");
                        dt.Columns.Remove("GroupName");
                        dt.Columns.Remove("Sorting");
                        dt.Columns.Remove("IsData");
                        dt.Columns.Remove("Id");
                        fields = new string[] { "Name", "Num", "TotalPrice" };
                        names = new string[] { "规格", "数量", "总金额" };
                        break;
                    case 4:
                        dt.Columns.Remove("Id");
                        dt.Columns.Remove("GroupId");
                        dt.Columns.Remove("Sorting");
                        dt.Columns.Remove("IsData");
                        fields = new string[] { "Name", "GroupName", "Num", "TotalPrice" };
                        names = new string[] { "蛋糕名称", "磅数", "数量", "金额" };
                        break;
                    case 5:
                        dt.Columns.Remove("GroupId");
                        dt.Columns.Remove("GroupName");
                        dt.Columns.Remove("Sorting");
                        dt.Columns.Remove("IsData");
                        dt.Columns.Remove("Id");
                        fields = new string[] { "Name", "Num", "TotalPrice" };
                        names = new string[] { "蛋糕名称", "数量", "总金额" };
                        break;
                }

                FCake.Core.Common.ExportExcel excelExport = new ExportExcel();
                string fileUrl = excelExport.ToExcel(fileName, dt, fields, names, null);


                return OpResult.Success("导出成功", null, fileUrl);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 财务报表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public dynamic GetFinancialReportByTime<T>(DateTime? beginTime, DateTime? endTime, DateTime? reqBeginTime, DateTime? reqEndTime, int? reviewStatus, int? pageSize, int? pageIndex, int? orderStatus, out int totalCount)
        {
            totalCount = 0;
            SqlParameter[] param = new SqlParameter[9];
            if (beginTime.HasValue)
            {
                param[0] = new SqlParameter("@beginTime", beginTime);
            }
            else
            {
                param[0] = new SqlParameter("@beginTime", DBNull.Value);
            }
            if (endTime.HasValue)
            {
                param[1] = new SqlParameter("@endTime", endTime);
            }
            else
            {
                param[1] = new SqlParameter("@endTime", DBNull.Value);
            }
            if (reqBeginTime.HasValue)
            {
                param[2] = new SqlParameter("@reqStartTime", reqBeginTime);
            }
            else
            {
                param[2] = new SqlParameter("@reqStartTime", DBNull.Value);
            }
            if (reqEndTime.HasValue)
            {
                param[3] = new SqlParameter("@reqCloseTime", reqEndTime);
            }
            else
            {
                param[3] = new SqlParameter("@reqCloseTime", DBNull.Value);
            }
            param[4] = new SqlParameter("@reviewStatus", reviewStatus);
            param[5] = new SqlParameter("@orderStatus", orderStatus);
            param[6] = new SqlParameter("@pageSize", pageSize);
            param[7] = new SqlParameter("@pageIndex", pageIndex);
            param[8] = new SqlParameter("@totalRecord", totalCount);
            param[8].Direction = ParameterDirection.Output;
            var result = context.Database.SqlQueryForDataTatable("EXEC dbo.Proc_FinancialReporting @beginTime=@beginTime,@endTime=@endTime,@reqStartTime=@reqStartTime,@reqCloseTime=@reqCloseTime,@reviewStatus=@reviewStatus,@orderStatus=@orderStatus,@pageSize=@pageSize,@pageIndex=@pageIndex,@totalRecord=@totalRecord output", param);
            totalCount = int.Parse(param[8].Value.ToString());
            return result;

        }
        /// <summary>
        /// 下单时段报表导出
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="type"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public OpResult ExportOrderTime(DateTime? beginTime, DateTime? endTime, int type, string param)
        {
            OpResult result = new OpResult();
            result.Successed = false;
            if (beginTime == null || endTime == null)
            {
                if (beginTime == null)
                {
                    beginTime = Convert.ToDateTime("2015-01-01 00:00:00");
                }
                if (endTime == null)
                {
                    endTime = DateTime.Now;
                }
            }
            SqlParameter[] parameter = new SqlParameter[4];
            parameter[0] = new SqlParameter("@startTime", beginTime);
            parameter[1] = new SqlParameter("@closeTime", endTime);
            parameter[2] = new SqlParameter("@type", type);
            parameter[3] = new SqlParameter("@param", param);

            try
            {
                DataTable dt = context.Database.SqlQueryForDataTatable("EXEC dbo.ProcOrderTimeCount @startTime,@closeTime,@type,@param", parameter);
                if (dt == null || dt.Rows.Count == 0)
                {
                    result.Message = "没有可导出数据！";
                    return result;
                }

                var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                string[] fields = null;
                string[] names = null;

                switch (type)
                {
                    case 1:
                        dt.Columns.Remove("OrderTimeType");
                        dt.Columns.Remove("ID");
                        dt.Columns.Remove("Name");
                        dt.Columns.Remove("GroupName");
                        dt.Columns.Remove("Size");
                        fields = new string[] { "dicName", "TotalPrice", "Num" };
                        names = new string[] { "时间段", "金额", "数量" };
                        break;
                    case 2:
                        dt.Columns.Remove("ID");
                        dt.Columns.Remove("GroupName");
                        dt.Columns.Remove("OrderTimeType");
                        dt.Columns.Remove("Size");
                        fields = new string[] { "dicName", "TotalPrice", "Num" };
                        names = new string[] { "时间段", "总金额", "数量" };
                        break;
                    case 3:
                        dt.Columns.Remove("ID");
                        dt.Columns.Remove("GroupName");
                        dt.Columns.Remove("OrderTimeType");
                        fields = new string[] { "dicName", "TotalPrice", "Num", "Name", "Size" };
                        names = new string[] { "时间段", "总金额", "数量", "名称", "磅数" };
                        break;
                    case 4:
                        dt.Columns.Remove("ID");
                        dt.Columns.Remove("GroupName");
                        dt.Columns.Remove("OrderTimeType");
                        fields = new string[] { "dicName", "TotalPrice", "Num", "Name", "Size" };
                        names = new string[] { "时间段", "总金额", "数量", "名称", "磅数" };
                        break;
                }

                FCake.Core.Common.ExportExcel excelExport = new ExportExcel();
                string fileUrl = excelExport.ToExcel(fileName, dt, fields, names, null);


                return OpResult.Success("导出成功", null, fileUrl);
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }
            return result;
        }
        /// <summary>
        /// 最开始做的财务报表，没有订单详细
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public OpResult GetFinancialByTime<T>(DateTime? beginTime, DateTime? endTime, DateTime? reqBeginTime, DateTime? reqEndTime, int? reviewStatus, int? pageSize, int? pageIndex, int? orderStatus, out int totalCount)
        {
            //var virtualPath = string.Empty;
            //var filePath = GetTempExcelFilePath(out virtualPath);
            totalCount = 0;
            SqlParameter[] param = new SqlParameter[9];
            if (beginTime.HasValue)
            {
                param[0] = new SqlParameter("@beginTime", beginTime);
            }
            else
            {
                param[0] = new SqlParameter("@beginTime", DBNull.Value);
            }
            if (endTime.HasValue)
            {
                param[1] = new SqlParameter("@endTime", endTime);
            }
            else
            {
                param[1] = new SqlParameter("@endTime", DBNull.Value);
            }

            if (reqBeginTime.HasValue)
            {
                param[2] = new SqlParameter("@reqStartTime", reqBeginTime);
            }
            else
            {
                param[2] = new SqlParameter("@reqStartTime", DBNull.Value);
            }
            if (reqEndTime.HasValue)
            {
                param[3] = new SqlParameter("@reqCloseTime", reqEndTime);
            }
            else
            {
                param[3] = new SqlParameter("@reqCloseTime", DBNull.Value);
            }
            param[4] = new SqlParameter("@reviewStatus", reviewStatus);
            param[5] = new SqlParameter("@orderStatus", orderStatus);
            param[6] = new SqlParameter("@pageSize", 66000);//66000全部导出
            param[7] = new SqlParameter("@pageIndex", pageIndex);
            param[8] = new SqlParameter("@totalRecord", totalCount);
            try
            {
                DataTable dt = context.Database.SqlQueryForDataTatable("EXEC dbo.Proc_FinancialReporting @beginTime=@beginTime,@endTime=@endTime,@reqStartTime=@reqStartTime,@reqCloseTime=@reqCloseTime,@reviewStatus=@reviewStatus,@orderStatus=@orderStatus,@pageSize=@pageSize,@pageIndex=@pageIndex,@totalRecord=@totalRecord output", param);
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                //#region 设置数据源
                ////删除Sorting
                dt.Columns.Remove("Sorting");
                //替换字典值
                var feetypes = EnumHelper.GetList(typeof(FeeType));
                var feeDic = feetypes.ToDictionary<DropdownItem, string>(f => f.Value);
                foreach (DataRow row in dt.Rows)
                {
                    if (row["FeeType"] != null)
                    {
                        var fee = row["FeeType"].ToString();
                        if (feeDic.ContainsKey(fee))
                        {
                            row["FeeType"] = feeDic[fee].Text;
                        }
                    }
                }
                ////设置中文列名
                //dt.Columns["OrderNo"].ColumnName = "订单号";
                //dt.Columns["CreatedOn"].ColumnName = "订单时间";
                //dt.Columns["FeeType"].ColumnName = "支付方式";
                //dt.Columns["TotalPrice"].ColumnName = "总价";
                //dt.Columns["RealPay"].ColumnName = "支付金额";
                //dt.Columns["CouponPay"].ColumnName = "优惠券支付";
                //dt.Columns["TotalMoney"].ColumnName = "总额";
                //#endregion
                string[] fields = { "OrderNo", "CreatedOn", "RequiredTime", "FeeType", "OrderStatus", "ReviewStatus", "TotalPrice", "RealPay", "CouponPay", "TotalMoney" };
                string[] names = { "订单号", "订单时间", "要求送达时间", "支付方式", "订单状态", "审核状态", "总价", "支付金额", "优惠券支付", "总额" };


                //FCake.Core.Common.ExcelHelper.Export(dt, beginTime + "--" + endTime, filePath);

                FCake.Core.Common.ExportExcel excelExport = new ExportExcel();
                string fileUrl = excelExport.ToExcel(fileName, dt, fields, names, null);


                return OpResult.Success("导出成功", null, fileUrl);
            }
            catch (Exception ex)
            {
                return OpResult.Fail("导出失败:" + ex.Message);
            }

        }
        /// <summary>
        /// 导出订单明细报表
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public OpResult ExportOrderReport(DateTime? beginTime, DateTime? endTime, DateTime? reqBeginTime, DateTime? reqEndTime, int? reviewStatus, int? pageSize, int? pageIndex, int? orderStatus, out int totalCount)
        {
            totalCount = 0;
            try
            {

                var fileName = DateTime.Now.ToString("yy.MM.dd");
                //var virtualPath = string.Empty;
                //var filePath = GetTempExcelFilePath(out virtualPath);
                SqlParameter[] param = new SqlParameter[9];
                if (beginTime.HasValue)
                {
                    param[0] = new SqlParameter("@beginTime", beginTime);
                }
                else
                {
                    param[0] = new SqlParameter("@beginTime", DBNull.Value);
                }
                if (endTime.HasValue)
                {
                    param[1] = new SqlParameter("@endTime", endTime);
                }
                else
                {
                    param[1] = new SqlParameter("@endTime", DBNull.Value);
                }

                if (reqBeginTime.HasValue)
                {
                    param[2] = new SqlParameter("@reqStartTime", reqBeginTime);
                }
                else
                {
                    param[2] = new SqlParameter("@reqStartTime", DBNull.Value);
                }
                if (reqEndTime.HasValue)
                {
                    param[3] = new SqlParameter("@reqCloseTime", reqEndTime);
                }
                else
                {
                    param[3] = new SqlParameter("@reqCloseTime", DBNull.Value);
                }
                param[4] = new SqlParameter("@reviewStatus", reviewStatus);
                param[5] = new SqlParameter("@orderStatus", orderStatus);
                param[6] = new SqlParameter("@pageSize", 66000);//66000全部导出
                param[7] = new SqlParameter("@pageIndex", pageIndex);
                param[8] = new SqlParameter("@totalRecord", totalCount);
                var dt = context.Database.SqlQueryForDataTatable("EXEC Proc_FinanceReport @startTime=@beginTime,@closeTime=@endTime,@reqStartTime=@reqStartTime,@reqCloseTime=@reqCloseTime,@reviewStatus=@reviewStatus,@orderStatus=@orderStatus,@pageSize=@pageSize,@pageIndex=@pageIndex,@totalRecord=@totalRecord output", param);


                string[] fields = { "OrderNo", "OrderSource", "OrderTime", "ReviceTime", "ProductName", "ReviewStatus", "OrderStatus", "SizeTitle", "Number", "ProductPrice", "NeedPay", "ActualPay", "TotalPrice", "CouponPay", "GiftCardPay", "IntegralPay", "PayWay", "DeliverMsg", "Remark" };
                string[] names = { "订单号", "订单来源", "下单日期", "送货日期", "产品名称", "审核状态", "订单状态", "规格", "数量", "单价", "需支付", "实际支付", "总价", "优惠券支付", "代金卡支付", "积分支付", "支付方式", "客户留言", "订单备注" };

                int[] merger = { 0, 1, 2, 3, 5, 6, 10, 11, 12, 13, 14, 15, 16, 17, 18 };

                FCake.Core.Common.ExportExcel excelExport = new ExportExcel();
                string fileUrl = excelExport.ToExcel(fileName, dt, fields, names, merger);
                return OpResult.Success("导出成功", null, fileUrl);
            }
            catch (Exception e)
            {
                return OpResult.Fail("导出失败:" + e.Message);
            }

        }
        public string GetTempExcelFilePath(out string virtualPath)
        {
            //判断是否有对应月份的临时文件夹，若没有则创建
            var dirName = DateTime.Now.ToString("yyyyMMdd");
            virtualPath = "/TempFile/Excel/" + dirName;
            string dirPath = HttpContext.Current.Server.MapPath(virtualPath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xls";
            virtualPath += "/" + fileName;
            return dirPath + "/" + fileName;
        }
        /// <summary>
        /// 订单，订单明细报表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public DataTable OrderReport(DateTime? beginTime, DateTime? endTime, DateTime? reqBeginTime, DateTime? reqEndTime, int? reviewStatus, int pageSize, int pageIndex, int orderStatus, out int totalCount)
        {
            totalCount = 0;
            SqlParameter[] param = new SqlParameter[9];
            if (beginTime.HasValue)
            {
                param[0] = new SqlParameter("@beginTime", beginTime);
            }
            else
            {
                param[0] = new SqlParameter("@beginTime", DBNull.Value);
            }
            if (endTime.HasValue)
            {
                endTime = DateTime.Parse(endTime.ToString()).AddHours(23).AddMinutes(59).AddSeconds(59);
                param[1] = new SqlParameter("@endTime", endTime);
            }
            else
            {
                param[1] = new SqlParameter("@endTime", DBNull.Value);
            }

            if (reqBeginTime.HasValue)
            {
                param[2] = new SqlParameter("@reqStartTime", reqBeginTime);
            }
            else
            {
                param[2] = new SqlParameter("@reqStartTime", DBNull.Value);
            }
            if (reqEndTime.HasValue)
            {
                reqEndTime = DateTime.Parse(reqEndTime.ToString()).AddHours(23).AddMinutes(59).AddSeconds(59);
                param[3] = new SqlParameter("@reqCloseTime", reqEndTime);
            }
            else
            {
                param[3] = new SqlParameter("@reqCloseTime", DBNull.Value);
            }
            param[4] = new SqlParameter("@reviewStatus", reviewStatus);
            param[5] = new SqlParameter("@orderStatus", orderStatus);
            param[6] = new SqlParameter("@pageSize", pageSize);
            param[7] = new SqlParameter("@pageIndex", pageIndex);
            param[8] = new SqlParameter("@totalRecord", totalCount);
            param[8].Direction = ParameterDirection.Output;

            //var dt = context.Database.SqlQueryForDataTatable("EXEC Proc_FinanceReport @beginTime,@endTime", param);
            var result = context.Database.SqlQueryForDataTatable("EXEC Proc_FinanceReport @startTime=@beginTime,@closeTime=@endTime,@reqStartTime=@reqStartTime,@reqCloseTime=@reqCloseTime,@reviewStatus=@reviewStatus,@orderStatus=@orderStatus,@pageSize=@pageSize,@pageIndex=@pageIndex,@totalRecord=@totalRecord output", param);
            totalCount = int.Parse(param[8].Value.ToString());
            return result;
        }

        #region 销售统计报表
        public dynamic GetSalesStatisticsReportData<T>(int year, out int totalCount)
        {
            totalCount = 0;
            SqlParameter[] param = new SqlParameter[1];
            param[0] = new SqlParameter("@year", year);
            var dtSource = context.Database.SqlQueryForDataTatable("EXEC Proc_SalesStatisticsReport @year=@year", param);


            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("OMonth", typeof(string));
            dtResult.Columns.Add("OrderClient", typeof(string));
            dtResult.Columns.Add("OrderQuantity", typeof(string));
            dtResult.Columns.Add("ActualPay", typeof(string));
            dtResult.Columns.Add("CouponPay", typeof(string));
            dtResult.Columns.Add("GiftCardPay", typeof(string));
            dtResult.Columns.Add("IntegralPay", typeof(string));
            dtResult.Columns.Add("TotalPrice", typeof(string));

            int yearOrderQuantity = 0;
            decimal yearActualPay = 0;
            decimal yearCouponPay = 0;
            decimal yearGiftCardPay = 0;
            decimal yearIntegralPay = 0;
            decimal yearTotalPrice = 0;

            for (var m = 1; m <= 12; m++)
            {
                int curMonthOrderQuantity = 0;
                decimal curMonthActualPay = 0;
                decimal curMonthCouponPay = 0;
                decimal curMonthGiftCardPay = 0;
                decimal curMonthIntegralPay = 0;
                decimal curMonthTotalPrice = 0;

                for (var i = 0; i < dtSource.Rows.Count; i++)
                {

                    int OMonth = int.Parse(string.IsNullOrEmpty(dtSource.Rows[i][0].ToString()) ? "0" : dtSource.Rows[i][0].ToString());
                    string OrderClient = string.IsNullOrEmpty(dtSource.Rows[i][1].ToString()) ? "电话端" :
                        (int.Parse(dtSource.Rows[i][1].ToString()) == 0 ? "PC端" : (int.Parse(dtSource.Rows[i][1].ToString()) == 1 ? "手机端" : "电话端"));
                    int OrderQuantity = int.Parse(string.IsNullOrEmpty(dtSource.Rows[i][2].ToString()) ? "0" : dtSource.Rows[i][2].ToString());
                    decimal ActualPay = decimal.Parse(string.IsNullOrEmpty(dtSource.Rows[i][3].ToString()) ? "0" : dtSource.Rows[i][3].ToString());
                    decimal CouponPay = decimal.Parse(string.IsNullOrEmpty(dtSource.Rows[i][4].ToString()) ? "0" : dtSource.Rows[i][4].ToString());
                    decimal GiftCardPay = decimal.Parse(string.IsNullOrEmpty(dtSource.Rows[i][5].ToString()) ? "0" : dtSource.Rows[i][5].ToString());
                    decimal IntegralPay = decimal.Parse(string.IsNullOrEmpty(dtSource.Rows[i][6].ToString()) ? "0" : dtSource.Rows[i][6].ToString());
                    decimal TotalPrice = decimal.Parse(string.IsNullOrEmpty(dtSource.Rows[i][7].ToString()) ? "0" : dtSource.Rows[i][7].ToString());

                    if (OMonth == m)
                    {
                        curMonthOrderQuantity += OrderQuantity;
                        curMonthActualPay += ActualPay;
                        curMonthCouponPay += CouponPay;
                        curMonthGiftCardPay += GiftCardPay;
                        curMonthIntegralPay += IntegralPay;
                        curMonthTotalPrice += TotalPrice;
                        DataRow row = dtResult.NewRow();
                        row["OMonth"] = OMonth.ToString();
                        row["OrderClient"] = OrderClient.ToString();
                        row["OrderQuantity"] = OrderQuantity.ToString();
                        row["ActualPay"] = ActualPay.ToString("N2");
                        row["CouponPay"] = CouponPay.ToString("N2");
                        row["GiftCardPay"] = GiftCardPay.ToString("N2");
                        row["IntegralPay"] = IntegralPay.ToString("N2");
                        row["TotalPrice"] = TotalPrice.ToString("N2");
                        dtResult.Rows.Add(row);
                    }

                }
                yearOrderQuantity += curMonthOrderQuantity;
                yearActualPay += curMonthActualPay;
                yearCouponPay += curMonthCouponPay;
                yearGiftCardPay += curMonthGiftCardPay;
                yearIntegralPay += curMonthIntegralPay;
                yearTotalPrice += curMonthTotalPrice;
                DataRow curMonthRow = dtResult.NewRow();
                curMonthRow["OMonth"] = m.ToString();
                curMonthRow["OrderClient"] = "小计";
                curMonthRow["OrderQuantity"] = curMonthOrderQuantity.ToString();
                curMonthRow["ActualPay"] = curMonthActualPay.ToString("N2");
                curMonthRow["CouponPay"] = curMonthCouponPay.ToString("N2");
                curMonthRow["GiftCardPay"] = curMonthGiftCardPay.ToString("N2");
                curMonthRow["IntegralPay"] = curMonthIntegralPay.ToString("N2");
                curMonthRow["TotalPrice"] = curMonthTotalPrice.ToString("N2");
                dtResult.Rows.Add(curMonthRow);


            }



            DataRow yearRow = dtResult.NewRow();
            yearRow["OMonth"] = "全年";
            yearRow["OrderClient"] = "统计";
            yearRow["OrderQuantity"] = yearOrderQuantity.ToString();
            yearRow["ActualPay"] = yearActualPay.ToString("N2");
            yearRow["CouponPay"] = yearCouponPay.ToString("N2");
            yearRow["GiftCardPay"] = yearGiftCardPay.ToString("N2");
            yearRow["IntegralPay"] = yearIntegralPay.ToString("N2");
            yearRow["TotalPrice"] = yearTotalPrice.ToString("N2");
            dtResult.Rows.Add(yearRow);

            totalCount = dtResult.Rows.Count;
            return dtResult;
        }
        #endregion
        #region 首页报表
        /// <summary>
        /// 根据时间段获得销售量前10的商品
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="topProductName"></param>
        /// <param name="topProductSaleNum"></param>
        public void SaleTopProduct(DateTime beginTime, DateTime endTime, out List<string> topProductName, out List<int> topProductSaleNum)
        {
            topProductName = new List<string>();
            topProductSaleNum = new List<int>();
            int i = 0;
            SqlParameter[] param = new SqlParameter[3];
            param[0] = new SqlParameter("@startDate", beginTime);
            param[1] = new SqlParameter("@endDate", endTime);
            param[2] = new SqlParameter("@type", "1");
            var dt = context.Database.SqlQueryForDataTatable("EXEC Proc_IndexSalesData @startDate=@startDate, @endDate=@endDate, @type=@type", param);
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                topProductName.Add(dr["Name"].ToString());
                topProductSaleNum.Add(Convert.ToInt32(dr["Quantity"]));

                if (i >= 9)
                {
                    break;
                }
                i++;
            }

        }
        #endregion

        #region 统计分析
        public List<StatisticalAnalysisModel> StatisticalAnalysis(DateTime beginTime, DateTime endTime, int type, out List<string> title)
        {
            title = new List<string>();
            List<StatisticalAnalysisModel> result = new List<StatisticalAnalysisModel>();
            SqlParameter[] param = new SqlParameter[3];
            param[0] = new SqlParameter("@startDate", beginTime);
            param[1] = new SqlParameter("@endDate", endTime);
            param[2] = new SqlParameter("@type", type);
            var dt = context.Database.SqlQueryForDataTatable("EXEC Proc_StatisticalAnalysis @startDate=@startDate, @endDate=@endDate, @type=@type", param);
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                StatisticalAnalysisModel model = new StatisticalAnalysisModel();
                model.name = dr["name"].ToString();
                model.value = Math.Round(Convert.ToDecimal(dr["value"]), 2, MidpointRounding.AwayFromZero);
                result.Add(model);
                title.Add(model.name);
            }
            return result;
        }
        #endregion
    }
}
