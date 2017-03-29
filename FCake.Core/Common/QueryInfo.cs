using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
#if !NET_2_0
using System.Runtime.Serialization;
#endif

namespace FCake.Framework
{
    public delegate QueryInfo FindByQueryInfo(QueryInfo info);
#if !NET_2_0
    [DataContract]
#endif
    public class QueryInfo : ICloneable
    {
        /// <summary>
        /// NamedQuery/SP/SQL时:QueryObject/CustomSQL
        /// </summary>
        public QueryInfo()
        {
        }
        /// <summary>
        /// queryObject: 仅HQL时,可包括别名
        /// </summary>
        public QueryInfo(string queryObj)
        {
            this.QueryObject = queryObj;
        }

        private int startRecord;
#if !NET_2_0
        [DataMember]
#endif
        public int StartRecord
        {
            get { return startRecord; }
            set { startRecord = value; }
        }
        private int pageSize;
#if !NET_2_0
        [DataMember]
#endif
        public int PageSize
        {
            get
            {
                if (pageSize == 0) pageSize = 15;
                return pageSize;
            }
            set { pageSize = value; }
        }
        private int totalCount;
        /// <summary>
        /// 设置为1 时，查询总数并分页， 设置为2时，仅按要求分页，不查询总数
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public int TotalCount
        {
            get { return totalCount; }
            set { totalCount = value; }
        }

        private IDictionary<string, string> where;
#if !NET_2_0
        [DataMember]
#endif
        public IDictionary<string, string> Where
        {
            get
            {
                if (where == null) where = new Dictionary<string, string>();
                return where;
            }
            set { where = value; }//deserialize
        }

        private IDictionary<string, string> filters;
        /// <summary>
        /// Add(Filter关联参数名,Filter名)
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public IDictionary<string, string> Filters
        {
            get
            {
                if (filters == null) filters = new Dictionary<string, string>();
                return filters;
            }
            set { filters = value; }//deserialize
        }

        private List<string> orderBy;
#if !NET_2_0
        [DataMember]
#endif
        public List<string> OrderBy
        {
            get
            {
                if (orderBy == null) orderBy = new List<string>();
                return orderBy;
            }
            set { orderBy = value; }//deserialize
        }

        private string queryObject;
        /// <summary>
        /// 实体名和别名,逗号分隔多个实体。 如: Customer cust,...
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string QueryObject
        {
            get { return queryObject; }
            set
            {
                queryObject = value;
                if (queryObject != null && queryObject.IndexOf(',') < 0)//only one entity
                {
                    int iAlias = queryObject.LastIndexOf(' ');
                    if (iAlias > 2)
                        this.alias = queryObject.Substring(iAlias + 1).Trim();//别名
                }
            }
        }

        /// <summary>
        /// 查询别名 QueryObject=User u, 则别名为" u. "
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        private string alias = string.Empty;

        /// <summary>
        /// 必须为带别名的属性：在Find拦截时根据此属性(alias.Property)过滤，如i.Id , 还可以是CreatedOffice,CreatedBy
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string AclProperty
        {
            get;
            set;
        }

        private string countField;
        /// <summary>
        /// 默认将采用count(*)统计
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string CountField
        {
            get
            {
                if (countField == null) countField = "*";
                return countField;
            }
            set { countField = value; }
        }

        private string customSQL;
        /// <summary>
        /// 自定义SQL/HQL语句,除where部分
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string CustomSQL
        {
            get { return customSQL; }
            set { customSQL = value; }
        }
#if !NET_2_0
        [DataMember]
#endif
        //源记录数
        public Int32 SoureceTotalCount
        {
            get;
            set;
        }
        /// <summary>
        /// 简单SQL分离为field +from +where +groupby +orderby
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string GroupBy
        {
            get;
            set;
        }


        private string namedQuery;
        /// <summary>
        /// NamedQuery/SP
        /// 注意：Parameters的参数顺序，请务必与存储过程内部顺序相同!
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string NamedQuery
        {
            get { return namedQuery; }
            set { namedQuery = value; }
        }

        /// <summary>
        /// HQL without OrderBy string
        /// </summary>
        public string ToHQLString()
        {
            //if (!string.IsNullOrEmpty(this.AclProperty))
            //FireAclInjection();

            StringBuilder sb = new StringBuilder();
            if (this.CustomSQL == null)//no statement customized,let's build it.
            {
                if (this.QueryObject != null && this.QueryObject.IndexOf(" from ", StringComparison.InvariantCultureIgnoreCase) < 0)
                    sb.Append(" from ");
                sb.Append(this.QueryObject);
                if (this.Where.Count > 0)
                    sb.Append(" 1=1");
            }
            else
                BuildCustomSQL(sb);

            BuildWhere(sb);
            return sb.ToString();
        }

        //private void FireAclInjection()
        //{
        //    AclInjector.Invoke(_aclInjectorObj, new object[] { this });
        //}

        //static object _aclInjectorObj = null;
        //static System.Reflection.MethodInfo _aclInjector=null;
        //static System.Reflection.MethodInfo AclInjector 
        //{
        //    get
        //    {
        //        if (_aclInjector == null)
        //        {
        //            object injector = Framework.Proxy.Context.ApplicationContext.GetContext().GetObject("AclInjector");
        //            if (injector != null)
        //            {
        //                Spring.Objects.Support.MethodInvoker mi = new Spring.Objects.Support.MethodInvoker();
        //                mi.TargetObject = injector;
        //                mi.TargetMethod = "Invoke";
        //                mi.Arguments = new object[] { new QueryInfo() };
        //                mi.Prepare();

        //                _aclInjectorObj = injector;
        //               _aclInjector = mi.GetPreparedMethod();
        //            }
        //            else
        //                throw new Exception("调用AclProerty注入时，未配置带有'Invoke(QueryInfo info)'方法的对象'AclInjector'.");
        //        }
        //        return _aclInjector;
        //    }
        //}

        /// <summary>
        /// SQL without OrderBy string
        /// </summary>
        public string ToSQLString()
        {
            StringBuilder sb = new StringBuilder();
            if (this.CustomSQL == null)//no statement customized,let's build it.
            {
                if (this.QueryObject != null && this.QueryObject.IndexOf(" from ", StringComparison.InvariantCultureIgnoreCase) < 0)
                    sb.Append(" from ");
                sb.Append(this.QueryObject);
                if (this.Where.Count > 0)
                    sb.Append(" 1=1");
            }
            else
                BuildCustomSQL(sb);
            //if (this.CustomSQL.IndexOf("select ") < 0)//not whole statement customized,let's build it.
            //    sb.Append("select * from ");            

            BuildWhere(sb);
            return sb.ToString();
        }
        public object[] GetParams()
        {
            object[] arr = new object[this.Parameters.Count];
            int i = -1;
            foreach (var item in this.Parameters)
            {
                i += 1;
                arr[i] = item.Value;
            }
            return arr;
        }
        private void BuildCustomSQL(StringBuilder sb)
        {
            //edit daidz 2014-02-28
            //int i = this.CustomSQL.IndexOf("order by");
            //if (i > 0)//将Order By从SQL拆开
            //{
            //    if (this.OrderBy.Count == 0)
            //        this.OrderBy.Add(this.CustomSQL.Substring(i + 8));
            //    this.CustomSQL = this.CustomSQL.Substring(0, i);
            //}
            sb.Append(this.CustomSQL);

            if (this.Where.Count > 0 && this.CustomSQL.IndexOf(" where ") < 0)//准备好where在最后
                sb.Append(" 1=1");
        }
        private void BuildWhere(StringBuilder sb)
        {
            //where fragment
            if (this.Where.Count > 0)
            {
                foreach (string con in this.Where.Values)
                    sb.Append(" ").Append(con);
            }
            if (!string.IsNullOrEmpty(GroupBy))
            {
                sb.Append(" group by ");
                sb.Append(GroupBy);
            }
        }

        /// <summary>
        /// OrderBy is not supported when 'COUNT' query. so seperate it.
        /// </summary>
        public string ToOrderBy()
        {
            StringBuilder sb = new StringBuilder();
            //order by fragment
            if (this.OrderBy.Count > 0)
            {
                sb.Append(" order by ");
                foreach (string ord in this.OrderBy)
                    sb.Append(ord).Append(",");
                sb.Remove(sb.Length - 1, 1);//last comm
            }
            return sb.ToString();
        }

        private static readonly string EQ_EXPRESSION = "and {0} = @{1}";//默认=表达式

        /// <summary>
        /// 控件名为 prm_Name_LLK_u_ 分别为prm_{字段名}_{匹配类型?}_{别名?}_. 匹配及别名可有可无
        /// 如：prm_Name_u_ , prm_Name_GT_ , prm_Time_u_V2 都有效
        /// </summary>
        public void AddParam(object dictionary)
        {
            AddParam((IDictionary)dictionary);
        }

        static readonly Regex dtRegex = new Regex(@"^\d{2,4}-\d{1,2}-\d{1,2}", RegexOptions.Compiled);
        static readonly Regex paramRegex = new Regex(@"^(?<NAME>\w*?){1}(_(?<PAT>\w{2,3}))?(_(?<ALIAS>\w))?$", RegexOptions.Compiled);
        private object ChangeDbType(object valOld)
        {
            string val = valOld as string;
            if (val != null)
            {
                if (dtRegex.Match(val).Success)
                    return DateTime.Parse(val);
                int n;
                if (int.TryParse(val.ToString(), out n))
                    return n;
            }
            return valOld;
        }
        private void AddParam(IDictionary ps)
        {
            var j = -1;
            foreach (string key in ps.Keys)
            {
                j += 1;
                if (ps[key] != null)
                {
                    object val = ps[key];//参数值
                    string type = "";
                    //val = val.Trim();
                    if (val != null && !string.IsNullOrEmpty(val.ToString()))
                    {
                        Match m = paramRegex.Match(key);//{Field}_{LLK}_{u}
                        string sField = m.Groups["NAME"].Value;
                        string alias = m.Groups["ALIAS"].Value;
                        string patten = m.Groups["PAT"].Value;
                        if (!string.IsNullOrEmpty(patten))//'LLK,GEQ'
                        {
                            bool bChangedDbType = false;//参数可能需要转换类型
                            string sParam = key;//参数名
                            StringBuilder exp = new StringBuilder("and ");//where query expression

                            switch (patten)
                            {
                                #region Like类型
                                case "LLK"://Left like? like '%value'
                                    val = val.ToString();
                                    type = "EndsWith";
                                    break;
                                case "RLK"://Right like? like 'value%'
                                    val = val.ToString();
                                    type = "StartsWith";
                                    break;
                                case "LK"://Left like? like '%value%'
                                    val = val.ToString();
                                    type = "Contains";
                                    break;
                                #endregion

                                #region 大于，小于
                                case "GT":
                                    exp.AppendFormat("{0} > @{1}", sField, j);
                                    bChangedDbType = true;
                                    break;
                                case "GEQ":
                                    exp.AppendFormat("{0} >= @{1}", sField, j);
                                    bChangedDbType = true;
                                    break;
                                case "LT":
                                    exp.AppendFormat("{0} < @{1}", sField, j);
                                    bChangedDbType = true;
                                    break;
                                case "LEQ":
                                    exp.AppendFormat("{0} <= @{1}", sField, j);
                                    bChangedDbType = true;
                                    break;
                                case "NEQ":
                                    exp.AppendFormat("{0} <> @{1}", sField, j);
                                    bChangedDbType = true;
                                    break;
                                case "EQ":
                                    exp.AppendFormat("{0} = @{1}", sField, j);
                                    bChangedDbType = true;
                                    break;
                                #endregion

                                case "NVL":
                                    exp.AppendFormat("{0} is null", sField);
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException("Match 'patten' is not valid.");
                            }
                            #region Like 逐字段组装
                            if (patten.EndsWith("LK"))
                            {
                                exp.Append("(");
                                //string[] fields = sField.Split('_');
                                //sParam = fields[0] + fields.Length.ToString();//重新设置参数名
                                //for (int i = 0; i < fields.Length; i++)//遍历每个字段
                                //{
                                //    exp.AppendFormat("{{1}}{0}.Contains(@) or ", fields[i], sParam);//fieldName like :ParamName
                                //}
                                exp.AppendFormat("{0}.{1}(@{2}) or ", sField,type, j);//fieldName like :ParamName
                                exp.Remove(exp.Length - 4, 4);
                                exp.Append(")");
                            }
                            #endregion
                            AddParam(alias, sParam, (bChangedDbType && val is string) ? ChangeDbType(val) : val, exp.ToString());
                        }
                        else {
                            string exp = string.Format(EQ_EXPRESSION, sField, j);
                            AddParam(alias, key, ChangeDbType(val), exp);
                        }
                    }
                }//val not null
                else
                    AddParam(null, key, null, string.Empty);

            }//foreach
        }
        /// <summary>
        /// 为语句提供参数值,若非NamedQuery,并且CustomSQL和where中均不存在此参数,则自动添加 and {0}=:{0}
        /// </summary>
        public void AddParam(string namedParam, object value)
        {
            AddParam(namedParam, value, EQ_EXPRESSION);
        }
        /// <summary>
        /// 为语句提供参数值,若value=null,value=>DBNull.Value
        /// </summary>
        public void AddParam(string namedParam, object value, string expression)
        {
            string pAlias = this.alias;
            int i = namedParam.IndexOf(".");
            if (i > 0)//参数名带点号
            {
                string[] p = namedParam.Split('.');
                pAlias = p[0];//实体别名
                namedParam = p[1];//参数名
            }
            AddParam(pAlias, namedParam, value, expression);
        }

        private void AddParam(string alias, string namedParam, object value, string expression)
        {
            if (this.NamedQuery != null)//sp?
            {
                if (Parameters.ContainsKey(namedParam))
                    throw new ArgumentException("试图重复添加同一参数名:" + namedParam);
                else
                    Parameters.Add(namedParam, value);

                return;
            }

            if (value != null)
            {
                if (Parameters.ContainsKey(namedParam))
                    throw new ArgumentException("试图重复添加同一参数名:" + namedParam);
                else
                    Parameters.Add(namedParam, value);
            }
            else if (expression.IndexOf(":" + namedParam) > 0 || expression.IndexOf(":{0}") > 0)//该Param存在与Sql中,必须添加到Parameters或移除expression
                expression = "and {1}{0} is null";

            if (!string.IsNullOrEmpty(alias))
                alias += ".";
            if ((this.CustomSQL == null || this.CustomSQL.IndexOf(":" + namedParam) < 0) //not in sql text
                && !Where.ContainsKey(namedParam)) //not in where
                this.Where.Add(namedParam, string.Format(expression, namedParam, alias));//add to where
        }
        private IDictionary<string, object> parameters;
        /// <summary>
        /// 输入、输出参数表
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public IDictionary<string, object> Parameters
        {
            get
            {
                if (parameters == null) parameters = new Dictionary<string, object>();
                return parameters;
            }
            set
            {
                parameters = value;
            }
        }


        private string[] initProps;
        /// <summary>
        /// Properties should be initialized
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string[] InitProps
        {
            get { return initProps; }
            set { initProps = value; }
        }
        private string[] unInitProps;
        /// <summary>
        /// Properties should not be init!
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public string[] UnInitProps
        {
            get { return unInitProps; }
            set { unInitProps = value; }
        }

        private IList list;
        /// <summary>
        /// Query result to be returned!
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public IList List
        {
            get { return list; }
            set { list = value; }
        }

        /// <summary>
        /// Resultset should be transform to which type.
        /// It can be typeof(Entity) or Entity.FullName.
        /// </summary>
#if !NET_2_0
        [DataMember]
#endif
        public object Transformer
        {
            get;
            set;
        }

        /// <summary>
        /// 重复使用实例时,需重新初始化
        /// </summary>
        public void Init()
        {
            this.QueryObject = null;
            this.StartRecord = 0;
            this.PageSize = 0;
            this.TotalCount = 0;
            this.alias = string.Empty;
            this.Where.Clear();
            this.Filters.Clear();
            this.OrderBy.Clear();
            this.CountField = null;
            this.CustomSQL = null;
            this.NamedQuery = null;
            this.Parameters.Clear();
            this.InitProps = null;
            this.UnInitProps = null;
            this.List = null;
            this.Transformer = null;
        }

        public object Clone()
        {
            QueryInfo info = new QueryInfo();
            IEnumerator<KeyValuePair<string, object>> enumer = this.Parameters.GetEnumerator();
            while (enumer.MoveNext())
                info.Parameters.Add(enumer.Current.Key, enumer.Current.Value);

            IEnumerator<KeyValuePair<string, string>> senumer = this.Where.GetEnumerator();
            while (senumer.MoveNext())
                info.Where.Add(senumer.Current.Key, senumer.Current.Value);

            IEnumerator<KeyValuePair<string, string>> fenumer = this.Filters.GetEnumerator();
            while (fenumer.MoveNext())
                info.Filters.Add(fenumer.Current.Key, fenumer.Current.Value);

            info.CountField = this.CountField;
            info.CustomSQL = this.CustomSQL;
            info.InitProps = this.InitProps;
            info.NamedQuery = this.NamedQuery;
            info.OrderBy = this.OrderBy;
            info.PageSize = this.PageSize;
            info.QueryObject = this.QueryObject;
            info.StartRecord = this.StartRecord;
            info.TotalCount = this.TotalCount;
            info.Transformer = this.Transformer;
            info.UnInitProps = this.UnInitProps;
            return info;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new StringBuilder();
            sb.Append("\tTotalCount:");
            sb.Append(this.TotalCount);
            sb.Append(Environment.NewLine);

            sb.Append(this.NamedQuery ?? (this.ToSQLString() + this.ToOrderBy()));
            IEnumerator<KeyValuePair<string, object>> enumer = this.Parameters.GetEnumerator();
            while (enumer.MoveNext())
            {
                sb.Append(Environment.NewLine);
                sb.Append("\t");
                sb.Append(enumer.Current.Key);
                sb.Append(":");
                sb.Append(enumer.Current.Value);
            }

            return sb.ToString();
        }
        /// <summary>
        /// 系列化对象
        /// </summary>
        [DataMember]
        public string JsonReulst
        {
            get;
            set;
        }
        /// <summary>
        /// 是否需要系列化
        /// </summary>
        [DataMember]
        public bool IsSerialize
        {
            get;
            set;
        }
    }
}
