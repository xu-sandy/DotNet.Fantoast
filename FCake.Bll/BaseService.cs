using FCake.DAL;
using FCake.Domain;
using FCake.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FCake.Bll
{

    public class BaseService
    {
       protected EfRepository DAL { get; set; }

        public BaseService()
        {
            DAL = new EfRepository(new EFDbContext());
        }
        public BaseService(DbContext context)
        {
            DAL = new EfRepository(context);
        }

        public T Find<T>(string id) where T : BaseEntity {
            return DAL.Find<T>(id);
        }
        public bool Any<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity
        {
            return DAL.Any<T>(predicate);
        }

        public int Delete<T>(Expression<Func<T, bool>> predicate, string userID) where T : BaseEntity
        {
            DAL.DeleteRange<T>(predicate, userID);
            return DAL.Commit();
        }
        public int Delete<T>(Expression<Func<T, bool>> predicate, Action<T> action, string userID) where T : BaseEntity
        {
            var infos = DAL.GetQuery<T>(predicate);
            foreach (var x in infos)
            {
                action(x);
                DAL.Delete(x, userID);
            }
            return DAL.Commit();
        }

        public int Modify<T>(Expression<Func<T, bool>> predicate, Action<T> action, string userID) where T : BaseEntity
        {
            DAL.ModifyRange<T>(predicate, action, userID);
            return DAL.Commit();
        }
        public int Modify<T>(T entity, Action<T> action, string userID) where T : BaseEntity
        {
            var info = DAL.Replace(entity);

            action(info);
            DAL.AddOrModify(entity, userID);

            return DAL.Commit();
        }

        public IQueryable<T> GetQuery<T>(params string[] pars) where T : BaseEntity
        {
            return DAL.GetQuery<T>(pars);
        }
        public IQueryable<T> GetQuery<T>(Expression<Func<T, bool>> predicate, params string[] pars) where T : BaseEntity
        {
            return DAL.GetQuery<T>(predicate, pars);
        }

        public T SingleOrDefault<T>(Expression<Func<T, bool>> predicate, params string[] pars) where T : BaseEntity
        {
            return DAL.SingleOrDefault<T>(predicate, pars);
        }
        public T SingleOrDefault<T>(Expression<Func<T, bool>> predicate,Action<T> action, params string[] pars) where T : BaseEntity
        {
            var info= DAL.SingleOrDefault<T>(predicate, pars);
            action(info);
            return info;
        }
    }
}
