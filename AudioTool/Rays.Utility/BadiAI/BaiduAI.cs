using Baidu.Aip.Speech;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rays.Utility.BadiAI
{
    public class BaiduAI
    {
        private static string para_API_id = "10329737";
        private static string Api_Key = "tCnh7RyZuzvN3sVcec0LbOnO";
        private static string Secret_Key = "HCzIHZsBM0ly7DiAu19RLB4TnF4aaAf3";

        /// <summary>
        /// 音频文件转语音
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string BaiduTranslateToText(string filepath, string language, string para_HZ)
        {
            string returnStr = "";
            if (string.IsNullOrEmpty(filepath))
            {
                returnStr = "文件不存在";
            }
            else
            {
                string format = Path.GetExtension(filepath).Substring(1);
                int rate = int.Parse(para_HZ);
                string cuid = para_API_id;
                FileStream fs = new FileStream(filepath, FileMode.Open);
                byte[] voice = new byte[fs.Length];
                fs.Read(voice, 0, voice.Length);
                fs.Close();
                Dictionary<string, object> options = new Dictionary<string, object>();
                if (language == "zh")
                {
                    options.Add("dev_pid", 1536);
                }
                else
                {
                    options.Add("dev_pid", 1737);
                }
                Asr asr = new Asr(Api_Key, Secret_Key);
                var result=asr.Recognize(voice, format, rate, options);
                JObject obj = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(result));
                if (obj["err_msg"].Value<string>() == "success.")
                {
                    returnStr = obj["result"][0].ToString();
                }
                else
                {
                    returnStr = "3301-百度语音转文字出错";
                }
            }
            return returnStr;
        }
    }
}
