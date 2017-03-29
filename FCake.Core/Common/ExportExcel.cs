/*----------------------------------------------------------------
// 功能描述：导出Excel文件
 * 创 建 人：蔡少发
// 创建时间：2015-04-23
//----------------------------------------------------------------*/

using System;
using System.Web;
using System.IO;
using System.Text;
using System.Data;
using System.Security.Permissions;

using NPOI.HPSF;
using NPOI.SS.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using System.Collections.Generic;

namespace FCake.Core.Common
{
    /// <summary>
    /// 导出Excel业务逻辑
    /// </summary>
    public class ExportExcel
    {
        /// <summary>
        /// 导出示例
        /// </summary>
        /// <returns>true:成功，false:失败</returns>
        public void DaoChuTest()
        {
            DataTable dt = new DataTable(); //要导出的数据表

            string[] fields = { "SN", "Branch", "Department", "FullName", "Position", "Tel", "MobilePhone", "Email", "QQ", "WeiXin" };
            string[] names = { "序号", "分公司", "部门", "姓名", "职务", "电话/分机", "手机", "E-Mail", "QQ", "微信号" };

            int[] merger = { 1, 2 };    //合并第1列的行，及第2列的行
            this.ToExcel("员工通讯录", dt, fields, names, merger);
        }


        #region 文档属性、单元格赋值、输出Excel

        /// <summary>
        /// 是否直接缓冲输出文件下载（默认false）
        /// </summary>
        public bool IsBufferOutput { get; set; }

        /// <summary>
        /// 导出Excel文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="dt">源数据</param>
        /// <param name="merger">合并列相同的值，用于上下行单元格重复合并（多个列间用半角逗号间隔）</param>
        /// <returns>返回生成后的URL下载地址</returns>
        public string ToExcel(string fileName, DataTable dt, int[] merger = null, int[] totalColumn = null)
        {
            if (dt != null && dt.Rows.Count > 0)
            {
                int count = dt.Columns.Count;
                string[] fields = new string[count];
                string[] names = new string[count];
                for (int i = 0; i < count; i++)
                {
                    fields[i] = dt.Columns[i].ColumnName;
                    names[i] = fields[i];
                }

                return this.ToExcel(fileName, dt, fields, names, merger, totalColumn);
            }
            return string.Empty;
        }


        /// <summary>
        /// 导出Excel文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="dt">源数据</param>
        /// <param name="fields">字段名（与 names 列名顺序一致，含个数）</param>
        /// <param name="names">列名（与 fields 字段名顺序一致，含个数）</param>
        /// <param name="merger">合并列相同的值，用于上下行单元格重复合并（多个列间用半角逗号间隔）</param>
        /// <param name="totalColumn">合计列的值，仅对数字、货币类型有效（在最后一行自动合计显示）</param>
        /// <returns>返回生成后的URL下载地址</returns>
        public string ToExcel(string fileName, DataTable dt, string[] fields, string[] names, int[] merger, int[] totalColumn = null, Dictionary<int, int> columnWidth = null)
        {
            if (dt != null && dt.Rows.Count > 0 && fields.Length == names.Length)
            {
                //创建Excel文件
                string sheetName = fileName;
                string headerText = fileName;

                fileName = string.Format("{0}_{1}.xls", fileName, DateTime.Now.ToString("yyyy-MM-dd"));
                HSSFWorkbook book = new HSSFWorkbook();
                book.DocumentSummaryInformation = DSI;
                book.SummaryInformation = SummaryInfo(fileName);

                //创建表
                ISheet iSheet = book.CreateSheet(sheetName);

                //创建数据格式
                IDataFormat iDataFormat = book.CreateDataFormat();
                //设置日期格式
                ICellStyle dataStyle = book.CreateCellStyle();

                int namesCount = names.Length;
                int fieldsCount = fields.Length;


                #region //自适应列宽

                int colCount = names.Length;
                int[] colWidth = new int[colCount];
                for (int i = 0; i < namesCount; i++)
                {
                    //colWidth[i] = Encoding.GetEncoding(936).GetBytes(names[i]).Length;
                    //colWidth[i] = (colWidth[i] < 5) ? 5 : colWidth[i] + 2;
                    if (columnWidth != null && columnWidth.ContainsKey(i))
                    {
                        colWidth[i] = (columnWidth[i] < 5) ? 5 : columnWidth[i] + 2;
                    }
                    else
                    {
                        colWidth[i] = Encoding.GetEncoding(936).GetBytes(names[i]).Length;
                        colWidth[i] = (colWidth[i] < 5) ? 5 : colWidth[i] + 2;
                    }
                }

                DataRowCollection drc = dt.Rows;
                int rowCount = drc.Count;
                for (int i = 0; i < rowCount; i++)
                {
                    for (int j = 0; j < colCount; j++)
                    {
                        int tmp = Encoding.GetEncoding(936).GetBytes(drc[i][fields[j]].ToString()).Length;
                        if (tmp > colWidth[j])
                        {
                            colWidth[j] = tmp;
                        }
                    }
                }

                #endregion

                IFont titleFont = book.CreateFont();
                IFont headFont = book.CreateFont();
                IFont textFont = book.CreateFont();
                ICellStyle css = book.CreateCellStyle();

                //
                int cur = 0;
                int tab = 1;
                decimal dec = 0;
                Dictionary<int, decimal> heji = new Dictionary<int, decimal>();

                for (int index = 0; index < rowCount; index++)
                {
                    #region 设置表头、列名、样式

                    if (index == 0 || index % this.MaxRows == 0)
                    {
                        if (cur != 0)
                        {
                            iSheet = book.CreateSheet(string.Format("{0}({1})", sheetName, tab));
                            tab++;
                        }

                        //表头
                        IRow header = iSheet.CreateRow(0);
                        header.HeightInPoints = 25;
                        //样式
                        ICellStyle cellStyle = book.CreateCellStyle();
                        cellStyle.Alignment = HorizontalAlignment.CENTER;
                        cellStyle.VerticalAlignment = VerticalAlignment.CENTER;

                        cellStyle.BorderTop = BorderStyle.THIN;
                        cellStyle.BorderBottom = BorderStyle.THIN;
                        cellStyle.BorderLeft = BorderStyle.THIN;
                        cellStyle.BorderRight = BorderStyle.THIN;

                        titleFont.FontHeightInPoints = 18;
                        cellStyle.SetFont(titleFont);

                        header.CreateCell(0).SetCellValue(headerText);
                        header.GetCell(0).CellStyle = cellStyle;

                        CellRangeAddress ra = new CellRangeAddress(0, 0, 0, colCount - 1);
                        iSheet.AddMergedRegion(ra);
                        ((HSSFSheet)iSheet).SetEnclosedBorderOfRegion(ra, BorderStyle.THIN, NPOI.HSSF.Util.HSSFColor.BLACK.index);

                        //
                        //列头
                        header = iSheet.CreateRow(1);
                        header.HeightInPoints = 18;
                        cellStyle = book.CreateCellStyle();
                        cellStyle.Alignment = HorizontalAlignment.CENTER;
                        cellStyle.VerticalAlignment = VerticalAlignment.CENTER;

                        cellStyle.BorderTop = BorderStyle.THIN;
                        cellStyle.BorderBottom = BorderStyle.THIN;
                        cellStyle.BorderLeft = BorderStyle.THIN;
                        cellStyle.BorderRight = BorderStyle.THIN;

                        headFont.FontHeightInPoints = 12;
                        cellStyle.SetFont(headFont);

                        for (int i = 0; i < namesCount; i++)
                        {
                            header.CreateCell(i).SetCellValue(names[i]);
                            header.GetCell(i).CellStyle = cellStyle;
                            iSheet.SetColumnWidth(i, (int)Math.Ceiling((double)((colWidth[i] + 1) * 256)));
                        }

                        cur = 2;
                    }

                    #endregion

                    #region 填充内容

                    IRow rows = iSheet.CreateRow(cur);
                    rows.HeightInPoints = 18;

                    for (int i = 0; i < fieldsCount; i++)
                    {
                        ICell cell = rows.CreateCell(i);
                        css.Alignment = HorizontalAlignment.LEFT;
                        css.VerticalAlignment = VerticalAlignment.CENTER;
                        css.WrapText = true;

                        css.BorderTop = BorderStyle.THIN;
                        css.BorderBottom = BorderStyle.THIN;
                        css.BorderLeft = BorderStyle.THIN;
                        css.BorderRight = BorderStyle.THIN;

                        textFont.FontHeightInPoints = 10;
                        css.SetFont(textFont);
                        cell.CellStyle = css;

                        if (index > 0 && IsExistMerger(merger, i) && (index % this.MaxRows != 0))
                        {
                            if (string.Compare(Convert.ToString(drc[index][fields[i]]), Convert.ToString(drc[index - 1][fields[i]])) == 0
                                && string.Compare(Convert.ToString(drc[index][fields[merger[0]]]), Convert.ToString(drc[index - 1][fields[merger[0]]])) == 0)
                            {
                                iSheet.AddMergedRegion(new CellRangeAddress(cur - 1, cur, i, i));
                            }
                        }

                        string val = Convert.ToString(drc[index][fields[i]]);
                        dec = 0;
                        this.SetCellValue(cell, iDataFormat, dataStyle, drc[index][fields[i]].GetType().ToString(), val, out dec);
                        iSheet.SetColumnWidth(i, (int)Math.Ceiling((double)((colWidth[i] + 1) * 256)));

                        if (IsExistMerger(totalColumn, i))
                        {
                            if (!heji.ContainsKey(i))
                            {
                                heji.Add(i, dec);
                            }
                            else
                            {
                                dec += heji[i];
                                heji.Remove(i);
                                heji.Add(i, dec);
                            }
                        }
                    }

                    #endregion

                    cur++;
                }

                #region 对指定列进行合计

                int beginColumn = 0;
                if (totalColumn != null && totalColumn.Length > 0)
                {
                    IRow rows = iSheet.CreateRow(cur);
                    rows.HeightInPoints = 18;

                    for (int i = 0; i < fieldsCount; i++)
                    {
                        ICell cell = rows.CreateCell(i);
                        css.Alignment = HorizontalAlignment.LEFT;
                        css.VerticalAlignment = VerticalAlignment.CENTER;
                        css.WrapText = true;

                        css.BorderTop = BorderStyle.THIN;
                        css.BorderBottom = BorderStyle.THIN;
                        css.BorderLeft = BorderStyle.THIN;
                        css.BorderRight = BorderStyle.THIN;

                        textFont.FontHeightInPoints = 10;
                        css.SetFont(textFont);
                        cell.CellStyle = css;

                        dec = 0;
                        if (IsExistMerger(totalColumn, i))
                        {
                            this.SetCellValue(cell, iDataFormat, dataStyle, heji[i].GetType().ToString(), heji[i].ToString(), out dec);
                            if (beginColumn == 0)
                            {
                                beginColumn = i;
                            }
                        }
                        else if (beginColumn > 0)
                        {
                            cell = rows.GetCell(0);

                            css.Alignment = HorizontalAlignment.RIGHT;
                            css.SetFont(textFont);
                            cell.CellStyle = css;

                            iSheet.AddMergedRegion(new CellRangeAddress(cur, cur, 0, beginColumn - 1));
                            this.SetCellValue(cell, iDataFormat, dataStyle, "System.String", "合计：", out dec);
                            beginColumn = -1;
                        }
                        else
                        {
                            this.SetCellValue(cell, iDataFormat, dataStyle, "System.String", "", out dec);
                        }

                        iSheet.SetColumnWidth(i, (int)Math.Ceiling((double)((colWidth[i] + 2) * 256)));
                    }
                }

                #endregion

                //输出文件
                return this.OutputFile(book, fileName);
            }
            return string.Empty;
        }




        /// <summary>
        /// 若单页超出5000行（最多可设65535行)，则另外新建表
        /// </summary>
        private int MaxRows { get { return 5000; } }

        private bool IsExistMerger(int[] merger, int curCol)
        {
            if (merger != null)
            {
                foreach (int c in merger)
                {
                    if (c == curCol)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 文档属性
        /// </summary>
        private DocumentSummaryInformation DSI
        {
            get
            {
                DocumentSummaryInformation si = PropertySetFactory.CreateDocumentSummaryInformation();
                si.Company = "CAISHAOFA";
                return si;
            }
        }

        /// <summary>
        /// 属性摘要信息
        /// </summary>
        /// <param name="fileName">文件名称</param>
        private SummaryInformation SummaryInfo(string fileName)
        {
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();

            si.Author = "CAISHAOFA";
            si.LastAuthor = si.Author;
            si.Comments = si.Author;

            si.ApplicationName = fileName;
            si.Title = fileName;
            si.Subject = fileName;
            si.CreateDateTime = DateTime.Now;

            return si;
        }

        /// <summary>
        /// 填充单元格数据
        /// </summary>
        /// <param name="cell">单元格对象</param>
        /// <param name="df">数据格式</param>
        /// <param name="style">样式</param>
        /// <param name="type">数据类型</param>
        /// <param name="value">数据值</param>
        /// <param name="val">输出数字或货币值（非数字或非货币类型的统一为0）</param>
        private void SetCellValue(ICell cell, IDataFormat df, ICellStyle style, string type, string value, out decimal val)
        {
            val = 0;

            switch (type)
            {
                case "System.String":
                    cell.SetCellValue(value);
                    break;
                case "System.DateTime":
                    DateTime dt;
                    if (DateTime.TryParse(value, out dt))
                    {
                        style.DataFormat = df.GetFormat("yyyy-MM-dd");
                        cell.CellStyle = style;
                        cell.SetCellValue(dt);
                    }
                    else
                    {
                        cell.SetCellValue("");
                    }
                    break;
                case "System.Boolen":
                    bool bl = false;
                    if (bool.TryParse(value, out bl))
                    {
                        cell.SetCellValue(bl);
                    }
                    else
                    {
                        cell.SetCellValue("");
                    }
                    break;
                case "System.Byte":
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                    int i = 0;
                    if (int.TryParse(value, out i))
                    {
                        cell.SetCellValue(i);
                        val = i;
                    }
                    else
                    {
                        cell.SetCellValue(0);
                    }
                    break;
                case "System.Decimal":
                case "System.Double":
                    double dbl = 0;
                    if (double.TryParse(value, out dbl))
                    {
                        cell.SetCellValue(dbl);
                        val = Convert.ToDecimal(value);
                    }
                    else
                    {
                        cell.SetCellValue(0);
                    }
                    break;
                case "System.DBNull":
                default:
                    cell.SetCellValue("");
                    break;
            }
        }

        /// <summary>
        /// 输出并保存文件
        /// </summary>
        /// <param name="book">HSSFWorkbook 实例</param>
        /// <param name="fileName">文件全称含扩展名</param>
        private string OutputFile(HSSFWorkbook book, string fileName)
        {
            String savePath = HttpContext.Current.Server.MapPath(@"/TempFile/Excel/");//绝对路径
            var url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority).ToString();//网站路径
            string newFileName = Path.Combine(savePath, fileName);

            if (!Directory.Exists(savePath))
            {
                FileIOPermission ioPermission = new FileIOPermission(FileIOPermissionAccess.AllAccess, savePath);
                ioPermission.Demand();
                Directory.CreateDirectory(savePath);
            }
            using (FileStream fs = new FileStream(newFileName, FileMode.Create, FileAccess.ReadWrite))
            {
                book.Write(fs);
                fs.Dispose();
                fs.Close();
            }

            return url + "/TempFile/Excel/" + fileName;
            //FileStream stream = new FileStream(newFileName, FileMode.Open);
            //byte[] bytes = new byte[(int)stream.Length];
            //stream.Read(bytes, 0, bytes.Length);
            //stream.Close();
            //HttpResponse rp = HttpContext.Current.Response;
            //rp.Clear();
            //rp.ClearHeaders();
            //rp.Buffer = true;
            //rp.BufferOutput = true;
            //rp.ContentType = "application/octet-stream";
            //if (HttpContext.Current.Request.Browser.Browser.Equals("IE"))
            //{
            //    rp.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(fileName, Encoding.UTF8));
            //}
            //else
            //{
            //    rp.AppendHeader("Content-Disposition", "attachment;fileName=" + fileName);
            //}
            //rp.BinaryWrite(bytes);
            //rp.Flush();
            //rp.End();
        }

        #endregion
    }
}
