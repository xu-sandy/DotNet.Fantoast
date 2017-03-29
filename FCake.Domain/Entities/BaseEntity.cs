using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class BaseEntity
    {
        [DataMember]
        public virtual string Id { get; set; }
        [DataMember]
        public virtual string CreatedBy { get; set; }
        [DataMember]
        public virtual DateTime? CreatedOn { get; set; }
        [DataMember]
        public virtual string ModifiedBy { get; set; }
        [DataMember]
        public virtual DateTime? ModifiedOn { get; set; }
        [DataMember]
        public virtual int? IsDeleted { get; set; }

        public virtual string NewGuid() {
            return Guid.NewGuid().ToString("N").ToUpper();
        }

       
        /// <summary>
        /// 复制可修改属性
        /// </summary>
        /// <param name="entity"></param>
        public void CopyProperty(BaseEntity entity)
        {
            var curType=this.GetType();
            var tarType = entity.GetType();
            var baseType=typeof(BaseEntity);

            var propertyList = (from x in curType.GetProperties()
                                join y in tarType.GetProperties() on x.Name equals y.Name
                                where baseType.GetProperties().Select(a => a.Name).Contains(x.Name) == false
                                select new
                                {
                                    cur=x,
                                    tar=y
                                });

            foreach (var p in propertyList)
            {
                try
                {
                    p.cur.SetValue(this, p.tar.GetValue(entity, null), null);
                }
                catch
                { }
            }
        }

    }
}
