using AudioToolNew.Common;
using AudioToolNew.Models.Musicline;
using NAudio.Wave;
using Rays.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AudioToolNew.Commom
{
    /// <summary>
    /// 2018-05-17：截取音频，上传，百度语音转文字，不再匹配
    /// </summary>
    public class Musicline2
    {
        private List<TimeSpan> voicePoint = new List<TimeSpan>();//存储开始和结束的时间戳，用来语音转文字
        private List<double> voiceMilliSecond = new List<double>();//存放每次语音开始时的时间戳
        public List<MusicResult> return_list { get; set; }
        public HttpContext context { get; set; }
        //结束标识
        public bool isFinish = false;
        //存放截取的小文件路径
        private List<string> voiceFiles = new List<string>();
        //存放百度语音转文字
        private List<string> baidu_text = new List<string>();
        //word原文
        public string originalText { get; set; }
        //对比相似度
        public List<TextContrastResult> results { get; set; }

        //因为英文的特殊情况，所以将index作为全局变量，只有当每次匹配成功后才会变
        public int en_index { get; set; }
        public Musicline2()
        {
            return_list = new List<MusicResult>();
            context = HttpContext.Current;
        }
        /// <summary>
        /// 解析音频文件，获取时间戳，截取音频，语音转文字
        /// </summary>
        /// <param name="sound_path"></param>
        /// <param name="word_path"></param>
        /// <param name="language"></param>
        /// <param name="splitTime"></param>
        public void GetTimeSpan(string sound_path, string word_path, string language, double splitTime = 1.5)
        {
            //通过NAudio读取音频文件流
            AudioFileReader inputStream = null;
            string sound_path_mp3 = "";
            try
            {

                if (sound_path.ToLower().EndsWith(".mp3"))
                {
                    sound_path_mp3 = sound_path;
                    //通过NAudio将文件转换为WAV格式，返回新的文件路径
                    sound_path = NAudioHelper.GetWavPath(sound_path);
                }
                inputStream = new AudioFileReader(sound_path);

                //WAV文件读取
                WAVReader reader = new WAVReader();
                reader.GetTimeSpan(sound_path, out voicePoint, out voiceMilliSecond, splitTime);
                //voicePoint是为了截取小音频
                //voicePoint是双数的原因是有开始时间就有一个结束时间
                //最后一次的时间要加上（结束时，没有停顿时间）
                if (voicePoint.Count % 2 != 0)
                {
                    voicePoint.Add(inputStream.TotalTime);
                }
                LogHelper.Info("时间戳获取：成功");
                //string totalTime=FfmpegHelper.getMediaDuration(sound_path);
                int _name = 1;//命名
                //一个时间点对应一段音频对应一段文字
                LogHelper.Info("语音转文字：开始");
                for (int i = 0; i < voicePoint.Count; i += 2)
                {
                    GetBaiduSpeech(voicePoint[i], voicePoint[i + 1], sound_path, _name, language, splitTime);
                    ++_name;
                }
                //word原文
                if (Path.GetExtension(word_path).Contains("doc") || Path.GetExtension(word_path).Contains("docx"))
                {
                    originalText = NPOIHelper.ExcuteWordText(word_path);
                    //word原文的集合
                    results = GetTextContrast(voiceFiles.ToArray(), voiceMilliSecond.ToArray(), baidu_text.ToArray(), originalText, language);
                }
                else if (Path.GetExtension(word_path).Contains("txt") || Path.GetExtension(word_path).Contains("lrc"))
                {
                    string _originalText = Util.ReadTxt(word_path);
                    originalText = string.Join("", _originalText.Split('|'));
                    //word原文的集合
                    results = GetTextContrast(voiceFiles.ToArray(), voiceMilliSecond.ToArray(), baidu_text.ToArray(), _originalText, language);
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
            finally
            {
                inputStream.Close();
                inputStream.Dispose();
                isFinish = true;
                //删除MP3文件
                if (Util.isNotNull(sound_path_mp3) && File.Exists(sound_path_mp3))
                {
                    File.Delete(sound_path_mp3);
                    string mp3_folder = Path.GetDirectoryName(sound_path_mp3);
                    if (Directory.Exists(mp3_folder) && Directory.GetFiles(mp3_folder).Length == 0)
                    {
                        Directory.Delete(mp3_folder);
                    }
                }
                //删除word文件
                if (File.Exists(word_path))
                {
                    File.Delete(word_path);
                    //删除word文件目录
                    string word_folder = Path.GetDirectoryName(word_path);
                    if (Directory.Exists(word_folder) && Directory.GetFiles(word_folder).Length == 0)
                    {
                        Directory.Delete(word_folder);
                    }
                }

                //删除文件夹，删除原音频文件
                if (File.Exists(sound_path))
                {
                    File.Delete(sound_path);
                    //删除原音频目录
                    string sound_folder = Path.GetDirectoryName(sound_path);
                    if (Directory.Exists(sound_folder) && Directory.GetFiles(sound_folder).Length == 0)
                    {
                        Directory.Delete(sound_folder);
                    }
                }
            }
        }

        #region 百度转语音
        /// <summary>
        /// 截取音频后，对每个小音频获语音转文字，一个音频对应一段文字
        /// </summary>
        /// <param name="startMilliSecond"></param>
        /// <param name="endMilliSecond"></param>
        /// <param name="reader"></param>
        /// <param name="sound_path"></param>
        /// <param name="_name"></param>
        /// <param name="language"></param>
        /// <param name="splitTime"></param>
        private void GetBaiduSpeech(TimeSpan startMilliSecond, TimeSpan endMilliSecond, string sound_path, int i, string language, double splitTime)
        {
            string newFile = "";
            string _newFile = "";
            string pcm_newFile = "";
            bool need_delete = true;
            //将文件保存到新的文件夹（sound_path是原音频路径，newFolder是新的小音频路径，使用完成后将上传到服务器成功的音频删除）
            string newFolder = System.AppDomain.CurrentDomain.BaseDirectory + "NewSoundFiles/" + Path.GetFileNameWithoutExtension(sound_path) + "/";
            newFolder = newFolder.Replace("/", "\\");
            AudioFileReader reader = new AudioFileReader(sound_path);
            AudioFileReader _reader = null;//超过60s音频截取小音频
            try
            {
                #region 为截取音频做准备
                //开始时间往前取startMilliSecond一半的偏移，结束时间往后取间隔时间的一半的偏移
                if (i == 0)
                {
                    startMilliSecond = startMilliSecond - TimeSpan.FromMilliseconds(startMilliSecond.TotalMilliseconds / 2);
                }
                else
                {
                    startMilliSecond = startMilliSecond - TimeSpan.FromMilliseconds(splitTime / 2);
                }
                if (endMilliSecond < reader.TotalTime)//最后一次不用取偏移
                {
                    endMilliSecond = endMilliSecond + TimeSpan.FromMilliseconds(splitTime / 2);
                }
                TimeSpan span = endMilliSecond - startMilliSecond;
                if (span.TotalSeconds > 60)//超过60s，只取50秒
                {
                    span = TimeSpan.FromSeconds(50);
                }
                var trimed = reader.Skip(startMilliSecond).Take(endMilliSecond - startMilliSecond);
                #endregion
                string fileName = Path.GetFileNameWithoutExtension(sound_path) + "_" + i + Path.GetExtension(sound_path);//重命名文件

                //重新存储到一个新的文件目录
                if (!System.IO.Directory.Exists(newFolder))
                {
                    System.IO.Directory.CreateDirectory(newFolder);
                }
                //拼接后的文件路径
                newFile = newFolder + fileName;
                //截取小音频文件
                WaveFileWriter.CreateWaveFile16(newFile, trimed);
                //上传到文件服务器
                string server_path = UploadFile.PostFile(newFile);
                if (Util.isNotNull(server_path))
                {
                    //上传成功
                    voiceFiles.Add(server_path);
                }
                else
                {
                    need_delete = false;
                    //上传失败,在服务器上的路径
                    voiceFiles.Add(Util.getServerPath() + "/NewSoundFiles/" + Path.GetFileNameWithoutExtension(sound_path) + "/" + fileName);
                }
                //大于60s的需要再处理
                if (span == TimeSpan.FromSeconds(50))//音频大于60s,只截取50s
                {
                    //小音频
                    _reader = new AudioFileReader(sound_path);
                    var _trimed = _reader.Skip(startMilliSecond).Take(span);
                    //保存新的音频文件
                    string _fileName = "_" + Path.GetFileNameWithoutExtension(sound_path) + "_" + i + ".pcm";//重命名文件
                    _newFile = newFolder + _fileName;
                    WaveFileWriter.CreateWaveFile16(_newFile, _trimed);
                    //将音频转换为文字
                    //baidu_text.Add(BaiduSpeech.BaiduTranslateToText(_newFile, language, _trimed.WaveFormat.SampleRate.ToString()));
                    baidu_text.Add(Rays.Utility.BadiAI.BaiduAI.BaiduTranslateToText(_newFile, language, trimed.WaveFormat.SampleRate.ToString()));

                }
                else
                {
                    //将wav转换成pcm可以提高识别速度
                    pcm_newFile = NAudioHelper.GetPcmPath(newFile);
                    //将音频转换为文字
                    //baidu_text.Add(BaiduSpeech.BaiduTranslateToText(pcm_newFile, language, trimed.WaveFormat.SampleRate.ToString()));
                    baidu_text.Add(Rays.Utility.BadiAI.BaiduAI.BaiduTranslateToText(pcm_newFile, language, trimed.WaveFormat.SampleRate.ToString()));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("GetBaiduSpeech:" + ex.Message);
            }
            finally
            {
                reader.Close();
                reader.Dispose();
                if (_reader != null)
                {
                    _reader.Close();
                    _reader.Dispose();
                }
                //删除文件
                if (File.Exists(_newFile))
                {
                    File.Delete(_newFile);
                }
                if (need_delete)
                {
                    if (File.Exists(newFile))
                    {
                        File.Delete(newFile);
                    }
                }
                //删除目录
                if (Directory.Exists(newFolder) && Directory.GetFiles(newFolder).Length == 0)
                {
                    Directory.Delete(newFolder);
                }

                if (File.Exists(pcm_newFile))
                {
                    File.Delete(pcm_newFile);
                    string pcm_folder = Path.GetDirectoryName(pcm_newFile);
                    //删除目录
                    if (Directory.Exists(pcm_folder) && Directory.GetFiles(pcm_folder).Length == 0)
                    {
                        Directory.Delete(pcm_folder);
                    }
                }
            }
        }


        /// <summary>
        /// 截取音频后，对每个小音频获语音转文字
        /// </summary>
        /// <param name="startMilliSecond">开始时间戳</param>
        /// <param name="endMilliSecond">结束时间戳</param>
        /// <param name="reader">音频流</param>
        /// <param name="i">用于创建新的文件</param>
        /// <param name="language">语言（zh，en）</param>
        /// <param name="splitTime">时间间隔</param>
        private void GetBaiduSpeech1(TimeSpan startMilliSecond, TimeSpan endMilliSecond, string sound_path, int i, string language, double splitTime)
        {
            try
            {
                var reader = new AudioFileReader(sound_path);
                //开始时间往前取startMilliSecond一半的偏移，结束时间往后取间隔时间的一半的偏移
                if (i == 0)
                {
                    startMilliSecond = startMilliSecond - TimeSpan.FromMilliseconds(startMilliSecond.TotalMilliseconds / 2);
                }
                else
                {
                    startMilliSecond = startMilliSecond - TimeSpan.FromMilliseconds(splitTime / 2);
                }
                if (endMilliSecond < reader.TotalTime)//最后一次不用取偏移
                {
                    endMilliSecond = endMilliSecond + TimeSpan.FromMilliseconds(splitTime / 2);
                }
                TimeSpan span = endMilliSecond - startMilliSecond;
                if (span.TotalSeconds > 60)//超过60s，只取60秒
                {
                    span = TimeSpan.FromSeconds(60);
                }
                var trimed = reader.Skip(startMilliSecond).Take(endMilliSecond - startMilliSecond);
                //保存新的音频文件
                string fileName = Path.GetFileNameWithoutExtension(sound_path) + "_" + i + Path.GetExtension(sound_path);//重命名文件
                string newFolder = System.AppDomain.CurrentDomain.BaseDirectory + "NewSoundFiles/" + Path.GetFileNameWithoutExtension(sound_path) + "/";
                //重新存储到一个新的文件目录
                if (!System.IO.Directory.Exists(newFolder))
                {
                    System.IO.Directory.CreateDirectory(newFolder);
                }
                string newFile = newFolder + fileName;
                WaveFileWriter.CreateWaveFile16(newFile, trimed);
                //绝对路径
                string path_absolute = context.Server.MapPath("/NewSoundFiles/" + Path.GetFileNameWithoutExtension(sound_path) + "/" + fileName);
                voiceFiles.Add(UploadFile.PostFile(path_absolute));
                if (span == TimeSpan.FromSeconds(60))//音频大于60s
                {
                    //小音频
                    var _reader = new AudioFileReader(sound_path);
                    var _trimed = _reader.Skip(startMilliSecond).Take(span);
                    //保存新的音频文件
                    string _fileName = "_" + Path.GetFileNameWithoutExtension(sound_path) + "_" + i + Path.GetExtension(sound_path);//重命名文件
                    string _newFile = newFolder + _fileName;
                    WaveFileWriter.CreateWaveFile16(_newFile, _trimed);
                    baidu_text.Add(BaiduSpeech.BaiduTranslateToText(_newFile, language, _trimed.WaveFormat.SampleRate.ToString()));
                }
                else
                {
                    baidu_text.Add(BaiduSpeech.BaiduTranslateToText(newFile, language, trimed.WaveFormat.SampleRate.ToString()));
                }
                reader.Close();
                reader.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }
        #endregion


        #region 按字符串相似度匹配字符串



        /// <summary>
        /// 文本匹配算法
        /// </summary>
        /// <param name="fileUrls"></param>
        /// <param name="timeSpans"></param>
        /// <param name="baidu"></param>
        /// <param name="originalText"></param>
        /// <returns></returns>
        private List<TextContrastResult> GetTextContrast(string[] fileUrls, double[] timeSpans, string[] baidu, string originalText, string lan)
        {
            //将原文按标点符号分成数组，一个一个的去对比
            //string[] originalList = System.Text.RegularExpressions.Regex.Split(originalText, @"[。？！?.!……]").Where(o=>o!=" "&&o!="").ToArray();
            string[] originalList = originalText.Replace(".", ".|").Replace("。", "。|").Replace("?", "?|").Replace("？", "？|").Replace("！", "！|").Replace("!", "!|").Replace("……", "……|").Split('|').Where(o => o != " " && o != "").ToArray();
            //1.先排除baidu里面没有转成功的
            //2.先判断两个数组的长度，一般情况是原文的长度大于等于百度翻译的长度（所以这里会有两种情况）
            //3.如果原文的长，开始取百度的第一个跟原文的第一个元素比较，然后再跟原文的第一个加上第二个的字符串比较，如果第二次的百分比大于第一次的百分比，那么继续往下比较
            //如果百分比降低了，再比较结尾

            double curr_precent = 0;//相似度
            string org_contrast = "";//匹配到的原文
            int index = 0;
            List<TextContrastResult> list = new List<TextContrastResult>();
            for (int i = 0; i < baidu.Length; i++)
            {
                string bd = baidu[i];
                double time = timeSpans[i];
                string file = fileUrls[i];
                //（注意一点的就是，如果转换的是中文，那么当出现英文时，直接不用匹配，反之亦然）
                if (bd != "3301-百度语音转文字出错" && Util.isNotNull(bd))
                {
                    TextContrastResult result = new TextContrastResult();
                    if (lan == "zh")
                    {
                        RecursionHz(bd.Replace(" ", ""), originalList, out org_contrast, out curr_precent, ref index);
                    }
                    else
                    {
                        RecursionYw(bd, originalList, out org_contrast, out curr_precent, en_index);
                    }
                    if (curr_precent <= 50)
                    {
                        org_contrast = "未找到匹配的字符串！";
                    }
                    result.baiduText = bd;
                    result.file_url = file;
                    result.timespan = time;
                    result.contractText = org_contrast;
                    result.precent = curr_precent + "%";
                    list.Add(result);
                }
                else
                {
                    TextContrastResult result = new TextContrastResult();
                    result.baiduText = bd;
                    result.file_url = file;
                    result.timespan = time;
                    result.contractText = "";
                    result.precent = "0%";
                    list.Add(result);
                }
            }
            return list;
        }
        /// <summary>
        /// 开始计算相似度(中文：一个汉字是一个字符)
        /// </summary>
        /// <param name="baiduText"></param>
        /// <param name="orgs"></param>
        /// <param name="contrastText"></param>
        /// <param name="precent"></param>
        /// <param name="index"></param>
        private void RecursionHz(string baiduText, string[] orgs, out string contrastText, out double precent, ref int index)
        {
            contrastText = "";
            precent = 0;
            bool is_break = false;
            for (int i = index; i < orgs.Length; i++)
            {
                if (Util.isNotNull(orgs[i]))
                {
                    contrastText += orgs[i].Replace(" ", "");

                    Similar similar = TextContrast.GetStringSimilarityPerZw(baiduText, contrastText);
                    if (similar.success)
                    {
                        precent = similar.precent;
                        if (i < orgs.Length - 1)
                        {
                            for (int j = i + 1; j < orgs.Length; j++)
                            {
                                string next_contrastText = contrastText;
                                next_contrastText += orgs[j].Replace(" ", "");
                                Similar next_similar = TextContrast.GetStringSimilarityPerZw(baiduText, next_contrastText);
                                if (next_similar.success)
                                {
                                    if (next_similar.precent > precent)
                                    {
                                        precent = next_similar.precent;
                                        contrastText = next_contrastText;
                                    }
                                    else
                                    {
                                        //移除掉之前匹配的字符串
                                        index = j;
                                        is_break = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    LogHelper.Error("字符串计算相似度百分比时出错：" + similar.error);
                                    break;
                                }
                            }
                            is_break = true;
                        }
                    }
                    else//出错了就不比较了
                    {
                        LogHelper.Error("字符串计算相似度百分比时出错：" + similar.error);
                        contrastText = "";
                        precent = 0;
                        index = i + 1;
                        break;
                    }

                    //
                    if (is_break)
                    {
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// 开始计算相似度(英文：一个单词是一个字符)
        /// </summary>
        /// <param name="baiduText"></param>
        /// <param name="orgs"></param>
        /// <param name="contrastText"></param>
        /// <param name="precent"></param>
        /// <param name="index"></param>
        private void RecursionYw(string baiduText, string[] orgs, out string contrastText, out double precent, int index)
        {
            contrastText = "";
            precent = 0;
            bool is_break = false;
            for (int i = index; i < orgs.Length; i++)
            {
                if (Util.isNotNull(orgs[i]))
                {
                    contrastText += orgs[i];

                    Similar similar = TextContrast.GetStringSimilarityPerYw(baiduText, contrastText);
                    if (similar.success)
                    {
                        precent = similar.precent;
                        if (i < orgs.Length - 1)
                        {
                            for (int j = i + 1; j < orgs.Length; j++)
                            {
                                string next_contrastText = contrastText;
                                next_contrastText += orgs[j];
                                Similar next_similar = TextContrast.GetStringSimilarityPerYw(baiduText, next_contrastText);
                                if (next_similar.success)
                                {
                                    //增加容错率
                                    if (next_similar.precent > precent || (next_similar.precent <= precent && precent - next_similar.precent <= 0.5))
                                    {
                                        precent = next_similar.precent;
                                        contrastText = next_contrastText;
                                    }
                                    else
                                    {
                                        //如果相似度最高连50%都达不到，那么就当没有匹配项
                                        if (precent <= 50.0)
                                        {
                                            //index = i;//重置
                                            //contrastText = "相似度太低，未找到匹配项";
                                            //如果相似度太低那么继续递归查询
                                            int _index = i + 1;
                                            RecursionYw(baiduText, orgs, out contrastText, out precent, _index);
                                        }
                                        else
                                        {
                                            //移除掉之前匹配的字符串
                                            en_index = j;
                                        }
                                        is_break = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    LogHelper.Error("字符串计算相似度百分比时出错：" + similar.error);
                                    break;
                                }
                            }
                            is_break = true;
                        }
                    }
                    else//出错了就不比较了
                    {
                        LogHelper.Error("字符串计算相似度百分比时出错：" + similar.error);
                        contrastText = "";
                        precent = 0;
                        en_index = i + 1;
                        break;
                    }

                    //
                    if (is_break)
                    {
                        break;
                    }
                }
            }
        }
        #endregion
    }
}