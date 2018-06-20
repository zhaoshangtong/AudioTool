using AudioToolNew.Models.Musicline;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using Rays.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AudioToolNew.Common
{
    /// <summary>
    /// 播放音频，并绘制波形
    /// </summary>
    public class Musicline
    {
        private bool _hasVoice = true;
        private bool _first = true;
        private TimeSpan startMilliSecond;//有声音的时间，用来检查一段对话是否结束
        private List<TimeSpan> voicePoint = new List<TimeSpan>();//存储开始和结束的时间戳，用来语音转文字
        private List<string> baidu_text = new List<string>();//存放百度语音转文字
        public List<MusicResult> return_list { get; set; }
        public string originalText { get; set; }
        private List<double> voiceMilliSecond = new List<double>();//存放每次语音开始时的时间戳
        private List<string> voiceFiles = new List<string>();//存放截取的小文件路径
        private List<string> baiduText = new List<string>();//存放对比后的歌词文本
        public bool isFinish = false;
        public HttpContext context { get; set; }
        public Musicline()
        {
            return_list = new List<Models.Musicline.MusicResult>();
            context = HttpContext.Current;
        }
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <param name="sound_path"></param>
        /// <param name="word_path"></param>
        /// <param name="language"></param>
        /// <param name="splitTime"></param>
        public void GetTimeSpan(string sound_path, string word_path, string language, double splitTime = 1.5)
        {
            var inputStream = new AudioFileReader(sound_path);
            try
            {
                if (sound_path.Contains(".mp3"))
                {
                    sound_path = NAudioHelper.GetWavPath(sound_path);
                }
                WAVReader reader = new WAVReader();
                reader.GetTimeSpan(sound_path, out voicePoint, out voiceMilliSecond, splitTime);
                if (voicePoint.Count % 2 != 0)//voicePoint是为了截取小音频
                {
                    voicePoint.Add(inputStream.TotalTime);
                }
                int _name = 1;//命名
                for (int i = 0; i < voicePoint.Count; i += 2)
                {
                    GetBaiduSpeech(voicePoint[i], voicePoint[i + 1], sound_path, _name, language, splitTime);
                    ++_name;
                }
                //开始比对
                //获取文档内容
                CompareText(word_path, language);
                //通过时间戳集合去匹配路径与歌词文本（百度不准）
                for(int i=0;i< voiceMilliSecond.Count; i++)
                {
                    MusicResult result = new MusicResult();
                    result.timeSpan = voiceMilliSecond[i];
                    string url = voiceFiles[i];
                    if (url == "")//上传失败，重新上传
                    {
                        string fileName = Path.GetFileNameWithoutExtension(sound_path) + i + ".wav";//重命名文件
                        string newFolder = System.AppDomain.CurrentDomain.BaseDirectory + "NewSoundFiles/" + Path.GetFileNameWithoutExtension(sound_path) + "/";
                        string newFile = newFolder + fileName;
                        //绝对路径
                        string path_absolute = context.Server.MapPath("/NewSoundFiles/" + Path.GetFileNameWithoutExtension(sound_path) + "/" + fileName);
                        result.fileUrl = UploadFile.PostFile(path_absolute);

                    }
                    else
                    {
                        result.fileUrl = i < voiceFiles.Count ? voiceFiles[i] : "";
                    }
                    
                    result.text = i < baiduText.Count ? baiduText[i] : "未匹配上字符串";
                    return_list.Add(result);
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
            finally
            {
                inputStream.Close();
                inputStream.Dispose();
                isFinish = true;
            }
        }

        #region NAudio处理音频

        /// <summary>
        /// 上传音频获取时间戳(百度不支持MP3，所以要先转成wav)
        /// </summary>
        /// <param name="sound_path">音频路径</param>
        /// <param name="word_path">文本路径</param>
        /// <param name="language">语言</param>
        /// <param name="splitTime">间隔时间</param>
        public void GetTimeSpanByNAudio(string sound_path, string word_path, string language, double splitTime = 1.5)
        {
            try
            {
                Task task_max = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (sound_path.Contains(".mp3"))
                        {
                            sound_path = NAudioHelper.GetWavPath(sound_path);
                        }
                        var inputStream = new AudioFileReader(sound_path);
                        string file_type = Path.GetExtension(sound_path).Substring(1);

                        var aggregator = new SampleAggregator(inputStream);
                        aggregator.NotificationCount = inputStream.WaveFormat.SampleRate / 100;
                        aggregator.MaximumCalculated += (s, a) =>
                        {
                            MaximumCalculated(a, file_type, inputStream, splitTime);
                        };

                        //IWavePlayer playbackDevice = new WaveOut { DesiredLatency = 200 };
                        IWavePlayer playbackDevice = new DirectSoundOut(DirectSoundOut.DSDEVID_DefaultPlayback);
                        playbackDevice.Init(aggregator);
                        playbackDevice.PlaybackStopped += (s, a) =>
                        {
                            PlaybackStopped(a, sound_path, word_path, language, out isFinish, splitTime, inputStream, s as IWavePlayer);
                        };
                        playbackDevice.Play();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                    }
                    
                });
                Task.WaitAny(task_max);
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
            
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sound_path">音频路径</param>
        /// <param name="word_path">文档路径</param>
        /// <param name="language">语言</param>
        /// <param name="splitTime">间隔时间</param>
        private void PlaybackStopped(StoppedEventArgs e, string sound_path, string word_path, string language, out bool isFinish, double splitTime, AudioFileReader reader,IWavePlayer playbackDevice)
        {
            //最后的结束时间
            if (voicePoint.Count % 2 != 0)
            {
                voicePoint.Add(reader.TotalTime);
            }
            int _name = 1;//命名
            for (int i = 0; i < voicePoint.Count; i += 2)
            {
                GetBaiduSpeech(voicePoint[i], voicePoint[i + 1], sound_path, _name, language, splitTime);
                ++_name;
            }
            //开始比对
            //获取文档内容
            CompareText(word_path, language);
            reader.Close();
            reader.Dispose();
            if (playbackDevice != null)
            {
                playbackDevice.Stop();
            }
            isFinish = true;
        }

        #endregion
        /// <summary>
        /// 对比文字内容
        /// </summary>
        /// <param name="word_path">word文件路径</param>
        private void CompareText(string word_path,string language)
        {
            try
            {
                if (Util.isNotNull(word_path))
                {
                    #region 当有文档时
                    string word_text = NPOIHelper.ExcuteWordText(word_path);
                    originalText = word_text;
                    #region 重新算匹配
                    if (language.Equals("zh"))
                    {
                        //记录每次匹配到的在原字符串的位置
                        List<int> index_num = new List<int>();
                        //遍历百度语音转文字的集合
                        #region 遍历内容，并标注匹配到的序号
                        foreach (var _text in baidu_text)
                        {
                            var baidu_text_chars = _text.ToArray();
                            //百度每句话结尾都有一个逗号，无意义
                            int fail = 0;
                            for (int i = baidu_text_chars.Length - 2; i > 0; i--)
                            {
                                Regex regex = new Regex(@"\p{P}");
                                //先判断最后一个字符在word_text中第一次出现的位置，从后往前比较
                                var new_char = baidu_text_chars[i];
                                if (regex.IsMatch(new_char.ToString()))//不比较标点符号
                                {
                                    continue;
                                }
                                int index = word_text.IndexOf(new_char);
                                if (index > 0)//如是有相同的
                                {
                                    //判断word_text下一个字符是否是标点符号（还有下下下个字符"《xx'x'》。"直到是正文为止）,第一次不是，那么表示这句话还没完，要继续往下取，如果
                                    //第一次是，那么再往下取，直到不是标点符合为止
                                    
                                    bool first = true;//第一次出现标点
                                    //找到相同的，并继续查看下一个字符是否是标点符号（没有标点符号的情况暂时不考虑）
                                    int _index = index+1;
                                    #region 标点符号
                                    //for (int j = index; j < word_text.Length;)
                                    //{
                                    //    if (regex.IsMatch(word_text.Substring(j=j== word_text.Length-1?j:j+1, 1)))
                                    //    {
                                    //        ++index;//表示依旧是标点符号
                                    //        ++j;
                                    //        first = false;
                                    //    }
                                    //    else
                                    //    {
                                    //        if (first)
                                    //        {
                                    //            ++index;//表示这句话没完，继续向后取
                                    //            ++j;
                                    //            first = false;//
                                    //        }
                                    //        else
                                    //        {
                                    //            break;//表示一句话取完了（会有误差的可能性，百度转文字如果不准确率太高，会出现丢失的情况）
                                    //        }
                                    //    }
                                    //}
                                    #endregion
                                    for(int j=_index;j< word_text.Length; j++)
                                    {
                                        if (regex.IsMatch(word_text.Substring(j, 1)))
                                        {
                                            first = false;
                                            continue;
                                        }
                                        else
                                        {
                                            if (first)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                index = j;
                                                break;
                                            }
                                        }
                                    }
                                    index = index >= word_text.Length - 1 ? word_text.Length - 1 : index;//如果index已经比原字符串还大，那默认取源字符串长度
                                                                                                         //如果再次匹配的index与之前的一样，则继续往下匹配
                                    if (index_num.Contains(index)&& index< word_text.Length-1)
                                    {
                                        //新截取的字符串中再次查找
                                        string new_word_text = word_text.Substring(index + 1);
                                        int new_index = new_word_text.IndexOf(new_char);//在新的字符串中继续比较
                                        if (new_index > 0)//如是有相同的
                                        {
                                            //判断word_text下一个字符是否是标点符号（还有下下下个字符"《xx'x'》。"直到是正文为止）,第一次不是，那么表示这句话还没完，要继续往下取，如果
                                            //第一次是，那么再往下取，直到不是标点符合为止
                                            bool _first = true;
                                            int _new_index = new_index + 1;
                                            //找到相同的，并继续查看下一个字符是否是标点符号（没有标点符号的情况暂时不考虑）
                                            #region 标点符号
                                            for (int _j = _new_index; _j < word_text.Length;_j++)
                                            {
                                                if (regex.IsMatch(new_word_text.Substring(_j=_j== word_text.Length-1?_j:_j+1, 1)))
                                                {
                                                    _first = false;
                                                    continue;
                                                }
                                                else
                                                {
                                                    if (_first)
                                                    {
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        new_index = _j;
                                                        break;//表示一句话取完了（会有误差的可能性，百度转文字如果不准确率太高，会出现丢失的情况）
                                                    }
                                                }
                                            }
                                            #endregion
                                            new_index = new_index >= new_word_text.Length - 1 ? new_word_text.Length - 1 : new_index;

                                            index_num.Add(new_index + index);//有相同的就必须得加上之前截断的
                                        }
                                        else//这里就表示此字符在截断后的字符串中没有相同的
                                        {
                                            continue;
                                        }
                                    }
                                    index_num.Add(index);
                                    break;
                                }
                                else
                                {
                                    if (baidu_text_chars.Length - 2 == fail)//百度语音转文字的都匹配了一遍，没有相同的
                                    {
                                        index_num.Add(index);//index=-1
                                    }
                                    ++fail;//每失败一次都加1，全部失败时
                                    continue;
                                }
                            }
                        }
                        #endregion
                        #region 遍历序号，找出异常情况，一般有异常的直接就属于没匹配到的（文字/时间戳/音频文件都是一样多的）
                        for (int start = 0; start < index_num.Count; start++)
                        {
                            if (index_num[start] < 0)//属于没有匹配的情况
                            {
                                //MusicResult result = new MusicResult();
                                //result.text = "未匹配上字符串";
                                //result.timeSpan = voiceMilliSecond[start];
                                //result.fileUrl = voiceFiles[start];
                                //return_list.Add(result);
                                baiduText.Add("未匹配上字符串");
                                continue;
                            }
                            else if (start <= index_num.Count - 2 && index_num[start] >= index_num[start + 1] && index_num[start] > 0)//不是最后一行，且比下一行的数字大
                            {
                                //MusicResult result = new MusicResult();
                                //result.text = "未匹配上字符串";
                                //result.timeSpan = voiceMilliSecond[start];
                                //result.fileUrl = voiceFiles[start];
                                //return_list.Add(result);
                                baiduText.Add("未匹配上字符串");
                                index_num[start] = -1;//将无匹配项更新为-1
                                continue;
                            }
                            else
                            {
                                //MusicResult result = new MusicResult();
                                if (start == 0)//第一行
                                {
                                    //result.text = word_text.Substring(0, index_num[start] + 1);
                                    baiduText.Add(word_text.Substring(0, index_num[start] + 1));
                                    continue;
                                }
                                else if (start == index_num.Count - 1)//最后一行
                                {
                                    if (index_num[start - 1] > 0)//如果上一行匹配正确
                                    {
                                        //result.text = word_text.Substring(index_num[start - 1]+1, word_text.Length - index_num[start - 1] - 1);
                                        baiduText.Add(word_text.Substring(index_num[start - 1] + 1, word_text.Length - index_num[start - 1] - 1));
                                        continue;
                                    }
                                    else //如果上一行匹配失败，则倒着遍历所有项，直到为0，找到正确匹配项
                                    {
                                        for (int n = start - 2; n >= 0; n--)
                                        {
                                            if (index_num[n] > 0)
                                            {
                                                //result.text = word_text.Substring(index_num[n]+1, word_text.Length - index_num[n] - 1);
                                                baiduText.Add(word_text.Substring(index_num[n] + 1, word_text.Length - index_num[n] - 1));
                                                break;
                                            }
                                            else
                                            {
                                                if (n == 0)//匹配到最后都没有正确匹配的
                                                {
                                                    //result.text = word_text.Substring(0, word_text.Length-1);
                                                    baiduText.Add(word_text.Substring(0, word_text.Length - 1));
                                                    break;
                                                }
                                                continue;
                                            }
                                        }
                                        continue;
                                    }
                                }
                                else //其他行都是上一行到自己行的
                                {
                                    if (index_num[start - 1] > 0)//如果上一行匹配正确
                                    {
                                        //result.text = word_text.Substring(index_num[start - 1] + 1, index_num[start] - index_num[start - 1]);
                                        baiduText.Add(word_text.Substring(index_num[start - 1] + 1, index_num[start] - index_num[start - 1]));
                                        continue;
                                    }
                                    else //如果上一行匹配失败，则倒着遍历所有项，直到为0，找到正确匹配项
                                    {
                                        for (int n = start - 2; n >= 0; n--)
                                        {
                                            if (index_num[n] > 0)
                                            {
                                                //result.text = word_text.Substring(index_num[n] + 1, index_num[start] - index_num[n]);
                                                baiduText.Add(word_text.Substring(index_num[n] + 1, index_num[start] - index_num[n]));
                                                break;
                                            }
                                            else
                                            {
                                                if (n == 0)//匹配到最后都没有正确匹配的
                                                {
                                                    //result.text = word_text.Substring(0, index_num[start]);
                                                    baiduText.Add(word_text.Substring(0, index_num[start]));
                                                    break;
                                                }
                                                continue;
                                            }
                                        }
                                        continue;
                                    }
                                }
                                //result.timeSpan = voiceMilliSecond[start];
                                //result.fileUrl = voiceFiles[start];
                                //return_list.Add(result);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        CompareEnglish(word_text);
                    }

                    #endregion
                    #endregion
                }
                else
                {
                    for (int i = 0; i < baidu_text.Count; i++)//无需匹配
                    {
                        baiduText.Add(baidu_text[i]);
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
            
        }
        /// <summary>
        /// 匹配英文（跟匹配中文不一样）
        /// </summary>
        private void CompareEnglish(string word_text)
        {
            try
            {
                Regex regex = new Regex(@"\p{P}");
                var word_text_chars = word_text.Split(' ');//拆分成单词数组
                List<int> index_num = new List<int>();
                #region 遍历内容，并标注匹配到的序号
                foreach (var _text in baidu_text)
                {
                    var baidu_text_chars = _text.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);//单词数组，用单词匹配
                                                                                                                       //百度每句话结尾都有一个逗号，无意义
                    int fail = 0;
                    for (int i = baidu_text_chars.Length - 1; i >= 0; i--)
                    {
                        //先判断最后一个字符在word_text中第一次出现的位置，从后往前比较
                        var new_char = baidu_text_chars[i];
                        bool isCompare = false;
                        //遍历文档获取的内容数组
                        int index = 0;
                        for (int j = 0; j < word_text_chars.Count(); j++)
                        {
                            if (word_text_chars[j].Trim().ToLower().IndexOf(new_char) >= 0)
                            {
                                isCompare = true;
                                index = j;
                                #region 下个单词是否包含标点符号
                                for (int n = j; n < word_text_chars.Count(); n++)
                                {
                                    if (regex.IsMatch(word_text_chars[n]))
                                    {
                                        index = n;//表示依旧you标点符号，找到第一个包含标点的就不往下找了
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                #endregion

                            }

                        }
                        if (isCompare)//匹配成功
                        {
                            index_num.Add(index);//第几个单词匹配上了
                            break;//不用再匹配了
                        }
                        else//匹配失败
                        {
                            if (baidu_text_chars.Length - 1 == fail)//百度语音转文字的都匹配了一遍，没有相同的
                            {
                                index_num.Add(-1);//index=-1
                            }
                            ++fail;
                            continue;//继续匹配下一个单词
                        }
                    }
                }
                #endregion
                #region 这里的序号是集合里的单词下标
                for (int start = 0; start < index_num.Count; start++)
                {
                    if (index_num[start] < 0)//属于没有匹配的情况
                    {
                        baiduText.Add("未匹配上字符串");
                        continue;
                    }
                    else if (start <= index_num.Count - 2 && index_num[start] > index_num[start + 1] && index_num[start] > 0)//不是最后一行，且比下一行的数字大
                    {
                        baiduText.Add("未匹配上字符串");
                        index_num[start] = -1;//将无匹配项更新为-1
                        continue;
                    }
                    else
                    {
                        if (start == 0)//第一行
                        {
                            string text = GetArrayToString(word_text_chars, 0, index_num[start]);
                            baiduText.Add(text);
                            continue;
                        }
                        else if (start == index_num.Count - 1)//最后一行
                        {
                            if (index_num[start - 1] > 0)//如果上一行匹配正确
                            {
                                string text = GetArrayToString(word_text_chars, index_num[start - 1] + 1, index_num[start]);
                                baiduText.Add(text);
                                continue;
                            }
                            else //如果上一行匹配失败，则倒着遍历所有项，直到为0，找到正确匹配项
                            {
                                for (int n = start - 2; n >= 0; n--)
                                {
                                    if (index_num[n] > 0)
                                    {
                                        string text = GetArrayToString(word_text_chars, index_num[n] + 1, index_num[start]);
                                        baiduText.Add(text);
                                        break;
                                    }
                                    else
                                    {
                                        if (n == 0)//匹配到最后都没有正确匹配的
                                        {
                                            string text = GetArrayToString(word_text_chars, 0, index_num[start]);
                                            baiduText.Add(text);
                                            break;
                                        }
                                        continue;
                                    }
                                }
                                continue;
                            }
                        }
                        else //其他行都是上一行到自己行的
                        {
                            if (index_num[start - 1] > 0)//如果上一行匹配正确
                            {
                                string text = GetArrayToString(word_text_chars, index_num[start - 1] + 1, index_num[start]);
                                baiduText.Add(text);
                                continue;
                            }
                            else //如果上一行匹配失败，则倒着遍历所有项，直到为0，找到正确匹配项
                            {
                                if (start - 2 >= 0)
                                {
                                    for (int n = start - 2; n >= 0; n--)
                                    {
                                        if (index_num[n] > 0)
                                        {
                                            string text = GetArrayToString(word_text_chars, index_num[n] + 1, index_num[start]);
                                            baiduText.Add(text);
                                            break;
                                        }
                                        else
                                        {
                                            if (n == 0)//匹配到最后都没有正确匹配的
                                            {
                                                string text = GetArrayToString(word_text_chars, 0, index_num[start]);
                                                baiduText.Add(text);
                                                break;
                                            }
                                            continue;
                                        }
                                    }
                                    continue;
                                }
                                else//表示往前一行就已经是第一行了
                                {
                                    string text = GetArrayToString(word_text_chars, 0, index_num[start]);
                                    baiduText.Add(text);
                                    continue;
                                }

                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
            #endregion
        }
        /// <summary>
        /// 将数组拼成字符串(前后包含)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="start_index"></param>
        /// <param name="end_index"></param>
        /// <returns></returns>
        private string GetArrayToString(string[] array,int start_index,int end_index)
        {
            string[] newArray = array.Skip(start_index).Take(end_index - start_index+1).ToArray();
            string returnStr = "";
            foreach(string str in newArray)
            {
                returnStr += str+" ";
            }
            return returnStr;
        }

        #region 使用NAudio播放获取时间戳
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <param name="e"></param>
        /// <param name="file_type">文件类型</param>
        /// <param name="reader"></param>
        /// <param name="splitTime">间隔时间</param>
        private void MaximumCalculated(MaxSampleEventArgs e, string file_type, AudioFileReader reader, double splitTime)
        {
            try
            {
                var max = e.MaxSample * 100;
                if (_hasVoice && max < 1 && _first)//第一次进入
                {
                    _first = false;
                    _hasVoice = false;
                }
                else if (!_first && _hasVoice && max < 1 && (reader.CurrentTime - startMilliSecond).TotalMilliseconds > splitTime * 1000)//每次结束对话
                {
                    voicePoint.Add(reader.CurrentTime - TimeSpan.FromMilliseconds(splitTime * 1000));
                    _hasVoice = false;
                    //Console.WriteLine("End:" + (reader.CurrentTime-TimeSpan.FromMilliseconds(splitTime * 1000)) + "   ");
                }
                else if (!_hasVoice && max > 1)//开始有声音
                {
                    //Console.WriteLine("Start:" + reader.CurrentTime + "   ");
                    startMilliSecond = reader.CurrentTime;
                    _hasVoice = true;
                    voicePoint.Add(reader.CurrentTime);
                    voiceMilliSecond.Add(reader.CurrentTime.TotalSeconds);
                }
                else if ( _hasVoice && max > 1)//正在播声音
                {
                    startMilliSecond = reader.CurrentTime;
                }
            } catch(Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
            
        }
        #endregion
        #region 截取音频，并使用百度语言转文字
        /// <summary>
        /// 获取百度语音转文字
        /// </summary>
        /// <param name="startMilliSecond">开始时间戳</param>
        /// <param name="endMilliSecond">结束时间戳</param>
        /// <param name="reader">音频流</param>
        /// <param name="i">用于创建新的文件</param>
        /// <param name="language">语言（zh，en）</param>
        /// <param name="splitTime">时间间隔</param>
        private void GetBaiduSpeech(TimeSpan startMilliSecond, TimeSpan endMilliSecond, string sound_path, int i, string language, double splitTime)
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
                string fileName = Path.GetFileNameWithoutExtension(sound_path) + i + ".wav";//重命名文件
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
                    var _reader = new AudioFileReader(sound_path);
                    var _trimed = _reader.Skip(startMilliSecond).Take(span);
                    //保存新的音频文件
                    string _fileName = "_"+Path.GetFileNameWithoutExtension(sound_path) + i + ".wav";//重命名文件
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
            } catch(Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }
        #endregion
    }
}