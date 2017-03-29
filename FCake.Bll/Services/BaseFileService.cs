using FCake.Core.Common;
using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace FCake.Bll
{
    public class BaseFileService
    {
        private EFDbContext _context = new EFDbContext();
        /// <summary>
        /// 更具ID返回文件地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetImgUrlById(string id)
        {
            var bf = _context.BaseFiles.SingleOrDefault(a => a.Id == id && a.IsDeleted != 1);
            if (bf != null)
            {
                if (System.IO.File.Exists(HttpContext.Current.Server.MapPath(bf.Url)))
                    return bf.Url;
            }
            return "";
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="files"></param>
        /// <param name="linkid"></param>
        /// <param name="filetype"></param>
        /// <param name="currentUserId"></param>
        /// <param name="width">200</param>
        /// <param name="height">200</param>
        /// <param name="iscut"></param>
        /// <returns></returns>
        public List<BaseFile> SaveBaseFiles(HttpFileCollectionBase files, string linkid, string filetype, string currentUserId)
        {
            List<BaseFile> bf = new List<BaseFile>();
            Regex reg = new Regex(@"[^.]+$");
            for (int i = 0; i < files.Count; i++)
            {
                var f = files[i];
                var name = f.FileName;
                var newname = DataHelper.GetSystemID();

                if (f.ContentLength > 3 * 1024 * 1024)
                    throw new Exception("上传文件大小超过了3M");

                if (filetype == "image")
                {
                    Regex r = new Regex(@"\.(jpg|jpeg|gif|png)$");
                    if (r.IsMatch(name) == false)
                        throw new Exception("上传文件格式不正确：" + name);
                }


                string dir = "/files/" + DateTime.Now.ToString("yyyy-MM") + "/" + DateTime.Now.ToString("dd");

                if (Directory.Exists(HttpContext.Current.Server.MapPath(dir)) == false)
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir));

                string u = dir + "/" + newname;
                if (reg.IsMatch(name))
                    u += "." + reg.Match(name).ToString();

                f.SaveAs(HttpContext.Current.Server.MapPath(u));
                if (filetype == "image")
                {
                    PictureHelper.MakeThumbnail(HttpContext.Current.Server.MapPath(u), HttpContext.Current.Server.MapPath(u + "_middle.jpg"), 200, 200, "Cut");
                    //PictureHelper.ResetPicture(f, width, height, iscut == 0 ? "cut" : "maxsize", HttpContext.Current.Server.MapPath(u + "_thumb.jpg"));
                }

                BaseFile temp = new BaseFile();
                temp.Id = newname;
                temp.Length = f.ContentLength;
                temp.IsDeleted = 0;
                temp.LinkId = linkid;
                temp.CreatedBy = currentUserId;
                temp.CreatedOn = DateTime.Now;
                temp.ModifiedBy = currentUserId;
                temp.ModifiedOn = DateTime.Now;
                temp.NewName = newname;
                temp.OldName = name;
                if (reg.IsMatch(name))
                    temp.SuffixName = reg.Match(name).ToString();
                temp.Url = u;
                _context.BaseFiles.Add(temp);
                _context.SaveChanges();
                bf.Add(temp);
            }

            return bf;
        }

        /// <summary>
        /// 根据LinkId获取对应附件或图片等信息集合
        /// </summary>
        /// <param name="linkId"></param>
        /// <returns></returns>
        public List<BaseFile> GetBaseFiles(string linkId)
        {
            var query = this._context.BaseFiles.Where(a => a.LinkId.Equals(linkId, StringComparison.OrdinalIgnoreCase) && a.IsDeleted == 0).OrderBy(f => f.Sorting);
            return query.ToList();
        }

        public List<BaseFile> SaveBaseFiles(List<BaseFile> files, string linkId, string currentUserId)
        {
            var nowDate = DateTime.Now;
            var entities = new List<BaseFile>();
            if (files != null && files.Count > 0)
            {
                var oldFiles = GetBaseFiles(linkId);
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    if (file != null)
                    {
                        var entity = oldFiles.Where(f => f.Sorting == file.Sorting).FirstOrDefault();
                        if (entity == null)
                        {
                            entity = new BaseFile();
                            entity.Id = file.NewGuid();
                            entity.LinkId = linkId;
                            entity.CreatedBy = currentUserId;
                            entity.CreatedOn = nowDate;
                            entity.Sorting = file.Sorting;
                            this._context.BaseFiles.Add(entity);
                        }
                        entity.Length = file.Length;
                        entity.IsDeleted = 0;
                        entity.ModifiedBy = currentUserId;
                        entity.ModifiedOn = nowDate;
                        entity.OldName = file.OldName;
                        entity.NewName = file.NewName;
                        entity.SuffixName = file.SuffixName;
                        entity.Url = file.Url;
                        entities.Add(entity);
                    }
                }
                var result = this._context.SaveChanges();
                if (result > 0)
                    return entities;
                else
                    return null;
            }
            return null;
        }
    }
}
