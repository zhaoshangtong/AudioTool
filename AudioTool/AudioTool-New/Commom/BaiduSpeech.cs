using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AudioToolNew.Common
{
    public class BaiduSpeech
    {
        private static  string para_API_id= "10329737";
        private static  string Api_Key = "tCnh7RyZuzvN3sVcec0LbOnO";
        private static  string Secret_Key = "HCzIHZsBM0ly7DiAu19RLB4TnF4aaAf3";

        private static bool retry = false;
        /// <summary>
        /// 音频文件转语音
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string BaiduTranslateToText(string filepath,string language,string para_HZ)
        {
            string returnStr = "";
            if (string.IsNullOrEmpty(filepath))
            {
                returnStr = "文件不存在";
            }
            else
            {
                string fileType = Path.GetExtension(filepath).Substring(1);
                #region RestApi SDK
                //SpeechDemo speech = new Baidu.SpeechDemo();
                //JObject result = speech.AsrData(filepath, fileType, 8000);
                //if (result["err_no"].ToString() == "0")
                //{
                //    returnStr = "{\"success\":true,\"data\":\"" + result["result"][0].ToString() + "\"}";
                //}
                #endregion
                #region Rest API
                string token = GetAccessToken();
                retry = true;
                string result = getStrText(para_API_id, token, language, filepath, fileType, para_HZ);
                returnStr =  result ;
                #endregion
            }
            return returnStr;
        }
        /// <summary>
        ///  wav的字节数组转语音
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <param name="type">文件类型（只支持pcm,wav）</param>
        /// <param name="para_Hz"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string BaiduTranslateToText(byte[] buffer, string type,string para_Hz,  string language= "zh")
        {
            string token = GetAccessToken();
            return  GetBaiduSpeechText(para_API_id, token, language, type, para_Hz, buffer);
        }

        #region

        private static string GetAccessToken()
        {
            string url = string.Format("https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials&client_id={0}&client_secret={1}", Api_Key, Secret_Key);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    string content = reader.ReadToEnd();
                    JObject json = JObject.Parse(content);
                    return json["access_token"].ToString();
                }
            }
        }
        #endregion
        private static string getStrText(string para_API_id, string para_API_access_token, string para_API_language, string para_API_record, string para_format, string para_Hz)
        {
            //方法参数说明:
            //para_API_id: API_id(你的ID)
            //para_API_access_token(getStrAccess(...)方法得到的access_token口令)
            //para_API_language(你要识别的语言, zh, en, ct)
            //para_API_record(语音文件的路径)
            //para_format(语音文件的格式)
            //para_Hz(语音文件的采样率 16000或者8000)

            //该方法返回值:
            //该方法执行正确返回值是语音翻译的文本,错误是错误号,可以去看百度语音文档,查看对应错误

            FileInfo fi = new FileInfo(para_API_record);
            FileStream fs = new FileStream(para_API_record, FileMode.Open);
            byte[] voice = new byte[fs.Length];
            fs.Read(voice, 0, voice.Length);
            fs.Close();
            return  GetBaiduSpeechText(para_API_id, para_API_access_token, para_API_language, para_format, para_Hz, voice);

        }

        private static string GetBaiduSpeechText(string para_API_id, string para_API_access_token, string para_API_language, string para_format, string para_Hz,  byte[] voice)
        {
            string strJSON = "";
            string strText = null;
            string error = null;
            string getTextUrl = "http://vop.baidu.com/server_api?lan=" + para_API_language + "&cuid=" + para_API_id + "&token=" + para_API_access_token;
            HttpWebRequest getTextRequst = WebRequest.Create(getTextUrl) as HttpWebRequest;
            getTextRequst.ContentType = "audio /" + para_format + ";rate=" + para_Hz;
            getTextRequst.ContentLength = voice.Length;
            getTextRequst.Method = "post";
            getTextRequst.Accept = "*/*";
            getTextRequst.KeepAlive = false;
            //getTextRequst.Timeout = 30000;//30秒连接不成功就中断 
            using (Stream writeStream = getTextRequst.GetRequestStream())
            {
                writeStream.Write(voice, 0, voice.Length);
            }

            HttpWebResponse getTextResponse = getTextRequst.GetResponse() as HttpWebResponse;
            using (StreamReader reader = new StreamReader(getTextResponse.GetResponseStream(), Encoding.UTF8))
            {
                strJSON = reader.ReadToEnd();
            }
            JObject jsons = JObject.Parse(strJSON);//解析JSON
            if (jsons["err_msg"].Value<string>() == "success.")
            {
                strText = jsons["result"][0].ToString();
                return strText;
            }
            else
            {
                //重试
                if (retry)
                {
                    retry = false;
                    //如果是中文
                    if (para_API_language == "zh")
                    {
                        para_API_language = "en";
                    }
                    else
                    {
                        para_API_language = "zh";
                    }
                    return DoReTry(para_API_id, para_API_access_token, para_API_language, para_format, para_Hz, voice);
                }
                error = jsons["err_no"].Value<string>() + jsons["err_msg"].Value<string>();
                return "3301-百度语音转文字出错";
            }
        }


        //如果中英文都有的情况，特别是听力题目，那么先用英文如果失败，再用中文
        private static string DoReTry(string para_API_id, string para_API_access_token, string para_API_language, string para_format, string para_Hz, byte[] voice)
        {
            return GetBaiduSpeechText(para_API_id, para_API_access_token, para_API_language, para_format, para_Hz, voice);
        }
    }
}
