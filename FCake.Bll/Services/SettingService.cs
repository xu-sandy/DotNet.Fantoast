using FCake.Core.Common;
using FCake.Core.MvcCommon;
using FCake.Domain;
using FCake.Domain.Common;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Bll.Services
{
    public class SettingService
    {
        EFDbContext context = new EFDbContext();

        public List<DictionaryType> FilterDictionary(DictionaryType dictionary, out int totalCount, PageInfo pageInfo)
        {
            var name = StringHelper.IsNEAndTrim(dictionary.Name);
            var code = StringHelper.IsNEAndTrim(dictionary.Code);

            var result = context.DictionaryType.Where(p => p.IsDeleted != 1);
            if (name != "")
            {
                result = result.Where(d => d.Name.Contains(name));
            }
            if (code != "")
            {
                result = result.Where(d => d.Code.Contains(code));
            }
            result = result.OrderBy(d => d.Code);
            List<DictionaryType> data = null;
            totalCount = result.Count();
            if (pageInfo != null)
            {
                data = result.Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows).ToList();
            }
            else
            {
                data = result.ToList();
            }
            return data;
        }

        public DictionaryType GetDictionaryTypeById(string id)
        {
            var result = context.DictionaryType.Where(d => d.Id == id).FirstOrDefault();
            if (result == null)
                result = new DictionaryType();
            return result;
        }

        public bool DictionaryCodeExsist(string code)
        {
            var user = (from tb in context.DictionaryType
                        where tb.Code == code && tb.IsDeleted != 1
                        select tb).FirstOrDefault();
            if (user != null)
                return true;
            else
                return false;
        }

        public OpResult SaveDictionaryType(DictionaryType dictionary, string userId)
        {
            OpResult result = null;
            var newDictionary = dictionary;
            try
            {

                if (string.IsNullOrEmpty(dictionary.Id))
                {
                    newDictionary.Id = DataHelper.GetSystemID();
                    newDictionary.CreatedBy = userId;
                    newDictionary.CreatedOn = DateTime.Now;
                    newDictionary.IsDeleted = 0;
                    context.DictionaryType.Add(newDictionary);
                }
                else
                {
                    newDictionary = context.DictionaryType.Where(d => d.Id == dictionary.Id).FirstOrDefault();
                    if (newDictionary != null)
                    {
                        newDictionary.Name = dictionary.Name;
                        newDictionary.Code = dictionary.Code;
                        newDictionary.ModifiedBy = userId;
                        newDictionary.ModifiedOn = DateTime.Now;
                    }
                }
                if (newDictionary != null)
                {
                    context.SaveChanges();
                    result = new OpResult() { Successed = true, Message = "数据保存成功", Data = newDictionary };
                }
                else
                {
                    throw new Exception("非法数据，保存失败");
                }
            }
            catch (Exception ex)
            {
                result = new OpResult() { Successed = false, Message = ex.Message };
            }
            return result;
        }

        public List<DictionaryData> GetDictionaryDataByDicCode(string dicCode, out int totalCount, PageInfo pageInfo)
        {
            var data = context.DictionaryData.Where(d => d.DicCode == dicCode && d.IsDeleted != 1);
            totalCount = data.Count();
            var result = data.OrderBy(a=>a.Sorting).Skip((pageInfo.Page - 1) * pageInfo.Rows).Take(pageInfo.Rows).ToList();
            return result;
        }
    }
}
