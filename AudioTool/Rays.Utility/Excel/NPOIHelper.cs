using System;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using System.Text;
using System.Collections.Generic;

namespace Rays.Utility
{
    /// <summary>
    /// NPOIHelper 的摘要说明
    /// </summary>
    public class NPOIHelper
    {
        //Datatable导出Excel
        public static void GridToExcelByNPOI(DataTable dt, string strExcelFileName, string title)
        {
            HSSFWorkbook workbook = new HSSFWorkbook();
            try
            {
                ISheet sheet = workbook.CreateSheet("Sheet1");
                ICellStyle HeadercellStyle = workbook.CreateCellStyle();
                HeadercellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                HeadercellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                HeadercellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                HeadercellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                HeadercellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                //字体
                NPOI.SS.UserModel.IFont headerfont = workbook.CreateFont();
                headerfont.Boldweight = (short)FontBoldWeight.Bold;
                HeadercellStyle.SetFont(headerfont);


                //标题头
                int icolIndex = 0;
                CellRangeAddress region = new CellRangeAddress(0, 0, 0, dt.Columns.Count > 0 ? dt.Columns.Count - 1 : 0);
                sheet.AddMergedRegion(region);
                ((HSSFSheet)sheet).SetEnclosedBorderOfRegion(region, BorderStyle.Thin, NPOI.HSSF.Util.HSSFColor.Black.Index);
                IRow headerRow = sheet.CreateRow(0);
                headerRow.HeightInPoints = 20;
                NPOI.SS.UserModel.ICell celltitle = headerRow.CreateCell(0);
                celltitle.SetCellValue(title);
                celltitle.CellStyle = HeadercellStyle;

                //用column name 作为列名
                IRow headerRow1 = sheet.CreateRow(1);
                foreach (DataColumn item in dt.Columns)
                {
                    NPOI.SS.UserModel.ICell cell1 = headerRow1.CreateCell(icolIndex);
                    cell1.SetCellValue(item.ColumnName);
                    cell1.CellStyle = HeadercellStyle;
                    icolIndex++;
                }

                ICellStyle cellStyle = workbook.CreateCellStyle();

                //为避免日期格式被Excel自动替换，所以设定 format 为 『@』 表示一率当成text來看
                cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");
                cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;


                NPOI.SS.UserModel.IFont cellfont = workbook.CreateFont();
                cellfont.Boldweight = (short)FontBoldWeight.Normal;
                cellStyle.SetFont(cellfont);

                //建立内容行
                int iRowIndex = 2;
                int iCellIndex = 0;
                foreach (DataRow Rowitem in dt.Rows)
                {
                    IRow DataRow = sheet.CreateRow(iRowIndex);
                    foreach (DataColumn Colitem in dt.Columns)
                    {

                        NPOI.SS.UserModel.ICell cell = DataRow.CreateCell(iCellIndex);
                        cell.SetCellValue(Rowitem[Colitem].ToString());
                        cell.CellStyle = cellStyle;
                        iCellIndex++;
                    }
                    iCellIndex = 0;
                    iRowIndex++;
                }

                //自适应列宽度
                for (int i = 0; i < icolIndex; i++)
                {
                    sheet.AutoSizeColumn(i);
                }
                //写Excel
                FileStream file = new FileStream(strExcelFileName, FileMode.OpenOrCreate);
                workbook.Write(file);
                file.Flush();
                file.Close();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            finally
            {
                workbook = null;
            }

        }
        /// <summary>
        /// Excel文件导成Datatable
        /// </summary>
        /// <param name="strFilePath">Excel文件目录地址</param>
        /// <param name="strTableName">Datatable表名</param>
        /// <param name="iSheetIndex">Excel sheet index</param>
        /// <returns></returns>
        public static DataTable XlSToDataTable(string strFilePath, string strTableName, int iSheetIndex)
        {

            string strExtName = Path.GetExtension(strFilePath);

            DataTable dt = new DataTable();
            if (!string.IsNullOrEmpty(strTableName))
            {
                dt.TableName = strTableName;
            }

            if (strExtName.Equals(".xls") || strExtName.Equals(".xlsx"))
            {
                using (FileStream file = new FileStream(strFilePath, FileMode.Open, FileAccess.Read))
                {
                    ISheet sheet;
                    if (strExtName.Equals(".xls"))//2003
                    {
                        HSSFWorkbook workbook = new HSSFWorkbook(file);
                        sheet = workbook.GetSheetAt(iSheetIndex);
                    }
                    else//2007
                    {
                        XSSFWorkbook workbook = new XSSFWorkbook(file);
                        sheet = workbook.GetSheetAt(iSheetIndex);
                    }

                    //列头
                    foreach (NPOI.SS.UserModel.ICell item in sheet.GetRow(sheet.FirstRowNum).Cells)
                    {
                        dt.Columns.Add(item.ToString(), typeof(string));
                    }

                    //写入内容
                    System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
                    while (rows.MoveNext())
                    {
                        IRow row;
                        if (strExtName.Equals(".xlsx"))
                        {
                            row = (XSSFRow)rows.Current;
                        }
                        else
                        {
                            row = (HSSFRow)rows.Current;
                        }
                        if (row.RowNum == sheet.FirstRowNum)
                        {
                            continue;
                        }

                        DataRow dr = dt.NewRow();
                        foreach (NPOI.SS.UserModel.ICell item in row.Cells)
                        {
                            switch (item.CellType)
                            {
                                case CellType.Boolean:
                                    dr[item.ColumnIndex] = item.BooleanCellValue;
                                    break;
                                case CellType.Error:
                                    dr[item.ColumnIndex] = ErrorEval.GetText(item.ErrorCellValue);
                                    break;
                                case CellType.Formula:
                                    switch (item.CachedFormulaResultType)
                                    {
                                        case CellType.Boolean:
                                            dr[item.ColumnIndex] = item.BooleanCellValue;
                                            break;
                                        case CellType.Error:
                                            dr[item.ColumnIndex] = ErrorEval.GetText(item.ErrorCellValue);
                                            break;
                                        case CellType.Numeric:
                                            if (DateUtil.IsCellDateFormatted(item))
                                            {
                                                dr[item.ColumnIndex] = item.DateCellValue.ToString("yyyy-MM-dd hh:MM:ss");
                                            }
                                            else
                                            {
                                                dr[item.ColumnIndex] = item.NumericCellValue;
                                            }
                                            break;
                                        case CellType.String:
                                            string str = item.StringCellValue;
                                            if (!string.IsNullOrEmpty(str))
                                            {
                                                dr[item.ColumnIndex] = str.ToString();
                                            }
                                            else
                                            {
                                                dr[item.ColumnIndex] = null;
                                            }
                                            break;
                                        case CellType.Unknown:
                                        case CellType.Blank:
                                        default:
                                            dr[item.ColumnIndex] = string.Empty;
                                            break;
                                    }
                                    break;
                                case CellType.Numeric:
                                    if (DateUtil.IsCellDateFormatted(item))
                                    {
                                        dr[item.ColumnIndex] = item.DateCellValue.ToString("yyyy-MM-dd hh:MM:ss");
                                    }
                                    else
                                    {
                                        dr[item.ColumnIndex] = item.NumericCellValue;
                                    }
                                    break;
                                case CellType.String:
                                    string strValue = item.StringCellValue;
                                    if (!string.IsNullOrEmpty(strValue))
                                    {
                                        dr[item.ColumnIndex] = strValue.ToString();
                                    }
                                    else
                                    {
                                        dr[item.ColumnIndex] = null;
                                    }
                                    break;
                                case CellType.Unknown:
                                case CellType.Blank:
                                default:
                                    dr[item.ColumnIndex] = string.Empty;
                                    break;
                            }
                        }
                        dt.Rows.Add(dr);
                    }
                }
            }

            return dt;
        }


        /// <summary>
        /// 读取Word内容
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ExcuteWordText(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            using (FileStream stream = File.OpenRead(fileName))
            {
                XWPFDocument doc = new XWPFDocument(stream);
                foreach (var para in doc.Paragraphs)
                {
                    string text = para.ParagraphText; //获得文本  

                    var runs = para.Runs;
                    string styleid = para.Style;

                    for (int i = 0; i < runs.Count; i++)
                    {
                        var run = runs[i];
                        text = run.ToString()+" "; //获得run的文本  
                        sb.Append(text);
                    }
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// 读取Word内容
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<string> GetWordTextList(string fileName)
        {
            //StringBuilder sb = new StringBuilder();
            List<string> list = new List<string>();
            using (FileStream stream = File.OpenRead(fileName))
            {
                XWPFDocument doc = new XWPFDocument(stream);
                foreach (var para in doc.Paragraphs)
                {
                    string text = para.ParagraphText; //获得文本  

                    var runs = para.Runs;
                    string styleid = para.Style;

                    for (int i = 0; i < runs.Count; i++)
                    {
                        var run = runs[i];
                        text = run.ToString(); //获得run的文本  
                        //sb.Append(text);
                        list.Add(text);
                    }
                }
            }
            return list;
        }
    }
}