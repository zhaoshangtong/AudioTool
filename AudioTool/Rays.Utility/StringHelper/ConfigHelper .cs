using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rays.Utility
{
    /// <summary>
    /// 配置文件帮助类 
    /// </summary>
    public class ConfigHelper
    {
        /// <summary>  
        /// 根据Key取Value值  
        /// </summary>  
        /// <param name="key"></param>  
        public static string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key].ToString().Trim();
        }

        /// <summary>  
        /// 根据Key修改Value  
        /// </summary>  
        /// <param name="key">要修改的Key</param>  
        /// <param name="value">要修改为的值</param>  
        public static void SetValue(string key, string value)
        {
            ConfigurationManager.AppSettings.Set(key, value);
        }

        /// <summary>  
        /// 添加新的Key ，Value键值对  
        /// </summary>  
        /// <param name="key">Key</param>  
        /// <param name="value">Value</param>  
        public static void Add(string key, string value)
        {
            ConfigurationManager.AppSettings.Add(key, value);
        }

        /// <summary>  
        /// 根据Key删除项  
        /// </summary>  
        /// <param name="key">Key</param>  
        public static void Remove(string key)
        {
            ConfigurationManager.AppSettings.Remove(key);
        }
    }
}
