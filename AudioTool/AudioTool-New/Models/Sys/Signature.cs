using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PictureBook.Models
{
    /// <summary>
    /// 签名对象
    /// </summary>
    public class Signature
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 密钥
        /// </summary>
        public string AppSecret { get; set; }
        /// <summary>
        /// 用户名对应签名Token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 接口名
        /// </summary>
        public string ActionName { get; set; }
    }
}