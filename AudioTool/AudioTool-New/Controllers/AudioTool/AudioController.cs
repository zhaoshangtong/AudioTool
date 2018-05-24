using AudioToolNew.Commom;
using AudioToolNew.Common;
using AudioToolNew.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace AudioToolNew.Controllers.AudioToolNew
{
    public class AudioController : ApiController
    {
        #region 音频处理
        /// <summary>
        /// 音频处理
        /// </summary>
        /// <param name="sound_path"></param>
        /// <param name="word_path"></param>
        /// <param name="language"></param>
        /// <param name="splitTime"></param>
        /// <returns></returns>
        private ApiResult SolutionAudioFile(string sound_path, string word_path, string language, double splitTime = 1.5)
        {
            ApiResult apiResult = new Models.ApiResult();
            try
            {
                Musicline2 music = new Musicline2();
                Task task_max = Task.Factory.StartNew(() => {
                    music.GetTimeSpan(sound_path, word_path, language, splitTime);
                });
                
                Task.WaitAny(task_max);
                while (!music.isFinish)
                {
                    Thread.Sleep(10);
                }
                apiResult.data = new { music.results,music.originalText};
                apiResult.success = true;
                apiResult.message = "转换成功";
            }
            catch (Exception ex)
            {
                apiResult.success = false;
                apiResult.message = ex.Message;
            }
            return apiResult;
        }

        /// <summary>
        /// 保存到应用服务器（后期需要截取文件）
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<string> SaveFile(Stream stream, string path)
        {
            try
            {
                //重新存储到一个新的文件目录
                if (!System.IO.Directory.Exists(Path.GetDirectoryName(path)))
                {
                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
                //无流数据
                if (stream == null || stream.Length <= 0) return "";
                using (FileStream file = new FileStream(path, FileMode.OpenOrCreate))
                {
                    byte[] bArr = new byte[1024];
                    int size = await stream.ReadAsync(bArr, 0, (int)bArr.Length);
                    while (size > 0)
                    {
                        await file.WriteAsync(bArr, 0, size);
                        size = await stream.ReadAsync(bArr, 0, (int)bArr.Length);
                    }
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
            
            //保存到文件服务器
            //UploadFile.PostFile(path);
            return path;
        }

        private string GetFileNameRandom()
        {
            return DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString()
                + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> AudioToolNew()
        {
            ApiResult apiResult = new ApiResult();
            try
            {
                string folder = System.AppDomain.CurrentDomain.BaseDirectory + "/NewSoundFiles/";

                //获取formdata数据
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }
                var multipartMemoryStreamProvider = await Request.Content.ReadAsMultipartAsync();
                Dictionary<string, object> dic = new Dictionary<string, object>();
                Stream fileStream = null;
                foreach (var content in multipartMemoryStreamProvider.Contents)
                {
                    if (!string.IsNullOrEmpty(content.Headers.ContentDisposition.FileName))
                    {
                        //文本文件前端药命名为wordFile,音频文件要命名为mp3File
                        if (content.Headers.ContentDisposition.Name.Contains("wordFile"))
                        {
                            fileStream = await content.ReadAsStreamAsync();
                            string fileName = content.Headers.ContentDisposition.FileName.Replace("\"", "");
                            string word_filePath = folder + Path.GetFileNameWithoutExtension(fileName) + "/" + content.Headers.ContentDisposition.FileName.Replace("\"", "");
                            word_filePath = await SaveFile(fileStream, word_filePath);
                            dic.Add("word_path", word_filePath);
                            fileStream.Close();
                            fileStream.Dispose();

                        }
                        else if (content.Headers.ContentDisposition.Name.Contains("mp3File"))
                        {
                            fileStream = await content.ReadAsStreamAsync();
                            string fileName = content.Headers.ContentDisposition.FileName.Replace("\"", "");
                            string mp3_filePath = folder + Path.GetFileNameWithoutExtension(fileName) + "/" + Path.GetFileNameWithoutExtension(content.Headers.ContentDisposition.FileName.Replace("\"", "")) + GetFileNameRandom() + Path.GetExtension(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                            mp3_filePath = await SaveFile(fileStream, mp3_filePath);
                            dic.Add("sound_path", mp3_filePath);
                            fileStream.Close();
                            fileStream.Dispose();

                        }
                        else
                        {
                            apiResult.success = false;
                            apiResult.message = "上传文件错误";
                        }
                    }
                    else
                    {
                        string val = await content.ReadAsStringAsync();
                        dic.Add(content.Headers.ContentDisposition.Name.Replace("\"", ""), val);
                    }
                }
                string language = !dic.Keys.Contains("language") ? "zh" : dic["language"].ToString();
                string splitTimeString = !dic.ContainsKey("splitTime") ? "" : dic["splitTime"].ToString();
                double splitTime = Util.isNotNull(splitTimeString) ? double.Parse(splitTimeString) : 1.5;
                string sound_path = !dic.ContainsKey("sound_path") ? "" : dic["sound_path"].ToString();
                string word_path = !dic.ContainsKey("word_path") ? "" : dic["word_path"].ToString();
                if (Util.isNotNull(sound_path) && Util.isNotNull(word_path))
                {
                    apiResult = SolutionAudioFile(sound_path, word_path, language, splitTime);
                }
                else
                {
                    return new Models.ApiResult()
                    {
                        success = false,
                        message = "音频文件或原文文件不能为空"
                    };
                }
                
            }
            catch (Exception ex)
            {
                apiResult.success = false;
                apiResult.message = ex.Message;
            }

            return apiResult;
        }
        #endregion
    }
}
