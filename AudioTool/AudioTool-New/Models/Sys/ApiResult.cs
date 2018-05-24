using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;

namespace AudioToolNew.Models
{
    /// <summary>
    /// 返回JSON数据
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public Util.ApiStatusCode status { get; set; }
        /// <summary>
        /// 返回成功与否
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// 返回信息，如果失败则是错误提示
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public object data { get; set; }
    }

    /// <summary>
    /// 返回分页JSON数据
    /// </summary>
    public class ApiPageResult : ApiResult
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        public int pageIndex { get; set; }
        /// <summary>
        /// 每页显示条数
        /// </summary>
        public int pageSize { get; set; }
        /// <summary>
        /// 总数据数
        /// </summary>
        public int totalCount { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int pageTotal
        {
            get
            {
                return pageSize != 0 ? totalCount % pageSize == 0 ? (totalCount / pageSize) : (totalCount / pageSize + 1) : 0;
            }
        }
    }

    /// <summary>
    /// 返回分页JSON数据
    /// </summary>
    public class ApiPageResult2 : ApiResult
    {
        /// <summary>
        /// 开始下标
        /// </summary>
        public int beginPoint { get; set; }
        /// <summary>
        /// 每页显示条数
        /// </summary>
        public int pageSize { get; set; }
        /// <summary>
        /// 总数据数
        /// </summary>
        public int totalCount { get; set; }
        /// <summary>
        /// 当前 Data 的 Count
        /// </summary>
        public int currentCount
        {
            get
            {
                if (data is DataTable)
                {
                    return ((DataTable)data)?.Rows?.Count ?? 0;
                }
                if (data is IEnumerable<object>)
                {
                    var data1 = data as IEnumerable<object>;
                    return (data1 != null && data1.Any()) ? data1.Count() : 0;
                }
                return 0;
            }
        }
    }

    /// <summary>
    /// 返回JSON数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T> : ApiResult
    {
        /// <summary>
        /// 返回信息
        /// </summary>
        public new T data { get; set; }
    }

    /// <summary>
    /// 返回分页JSON数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiPageResult<T> : ApiPageResult
    {
        /// <summary>
        /// 返回信息
        /// </summary>
        public new T data { get; set; }
    }
}