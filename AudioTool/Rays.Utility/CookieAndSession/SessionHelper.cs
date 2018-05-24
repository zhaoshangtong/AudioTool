/*
 源码己托管:http://git.oschina.net/kuiyu/dotnetcodes
 */
using System.Web;

namespace Rays.Utility
{
    /// <summary>
    /// Session 操作类
    /// 1、GetSession(string name)根据session名获取session对象
    /// 2、SetSession(string name, object val)设置session
    /// </summary>
    public class SessionHelper
    {
        /// <summary>
        /// 根据session名获取session对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetSession(string name)
        {
            if (HttpContext.Current.Session[name] == null)
            {
                return string.Empty;
            }
            else
            {
                return HttpContext.Current.Session[name].ToString();
            }
        }
        /// <summary>
        /// 添加Session
        /// </summary>
        /// <param name="name">session 名</param>
        /// <param name="strValue">session 值</param>
        public static void AddSession(string name, string strValue)
        {
            HttpContext.Current.Session.Remove(name);
            HttpContext.Current.Session.Add(name, strValue);
        }
        /// <summary>
        /// 添加Session
        /// </summary>
        /// <param name="name">Session对象名称</param>
        /// <param name="strValue">Session值</param>
        /// <param name="iExpires">调动有效期（分钟）</param>
        public static void AddSession(string name, string strValue, int iExpires)
        {
            HttpContext.Current.Session[name] = strValue;
            HttpContext.Current.Session.Timeout = iExpires;
        }

        /// <summary>
        /// 清空所有的Session
        /// </summary>
        /// <returns></returns>
        public static void ClearSession()
        {
            HttpContext.Current.Session.Clear();
        }

        /// <summary>
        /// 删除一个指定的session
        /// </summary>
        /// <param name="name">Session名称</param>
        /// <returns></returns>
        public static void DelSession(string name)
        {
            HttpContext.Current.Session[name] = null;
        }
    }
}
