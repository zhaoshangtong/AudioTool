using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Newtonsoft.Json;
using AudioToolNew.Common;


/// <summary>
/// 基础工具类
/// </summary>
public class Util
{
    #region 获得用户IP
    /// <summary>
    /// 获得用户IP
    /// </summary>
    public static string GetUserIp()
    {
        string ip;
        string[] temp;
        bool isErr = false;
        if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_ForWARDED_For"] == null)
            ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
        else
            ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_ForWARDED_For"].ToString();
        if (ip.Length > 15)
            isErr = true;
        else
        {
            temp = ip.Split('.');
            if (temp.Length == 4)
            {
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i].Length > 3) isErr = true;
                }
            }
            else
                isErr = true;
        }

        if (isErr)
            return "1.1.1.1";
        else
            return ip;
    }

    /// <summary>
    /// 获得服务器的域名路径，包含端口
    /// </summary>
    /// <returns></returns>
    public static string getServerPath()
    {
        HttpRequest req = System.Web.HttpContext.Current.Request;
        string serverpath = req.ServerVariables["SERVER_NAME"];
        bool ishascurrentport = false;
        for (int i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
        {
            if (ConfigurationManager.ConnectionStrings[i].Name == "CURRENTPORT")
            {
                ishascurrentport = true;
            }
        }
        if (ishascurrentport)
        {
            string currentport = ConfigurationManager.ConnectionStrings["CURRENTPORT"].ConnectionString;
            if (req.ServerVariables["SERVER_PORT"] != null && !"".Equals(req.ServerVariables["SERVER_PORT"]) && !currentport.Equals(req.ServerVariables["SERVER_PORT"])) { serverpath += ":" + req.ServerVariables["SERVER_PORT"]; }
        }
        else
        {
            if ("443".Equals(req.ServerVariables["SERVER_PORT"]))
            {
                return "https://" + serverpath;
            }
            else if (req.ServerVariables["SERVER_PORT"] != null && !"".Equals(req.ServerVariables["SERVER_PORT"]) && !"80".Equals(req.ServerVariables["SERVER_PORT"])) { serverpath += ":" + req.ServerVariables["SERVER_PORT"]; }
        }
        return "http://" + serverpath;
    }
    #endregion

    #region Stopwatch计时器  
    /// <summary>  
    /// 计时器开始  
    /// </summary>  
    /// <returns></returns>  
    public static Stopwatch TimerStart()
    {
        Stopwatch watch = new Stopwatch();
        watch.Reset();
        watch.Start();
        return watch;
    }
    /// <summary>  
    /// 计时器结束  
    /// </summary>  
    /// <param name="watch"></param>  
    /// <returns></returns>  
    public static string TimerEnd(Stopwatch watch)
    {
        watch.Stop();
        double costtime = watch.ElapsedMilliseconds;
        return costtime.ToString();
    }
    #endregion

    #region 删除数组中的重复项  
    /// <summary>  
    /// 删除数组中的重复项  
    /// </summary>  
    /// <param name="values"></param>  
    /// <returns></returns>  
    public static string[] RemoveDup(string[] values)
    {
        List<string> list = new List<string>();
        for (int i = 0; i < values.Length; i++)//遍历数组成员  
        {
            if (!list.Contains(values[i]))
            {
                list.Add(values[i]);
            };
        }
        return list.ToArray();
    }
    #endregion

    #region 自动生成日期编号  
    /// <summary>  
    /// 自动生成编号  201008251145409865  
    /// </summary>  
    /// <returns></returns>  
    public static string CreateNo()
    {
        Random random = new Random();
        string strRandom = random.Next(1000, 10000).ToString(); //生成编号   
        string code = DateTime.Now.ToString("yyyyMMddHHmmss") + strRandom;//形如  
        return code;
    }
    #endregion

    #region 生成0-9随机数  
    /// <summary>  
    /// 生成0-9随机数  
    /// </summary>  
    /// <param name="codeNum">生成长度</param>  
    /// <returns></returns>  
    public static string RndNum(int codeNum)
    {
        StringBuilder sb = new StringBuilder(codeNum);
        Random rand = new Random();
        for (int i = 1; i < codeNum + 1; i++)
        {
            int t = rand.Next(9);
            sb.AppendFormat("{0}", t);
        }
        return sb.ToString();

    }
    #endregion

    #region 删除最后一个字符之后的字符  
    /// <summary>  
    /// 删除最后结尾的一个逗号  
    /// </summary>  
    public static string DelLastComma(string str)
    {
        return str.Substring(0, str.LastIndexOf(","));
    }
    /// <summary>  
    /// 删除最后结尾的指定字符后的字符  
    /// </summary>  
    public static string DelLastChar(string str, string strchar)
    {
        return str.Substring(0, str.LastIndexOf(strchar));
    }
    /// <summary>  
    /// 删除最后结尾的长度  
    /// </summary>  
    /// <param name="str"></param>  
    /// <param name="Length"></param>  
    /// <returns></returns>  
    public static string DelLastLength(string str, int Length)
    {
        if (string.IsNullOrEmpty(str))
            return "";
        str = str.Substring(0, str.Length - Length);
        return str;
    }
    #endregion

    #region MD5加密
    /// <summary>
    /// 32位MD5加密
    /// </summary>
    /// <param name="strText">要加密字符串</param>
    /// <returns></returns>
    public static string MD5Encrypt(string strText)
    {
        string ret = "";
        if (!string.IsNullOrEmpty(strText))
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(strText);
            bytes = md5.ComputeHash(bytes);
            md5.Clear();
            for (int i = 0; i < bytes.Length; i++)
            {
                ret += bytes[i].ToString("X2");
            }
        }

        return ret;
    }
    #endregion

    #region http远程访问
    ///<summary>
    ///采用https协议访问网络
    ///</summary>
    ///<param name="URL">url地址</param>
    ///<param name="strPostdata">发送的数据</param>
    ///<param name="strEncoding">编码</param>
    ///<param name="contentType">数据类型</param>
    ///<returns></returns>
    public static string MethodPOST(string URL, string strPostdata, string strEncoding, string contentType = "application/x-www-form-urlencoded")
    {
        Encoding encoding = System.Text.Encoding.GetEncoding(strEncoding);
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
        request.Method = "post";
        request.Accept = "text/html, application/xhtml+xml, */*";
        request.ContentType = contentType;
        byte[] buffer = encoding.GetBytes(strPostdata);
        request.ContentLength = buffer.Length;
        request.GetRequestStream().Write(buffer, 0, buffer.Length);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        using (StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.GetEncoding(strEncoding)))
        {
            return reader.ReadToEnd();
        }
    }

    ///<summary>
    /// 异步访问访问网络
    ///</summary>
    ///<param name="URL">url地址</param>
    ///<param name="strPostdata">发送的数据</param>
    ///<param name="strEncoding">编码 默认 UTF-8</param>
    ///<param name="contentType">数据类型</param>
    ///<returns></returns>
    public async static Task<string> MethodPOSTAsync(string URL, string strPostdata, string strEncoding = "UTF-8", string contentType = "application/x-www-form-urlencoded")
    {
        //return MethodPOST(URL, strPostdata, strEncoding, contentType);
        //Encoding encoding = Encoding.GetEncoding(strEncoding);
        //var request = WebRequest.CreateHttp(URL);
        //request.Method = "POST";
        //request.Accept = "text/html, application/xhtml+xml, */*";
        //request.ContentType = contentType;
        //request.Timeout = 60 * 1000;

        //PictureBook.Common.LogHelper.Info("MethodPOSTAsync：" + strPostdata);

        //byte[] buffer = encoding.GetBytes(strPostdata);
        //request.ContentLength = buffer.Length;

        //using (var stream = await request.GetRequestStreamAsync())
        //{
        //    PictureBook.Common.LogHelper.Info("stream.Length before：" + stream.Length);
        //    await stream.WriteAsync(buffer, 0, buffer.Length);
        //    PictureBook.Common.LogHelper.Info("stream.Length after：" + stream.Length);
        //}

        //using (var reader = new StreamReader((await request.GetResponseAsync()).GetResponseStream(), encoding))
        //{
        //    return await reader.ReadToEndAsync();
        //}

        // 设置参数、编码
        var httpContent = new StringContent(strPostdata, Encoding.GetEncoding(strEncoding));
        // 设置参数类型
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        // 自动解压缩HTTP回应的GZIP压缩内容
        var handler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip
        };

        // 创建连接
        var httpClient = new HttpClient(handler);
        // 发起请求
        var response = await httpClient.PostAsync(URL, httpContent);

        // 确保返回成功，否则抛出异常
        response.EnsureSuccessStatusCode();
        // 读取返回值
        return await response.Content.ReadAsStringAsync();
    }


    /// <summary>
    /// 获得本地或远程请求的页面HTML源代码
    /// </summary>
    public static string MethodGET(string url, string encoding, string authorization = "")
    {
        System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
        if (!string.IsNullOrWhiteSpace(authorization))
            request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(System.Text.Encoding.GetEncoding(encoding).GetBytes(authorization)));
        request.Method = "GET";
        System.Net.WebResponse response = request.GetResponse();
        System.IO.Stream resStream = response.GetResponseStream();
        System.IO.StreamReader sr = new System.IO.StreamReader(resStream, System.Text.Encoding.GetEncoding(encoding));
        string str = sr.ReadToEnd();
        resStream.Close();
        sr.Close();

        return str;
    }


    /// <summary>
    /// 异步获得本地或远程请求的页面HTML源代码
    /// </summary>
    public async static Task<string> MethodGETAsync(string url, string encoding)
    {
        HttpWebRequest request = WebRequest.CreateHttp(url);
        using (var response = await request.GetResponseAsync())
        {
            using (var sr = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding)))
            {
                return await sr.ReadToEndAsync();
            }
        }
    }
    #region 文件上传至远程服务器
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
    #endregion
    #endregion

    #region 验证参数

    /// <summary>
    /// 验证参数
    /// <para>返回值：{ OK : false , Msg : "错误消息" }</para>
    /// </summary>
    /// <param name="parameters">需要进行验证的参数集合</param>
    /// <returns>{OK:false,Msg:"错误消息"}</returns>
    public static dynamic CheckParameters(params Parameter[] parameters)
    {
        //枚举 正则表达式 验证方式
        System.Text.RegularExpressions.Regex regex;
        foreach (Parameter temp in parameters)
        {
            regex = new System.Text.RegularExpressions.Regex(temp?.Regex ?? "");
            if (temp.IsCheck)
            {
                if (string.IsNullOrWhiteSpace(temp?.Value) || (!string.IsNullOrEmpty(temp?.Regex) && !regex.IsMatch(temp?.Value)))
                {
                    return new { OK = false, Msg = temp?.Msg };
                }
            }
        }
        return new { OK = true };
    }

    #endregion

    #region SHA1加密
    /// <summary>
    /// SHA1加密
    /// </summary>
    /// <param name="mk"></param>
    /// <param name="secretkey"></param>
    /// <returns></returns>
    public static string HmacSha1AndBase64(string mk, string secretkey)
    {
        HMACSHA1 hmacsha1 = new HMACSHA1();
        hmacsha1.Key = Encoding.UTF8.GetBytes(secretkey);
        byte[] dataBuffer = Encoding.UTF8.GetBytes(mk);
        byte[] hashBytes = hmacsha1.ComputeHash(dataBuffer);
        return Convert.ToBase64String(hashBytes);
    }
    #endregion

    #region 获取请求的Json参数
    /// <summary>
    /// 获取Post请求的Json参数
    /// </summary>
    /// <returns></returns>
    public static JObject GetJsonParamToJObject()
    {
        var stream = System.Web.HttpContext.Current.Request.InputStream;
        stream.Position = 0;
        using (StreamReader sw = new StreamReader(stream))
        {
            string strJson = sw.ReadToEnd();
            return JObject.Parse(strJson);
        }
    }
    #endregion

    #region 数字校验
    /// <summary>
    /// 检查数据是否为数字
    /// </summary>
    public static bool isNumber(string value)
    {
        return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
    }
    /// <summary>
    /// 检查数据是否为整数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsInt(string value)
    {
        return Regex.IsMatch(value, @"^[+-]?\d*$");
    }
    /// <summary>
    /// 检查数据是否为空
    /// </summary>
    public static bool isNotNull(Object o)
    {
        if (o != null)
        {
            string s = o.ToString();
            if (s != null && !"".Equals(s.Trim()))
            {
                return true;
            }
            else
            {
                return false;

            }
        }
        else
        {
            return false;
        }
    }
    #endregion

    #region 数据转换
    /// <summary>
    /// ValueObject转Hashtable
    /// </summary>
    public static Hashtable voToHashtable(Object ValueObject)
    {
        System.Reflection.PropertyInfo[] ps = ValueObject.GetType().GetProperties();
        Hashtable ht = new Hashtable();
        foreach (System.Reflection.PropertyInfo pi in ps)
        {
            //Name 为属性名称,GetValue 得到属性值(参数this 为对象本身,null)
            string name = pi.Name;
            object value = pi.GetValue(ValueObject, null);
            if (value != null)
            {
                ht.Add(name, Convert.ToString(value));
            }

        }

        return ht;
    }

    /// <summary>
    /// ValueObject转 XML
    /// </summary>
    public static string hashtableToXML(Hashtable ht)
    {
        string xml = "<xml>";

        IDictionaryEnumerator ide = ht.GetEnumerator();
        while (ide.MoveNext())
        {
            xml += "<" + ide.Key.ToString() + "><![CDATA[" + ide.Value.ToString() + "]]></" + ide.Key.ToString() + ">";
        }

        xml += "</xml>";

        return xml;
    }

    /// <summary>
    /// ValueObject转 XML
    /// </summary>
    public static string voToXML(Object ValueObject)
    {
        string xml = "<xml>";
        System.Reflection.PropertyInfo[] ps = ValueObject.GetType().GetProperties();
        Hashtable ht = new Hashtable();
        foreach (System.Reflection.PropertyInfo pi in ps)
        {
            //Name 为属性名称,GetValue 得到属性值(参数this 为对象本身,null)
            string name = pi.Name;
            object value = pi.GetValue(ValueObject, null);
            if (value != null)
            {
                xml += "<" + name + "><![CDATA[" + value + "]]></" + name + ">";
            }

        }
        xml += "</xml>";

        return xml;
    }

    /// <summary>
    /// json转vo
    /// </summary>
    public static Object xmlToVO(XmlDocument xml, string class_name)
    {

        XmlNodeList xml_list = xml.SelectSingleNode("xml").ChildNodes;
        //对应到  VO 值对象
        Type myType = Type.GetType(class_name);// 获得“类”类型
        Object o_Instance = System.Activator.CreateInstance(myType); // 实例化类
        PropertyInfo[] myPropertyInfo1 = myType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < xml_list.Count; i++)
        {


            for (int k = 0; k < myPropertyInfo1.Length; k++)
            {
                PropertyInfo myPropInfo = (PropertyInfo)myPropertyInfo1[k];
                if (xml_list.Item(i).Name.ToLower().Equals(myPropInfo.Name.ToLower()))
                {
                    switch (myPropInfo.PropertyType.ToString())
                    {
                        case "System.Int32":
                            myPropInfo.SetValue(o_Instance, int.Parse(xml_list.Item(i).InnerText), null);
                            break;
                        case "System.Double":
                            myPropInfo.SetValue(o_Instance, double.Parse(xml_list.Item(i).InnerText), null);
                            break;
                        case "System.String":
                            myPropInfo.SetValue(o_Instance, xml_list.Item(i).InnerText, null);
                            break;
                    }

                }

            }
        }
        return o_Instance;

    }

    #endregion

    #region 获取时间
    /// <summary>
    /// 获得时间戳
    /// </summary>
    public static long getLongTime()
    {
        DateTime timeStamp = new DateTime(1970, 1, 1); //得到1970年的时间戳
        long a = (DateTime.UtcNow.Ticks - timeStamp.Ticks) / 10000000; //注意这里有时区
        return a;
    }
    #endregion



    #region 自定义返回状态值
    /// <summary>
    /// 自定义返回状态值（可自行扩展自己需要的返回码）
    /// </summary>
    public enum ApiStatusCode
    {
        /// <summary>
        /// 请求成功
        /// </summary>
        [Description("请求成功")]
        OK = 0,
        /// <summary>
        /// 系统内部错误(代码错误)
        /// </summary>
        [Description("系统内部错误")]
        Error = 400,

        // 授权相关
        /// <summary>
        /// 没有权限
        /// </summary>
        [Description("没有权限")]
        Unauthorized = 1001,
        /// <summary>
        /// 授权用户不存在！
        /// </summary>
        [Description("授权用户不存在！")]
        InvalidClient = 1002,
        /// <summary>
        /// Token过期
        /// </summary>
        [Description("Token过期")]
        InvalidToken = 1003,
        /// <summary>
        /// 签名错误
        /// </summary>
        [Description("签名错误")]
        InvalidSign = 1004,
        /// <summary>
        /// 无效的时间戳
        /// </summary>
        [Description("无效的时间戳")]
        InvalidTimeStamp = 1005,

        /// <summary>
        /// 访问次数超过限制
        /// </summary>
        [Description("访问次数超过限制")]
        LimitRequest = 1006,

        // 业务逻辑错误状态码 3000-3999
        /// <summary>
        /// 数据已存在
        /// </summary>
        [Description("数据已存在")]
        DataExisted = 3001,
        /// <summary>
        /// 数据不存在
        /// </summary>
        [Description("数据不存在")]
        NotFound = 3002,
        /// <summary>
        /// 参数错误(invalid parameter)
        /// </summary>
        [Description("参数错误")]
        InvalidParam = 3003,

        // 系统异常 5000-5999
        /// <summary>
        /// 内部系统异常
        /// </summary>
        [Description("内部系统异常")]
        InternalServerError = 5001

    }
    #endregion

    #region 上传文件到文件服务器

    public static string UploadFileToServices(string path, string folder)
    {
        string imgpath = "";
        try
        {
            string domain = ConfigurationManager.ConnectionStrings["FILESERVERADDRESS"].ConnectionString;
            //上传文件到文件服务器并取得返回地址
            FileInfo file = new FileInfo(path);
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("folder", folder);
            string returnstr = Util.HttpPostFile(domain, file, dic);
            Dictionary<string, object> returnobj = JsonConvert.DeserializeObject<Dictionary<string, object>>(returnstr);
            bool success = Convert.ToBoolean(returnobj["success"]);
            if (success)
            {
                imgpath = returnobj["path"].ToString();
            };
        }
        catch (Exception ex)
        {
            LogHelper.Error(ex.Message, ex);
        }
        return imgpath;

    }
    #endregion

    #region 获取Int基数
    public static int GetBaseData(string key)
    {
        int count = 0;
        //try
        //{
        //    BaseBLL<base_table> baseBLL = new BaseBLL<base_table>();
        //    base_table model = baseBLL.Find(x => x.base_key == key);
        //    if (model != null)
        //    {
        //        count = Convert.ToInt32(model.base_value);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    LogHelper.Error(ex.Message, ex);
        //}
        return count;
    }

    #endregion

    #region 小程序浏览记录行为值
    /// <summary>
    /// 小程序浏览记录行为值
    /// </summary>
    public enum BrowserBehaviorCode
    {

        [Description("用户直接分享小程序给其他用户")]
        SHARE = 1,
        [Description("直接链接跳转到小程序")]
        LINK = 2,
        [Description("编辑小程序码进入小程序")]
        ADVISER_APPLET = 3,
        [Description("电视小程序码进入小程序")]
        TV_APPLET = 4,
        [Description("海报小程序码进入小程序")]
        POSTER_APPLET = 5,
        [Description("用户阅读，浏览")]
        READ = 6,
        [Description("通过其他小程序进入小程序")]
        APPLET_APPLET = 7,
        [Description("新增用户")]
        ADD_USER = 8
    }
    #endregion

    #region 获取音频时长
    /// <summary>
    /// 获得音频时长
    /// </summary>
    public static string GetMediaDuration(string sFile)
    {
        // ell32 END
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


    public static int GetDuration(string url)
    {
        try
        {
            string domain = ConfigurationManager.ConnectionStrings["FILESERVERADDRESS"].ConnectionString;
            string duration = "";

            if (!string.IsNullOrWhiteSpace(url))
            {
                duration = Util.MethodGET(domain + "/index.aspx?method=GetSourceInfo&filePath=" + url + "&fileType=sound", "UTF-8");
                JObject json = (JObject)JObject.Parse(duration);
                if ((Boolean)json["success"])
                {
                    duration = ((JObject)json["data"])["duration"].ToString();
                    return Convert.ToInt32(TimeSpan.Parse(duration).TotalSeconds);
                }
            }
            return 0;
        }
        catch
        {
            return 0;
        }
    }
    #endregion

    #region 读取txt
    /// <summary>
    /// 读取txt
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ReadTxt(string path)
    {
        StringBuilder line = new StringBuilder();
        //FileStream file = new FileStream(path, FileMode.Open);
        //var coding = GetEncoding(file, Encoding.Default);
        //file.Close();
        //file.Dispose();
        using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
        {
            string text;
            while (isNotNull(text = sr.ReadLine()))
            {
                string _text = "";
                //移除txt/lrc的时间戳
                int start = text.Trim().IndexOf('[');
                int end = text.Trim().IndexOf(']');
                if (start > 0)
                {
                    _text += text.Substring(0, start);
                }
                if (end < text.Trim().Length - 1)
                {
                    _text += text.Substring(end + 1, text.Trim().Length - end - 1);
                }
                if (isNotNull(_text))
                {
                    line.Append(_text + "|");
                }
            }
        }
        return line.ToString();
    }

    #endregion

    #region
    /// <summary>
    /// 取得一个文本文件流的编码方式。
    /// </summary>
    /// <param name="stream">文本文件流。</param>
    /// <param name="defaultEncoding">默认编码方式。当该方法无法从文件的头部取得有效的前导符时，将返回该编码方式。</param>
    /// <returns></returns>
    public static Encoding GetEncoding(Stream stream, Encoding defaultEncoding)
    {
        Encoding targetEncoding = defaultEncoding;
        if (stream != null && stream.Length >= 2)
        {
            //保存文件流的前4个字节
            byte byte1 = 0;
            byte byte2 = 0;
            byte byte3 = 0;
            byte byte4 = 0;
            //保存当前Seek位置
            long origPos = stream.Seek(0, SeekOrigin.Begin);
            stream.Seek(0, SeekOrigin.Begin);
            int nByte = stream.ReadByte();
            byte1 = Convert.ToByte(nByte);
            byte2 = Convert.ToByte(stream.ReadByte());
            if (stream.Length >= 3)
            {
                byte3 = Convert.ToByte(stream.ReadByte());
            }
            if (stream.Length >= 4)
            {
                byte4 = Convert.ToByte(stream.ReadByte());
            }
            //根据文件流的前4个字节判断Encoding
            //Unicode {0xFF, 0xFE};
            //BE-Unicode {0xFE, 0xFF};
            //UTF8 = {0xEF, 0xBB, 0xBF};
            if (byte1 == 0xFE && byte2 == 0xFF)//UnicodeBe
            {
                targetEncoding = Encoding.BigEndianUnicode;
            }
            if (byte1 == 0xFF && byte2 == 0xFE && byte3 != 0xFF)//Unicode
            {
                targetEncoding = Encoding.Unicode;
            }
            if (byte1 == 0xEF && byte2 == 0xBB && byte3 == 0xBF)//UTF8
            {
                targetEncoding = Encoding.UTF8;
            }
            //恢复Seek位置　　　 
            stream.Seek(origPos, SeekOrigin.Begin);
        }
        return targetEncoding;
    }
    #endregion
}


/// <summary>
/// 参数验证
/// </summary>
public class Parameter
{
    /// <summary>
    /// 待验证的值
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 错误提示消息
    /// </summary>
    public string Msg { get; set; }

    /// <summary>
    /// 正则表达式 （可空）
    /// </summary>
    public string Regex { get; set; }

    /// <summary>
    /// 是否对 Value 值进行验证 默认为 true
    /// <para>当IsCheck达成某个条件时，才对Value进行验证</para>
    /// <para>例如：IsCheck = string.IsNullOrEmpty(Temp) 即Temp不为空时，才对Value进行验证</para>
    /// </summary>
    public bool IsCheck { get; set; } = true;
}
