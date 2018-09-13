using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace AudioToolNew.Commom
{
    public class SppechEvaluation
    {
        /// <summary>
        /// 英文准确度
        /// </summary>
        /// <param name="enText"></param>
        /// <param name="words"></param>
        /// <returns></returns>
        public static int calaAccuracySocre(string enText, string[] words)
        {
            int accuracyNum = 0;
            string[] enWords = getSentenceWords(enText);
            for (int i = 0; i < enWords.Length; i++)
            {
                enWords[i] = replaceSymbol(enWords[i]);
                for (int j = 0; j < words.Length; j++)
                {
                    if (words[j].IndexOf(enWords[i].ToLower()) > -1)
                    {
                        accuracyNum += enWords[i].Length;
                        break;
                    }
                }
            }

            //string[] szEnText = getSentenceWords(enText);//按单词字母的数量评分
            int socre = (int)((float)accuracyNum * 100 / replaceSymbol(enText).Replace(" ", "").Length);
            return socre > 100 ? 100 : socre;
        }
        /// <summary>
        /// 中文准确度
        /// </summary>
        /// <param name="enText"></param>
        /// <param name="words"></param>
        /// <returns></returns>
        public static int calaAccuracySocre(string enText, char[] words)
        {
            int accuracyNum = 0;
            char[] enWords = getSentenceWordsZh(enText);
            for (int i = 0; i < enWords.Length; i++)
            {
                //enWords[i] = replaceSymbol(enWords[i]);
                for (int j = 0; j < words.Length; j++)
                {
                    if (words[j]==enWords[i])
                    {
                        accuracyNum += 1;
                        break;
                    }
                }
            }
            //string[] szEnText = getSentenceWords(enText);//按单词字母的数量评分
            int socre = (int)((float)accuracyNum * 100 / replaceSymbol(enText).Replace(" ", "").Length);
            return socre > 100 ? 100 : socre;
        }
        /// <summary>
        /// 流畅度
        /// </summary>
        /// <param name="enText"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static int calaFluencySocreZh(string enText, int seconds)
        {
            //float fluencySocre = (8 * (float)seconds * 100) / (6 * enText.Split(' ').Length);
            float s = (float)getSentenceWordsZh(enText).Length * 3 / 8;//满分应该是s秒
            if (seconds > 2 * s)
            {
                return 0;
            }
            else
            {
                float fluencySocre = (1 - (float)Math.Abs(s - seconds) / s) * 100;
                int socre = (int)fluencySocre;
                return socre > 100 ? 100 : socre;
            }
        }
        public static int calaFluencySocre(string enText, int seconds)
        {
            //float fluencySocre = (8 * (float)seconds * 100) / (6 * enText.Split(' ').Length);
            float s = (float)getSentenceWords(enText).Length * 6 / 8;//满分应该是s秒
            if (seconds > 2 * s)
            {
                return 0;
            }
            else
            {
                float fluencySocre = (1 - (float)Math.Abs(s - seconds) / s) * 100;
                int socre = (int)fluencySocre;
                return socre > 100 ? 100 : socre;
            }
        }
        /// <summary>
        /// 完整度
        /// </summary>
        /// <param name="enText"></param>
        /// <param name="words"></param>
        /// <param name="accuracySocre"></param>
        /// <returns></returns>
        public static int calaIntegritySocre(string enText, string[] words, float accuracySocre)
        {
            string[] enWords = getSentenceWords(replaceSymbol(enText));
            float integritySocre = 0;
            if (words.Length > enWords.Length)//识别单词数大于标准文本单词数
            {
                integritySocre = 50 + (accuracySocre / 2);
            }
            else
            {
                integritySocre = ((float)words.Length / enWords.Length * 50) + (accuracySocre / 2);
            }
            int socre = (int)integritySocre;
            return socre > 100 ? 100 : socre;
        }
        /// <summary>
        /// 完整度
        /// </summary>
        /// <param name="enText"></param>
        /// <param name="words"></param>
        /// <param name="accuracySocre"></param>
        /// <returns></returns>
        public static int calaIntegritySocre(string enText, char[] words, float accuracySocre)
        {
            char[] enWords = getSentenceWordsZh(enText);
            float integritySocre = 0;
            if (words.Length > enWords.Length)//识别单词数大于标准文本单词数
            {
                integritySocre = 50 + (accuracySocre / 2);
            }
            else
            {
                integritySocre = ((float)words.Length / enWords.Length * 50) + (accuracySocre / 2);
            }
            int socre = (int)integritySocre;
            return socre > 100 ? 100 : socre;
        }
        /// <summary>
        /// 获取一个句子的单词列表
        /// </summary>
        /// <param name="speechText"></param>
        /// <returns></returns>
        public static string[] getSentenceWords(string speechText)
        {
            speechText = speechText.Replace(",", " , ");
            speechText = speechText.Replace(".", " . ");
            speechText = speechText.Replace("，", " ， ");
            speechText = speechText.Replace("。", " 。 ");
            speechText = speechText.Replace("?", " ? ");
            speechText = speechText.Replace("!", " ! ");
            speechText = speechText.Replace("…", " … ");
            speechText = speechText.Replace("？", " ？ ");
            speechText = speechText.Replace("(", " ( ");
            speechText = speechText.Replace("（", " （ ");
            speechText = speechText.Replace("）", " ） ");
            speechText = speechText.Replace("！", " ！ ");
            speechText = speechText.Replace("\"", " \" ");
            speechText = speechText.Replace("“", " “ ");
            speechText = speechText.Replace("”", " ” ");
            string[] words = speechText.Split(' ');
            //string[] words = Regex.Split(speechText, "[,.:\\s!?！？，。]");           
            List<string> list = new List<string>();
            List<string> hs = new List<string>(words);
            foreach (string str in hs)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    list.Add(str);
                }
            }
            return list.ToArray();
        }
        /// <summary>
        /// 获取一个句子的汉子列表
        /// </summary>
        /// <param name="speechText"></param>
        /// <returns></returns>
        public static char[] getSentenceWordsZh(string speechText)
        {
            speechText= Regex.Replace(speechText, @"\p{P}", "");
            char[] words = speechText.ToCharArray();
            //string[] words = Regex.Split(speechText, "[,.:\\s!?！？，。]");           
            List<char> list = new List<char>();
            List<char> hs = new List<char>(words);
            foreach (char str in hs)
            {
                if (Util.isNotNull(str))
                {
                    list.Add(str);
                }
            }
            return list.ToArray();
        }
        public static string[] getSentenceWordsBySpace(string speechText)
        {
            string[] words = speechText.Split(' ');
            List<string> list = new List<string>();
            List<string> hs = new List<string>(words);
            foreach (string str in hs)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    list.Add(str);
                }
            }
            return list.ToArray();
        }
        /// <summary>
        /// "00:01:23"格式的时间转化为秒数
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static int timeToSecond(string duration)
        {
            string[] times = duration.Split(':');
            int seconds = int.Parse(times[0]) * 3600 + int.Parse(times[1]) * 60 + int.Parse(times[2]);
            return seconds;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="enText"></param>
        /// <param name="speechResult"></param>
        /// <returns></returns>
        public static string getSentenceAccuracy(string enText, List<string> speechResult)
        {
            string json = "";
            json = "[";
            string[] enWords = getSentenceWords(enText);//对比的时候去掉符号，可能修改，enWords1为原句子
            string[] enWords1 = getSentenceWords(enText);
            bool flag = false;
            for (int i = 0; i < enWords.Length; i++)
            {
                enWords[i] = replaceSymbol(enWords[i]);
                for (int j = 0; j < speechResult.Count; j++)
                {
                    flag = false;
                    if (speechResult[j].IndexOf(enWords[i].ToLower()) > -1)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    json += "{\"word\":\"" + enWords1[i] + "\",\"isError\":false},";
                }
                else
                {
                    json += "{\"word\":\"" + enWords1[i] + "\",\"isError\":true},";
                }
            }
            json = json.TrimEnd(',');
            json += "]";
            return json;
        }
        /// <summary>
        /// 获取返回句子（英文）
        /// </summary>
        /// <param name="enText"></param>
        /// <param name="speechResult"></param>
        /// <returns></returns>
        public static string getSentenceAccuracy2(string enText, List<string> speechResult)
        {
            string json = "";
            json = "[";
            string[] enWords = getSentenceWords(enText);//对比的时候去掉符号，可能修改，enWords1为原句子
            string[] enWords1 = getSentenceWords(enText);
            bool flag = false;
            for (int i = 0; i < enWords.Length; i++)
            {
                //enWords[i] = replaceSymbol(enWords[i]);//去掉符号
                //按照上面的截取规则，标点符号都在最后一个
                for (int j = 0; j < speechResult.Count; j++)
                {
                    flag = false;
                    if (speechResult[j].IndexOf(enWords[i].ToLower()) > -1)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    json += "{\"word\":\"" + enWords1[i] + "\",\"isError\":false},";
                }
                else
                {
                    json += "{\"word\":\"" + enWords1[i] + "\",\"isError\":true},";
                }
            }
            json = json.TrimEnd(',');
            json += "]";
            return json;
        }

        /// <summary>
        /// 获取返回句子（中文）
        /// </summary>
        /// <param name="enText"></param>
        /// <param name="speechResult"></param>
        /// <returns></returns>
        public static string getSentenceAccuracyZh(string enText, List<string> speechResult)
        {
            string json = "";
            json = "[";
            //string[] enWords = getSentenceWords(enText);//对比的时候去掉符号，可能修改，enWords1为原句子
            char[] enWordsZh= enText.ToCharArray();
            char[] enWordsZh1 = enText.ToCharArray();
            //string[] enWords1 = getSentenceWords(enText);
            bool flag = false;
            for (int i = 0; i < enWordsZh.Length; i++)
            {
                //按照上面的截取规则，标点符号都在最后一个
                for (int j = 0; j < speechResult.Count; j++)
                {
                    flag = false;
                    if (speechResult[j].IndexOf(enWordsZh[i]) > -1)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    json += "{\"word\":\"" + enWordsZh1[i] + "\",\"isError\":false},";
                }
                else
                {
                    json += "{\"word\":\"" + enWordsZh1[i] + "\",\"isError\":true},";
                }
            }
            json = json.TrimEnd(',');
            json += "]";
            return json;
        }
        private static string replaceSymbol(string sentence)
        {
            sentence = sentence.Replace(",", "");
            sentence = sentence.Replace(".", "");
            sentence = sentence.Replace("?", "");
            sentence = sentence.Replace("!", "");
            sentence = sentence.Replace("…", "");
            sentence = sentence.Replace("？", "");
            sentence = sentence.Replace("(", "");
            sentence = sentence.Replace(")", "");
            sentence = sentence.Replace("！", "");
            sentence = sentence.Replace("\"", "");
            return sentence;
        }

        internal static string getSentenceAccuracy3(string enText, List<string> speechResult)
        {
            string json = "";
            json = "[";
            string[] enWords = getSentenceWords(enText);//对比的时候去掉符号，可能修改，enWords1为原句子
            string[] enWords1 = getSentenceWords(enText);
            for (int i = 0; i < enWords.Length; i++)
            {
                json += "{\"word\":\"" + enWords1[i] + "\",\"isError\":true},";
            }
            json = json.TrimEnd(',');
            json += "]";
            return json;
        }
    }
}