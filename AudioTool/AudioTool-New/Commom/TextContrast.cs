using AudioToolNew.Models.Musicline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace AudioToolNew.Commom
{
    /// <summary>
    /// 文本对比
    /// </summary>
    public class TextContrast
    {
        /// <summary>
        /// 获取两个中文字符串的相似百分比
        /// </summary>
        /// <param name="str1">百度翻译的字符串</param>
        /// <param name="str2">原文</param>
        /// <returns></returns>
        public static Similar GetStringSimilarityPerZw(string str1,string str2)
        {
            if (Util.isNotNull(str1)&&Util.isNotNull(str2))
            {
                //计算公式 相似度=Kq*q/(Kq*q+Kr*r+Ks*s)==== Kq=2，Kr=Ks=1.理论上要注意近义词的情况，比如他（她，它），的（地），不过英文不存在这个问题
                //q是字符串1和字符串2中都存在的单词的总数，s是字符串1中存在，字符串2中不存在的单词总数，r是字符串2中存在，字符串1中不存在的单词总数.
                //1.去掉标点符号
                //计算q，s，r的值
                string baiduText = Regex.Replace(str1, @"\p{P}", "");
                string orgText = Regex.Replace(str2, @"\p{P}", "");
                int q, s, r;
                GetPSR(baiduText, orgText, out q, out s, out r);
                double p = (double)2 * q / (double)(2 * q + r + s);
                double pre =Math.Round(p, 4) * 100;
                return new Similar()
                {
                    success = true,
                    precent = pre
                };
            }
            else
            {
                return new Similar()
                {
                    success = false,
                    error = "对比的字符串不能为空"
                };
            }
        }
        /// <summary>
        /// 计算q，s，r的值---中文
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <param name="q">q是字符串1和字符串2中都存在的单词的总数</param>
        /// <param name="s">s是字符串1中存在，字符串2中不存在的单词总数</param>
        /// <param name="r">r是字符串2中存在，字符串1中不存在的单词总数</param>
        private static void GetPSR(string str1, string str2, out int q, out int s, out int r)
        {
            q = 0;
            s = 0;
            r = 0;
            char[] str1Chars = str1.ToCharArray();
            char[] str2Chars = str2.ToCharArray();
            q = str2Chars.Where(o => str1Chars.Contains(o)).Count();
            s = str1Chars.Length - q;
            r = str2Chars.Length - q;
        }


        /// <summary>
        /// 获取两个英文字符串的相似百分比
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public static Similar GetStringSimilarityPerYw(string str1, string str2)
        {
            if (Util.isNotNull(str1) && Util.isNotNull(str2))
            {
                //计算公式 相似度=Kq*q/(Kq*q+Kr*r+Ks*s)==== Kq=2，Kr=Ks=1.理论上要注意近义词的情况，比如他（她，它），的（地），不过英文不存在这个问题
                //q是字符串1和字符串2中都存在的单词的总数，s是字符串1中存在，字符串2中不存在的单词总数，r是字符串2中存在，字符串1中不存在的单词总数.
                //1.去掉标点符号
                //计算q，s，r的值
                //！！！！英文还有一个特殊情况就是对话！需要排除掉 冒号之前的这样才能更准确一点
                //！！！！英文大小写的问题
                //听力的音频会有重复听两次的情况。百度语音转文字时也转了两遍，去掉重复的有助于提高正确率
                //1.重复音频一般都很长，所以转换后的字符串长度相对较大,而且在转换过程中发现一个问题，就是第一遍句尾和第二遍局头是直接连在一起的，没有空格
                string baiduText = Regex.Replace(str1, @"\p{P}", " ");
                string[] no_repeat = baiduText.Split(' ');
                //最少40个单词才判断是读了两边的音频
                if (no_repeat.Length >= 40)
                {
                    //取index为1到10，10个单词
                    var repeat=no_repeat.Skip(1).Take(10).ToArray();
                    string repeat_str=string.Join(" ",repeat);
                    int first_index = baiduText.IndexOf(repeat_str);
                    int last_index = baiduText.LastIndexOf(repeat_str);
                    if(last_index > first_index)
                    {
                        baiduText=baiduText.Remove(last_index);
                    }
                }
                string orgText = Regex.Replace(str2.ToLower(), @"\p{P}", " ");
                int q, s, r;
                GetPSRYw(baiduText, orgText, out q, out s, out r);
                double p = (double)2 * q / (double)(2 * q + r + s);
                double pre = Math.Round(p, 4) * 100;
                return new Similar()
                {
                    success = true,
                    precent = pre
                };
            }
            else
            {
                return new Similar()
                {
                    success = false,
                    error = "对比的字符串不能为空"
                };
            }
        }
        /// <summary>
        /// 计算q，s，r的值---英文
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <param name="q"></param>
        /// <param name="s"></param>
        /// <param name="r"></param>
        private static void GetPSRYw(string str1, string str2, out int q, out int s, out int r)
        {
            q = 0;
            s = 0;
            r = 0;
            string[] str1Words = str1.Split(' ');
            string[] str2Words = str2.Split(' ').Where(o=>o!="").ToArray();
            q = str2Words.Where(o => str1Words.Contains(o)).Count();
            s = str1Words.Length - q;
            r = str2Words.Length - q;
        }
    }
}