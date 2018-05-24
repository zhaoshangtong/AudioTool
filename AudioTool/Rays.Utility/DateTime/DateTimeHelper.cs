using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rays.Utility
{
    public class DateTimeHelper
    {

        #region 获取该时间所在周的周一和周末2
        /// <summary>
        /// 获取本周周一和周日时间
        /// </summary>
        /// <param name="dt">传入时间</param>
        /// <returns></returns>
        public static DateTime[] GetMondayAndSundayByTime(DateTime dt)
        {
            //获取改时间是本周的第几天
            int thedayoftheweek = Convert.ToInt32(dt.DayOfWeek.ToString("d"));
            //本周周一
            DateTime startWeek = dt.AddDays(1 - (thedayoftheweek == 0 ? 7 : thedayoftheweek));
            return new DateTime[] { startWeek, startWeek.AddDays(6) };
        }
        #endregion

        #region 获取时间所在当天的开始和结束时间(注：在这里当天的结束时间是在明天的开始的基础上加上-1的Tick值)
        /// <summary>
        /// 获取时间所在当天的开始和结束时间
        /// </summary>
        /// <param name="dt">传入时间</param>
        /// <returns></returns>
        public static DateTime[] GetBeginAndEndTimeOfTheDay(DateTime dt)
        {
            DateTime beginTime = DateTime.Parse(dt.ToString("yyyy-MM-dd"));
            DateTime endTime = DateTime.Parse(dt.ToString("yyyy-MM-dd")).AddDays(1).AddTicks(-1);
            return new DateTime[] { beginTime, endTime };
        }
        #endregion

        #region 获取时间所在月的开始和结束时间
        /// <summary>
        /// 获取时间所在月的开始和结束时间
        /// </summary>
        /// <param name="dt">传入时间</param>
        /// <returns></returns>
        public static DateTime[] GetBeginAndEndTimeOfTheMonth(DateTime dt)
        {
            DateTime startMonth = dt.AddDays(1 - dt.Day);  //本月月初
            DateTime endMonth = startMonth.AddMonths(1).AddDays(-1);  //本月月末
            return new DateTime[] { startMonth, endMonth };
        }
        #endregion

        #region 获取时间所在季度的开始和结束时间
        /// <summary>
        /// 获取时间所在季度的开始和结束时间
        /// </summary>
        /// <param name="dt">传入时间</param>
        /// <returns></returns>
        public static DateTime[] GetBeginAndEndTimeOfQuarter(DateTime dt)
        {
            //本季度初
            DateTime startQuarter = dt.AddMonths(0 - (dt.Month - 1) % 3).AddDays(1 - dt.Day);
            //本季度末
            DateTime endQuarter = startQuarter.AddMonths(3).AddDays(-1);
            return new DateTime[] { startQuarter, endQuarter };
        }
        #endregion

        #region 获取本年年初和年末
        /// <summary>
        /// 获取本年年初和年末
        /// </summary>
        /// <param name="dt">传入时间</param>
        /// <returns></returns>
        public static DateTime[] GetBeginAndEndTimeOfTheYear(DateTime dt)
        {
            DateTime startYear = new DateTime(dt.Year, 1, 1);  //本年年初
            DateTime endYear = new DateTime(dt.Year, 12, 31);  //本年年末
            return new DateTime[] { startYear, endYear };
        }
        #endregion

        #region 获取某一年某一月有多少天
        /// <summary>
        /// 获取某一年某一月有多少天
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns></returns>
        public static int GetMonthDays(int year, int month)
        {
            return new DateTime(year, month, 1).AddMonths(1).AddDays(-1).Day;
        }
        #endregion

        #region 判断当前年份是否是闰年
        /// <summary>判断当前年份是否是闰年</summary>
        /// <param name="year">年份</param>
        /// <returns>是闰年：True ，不是闰年：False</returns>
        public static bool IsRuYear(int year)
        {
            //形式参数为年份
            //例如：2003
            int n = year;
            if ((n % 400 == 0) || (n % 4 == 0 && n % 100 != 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 判断当前日期所属的年份是否是闰年
        /// <summary>判断当前日期所属的年份是否是闰年</summary>
        /// <param name="dt">日期</param>
        /// <returns>是闰年：True ，不是闰年：False</returns>
        public static bool IsRuYear(DateTime time)
        {
            //形式参数为日期类型
            //例如：2003-12-12
            int n = time.Year;
            if ((n % 400 == 0) || (n % 4 == 0 && n % 100 != 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 将日期对象转化为格式字符串
        /// <summary>
        /// 将日期对象转化为格式字符串
        /// </summary>
        /// <param name="oDateTime">日期对象</param>
        /// <param name="strFormat">
        /// 格式：
        ///    "SHORTDATE"===短日期
        ///    "LONGDATE"==长日期
        ///    其它====自定义格式 yyyy-MM-dd hh:mm:ss
        /// </param>
        /// <returns>日期字符串</returns>
        public static string ConvertDateToString(DateTime oDateTime, string strFormat)
        {
            string strDate = "";
            try
            {
                switch (strFormat.ToUpper())
                {
                    case "SHORTDATE":
                        strDate = oDateTime.ToShortDateString();
                        break;
                    case "LONGDATE":
                        strDate = oDateTime.ToLongDateString();
                        break;
                    default:
                        strDate = oDateTime.ToString(strFormat);
                        break;
                }
            }
            catch (Exception)
            {
                strDate = oDateTime.ToShortDateString();
            }
            return strDate;
        }
        #endregion

        #region 判断是否为合法日期，必须大于1800年1月1日
        /// <summary>
        /// 判断是否为合法日期，必须大于1800年1月1日
        /// </summary>
        /// <param name="strDate">输入日期字符串</param>
        /// <returns>True/False</returns>
        public static bool IsDateTime(string strDate)
        {
            try
            {
                DateTime oDate = DateTime.Parse(strDate);
                if (oDate.CompareTo(DateTime.Parse("1800-1-1")) > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region 获取两个日期之间的差值 可返回年 月 日 小时 分钟 秒
        /// <summary>
        /// 获取两个日期之间的差值
        /// </summary>
        /// <param name="howtocompare">比较的方式可为：year month day hour minute second</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>时间差</returns>
        public static double DateDiff(string howtocompare, DateTime startDate, DateTime endDate)
        {
            double diff = 0;
            try
            {
                TimeSpan TS = new TimeSpan(endDate.Ticks - startDate.Ticks);
                switch (howtocompare.ToLower())
                {
                    case "year":
                        diff = Convert.ToDouble(TS.TotalDays / 365);
                        break;
                    case "month":
                        diff = Convert.ToDouble((TS.TotalDays / 365) * 12);
                        break;
                    case "day":
                        diff = Convert.ToDouble(TS.TotalDays);
                        break;
                    case "hour":
                        diff = Convert.ToDouble(TS.TotalHours);
                        break;
                    case "minute":
                        diff = Convert.ToDouble(TS.TotalMinutes);
                        break;
                    case "second":
                        diff = Convert.ToDouble(TS.TotalSeconds);
                        break;
                }
            }
            catch (Exception)
            {
                diff = 0;
            }
            return diff;
        }
        #endregion

        #region 计算两个日期之间相差的工作日天数
        /// <summary>
        /// 计算两个日期之间相差的工作日天数
        /// </summary>
        /// <param name="dtStart">开始日期</param>
        /// <param name="dtEnd">结束日期</param>
        /// <param name="flag">是否除去周六，周日</param>
        /// <returns>Int</returns>
        public static int CalculateWorkingDays(DateTime dtStart, DateTime dtEnd, bool flag)
        {
            int count = 0;
            for (DateTime dtTemp = dtStart; dtTemp < dtEnd; dtTemp = dtTemp.AddDays(1))
            {
                if (flag)
                {
                    if (dtTemp.DayOfWeek != DayOfWeek.Saturday && dtTemp.DayOfWeek != DayOfWeek.Sunday)
                    {
                        count++;
                    }
                }
                else
                {
                    count++;
                }
            }
            return count;
        }
        #endregion
    }
}
