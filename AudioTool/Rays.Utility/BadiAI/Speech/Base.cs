/*
 * Copyright 2017 Baidu, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 */

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace Baidu.Aip.Speech
{
    public class Base : AipServiceBase
    {
        public Base(string apiKey, string secretKey) : base(apiKey, secretKey)
        {
            IsDev = true;
        }

        protected string Cuid
        {
            get { return Utils.Md5(Token); }
        }


        protected override void DoAuthorization()
        {
            lock (AuthLock)
            {
                if (!NeetAuth())
                    return;
                //看access_token是否过期
                IsExistAccess_Token();

                HasDoneAuthoried = true;
            }
        }

        private Access_Token GetAccess_token()
        {
            Access_Token token = new Access_Token();
            //请求获取 access_token。access_token有效期为一个月，所以可以缓存
            //2018-05-22 读取缓存的access_token
            var resp = Auth.OpenApiFetchToken(ApiKey, SecretKey, true);
            ExpireAt = DateTime.Now.AddSeconds((int)resp["expires_in"] - 60*60);
            IsDev = true;
            Token = (string)resp["access_token"];
            token.access_token = Token;
            token.expires_in = ExpireAt.ToString();
            return token;
        }

        /// <summary>
        /// 根据当前日期 判断Access_Token 是否超期  如果超期返回新的Access_Token   否则返回之前的Access_Token  
        /// </summary>
        /// <returns></returns>
        private void IsExistAccess_Token()
        {
            try
            {
                DateTime YouXRQ;
                // 读取XML文件中的数据，并显示出来 ，注意文件路径 
                string folder = System.AppDomain.CurrentDomain.BaseDirectory + "Baidu/";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                string filepath = folder + "access_token.xml";
                XmlDocument xml = new XmlDocument();
                if (!File.Exists(filepath))
                {
                    var file=File.Create(filepath);
                    file.Close();
                    file.Dispose();
                    //向xml中写入节点
                    XmlElement root = xml.CreateElement("xml");
                    xml.AppendChild(root);
                    xml.Save(filepath);
                    XmlElement child1 = xml.CreateElement("Access_Token");
                    XmlElement child2 = xml.CreateElement("Access_YouXRQ");
                    xml.SelectSingleNode("xml").AppendChild(child1);
                    xml.SelectSingleNode("xml").AppendChild(child2);
                    xml.Save(filepath);

                }
                StreamReader str = new StreamReader(filepath, System.Text.Encoding.UTF8);
                if (str.BaseStream.Length == 0)
                {
                    YouXRQ = default(DateTime);
                    
                }
                else
                {
                    xml.Load(str);
                    str.Close();
                    str.Dispose();
                    Token = xml.SelectSingleNode("xml").SelectSingleNode("Access_Token").InnerText;
                    string time = xml.SelectSingleNode("xml").SelectSingleNode("Access_YouXRQ").InnerText;
                    if (string.IsNullOrEmpty(time))
                    {
                        YouXRQ = default(DateTime);
                    }
                    else
                    {
                        YouXRQ = Convert.ToDateTime(time);
                    }
                }
                

                if (DateTime.Now > YouXRQ || YouXRQ == default(DateTime))
                {
                    Access_Token mode = GetAccess_token();
                    xml.SelectSingleNode("xml").SelectSingleNode("Access_Token").InnerText = mode.access_token;
                    xml.SelectSingleNode("xml").SelectSingleNode("Access_YouXRQ").InnerText = mode.expires_in;
                    xml.Save(filepath);
                    ExpireAt = DateTime.Parse(mode.expires_in);
                    Token = mode.access_token;
                    IsDev = true;
                }
                ExpireAt = YouXRQ;
                IsDev = true;
            }
            catch (Exception ex)
            {
            }
            finally
            {

            }
        }
        protected override HttpWebRequest GenerateWebRequest(AipHttpRequest aipRequest)
        {
            return aipRequest.GenerateSpeechRequest(this.Timeout);
        }
    }
    class Access_Token
    {
        public string access_token { get; set; }
        public string expires_in { get; set; }
    }
}