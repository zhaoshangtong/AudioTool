using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AudioToolNew.Models.Musicline
{
    /// <summary>
    /// 返回的数据格式
    /// </summary>
    public class MusicResult
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public double timeSpan { get; set; }
        /// <summary>
        /// 音频内容
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string fileUrl { get; set; }
    }
}