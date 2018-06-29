using AudioToolNew.Commom;
using AudioToolNew.Common;
using AudioToolNew.Models;
using Rays.Utility.BadiAI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AudioToolNew.Controllers.AudioToolNew
{
    /// <summary>
    /// 音频处理
    /// </summary>
    public class AudioController : ApiController
    {
        #region 音频处理
        #region 音频处理方法
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
                apiResult.data = new { music.results, music.originalText };
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

        public async Task<ApiResult> Test(string file_path)
        {
            string txt = Util.ReadTxt(file_path);
            string[] originalList = txt.Replace(".", ".|").Replace("。", "。|").Replace("?", "?|").Replace("？", "？|").Replace("！", "！|").Replace("!", "!|").Replace("……", "……|").Split('|').Where(o => o != " " && o != "").ToArray();
            return new Models.ApiResult() { success = true, message = txt };
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
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
            return path;
        }

        private string GetFileNameRandom()
        {
            return DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString()
                + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
        }
        #endregion
        /// <summary>
        /// 上传文件，处理音频
        /// 参数：mp3File音频文件，wordFile原文文件，language（zh/en），splitTime间隔时长
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> AudioToolNew()
        {
            ApiResult apiResult = new ApiResult();
            try
            {
                string folder = System.AppDomain.CurrentDomain.BaseDirectory + "NewSoundFiles/";
                //获取formdata数据
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }
                var multipartMemoryStreamProvider = await Request.Content.ReadAsMultipartAsync();
                Dictionary<string, object> dic = new Dictionary<string, object>();
                Stream fileStream_audio = null;
                Stream fileStream_word = null;
                foreach (var content in multipartMemoryStreamProvider.Contents)
                {
                    if (!string.IsNullOrEmpty(content.Headers.ContentDisposition.FileName))
                    {
                        //文本文件前端药命名为wordFile,音频文件要命名为mp3File
                        if (content.Headers.ContentDisposition.Name.Contains("wordFile"))
                        {
                            fileStream_word = await content.ReadAsStreamAsync();
                            LogHelper.Info("wordFile流长度：" + fileStream_word.Length);
                            //string fileName = content.Headers.ContentDisposition.FileName.Replace("\"", "");
                            string fileName = "word_" + sys.getRandomStr() + Path.GetExtension(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                            string word_filePath = folder + sys.getRandomStr() + "_word" + "/" + fileName;
                            word_filePath = await SaveFile(fileStream_word, word_filePath.Replace("/", "\\"));
                            dic.Add("word_path", word_filePath);
                            fileStream_word.Close();
                            fileStream_word.Dispose();

                        }
                        else if (content.Headers.ContentDisposition.Name.Contains("mp3File"))
                        {
                            fileStream_audio = await content.ReadAsStreamAsync();
                            string fileName = "audio_" + sys.getRandomStr() + Path.GetExtension(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                            string mp3_filePath = folder + sys.getRandomStr() + "_audio" + "/" + fileName;
                            mp3_filePath = await SaveFile(fileStream_audio, mp3_filePath);
                            dic.Add("sound_path", mp3_filePath.Replace("/", "\\"));
                            fileStream_audio.Close();
                            fileStream_audio.Dispose();

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
                LogHelper.Info("sound_path:" + sound_path);
                LogHelper.Info("word_path:" + word_path);
                if (Util.isNotNull(sound_path) && Util.isNotNull(word_path))
                {
                    if (Path.GetExtension(word_path).Contains("doc") || Path.GetExtension(word_path).Contains("docx") || Path.GetExtension(word_path).Contains("txt") || Path.GetExtension(word_path).Contains("lrc"))
                    {
                        apiResult = SolutionAudioFile(sound_path, word_path, language, splitTime);
                    }
                    else
                    {
                        return new Models.ApiResult()
                        {
                            success = false,
                            message = "暂时不支持其他格式的原文文件，请上传word/txt/lrc文档,编码格式utf8"
                        };
                    }

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

        #region 音频截取
        /// <summary>
        /// 音频截取
        /// timespans:单位s，以逗号拼接。file：音频文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResult CutAudio()
        {
            ApiResult apiResult = new Models.ApiResult();
            List<string> output_list = new List<string>();
            string input = string.Empty;
            try
            {
                Dictionary<string, string> cut_files = new Dictionary<string, string>();
                HttpFileCollection fileList = HttpContext.Current.Request.Files;
                if (fileList.Count < 1)
                {
                    return new ApiResult() { success = false, message = "文件为空" };
                }
                HttpPostedFile file = fileList[0];
                string folder = System.AppDomain.CurrentDomain.BaseDirectory + "NewSoundFiles/";
                if (!System.IO.Directory.Exists(Path.GetDirectoryName(folder)))
                {
                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(folder));
                }
                //先保存文件
                input = folder + "cutAudio_" + sys.getRandomStr() + Path.GetExtension(file.FileName);
                input = input.Replace("/", "\\");
                file.SaveAs(input);
                string timespans = HttpContext.Current.Request.Form["timespans"];
                string[] times = timespans.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                string start = "";
                string end = "";
                List<string> cut_times = new List<string>();
                foreach (string time in times)
                {
                    if (Util.isNotNull(time))
                    {
                        cut_times.Add(time);
                    }
                }
                //没有结束时间，所以需要加上
                string _all_audio_time = FfmpegHelper.getMediaDuration(input);
                string[] all_audio_times = _all_audio_time.Split(':');
                string audio_times = (int.Parse(all_audio_times[0]) * 3600 + int.Parse(all_audio_times[1]) * 60 + double.Parse(all_audio_times[2])).ToString();
                cut_times.Add(audio_times);
                //确保没有空数据
                if (cut_times.Count > 1)
                {
                    for (int i = 1; i < cut_times.Count; i++)
                    {
                        try
                        {
                            start = cut_times[i - 1];
                            TimeSpan _start = TimeSpan.FromSeconds(double.Parse(start));
                            end = cut_times[i];
                            TimeSpan _end = TimeSpan.FromSeconds(double.Parse(end) - double.Parse(start));
                            string output = folder + i + "_cutAudio_" + sys.getRandomStr() + Path.GetExtension(file.FileName);
                            output = output.Replace("/", "\\");
                            output_list.Add(output);
                            bool is_success = FfmpegHelper.CutAudioFile(input, output, _start, _end);
                            //上传到服务器
                            if (File.Exists(output))
                            {
                                string server_path = UploadFile.PostFile(output);
                                cut_files.Add(start, server_path);
                            }
                        }
                        catch (Exception ex)
                        {
                            return new ApiResult()
                            {
                                success = false,
                                message = "timespans参数不对"
                            };
                        }

                    }
                    apiResult.success = true;
                    apiResult.message = "截取成功";
                    apiResult.data = cut_files;
                }
                else
                {
                    return new ApiResult()
                    {
                        success = false,
                        message = "timespans参数不对"
                    };
                }

            }
            catch (Exception ex)
            {
                return new ApiResult()
                {
                    success = false,
                    message = ex.Message
                };
            }
            finally
            {
                if (Util.isNotNull(input))
                {
                    if (File.Exists(input))
                    {
                        File.Delete(input);
                    }
                }
                foreach (string file in output_list)
                {
                    if (Util.isNotNull(file))
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                        }
                    }
                }
            }
            return apiResult;
        }

        #endregion

        #region 评分
        /// <summary>
        /// 音频评分。（音频时长不要超过60s）
        /// 参数：file:音频文件，enText:正确文本，language（zh/en）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResult speechEvaluation()
        {
            ApiResult apiResult = new ApiResult();
            string speechFilePath = string.Empty;
            string silkFile = string.Empty;
            string otherFile = string.Empty;
            try
            {
                string fileName = sys.getRandomStr();//用作最终生成的文件名
                List<string> speechResult = null;
                //接受小程序提交过来的录音文件
                System.Web.HttpFileCollection fileList = HttpContext.Current.Request.Files;
                string enText = HttpContext.Current.Request.Form["enText"];
                string lan = HttpContext.Current.Request.Form["language"];
                enText = enText.Replace("\n", "").Replace("\r", "");
                if (fileList.Count < 1)
                {
                    return new ApiResult() { success = false, message = "语音文件为空" };
                }
                System.Web.HttpPostedFile file = fileList[0];
                //判断文件是silk格式还是webm格式，不能通过文件后缀判断
                StreamReader reader = new StreamReader(file.InputStream);
                string fileString = reader.ReadToEnd();
                BaiduAI ai = new BaiduAI();
                if (fileString.IndexOf("SILK_V3") > -1)
                {
                    //silk格式文件                
                    silkFile = CommonServices.createFileFullPath("silk");
                    file.SaveAs(silkFile);
                    speechFilePath = CmdServices.silk2wav(silkFile);
                    string fileType = CommonServices.getFileType(speechFilePath);
                    speechResult = ai.AsrData(speechFilePath, fileType, 16000, lan);
                }
                else if (fileString.IndexOf("webm") > -1)
                {
                    #region
                    //webm格式文件
                    //微信小程序webm格式录音文件，data:audio/webm;base64,GkXfo59ChoEBQv...
                    //1.去掉data:audio/webm;base64,
                    //2.对GkXfo59ChoEBQv...字符串解码为webm文件
                    //3.webm文件转wav文件  
                    #endregion
                    fileString = fileString.Substring(fileString.IndexOf(',') + 1);
                    byte[] bytes = Convert.FromBase64String(fileString);
                    string webmFile = CommonServices.createFileFullPath("webm");
                    using (FileStream fs = new FileStream(webmFile, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Flush();
                    }
                    speechFilePath = CmdServices.webm2wav(webmFile);
                    string fileType = CommonServices.getFileType(speechFilePath);
                    speechResult = ai.AsrData(speechFilePath, fileType, 16000, lan);
                }
                else//mp3或者wav，如果不是16000HZ，16bit，单声道，都转格式
                {
                    string fileType = CommonServices.getFileType(file.FileName);
                    otherFile = CommonServices.createFileFullPath(fileType);
                    file.SaveAs(otherFile);
                    //转换成wav\
                    LogHelper.Info("MP3转wav");
                    speechFilePath = CmdServices.mp32wav(otherFile);
                    LogHelper.Info("百度转语音开始");
                    speechResult = ai.AsrData(speechFilePath, CommonServices.getFileType(speechFilePath), 16000, lan);
                }

                string duration = FileServices.getMediaDuration(speechFilePath);
                int seconds = SppechEvaluation.timeToSecond(duration);
                //评分
                int totalSocre = 0;
                int accuracySocre = 0;
                int fluencySocre = 0;
                int integritySocre = 0;
                int tempScore = 0;
                string sentence = "";
                LogHelper.Info("百度转语音完成，准备计算分数。seconds：" + seconds + ";count:" + speechResult.Count);
                if (speechResult == null)
                {
                    apiResult.data = new { totalSocre, accuracySocre, fluencySocre, integritySocre, speechFilePath = UploadFile.PostFile(speechFilePath) };
                }
                else
                {
                    //开始计算得分
                    for (int i = 0; i < speechResult.Count; i++)
                    {
                        if (lan == "zh")
                        {
                            char[] words = SppechEvaluation.getSentenceWordsZh(speechResult[i]);
                            tempScore = SppechEvaluation.calaAccuracySocre(enText, words);
                            if (tempScore > accuracySocre)
                            {
                                accuracySocre = tempScore;
                            }
                            tempScore = SppechEvaluation.calaFluencySocreZh(enText, seconds);
                            if (tempScore > fluencySocre)
                            {
                                fluencySocre = tempScore;
                            }
                            tempScore = SppechEvaluation.calaIntegritySocre(enText, words, accuracySocre);
                            if (tempScore > integritySocre)
                            {
                                integritySocre = tempScore;
                            }
                            sentence = SppechEvaluation.getSentenceAccuracyZh(enText, speechResult);
                        }
                        else
                        {
                            string[] words = SppechEvaluation.getSentenceWords(speechResult[i]);
                            //accuracySocre、fluencySocre、integritySocre取平均分，totalSocre取accuracySocre最高分
                            tempScore = SppechEvaluation.calaAccuracySocre(enText, words);
                            if (tempScore > accuracySocre)
                            {
                                accuracySocre = tempScore;
                            }

                            tempScore = SppechEvaluation.calaFluencySocre(enText, seconds);
                            if (tempScore > fluencySocre)
                            {
                                fluencySocre = tempScore;
                            }
                            tempScore = SppechEvaluation.calaIntegritySocre(enText, words, accuracySocre);
                            if (tempScore > integritySocre)
                            {
                                integritySocre = tempScore;
                            }

                            sentence = SppechEvaluation.getSentenceAccuracy2(enText, speechResult);
                        }
                    }
                    totalSocre = (int)((float)(accuracySocre * 60 + fluencySocre * 20 + integritySocre * 20) / 100);
                    //return "{\"success\":true,\"message\":\"\",\"data\":{\"totalSocre\":\"" + totalSocre + "\",\"accuracySocre\":\"" + accuracySocre + "\",\"fluencySocre\":\"" + fluencySocre + "\",\"integritySocre\":\"" + integritySocre + "\",\"speechFilePath\":\"" + FileServices.urlConvertor(speechFilePath) + "\",\"speechSeconds\":\"" + seconds.ToString() + "\",\"sentence\":" + sentence + "}}";
                    apiResult.data = new { totalSocre, accuracySocre, fluencySocre, integritySocre, speechFilePath = UploadFile.PostFile(speechFilePath), speechSeconds = seconds.ToString(), sentence };
                }
                apiResult.success = true;
                apiResult.message = "获取成功";
            }
            catch (Exception ex)
            {
                return new Models.ApiResult()
                {
                    success = false,
                    message = ex.Message
                };
            }
            finally
            {
                //删掉服务器上的文件
                if (Util.isNotNull(speechFilePath))
                {
                    if (File.Exists(speechFilePath))
                    {
                        File.Delete(speechFilePath);
                    }
                }
                if (Util.isNotNull(silkFile))
                {
                    if (File.Exists(silkFile))
                    {
                        File.Delete(silkFile);
                    }
                }
                if (Util.isNotNull(otherFile))
                {
                    if (File.Exists(otherFile))
                    {
                        File.Delete(otherFile);
                    }
                }
            }
            return apiResult;
        }
        #endregion

    }
}
