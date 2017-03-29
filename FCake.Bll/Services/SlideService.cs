using FCake.Core.Common;
using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using FCake.Domain.Enums;

namespace FCake.Bll.Services
{
    public class SlideService
    {
        private EFDbContext context = new EFDbContext();

        public List<T> GetSlides<T>() where T : new()
        {
            List<T> result = new List<T>();
            var outType = typeof(T);
            var outProInfos = outType.GetProperties();
            var outNames = outProInfos.Select(a => a.Name);

            var slides = (from x in context.Slides
                          where x.IsDeleted != 1
                          orderby x.Status, x.SortOrder descending
                          select new
                          {
                              x.Id,
                              x.Height,
                              x.Width,
                              x.Length,
                              x.LinkUrl,
                              Status = (int)x.Status,
                              x.Url,
                              x.Apply,
                              x.SortOrder
                          });

            foreach (var slide in slides)
            {
                T outItem = new T();
                var type = slide.GetType();
                var proInfos = type.GetProperties();
                foreach (var x in proInfos.Where(p => outNames.Contains(p.Name)))
                {
                    var p = outProInfos.SingleOrDefault(a => a.Name.Equals(x.Name));
                    p.SetValue(outItem, x.GetValue(slide, null), null);
                }
                result.Add(outItem);
            }

            return result;
        }

        public object SaveSlidePicture(string id, HttpPostedFileBase file, string curUserID)
        {
            Slide slide = context.Slides.SingleOrDefault(a => a.Id.Equals(id));


            string sysID = DataHelper.GetSystemID();

            string dir = "/files/slides/";
            if (Directory.Exists(HttpContext.Current.Server.MapPath(dir)) == false)
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir));

            string u = dir + sysID + file.FileName.Substring(file.FileName.LastIndexOf("."));
            file.SaveAs(HttpContext.Current.Server.MapPath(u));
            PictureHelper.MakeThumbnail(HttpContext.Current.Server.MapPath(u), HttpContext.Current.Server.MapPath(u + "_thumb.jpg"), 200, 100, "Cut");


            var img = Image.FromStream(file.InputStream, true);

            if (slide == null)
            {
                slide = new Slide();
                slide.CreatedBy = curUserID;
                slide.CreatedOn = DateTime.Now;
                slide.Id = sysID;
                slide.Length = file.ContentLength;
                slide.Width = img.Width;
                slide.Height = img.Height;
                slide.Url = u;
                slide.Status = SlideStatus.NoActive;
                slide.IsDeleted = 1;
                context.Slides.Add(slide);
                id = sysID;
            }
            else
            {
                slide.ModifiedBy = curUserID;
                slide.ModifiedOn = DateTime.Now;
                slide.Length = file.ContentLength;
                slide.Width = img.Width;
                slide.Height = img.Height;
                slide.Url = u;
            }
            context.SaveChanges();

            return new { validate = true, id = slide.Id, url = slide.Url };
        }

        public object SaveSlide(Slide model, string currentUserID)
        {
            Slide slide = context.Slides.Find(model.Id);
            if (slide == null)
                return new { validate = false, msg = "查无数据，请刷新后重试" };
            slide.IsDeleted = 0;
            slide.Status = model.Status;
            slide.ModifiedBy = currentUserID;
            slide.ModifiedOn = DateTime.Now;
            slide.LinkUrl = model.Url;
            slide.Apply = model.Apply;
            slide.SortOrder = model.SortOrder;
            context.SaveChanges();
            return new { validate = true };
        }

        public IQueryable<Slide> GetSlides(int apply)
        {
            return context.Slides.Where(a => a.IsDeleted != 1 && a.Status == SlideStatus.Active && a.Apply == apply)
                .OrderByDescending(s=>s.SortOrder);
        }
    }
}
