using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FCake.Core.Common
{
    public static class LinqHelper
    {
        /// <summary>
        /// Linq排序拓展
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyStr"></param>
        /// <returns></returns>
        public static IOrderedQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string propertyStr) where TEntity : class
        {
            var a = source.First();
            var t = a.GetType();
            //以下四句用来购建立 c=>c.propertyStr 的Expression
            ParameterExpression param = Expression.Parameter(t, "c");
            propertyStr = propertyStr.Trim();

            if (propertyStr == "")
                return source.OrderBy(b => "");

            var listproperty = propertyStr.Split(',').Where(temp => temp.IsNullOrTrimEmpty() == false).ToList();
            var pros = new List<PropertyInfo>();
            foreach (var x in listproperty)
            {
                PropertyInfo property = t.GetProperty(propertyStr);
                if (property == null)
                    return source.OrderBy(b => "");
                pros.Add(property);
            }

            int i = 0;

            dynamic result = source;

            foreach (var x in pros)
            {
                Expression propertyAccessExpression = Expression.MakeMemberAccess(param, x);
                LambdaExpression le = Expression.Lambda(propertyAccessExpression, param);
                MethodCallExpression resultExp = Expression.Call(i == 0 ? typeof(Queryable) : typeof(IOrderedQueryable), i == 0 ? "OrderBy" : "ThenBy", new Type[] { t, x.PropertyType }, source.Expression, Expression.Quote(le));
                result = (IOrderedQueryable<TEntity>)result.Provider.CreateQuery<TEntity>(resultExp);
                i++;
            }


            return result;
        }
    }
}
