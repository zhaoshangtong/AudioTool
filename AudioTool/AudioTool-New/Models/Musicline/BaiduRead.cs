using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AudioToolNew.Models.Musicline
{
    /// <summary>
    /// 百度识别
    /// </summary>
    public class BaiduRead
    {
        public double voice_time { get; set; }
        public TimeSpan start { get; set; }
        public TimeSpan end { get; set; }
        public string sound_path { get; set; }
        public int file_name { get; set; }
        public string language { get; set; }
        public double splitTime { get; set; }
    }
}