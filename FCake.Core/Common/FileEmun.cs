/****************************************************************************************
 *功能实现：实现Excel导出模版枚举 *******************************************************
 ****************************************************************************************/
namespace FCake.Core.Common
{
    #region 引用
    using System;
    using System.Collections.Generic;
    using System.Linq;
    #endregion
    #region 定义导出EXCEL模版枚举
    public class FileEmun
    {
        [Flags]
        public enum TemplateEmun
        {
            /// <summary>
            /// 个人所得税模版
            /// </summary>
            Template_Income = 1,
             /// <summary>
            /// 异常员工
            /// </summary>
            Template_ExceptionalStaff = 2,
            /// <summary>
            /// 全国岗位底薪标准
            /// </summary>
            Template_AllBasicPositionSalary = 3,
            /// <summary>
            /// 地区岗位底薪标准
            /// </summary>
            Template_AreaBasicPositionSalary = 4,
            /// <summary>
            /// 美容院岗位底薪标准
            /// </summary>
            Template_ShopBasicPositionSalary = 5,
            /// <summary>
            /// 员工工资报表
            /// </summary>
            Template_UserWage = 6
        }
    }
    #endregion
}