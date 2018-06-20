using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace AudioToolNew.Commom
{
    public class sys
    {
        public static string errMessage = "";
        /// 
        /// 获取错误信息.
        /// 
        public static string ErrMessage
        {
            get
            {
                return errMessage;
            }
        }


        /*产生验证码*/
        public static string getRandomCode(int codeLength)
        {

            string so = "2,3,5,6,7,8,9,A,B,C,D,E";
            string[] strArr = so.Split(',');
            string code = "";
            Random rand = new Random();
            for (int i = 0; i < codeLength; i++)
            {
                code += strArr[rand.Next(0, strArr.Length)];
            }
            return code;
        }

        public static string getIsTypeName(string v)
        {
            string name = "";
            if (v != null)
            {
                switch (v)
                {
                    case "1":
                        name = "客 户";
                        break;
                    default:
                        name = "供应商";
                        break;
                }
            }
            return name;

        }

        public static string getIsMainName(string v)
        {
            string name = "";
            if (v != null)
            {
                switch (v)
                {
                    case "0":
                        name = "主题";
                        break;
                    default:
                        name = "跟帖";
                        break;
                }
            }
            return name;

        }



        public static string getShowmodalName(string s)
        {

            string name = "";
            if (s != null)
            {
                switch (s)
                {
                    case "indexList":
                        name = "索引信息列表";
                        break;
                    case "infoList":
                        name = "标题与信息直接展现";
                        break;
                    case "pageList":
                        name = "普通列表";
                        break;
                    case "picList":
                        name = "图形列表";
                        break;
                    default:
                        name = "未知";
                        break;
                }
            }
            return name;
        }

        




        /// <summary>
        /// 创建文本文本文件
        /// </summary>
        public static void createContextTextFile(HttpContext context, string path, string s)
        {
            path = context.Server.MapPath(path);
            if (!System.IO.File.Exists(path))
            {
                System.IO.File.Create(path).Close();
            }
            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.GetEncoding("UTF-8"));
            sw.Write(s);
            sw.Flush();
            sw.Close();
        }


        /// <summary>
        /// 创建文本文本文件
        /// </summary>
        public static void createTextFile(string path, string s)
        {
            path = System.Web.HttpContext.Current.Server.MapPath(path);
            if (!System.IO.File.Exists(path))
            {
                System.IO.File.Create(path).Close();
            }
            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.GetEncoding("gb2312"));
            sw.Write(s);
            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// 创建文本文本文件
        /// </summary>
        public static void createUTF8TextFile(string path, string s)
        {
            path = System.Web.HttpContext.Current.Server.MapPath(path);
            if (!System.IO.File.Exists(path))
            {
                System.IO.File.Create(path).Close();
            }
            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.GetEncoding("utf-8"));
            sw.Write(s);
            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// 创建文本文本文件
        /// </summary>
        public static void createTextFileByMapPath(string map_path, string s)
        {
            if (!System.IO.File.Exists(map_path))
            {
                System.IO.File.Create(map_path).Close();
            }
            StreamWriter sw = new StreamWriter(map_path, false, System.Text.Encoding.GetEncoding("gb2312"));
            sw.Write(s);
            sw.Flush();
            sw.Close();
        }



        /// <summary>
        /// 创建文本文本文件
        /// </summary>
        public static void createTextFile(string path, string s, string encoding)
        {
            StreamWriter sw = new StreamWriter(System.Web.HttpContext.Current.Server.MapPath(path), false, System.Text.Encoding.GetEncoding(encoding));
            sw.Write(s);
            sw.Flush();
            sw.Close();
        }


        /// <summary>
        /// 获得目录下的文件，返回的是本地全路径
        /// </summary>
        public static string[] getFolderFile(string path)
        {
            return Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath(path));
        }


        /// <summary>
        /// 获得目录下的文件，返回的是本地全路径
        /// </summary>
        public static DataTable getFolderFileList(string path)
        {
            DataTable filesTable = new DataTable();
            DataRow row;
            filesTable.Columns.Add("number", typeof(int));
            filesTable.Columns.Add("filename", typeof(string));
            filesTable.Columns.Add("webpath", typeof(string));

            if (sys.existsDirectory(path))
            {
                int fileNumber = 0;
                string[] rootfiles = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath(path));
                if (rootfiles != null)
                {
                    for (int i = 0; i < rootfiles.Length; i++)
                    {
                        fileNumber++;
                        row = filesTable.NewRow();
                        row["number"] = fileNumber;
                        row["filename"] = rootfiles[i];
                        int d = rootfiles[i].LastIndexOf("\\");
                        row["webpath"] = path + "/" + rootfiles[i].Substring(d + 1, rootfiles[i].Length - d - 1);
                        filesTable.Rows.Add(row);
                    }
                }

            }
            return filesTable;
        }

        /// <summary>
        /// 获得目录下的所有文件同时遍历所有子目录，返回的是本地全路径
        /// </summary>
        public static DataTable getFolderFileDatatable(string path)
        {

            DataTable filesTable = new DataTable();
            DataRow row;
            filesTable.Columns.Add("number", typeof(int));
            filesTable.Columns.Add("filename", typeof(string));

            if (sys.existsDirectory(path))
            {
                int fileNumber = 0;
                string[] rootfiles = Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath(path));
                if (rootfiles != null)
                {
                    for (int i = 0; i < rootfiles.Length; i++)
                    {
                        fileNumber++;
                        row = filesTable.NewRow();
                        row["number"] = fileNumber;
                        row["filename"] = rootfiles[i];
                        filesTable.Rows.Add(row);
                    }
                }

                string[] dir = System.IO.Directory.GetDirectories(System.Web.HttpContext.Current.Server.MapPath(path));
                foreach (string s in dir)
                {
                    string[] childfiles = Directory.GetFiles(s);
                    if (childfiles != null)
                    {
                        for (int i = 0; i < childfiles.Length; i++)
                        {
                            fileNumber++;
                            row = filesTable.NewRow();
                            row["number"] = fileNumber;
                            row["filename"] = childfiles[i];
                            filesTable.Rows.Add(row);
                        }
                    }
                }
            }
            return filesTable;
        }


        /// <summary>
        /// 判断目录是否存在
        /// </summary>
        public static bool existsDirectory(string path)
        {
            try
            {
                return System.IO.Directory.Exists(System.Web.HttpContext.Current.Server.MapPath(path));
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 获得目录下的文件，返回的是本地全路径
        /// </summary>
        public static DataTable getgetFolderFileToDatTable(string path)
        {
            DataTable table = new DataTable();
            DataRow row;
            table.Columns.Add("number", typeof(int));
            table.Columns.Add("filename", typeof(string));
            string[] files = getFolderFile(path);
            for (int i = 0; i < files.Length; i++)
            {
                row = table.NewRow();
                row["number"] = i;
                row["filename"] = files[i];
                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// 删除某个文件，根据虚拟路径
        /// </summary>
        public static void deleteFileByMapPath(string path)
        {
            deleteFile(System.Web.HttpContext.Current.Server.MapPath(path));
        }

        /// <summary>
        /// 删除某个文件，根据物理路径
        /// </summary>
        public static void deleteFile(string path)
        {
            System.IO.File.Delete(path);
        }



        /// </summary>
        /// 删除一个目录，先遍历删除其下所有文件和目录（递归）
        /// <param name="strPath">路径</param>
        /// <returns>是否已经删除</returns>
        /// </summary>
        public static bool DeleteADirectory(string strPath)
        {
            string[] strTemp;
            try
            {
                //先删除该目录下的文件
                strTemp = System.IO.Directory.GetFiles(strPath);
                foreach (string str in strTemp)
                {
                    System.IO.File.Delete(str);
                }
                //删除子目录，递归
                strTemp = System.IO.Directory.GetDirectories(strPath);
                foreach (string str in strTemp)
                {
                    DeleteADirectory(str);
                }
                //删除该目录
                System.IO.Directory.Delete(strPath);
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        /// <summary>
        /// 判断文本文本文件是否存在，并返回读取的字符串
        /// </summary>
        public static bool existsFile(string path)
        {
            return System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(path));
        }

        /// <summary>
        /// 递归拷贝所有子目录。
        /// </summary>
        /// <param name="sPath">源目录</param>
        /// <param name="dPath">目的目录</param>
        public static void copyDirectory(string sPath, string dPath)
        {


            string[] directories = System.IO.Directory.GetDirectories(sPath);
            if (!System.IO.Directory.Exists(dPath))
                System.IO.Directory.CreateDirectory(dPath);
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(sPath);
            System.IO.DirectoryInfo[] dirs = dir.GetDirectories();
            CopyFile(dir, dPath);
            if (dirs.Length > 0)
            {
                foreach (System.IO.DirectoryInfo temDirectoryInfo in dirs)
                {
                    string sourceDirectoryFullName = temDirectoryInfo.FullName;
                    string destDirectoryFullName = sourceDirectoryFullName.Replace(sPath, dPath);
                    if (!System.IO.Directory.Exists(destDirectoryFullName))
                    {
                        System.IO.Directory.CreateDirectory(destDirectoryFullName);
                    }
                    CopyFile(temDirectoryInfo, destDirectoryFullName);
                    copyDirectory(sourceDirectoryFullName, destDirectoryFullName);
                }
            }

        }

        /// <summary>
        /// 拷贝目录下的所有文件到目的目录。
        /// </summary>
        /// <param name="path">源路径</param>
        /// <param name="desPath">目的路径</param>
        public static void CopyFile(System.IO.DirectoryInfo path, string desPath)
        {
            string sourcePath = path.FullName;
            System.IO.FileInfo[] files = path.GetFiles();
            foreach (System.IO.FileInfo file in files)
            {
                string sourceFileFullName = file.FullName;
                string destFileFullName = sourceFileFullName.Replace(sourcePath, desPath);
                file.CopyTo(destFileFullName, true);
            }
        }

        /// <summary>
        /// 读取文本文本文件
        /// </summary>
        public static StreamReader readerTextFile(string path)
        {
            StreamReader objReader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath(path), System.Text.Encoding.GetEncoding("gb2312"));
            return objReader;

        }


        //索引信息
        public static string showindexinfo(string s)
        {
            s = s.Replace(Convert.ToString((char)13), "<br>");

            return s;

        }

        //详细信息
        public static string showInfo(string s)
        {
            s = s.Replace("&lt;", "<");
            s = s.Replace("&gt;", ">");
            s = s.Replace("&amp;", "&");
            return s;
        }

        //相同的 DataTable 追加 DataTable
        public static DataTable pushDataTable(DataTable firstDataTable, DataTable lastDataTable)
        {
            DataTable dtn = new DataTable();
            dtn = firstDataTable.Clone();

            for (int i = 0; i < firstDataTable.Rows.Count; i++)
            {
                DataRow row = dtn.NewRow();
                for (int j = 0; j < firstDataTable.Columns.Count; j++)
                {
                    row[j] = firstDataTable.Rows[i][j];
                }
                dtn.Rows.Add(row);
            }



            for (int i = 0; i < lastDataTable.Rows.Count; i++)
            {
                DataRow row = dtn.NewRow();
                for (int j = 0; j < lastDataTable.Columns.Count; j++)
                {
                    row[j] = lastDataTable.Rows[i][j];
                }
                dtn.Rows.Add(row);
            }



            return dtn;
        }




        /// <summary>
        /// 取得随机字符串，按照年月日时分秒毫秒，返回如：2008_08_12_08_56_002
        /// </summary>
        public static string getRandomStr()
        {
            string s = "";
            DateTime myDateTime = System.DateTime.Now;
            s += myDateTime.Year + "_" + formartStringLeng(myDateTime.Month.ToString(), 2) + "_" + formartStringLeng(myDateTime.Day.ToString(), 2) + "_" + formartStringLeng(myDateTime.Hour.ToString(), 2) + formartStringLeng(myDateTime.Minute.ToString(), 2) + formartStringLeng(myDateTime.Second.ToString(), 2) + formartStringLeng(myDateTime.Millisecond.ToString(), 3);
            return s;
        }


        /// <summary>
        /// 取得年月日，返回如：20080808
        /// </summary>
        public static string getDateStr()
        {
            string s = "";
            DateTime myDateTime = System.DateTime.Now;
            s += myDateTime.Year + "" + formartStringLeng(myDateTime.Month.ToString(), 2) + "" + formartStringLeng(myDateTime.Day.ToString(), 2);
            return s;
        }


        /// <summary>
        /// 取得年月，返回如：200808
        /// </summary>
        public static string getYearMonthStr()
        {
            string s = "";
            DateTime myDateTime = System.DateTime.Now;
            s += myDateTime.Year + "" + formartStringLeng(myDateTime.Month.ToString(), 2);
            return s;
        }


        /// <summary>
        /// 按照长度格式化字符串，不足位时补"0"
        /// </summary>
        public static string formartStringLeng(string s, int leng)
        {
            int len = leng - s.Length;
            for (int i = 0; i < len; i++)
            {
                s = "0" + s;
            }
            return s;
        }


        public static void printScript(HttpRequest request, HttpResponse response, string info, string url)
        {
            response.Write("<script>alert('" + info + "');window.location='" + url + "';</script>");
            response.End();
        }
    }
}