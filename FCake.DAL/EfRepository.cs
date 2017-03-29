using FCake.Domain.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FCake.DAL
{
    public partial class EfRepository
    {
        public DbContext _context { get; set; }

        public EfRepository(DbContext context)
        {
            this._context = context;
        }

        public virtual IQueryable<T> Table<T>() where T : BaseEntity
        {
            return this.Entities<T>();
        }
        /// <summary>
        /// 根据主键获取实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Find<T>(string id) where T:BaseEntity{
            return this.Entities<T>().Find(id);
        }
        public bool Any<T>(Expression<Func<T, bool>> predicate) where T:BaseEntity {
            return this.Entities<T>().Any<T>(predicate);
        }
        public T SingleOrDefault<T>(Expression<Func<T, bool>> predicate, params string[] includeParams) where T : BaseEntity
        {
            var query = this.GetQuery<T>(predicate, includeParams);
            return query.SingleOrDefault();
        }
        public IQueryable<T> GetQuery<T>(Expression<Func<T, bool>> predicate, params string[] includeParams) where T : BaseEntity
        {
            var query = this.GetQuery<T>(includeParams);
            query = query.Where(predicate);
            return query;
        }

        public IQueryable<T> GetQuery<T>(params string[] includeParams) where T : BaseEntity
        {
            var query = this.Entities<T>().Where(a => a.IsDeleted !=1);
            foreach (var include in includeParams)
            {
                query = query.Include(include);
            }
            return query;
        }
        /// <summary>
        /// 逻辑删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="userID"></param>
        public void Delete<T>(T entity, string userID) where T : BaseEntity
        {
            entity.ModifiedBy = userID;
            entity.ModifiedOn = GetSystemTime();
            entity.IsDeleted = 1;
        }
        /// <summary>
        /// 逻辑删除集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="userID"></param>
        public void DeleteRange<T>(Expression<Func<T, bool>> predicate, string userID) where T : BaseEntity
        {
            var infos = this.Entities<T>().Where(predicate);
            foreach (var x in infos)
            {
                this.Delete<T>(x, userID);
            }
        }
        /// <summary>
        /// 逻辑删除集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="userID"></param>
        public void DeleteRange<T>(IEnumerable<T> infos, string userID) where T : BaseEntity
        {
            foreach (var x in infos)
            {
                this.Delete<T>(x, userID);
            }
        }
        /// <summary>
        /// 物理删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Remove<T>(T entity) where T:BaseEntity
        {
            this.Entities<T>().Remove(entity);
        }
        /// <summary>
        /// 物理删除集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void RemoveRange<T>(IQueryable<T> entities) where T : BaseEntity
        {
            this.Entities<T>().RemoveRange(entities);
        }

        public T Replace<T>(T entity) where T : BaseEntity
        {
            if (entity.Id == null || entity.Id.Trim() == "")
                return entity;

            var info = this.SingleOrDefault<T>(a => a.Id.Equals(entity.Id));

            if (info != null)
            {
                info.CopyProperty(entity);
            }

            return info;
        }

        public T Modify<T>(Expression<Func<T, bool>> predicate, Action<T> action, string userID) where T : BaseEntity
        {
            var info = this.SingleOrDefault<T>(predicate);
            if (info != null)
            {
                action(info);
                this.AddOrModify(info, userID);
            }
            return info;
        }
        public IQueryable<T> ModifyRange<T>(Expression<Func<T, bool>> predicate, Action<T> action, string userID) where T : BaseEntity
        {
            var infos = GetQuery<T>(predicate);
            foreach (var info in infos)
            {
                action(info);
                this.AddOrModify(info, userID);
            }
            return infos;
        }

        public void AddOrModify<T>(T entity, string userID) where T : BaseEntity
        {
            if (entity == null)
                return;

            entity.IsDeleted = 0;

            if (entity.Id == null || entity.Id.Trim() == "")
            {
                entity.Id = GetSystemID();
                entity.CreatedBy = userID;
                entity.CreatedOn = GetSystemTime();

                var dbSet = this.Entities<T>();
                dbSet.Add(entity);
            }
            else
            {
                entity.ModifiedBy = userID;
                entity.ModifiedOn = GetSystemTime();
            }
        }

        public int Commit()
        {
            return this._context.SaveChanges();
        }

        public DateTime GetSystemTime()
        {
            try
            {
                return DateTime.Now;
                //return context.Database.SqlQuery<DateTime>("exec getsystemtime").FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception("无法执行存储过程 exec getsystemtime,Error:"+ex.Message);
            }
        }
        public string GetSystemID()
        {
            return Guid.NewGuid().ToString("N").ToUpper();
        }

        /// <summary>
        /// 获取实体数据集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public DbSet<T> Entities<T>() where T : BaseEntity
        {
            return this._context.Set<T>();
        }
        private IQueryable<BaseEntity> Entities(Type targetType)
        {
            var set = this._context.Set(targetType);
            if (set == null)
            {
                return set.Local.OfType<BaseEntity>().AsQueryable<BaseEntity>();
            }
            return null;
        }
    }
}
