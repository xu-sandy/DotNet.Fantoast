using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FCake.Domain.Entities
{
    [Serializable]
    [DataContract(IsReference=true)]
    public class OperationLog:BaseEntity
    {
        public OperationLog() { }
        public OperationLog(string operatorId, string category,string businessId,string content):
            this(operatorId, operatorId, DateTime.Now, DateTime.Now, content, category, businessId)
        { 
        }
        public OperationLog(string createdBy, string operatorId, DateTime createdOn, DateTime operTime, string category, string businessId, string content)
        {
            Id = NewGuid();
            CreatedBy = createdBy;
            OperatorId = operatorId;
            CreatedOn = createdOn;
            OperTime = operTime;
            Category = category;
            BusinessId = businessId;
            Content = content;
        }
        /// <summary>
        /// 操作人Id
        /// </summary>
        [DataMember]
        public string OperatorId { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        [DataMember]
        public DateTime? OperTime { get; set; }
        /// <summary>
        /// 业务Id
        /// </summary>
        [DataMember]
        public string BusinessId { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [DataMember]
        public string Content { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        [DataMember]
        public string Category { get; set; }
        /// <summary>
        /// 父类型
        /// </summary>
        [DataMember]
        public string ParentCategory { get; set; }
    }
}
