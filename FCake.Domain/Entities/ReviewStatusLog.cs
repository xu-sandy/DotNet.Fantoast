using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference=true)]
    public class ReviewStatusLog:BaseEntity
    {
        //CreatedOn 操作时间
        //CreatedBy 操作人
        [DataMember]
        public string TableName { get; set; }   //审批数据表名
        [DataMember]
        public string TableNameId { get; set; } //审批数据表主键值
        [DataMember]
        public int? Status { get; set; }        //审批状态值
        [DataMember]
        public string Remark { get; set; }  //审批描述
    }
}
