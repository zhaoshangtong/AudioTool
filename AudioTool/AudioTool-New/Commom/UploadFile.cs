using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace AudioToolNew.Common
{
    public class UploadFile
    {
        /// <summary>
        /// 上传到文件服务器
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <returns></returns>
        public static string PostFile(string filePath)
        {
            string domain = "http://f3.5rs.me/";
            FileInfo fileInfo_sound = new FileInfo(filePath);
            if (fileInfo_sound.Exists)
            {
                string response = UploadFile.HttpPostFile(domain + "/index.aspx", fileInfo_sound, null);
                JObject jobject = JObject.Parse(response);
                int i = 0;
                if (bool.Parse(jobject["success"].ToString()))
                {
                    return jobject["path"].ToString();
                }
                else
                {
                    LogHelper.Error(jobject.ToString());
                }
            }
            else
            {
                LogHelper.Error("文件不存在");
            }
            return "";
        }
        /// <summary>
        /// 将文件上传到服务器
        /// </summary>
        /// <param name="url"></param>
        /// <param name="buffer"></param>
        /// <param name="fileName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string HttpPostFile(string url, byte[] buffer, string fileName, Dictionary<string, object> parameters)
        {
            //1>创建请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //2>Cookie容器
            request.Method = "POST";
            //request.Timeout = 20000;
            request.Credentials = System.Net.CredentialCache.DefaultCredentials;
            request.KeepAlive = true;

            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");//分界线
            byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            request.ContentType = "multipart/form-data; boundary=" + boundary; ;//内容类型

            //3>表单数据模板
            string formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

            //5>写入请求流数据
            string strHeader = "Content-Disposition:application/x-www-form-urlencoded; name=\"{0}\";filename=\"{1}\"\r\nContent-Type:{2}\r\n\r\n";
            strHeader = string.Format(strHeader,
                                     "file",
                                     fileName,
                                     "application/octet-stream");
            //6>HTTP请求头
            byte[] byteHeader = Encoding.ASCII.GetBytes(strHeader);
            try
            {
                using (Stream stream = request.GetRequestStream())
                {
                    //写入请求流
                    if (null != parameters)
                    {
                        foreach (KeyValuePair<string, object> item in parameters)
                        {
                            stream.Write(boundaryBytes, 0, boundaryBytes.Length);//写入分界线
                            byte[] formBytes = System.Text.Encoding.UTF8.GetBytes(string.Format(formdataTemplate, item.Key, item.Value));
                            stream.Write(formBytes, 0, formBytes.Length);
                        }
                    }
                    //6.0>分界线============================================注意：缺少次步骤，可能导致远程服务器无法获取Request.Files集合
                    stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                    //6.1>请求头
                    stream.Write(byteHeader, 0, byteHeader.Length);
                    //6.2>把文件流写入请求流
                    stream.Write(buffer, 0, buffer.Length);
                    //6.3>写入分隔流
                    byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    stream.Write(trailer, 0, trailer.Length);
                    //6.4>关闭流
                    stream.Close();
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return "{\"success\":false,\"message\":\"上传文件时远程服务器发生异常--" + ex.Message + "\"}";
            }
        }

        /// <summary>
        /// 文件上传至远程服务器
        /// </summary>
        /// <param name="url">远程服务地址</param>
        /// <param name="file">上传文件</param>
        /// <param name="parameters">POST参数</param>
        public static string HttpPostFile(string url,
                                        FileInfo file,
                                        Dictionary<string, object> parameters)
        {
            //1>创建请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //2>Cookie容器
            request.Method = "POST";
            //request.Timeout = 100000;
            request.Credentials = System.Net.CredentialCache.DefaultCredentials;
            request.KeepAlive = true;

            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");//分界线
            byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            request.ContentType = "multipart/form-data; boundary=" + boundary; ;//内容类型

            //3>表单数据模板
            string formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

            //4>读取流
            byte[] buffer = new byte[file.Length];
            FileStream fs = file.OpenRead();
            fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
            fs.Close();

            //5>写入请求流数据
            string strHeader = "Content-Disposition:application/x-www-form-urlencoded; name=\"{0}\";filename=\"{1}\"\r\nContent-Type:{2}\r\n\r\n";
            strHeader = string.Format(strHeader,
                                     "file",
                                     file.Name,
                                     "application/octet-stream");
            //6>HTTP请求头
            byte[] byteHeader = Encoding.ASCII.GetBytes(strHeader);
            try
            {
                using (Stream stream = request.GetRequestStream())
                {
                    //写入请求流
                    if (null != parameters)
                    {
                        foreach (KeyValuePair<string, object> item in parameters)
                        {
                            stream.Write(boundaryBytes, 0, boundaryBytes.Length);//写入分界线
                            byte[] formBytes = System.Text.Encoding.UTF8.GetBytes(string.Format(formdataTemplate, item.Key, item.Value));
                            stream.Write(formBytes, 0, formBytes.Length);
                        }
                    }
                    //6.0>分界线============================================注意：缺少次步骤，可能导致远程服务器无法获取Request.Files集合
                    stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                    //6.1>请求头
                    stream.Write(byteHeader, 0, byteHeader.Length);
                    //6.2>把文件流写入请求流
                    stream.Write(buffer, 0, buffer.Length);
                    //6.3>写入分隔流
                    byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    stream.Write(trailer, 0, trailer.Length);
                    //6.4>关闭流
                    stream.Close();
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return "{\"success\":false,\"message\":\"上传文件时远程服务器发生异常--" + ex.Message + "\"}";
            }
        }
    }
}