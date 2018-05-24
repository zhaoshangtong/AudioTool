using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AudioToolNew.Models.Musicline
{
    public class TextContrastResult
    {
        /// <summary>
        /// 时间点
        /// </summary>
        public double  timespan { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string file_url { get; set; }
        /// <summary>
        /// 百度转语音的文字
        /// </summary>
        public string baiduText { get; set; }
        /// <summary>
        /// 匹配的结果
        /// </summary>
        public string contractText { get; set; }
        /// <summary>
        /// 相似度
        /// </summary>
        public string precent { get; set; }
    }
}