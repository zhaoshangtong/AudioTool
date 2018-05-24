/*
 源码己托管:http://git.oschina.net/kuiyu/dotnetcodes
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Rays.Utility
{
    /// <summary>
    /// 文件下载类
    /// </summary>
    public class FileDown
    {
        public FileDown()
        { }

        /// <summary>
        /// 参数为虚拟路径
        /// </summary>
        public static string FileNameExtension(string FileName)
        {
            return Path.GetExtension(MapPathFile(FileName));
        }

        /// <summary>
        /// 获取物理地址
        /// </summary>
        public static string MapPathFile(string FileName)
        {
            return HttpContext.Current.Server.MapPath(FileName);
        }

        /// <summary>
        /// 普通下载
        /// </summary>
        /// <param name="FileName">文件虚拟路径</param>
        public static void DownLoadold(string FileName)
        {
            string destFileName = MapPathFile(FileName);
            if (File.Exists(destFileName))
            {
                FileInfo fi = new FileInfo(destFileName);
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.Buffer = false;
                HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(destFileName), System.Text.Encoding.UTF8));
                HttpContext.Current.Response.AppendHeader("Content-Length", fi.Length.ToString());
                HttpContext.Current.Response.ContentType = "application/octet-stream";
                HttpContext.Current.Response.WriteFile(destFileName);
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// 分块下载
        /// </summary>
        /// <param name="FileName">文件虚拟路径</param>
        public static void DownLoad(string FileName)
        {
            string filePath = MapPathFile(FileName);
            long chunkSize = 204800;             //指定块大小 
            byte[] buffer = new byte[chunkSize]; //建立一个200K的缓冲区 
            long dataToRead = 0;                 //已读的字节数   
            FileStream stream = null;
            try
            {
                //打开文件   
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                dataToRead = stream.Length;

                //添加Http头   
                HttpContext.Current.Response.ContentType = "application/octet-stream";
                HttpContext.Current.Response.AddHeader("Content-Disposition", "attachement;filename=" + HttpUtility.UrlEncode(Path.GetFileName(filePath)));
                HttpContext.Current.Response.AddHeader("Content-Length", dataToRead.ToString());

                while (dataToRead > 0)
                {
                    if (HttpContext.Current.Response.IsClientConnected)
                    {
                        int length = stream.Read(buffer, 0, Convert.ToInt32(chunkSize));
                        HttpContext.Current.Response.OutputStream.Write(buffer, 0, length);
                        HttpContext.Current.Response.Flush();
                        HttpContext.Current.Response.Clear();
                        dataToRead -= length;
                    }
                    else
                    {
                        dataToRead = -1; //防止client失去连接 
                    }
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("Error:" + ex.Message);
            }
            finally
            {
                if (stream != null) stream.Close();
                HttpContext.Current.Response.Close();
            }
        }

        #region 异步下载文件
        /// <summary>
        /// 异步下载文件
        /// </summary>
        /// <param name="uri">下载地址</param>
        /// <param name="savepath">文件保存的绝对路径</param>
        /// <param name="retryCount">重试下载次数</param>
        /// <returns></returns>
        public async static Task<bool> RetryDownloadAsync(string uri, string savepath, int retryCount = 3)
        {
            var dir = Path.GetDirectoryName(savepath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            try
            {
                //使用断点续传下载
                return await ResumeDownloadFileAsync(uri, savepath);
            }
            catch (Exception ex)
            {
                Log.Info("数据下载失败，ERROR：" + ex.Message);
                if (ex.Message.IndexOf("416") > -1)//416 即已经下载完毕的视频
                {
                    return true;
                }
                else if (retryCount < 3)
                {
                    Log.Info((10 * retryCount) + "秒后对数据进行重试下载，当前重试次数为 " + retryCount);
                    Thread.Sleep(1000 * 10 * retryCount);    // (10 * downloadcount) 秒后进行重试
                    //重试机制
                    return await RetryDownloadAsync(uri, savepath, ++retryCount);
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region 断点续传方式下载文件
        public async static Task<bool> ResumeDownloadFileAsync(string uri, string savepath)
        {
            //打开上次下载的文件的位置
            long SPosition = 0;
            FileStream fs;

            if (File.Exists(savepath))
            {
                fs = File.OpenWrite(savepath);
                SPosition = fs.Length;
                fs.Seek(SPosition, SeekOrigin.Current); //移动文件流中的当前指针到上次下载完的位置
            }
            else
            {
                fs = new FileStream(savepath, FileMode.Create);
                SPosition = 0;
            }

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    if (SPosition > 0)
                    {
                        httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(SPosition, null);
                    }
                    using (var stream = await httpClient.GetStreamAsync(uri))
                    {
                        byte[] btContent = new byte[512];
                        var intSize = 0;
                        while ((intSize = await stream.ReadAsync(btContent, 0, btContent.Length)) > 0)
                        {
                            await fs.WriteAsync(btContent, 0, intSize);
                        }
                    }
                }

                //HttpWebRequest request = WebRequest.CreateHttp(uri);
                //if (SPosition > 0)
                //{
                //    request.AddRange((int)SPosition);   //设置请求头的Range值
                //}

                ////获得服务器的回应数据流
                //using (Stream stream = (await request.GetResponseAsync()).GetResponseStream())
                //{
                //    byte[] btContent = new byte[512];

                //    var intSize = 0;
                //    while ((intSize = await stream.ReadAsync(btContent, 0, btContent.Length)) > 0)
                //    {
                //        await fs.WriteAsync(btContent, 0, intSize);
                //    }
                //}
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                fs.Close();
                fs.Dispose();
            }
        }
        #endregion

        /// <summary>
        ///  输出硬盘文件，提供下载 支持大文件、续传、速度限制、资源占用小
        /// </summary>
        /// <param name="_Request">Page.Request对象</param>
        /// <param name="_Response">Page.Response对象</param>
        /// <param name="_fileName">下载文件名</param>
        /// <param name="_fullPath">带文件名下载路径</param>
        /// <param name="_speed">每秒允许下载的字节数</param>
        /// <returns>返回是否成功</returns>
        //---------------------------------------------------------------------
        //调用：
        // string FullPath=Server.MapPath("count.txt");
        // ResponseFile(this.Request,this.Response,"count.txt",FullPath,100);
        //---------------------------------------------------------------------
        public static bool ResponseFile(HttpRequest _Request, HttpResponse _Response, string _fileName, string _fullPath, long _speed)
        {
            try
            {
                FileStream myFile = new FileStream(_fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader br = new BinaryReader(myFile);
                try
                {
                    _Response.AddHeader("Accept-Ranges", "bytes");
                    _Response.Buffer = false;

                    long fileLength = myFile.Length;
                    long startBytes = 0;
                    int pack = 10240;  //10K bytes
                    int sleep = (int)Math.Floor((double)(1000 * pack / _speed)) + 1;

                    if (_Request.Headers["Range"] != null)
                    {
                        _Response.StatusCode = 206;
                        string[] range = _Request.Headers["Range"].Split(new char[] { '=', '-' });
                        startBytes = Convert.ToInt64(range[1]);
                    }
                    _Response.AddHeader("Content-Length", (fileLength - startBytes).ToString());
                    if (startBytes != 0)
                    {
                        _Response.AddHeader("Content-Range", string.Format(" bytes {0}-{1}/{2}", startBytes, fileLength - 1, fileLength));
                    }

                    _Response.AddHeader("Connection", "Keep-Alive");
                    _Response.ContentType = "application/octet-stream";
                    _Response.AddHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(_fileName, System.Text.Encoding.UTF8));

                    br.BaseStream.Seek(startBytes, SeekOrigin.Begin);
                    int maxCount = (int)Math.Floor((double)((fileLength - startBytes) / pack)) + 1;

                    for (int i = 0; i < maxCount; i++)
                    {
                        if (_Response.IsClientConnected)
                        {
                            _Response.BinaryWrite(br.ReadBytes(pack));
                            Thread.Sleep(sleep);
                        }
                        else
                        {
                            i = maxCount;
                        }
                    }
                }
                catch
                {
                    return false;
                }
                finally
                {
                    br.Close();
                    myFile.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
