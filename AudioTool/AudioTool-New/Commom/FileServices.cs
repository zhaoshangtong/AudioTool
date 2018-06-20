using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace AudioToolNew.Commom
{
    public class FileServices
    {
        /// <summary>
        /// 获得音频时长
        /// </summary>
        public static string getMediaDuration(string sFile)
        {
            //引用shell32，服务器上需要打开windows功能：优化windows视频音频体验、桌面体验，需要重启服务器，留做备用//韩添ADD
            //ShellClass sh = new ShellClass();
            //Folder dir = sh.NameSpace(Path.GetDirectoryName(sFile));
            //FolderItem item = dir.ParseName(Path.GetFileName(sFile));            
            //return dir.GetDetailsOf(item, 27);
            //shell32 END
            string ffmpegPath = HttpContext.Current.Request.PhysicalApplicationPath + "MediaConvert\\";
            string szExeFilePath = ffmpegPath + "ffmpeg.exe ";
            Process p = new Process();
            p.StartInfo.FileName = szExeFilePath;
            p.StartInfo.Arguments = " -i " + sFile;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;     //重定向输入（一定是true） 
            p.StartInfo.RedirectStandardOutput = true;    //重定向输出    
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;

            string output = "";
            try
            {
                if (p.Start())//开始进程
                {
                    output = p.StandardError.ReadToEnd();//读取进程的输出,StandardOutput不行，因为ffmpeg输出都是StandardError
                    p.WaitForExit();
                    p.Close();

                }
            }
            catch (Exception e)
            {
                output = e.Message;
            }
            finally
            {
                if (p != null)
                { p.Close(); }
            }
            if (output.IndexOf("Duration") > -1)
            {
                output = output.Substring(output.IndexOf("Duration: ") + 10, 8);
            }
            else
            {
                output = "00:00:00";
            }
            return output;
        }
        public static string urlConvertor(string url1)
        {
            string tmpRootDir = HttpContext.Current.Server.MapPath(System.Web.HttpContext.Current.Request.ApplicationPath.ToString());//获取程序根目录
            string url2 = url1.Replace(tmpRootDir, ""); //转换成相对路径
            url2 = url2.Replace(@"\", @"/");
            return "http://" + HttpContext.Current.Request.Url.Host + "/" + url2;
        }
    }
}