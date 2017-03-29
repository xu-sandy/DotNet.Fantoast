using FCake.Core.MvcCommon;
using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using FCake.Core.Common;
using FCake.Domain.Common;
using System.Data;
using System.Data.SqlClient;

namespace FCake.Bll
{
    public class CommonService
    {
        EFDbContext context = new EFDbContext();
        public List<DictionaryData> GetDictionaryData(string code)
        {
            var query = context.DictionaryData.Where(p => p.IsDeleted != 1 && p.State != 1);
            if (code.Trim() != "" && code != null)
            {
                query = query.Where(p => p.DicCode == code);
            }
            query = query.OrderBy(o => o.Sorting);
            return query.ToList();
        }
        /// <summary>
        /// 从新的字典表里查下拉数据
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public List<DropdownItem> GetDictionaryByCode(string code, bool isall = false)
        {
            return GetDictionaryByCode(code, null, isall);
        }
        public List<DropdownItem> GetDictionaryByCode(string code,string groupName, bool isall = false)
        {
            var result = new List<DropdownItem>();
            var query = context.DictionaryData.Where(p => p.IsDeleted != 1 && p.State != 1);
            if (code.Trim() != "" && code != null)
            {
                query = query.Where(p => p.DicCode == code);
            }
            if (!groupName.IsNullOrTrimEmpty()) {
                query = query.Where(p => p.GroupName == groupName);
            }
            var data = query.ToList();
            if (isall)
                result.Add(new DropdownItem() { Value = "", Text = "不限",Sorting=-1 });
            if (data.Count() > 0)
            {
                result.AddRange(from tb in data select new DropdownItem() { Value = tb.Value, Text = tb.Name,Sorting=tb.Sorting });
            }
            return result;
        }

        #region 获得数据字典下拉(Value= 数据字典值,Text=数据字典描述)
        public List<DropdownItem> GetDictionaryDescriptionByCode(string code, bool isall = false)
        {
            return GetDictionaryDescriptionByCode(code, null, isall);
        }
        public List<DropdownItem> GetDictionaryDescriptionByCode(string code, string groupName, bool isall = false)
        {
            var result = new List<DropdownItem>();
            var query = context.DictionaryData.Where(p => p.IsDeleted != 1 && p.State != 1);
            if (code.Trim() != "" && code != null)
            {
                query = query.Where(p => p.DicCode == code);
            }
            if (!groupName.IsNullOrTrimEmpty())
            {
                query = query.Where(p => p.GroupName == groupName);
            }
            var data = query.ToList();
            if (isall)
                result.Add(new DropdownItem() { Value = "", Text = "不限", Sorting = -1 });
            if (data.Count() > 0)
            {
                result.AddRange(from tb in data select new DropdownItem() { Value = tb.Value, Text = tb.Description, Sorting = tb.Sorting });
            }
            return result;
        }
        #endregion


        public string GetDictionaryName(string code, string value)
        {
            var query = context.DictionaryData.Where(p => p.IsDeleted != 1&&p.DicCode==code&&p.Value==value);
            if (query.Any())
                return query.First().Name;
            return "";
        }

        #region 数据操作
        public string Save<T>(T source, EFDbContext context, UserData currentUser, bool savechange = true)
        {
            try
            {
                //获取ID数据
                var t = typeof(T);
                var crud = typeof(CurdService);
                var contexttype = typeof(EFDbContext);
                var ps = t.GetProperties();
                PropertyInfo p = null;
                object id_value = "";
                bool check = false;
                foreach (var x in ps)
                {
                    if (x.Name.ToLower().Equals("id"))
                    {
                        p = x;
                        check = true;
                        id_value = x.GetValue(source, null);
                    }
                }
                //如果没有ID属性 
                if (!check)
                    throw new Exception("此类无ID，无法保存数据");

                var t1 = typeof(DbSet<>).MakeGenericType(t);
                var et = typeof(EFDbContext);
                dynamic collection = null;
                foreach (var x in et.GetProperties())
                {
                    if (t1.IsAssignableFrom(x.PropertyType))
                    {
                        collection = x.GetValue(context, null);
                        break;
                    }
                }
                if (collection == null)
                    throw new Exception(string.Format("在EFDbContext中查无类型为DbSet<{0}>的属性", t.Name));
                Regex reg = new Regex("^_");


                if (id_value == null || id_value.ToString().Trim() == "" || reg.IsMatch(id_value.ToString()))
                {
                    //添加ID
                    if (id_value != null && reg.IsMatch(id_value.ToString()))
                        p.SetValue(source, id_value.ToString().Substring(1), null);
                    else
                        p.SetValue(source, FCake.Core.Common.DataHelper.GetSystemID(), null);

                    if (currentUser != null)
                    {
                        var CreatedBy = ps.SingleOrDefault(a => a.Name.Equals("CreatedBy", StringComparison.OrdinalIgnoreCase));
                        if (CreatedBy != null)
                            CreatedBy.SetValue(source, currentUser.Id, null);
                        var CreatedOn = ps.SingleOrDefault(a => a.Name.Equals("CreatedOn", StringComparison.OrdinalIgnoreCase));
                        if (CreatedOn != null)
                            CreatedOn.SetValue(source, DateTime.Now, null);
                        var IsDeleted = ps.SingleOrDefault(a => a.Name.Equals("IsDeleted", StringComparison.OrdinalIgnoreCase));
                        if (IsDeleted != null)
                            IsDeleted.SetValue(source, 0, null);

                    }

                    //处理数据
                    var function = crud.GetMethod("CheckCreate", new Type[] { t, contexttype });
                    if (function != null)
                    {
                        //throw new Exception("在Crud查无CheckCreate方法，无法保存数据");
                        var result = function.Invoke(new CurdService(), new object[] { source, context }).ToString();
                        if (result.IsNullOrTrimEmpty() == false)
                            throw new Exception(result);
                    }

                    //添加数据
                    function = t1.GetMethod("Add", new Type[] { t });
                    function.Invoke(collection, new object[] { source });

                    //存入数据库
                    if (savechange)
                        context.SaveChanges();
                }
                else
                {
                    var function = crud.GetMethod("GetByID", new Type[] { typeof(object), typeof(string) });
                    var r = function.Invoke(new CurdService(), new object[] { collection, id_value });
                    //首先在数据库得存在
                    if (r == null)
                        throw new Exception("查无此数据，无法保存，ID为：" + id_value);

                    //复制属性
                    r.CopyProperty<dynamic>(source);

                    if (currentUser != null)
                    {
                        var ModifiedBy = ps.SingleOrDefault(a => a.Name.Equals("ModifiedBy", StringComparison.OrdinalIgnoreCase));
                        if (ModifiedBy != null)
                            ModifiedBy.SetValue(r, currentUser.Id, null);
                        var ModifiedOn = ps.SingleOrDefault(a => a.Name.Equals("ModifiedOn", StringComparison.OrdinalIgnoreCase));
                        if (ModifiedOn != null)
                            ModifiedOn.SetValue(r, DateTime.Now, null);
                        var IsDeleted = ps.SingleOrDefault(a => a.Name.Equals("IsDeleted", StringComparison.OrdinalIgnoreCase));
                        if (IsDeleted != null)
                            IsDeleted.SetValue(r, 0, null);

                    }

                    //处理数据
                    function = crud.GetMethod("CheckEdit", new Type[] { t, contexttype });
                    if (function != null)
                    {
                       // throw new Exception("查无CheckEdit方法，无法保存数据");
                        var result = function.Invoke(new CurdService(), new object[] { r, context }).ToString();
                        if (result.IsNullOrTrimEmpty() == false)
                            throw new Exception(result);
                    }


                    //存入数据库
                    if (savechange)
                        context.SaveChanges();

                }


            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        public string Delete<T>(T source, EFDbContext context, UserData currentUser, bool savechange = true)
        {
            try
            {

                var t = typeof(T);
                var crud = typeof(CurdService);
                var contexttype = typeof(EFDbContext);
                var ps = t.GetProperties();
                PropertyInfo p = null;
                object id_value = "";
                bool check = false;
                foreach (var x in ps)
                {
                    if (x.Name.ToLower().Equals("id"))
                    {
                        p = x;
                        check = true;
                        id_value = x.GetValue(source, null);
                    }
                }
                //如果没有ID属性 
                if (!check)
                    throw new Exception("此类无ID，无法保存数据");

                var t1 = typeof(DbSet<>).MakeGenericType(t);
                var et = typeof(EFDbContext);
                dynamic collection = null;
                foreach (var x in et.GetProperties())
                {
                    if (t1.IsAssignableFrom(x.PropertyType))
                    {
                        collection = x.GetValue(context, null);
                        break;
                    }
                }
                if (collection == null)
                    throw new Exception(string.Format("在EFDbContext中查无类型为DbSet<{0}>的属性", t.Name));

                if (id_value == null || id_value.ToString().Trim() == "")
                    throw new Exception("ID为空，无法保存数据");

                var function = crud.GetMethod("GetByID", new Type[] { typeof(object), typeof(string) });
                var r = function.Invoke(new CurdService(), new object[] { collection, id_value });
                //首先在数据库得存在
                if (r == null)
                    throw new Exception("查无此数据，无法保存，ID为：" + id_value);

                //处理数据
                function = crud.GetMethod("CheckDelete", new Type[] { t, contexttype });
                if (function != null)
                {
                    //throw new Exception("查无CheckDelete方法，无法保存数据");
                    var result = function.Invoke(new CurdService(), new object[] { source, context }).ToString();
                    if (result.IsNullOrTrimEmpty() == false)
                        throw new Exception(result);
                }

                //删除数据
                //function = t1.GetMethod("Remove", new Type[] { t });
                //function.Invoke(collection, new object[] { r });

                if (currentUser != null)
                {
                    var ModifiedBy = ps.SingleOrDefault(a => a.Name.Equals("ModifiedBy", StringComparison.OrdinalIgnoreCase));
                    if (ModifiedBy != null)
                        ModifiedBy.SetValue(r, currentUser.Id, null);
                    var ModifiedOn = ps.SingleOrDefault(a => a.Name.Equals("ModifiedOn", StringComparison.OrdinalIgnoreCase));
                    if (ModifiedOn != null)
                        ModifiedOn.SetValue(r, DateTime.Now, null);
                    var IsDeleted = ps.SingleOrDefault(a => a.Name.Equals("IsDeleted", StringComparison.OrdinalIgnoreCase));
                    if (IsDeleted != null)
                        IsDeleted.SetValue(r, 1, null);
                }

                //存入数据库
                if (savechange)
                    context.SaveChanges();
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
        #endregion

        public IQueryable<T> RemoveDelete<T>(IQueryable<T> source)
            where T:BaseEntity
        {
            return source.Where(a => a.IsDeleted != 1);
        }

        #region 系统基础信息(版本号)
        /// <summary>
        /// 获取系统版本号
        /// </summary>
        /// <returns></returns>
        public string GetVersion() {
            var query = context.SysConfigs.Where(s => s.Code == "version");
            var data = query.FirstOrDefault();
            if (data != null)
            {
                return data.Value;
            }
            return string.Empty;
        }
        #endregion
    }

    public static class StaticCommonService
    {
        public static DataTable SqlQueryForDataTatable(this Database db,
                    string sql,
                    SqlParameter[] parameters)
        {

            SqlConnection conn = new System.Data.SqlClient.SqlConnection();
            conn.ConnectionString = db.Connection.ConnectionString;
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;

            if (parameters.Length > 0)
            {
                foreach (var item in parameters)
                {
                    cmd.Parameters.Add(item);
                }
            }


            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }
    }
}
