using System;
using System.Collections.Generic;
using System.Text;

namespace Atlantis.Grpc.Utilies
{
    public static class Exetension
    {
        //public static IList<TEntity> ToSqlServerPage<TEntity>(this DbContext context, string strsql, string orderBy, int pageIndex, int pageSize,out long totalRow, params object[] parameters)
        //{
        //    if (string.IsNullOrWhiteSpace(strsql)) throw new ArgumentNullException("sql 语句不能为空！");
        //    if (string.IsNullOrWhiteSpace(orderBy)) throw new ArgumentNullException("orderby 列名不能为空！");
        //    if (pageIndex <= 0) throw new InvalidOperationException("Page index 必须大于零！");
        //    if (pageSize <= 0) throw new InvalidOperationException("Page size 必须大于零！");

        //    var pageStartIndex = pageIndex == 1 ? 0 : (pageIndex - 1) * pageSize;
        //    string pagingSql = $"select * from (select *,ROW_NUMBER() over(order by {orderBy}) as rownum from ({strsql}) t) t where rownum>{pageStartIndex} and rownum<={pageIndex * pageSize}";
        //    string countSql = $"select count(0) from ({strsql}) t";

        //    var list = context.Database.SqlQuery<TEntity>(pagingSql, parameters).ToList();
        //    totalRow = context.Database.SqlQuery<int>(countSql, parameters).First();
        //    return list;
        //}
        
        /// <summary>
        /// Timestamp to datetime
        /// </summary>
        /// <param name="timestamp">source timestamp</param>
        /// <param name="type">target datetime type</param>
        /// <returns></returns>
        public static DateTime ToTime(this long timestamp,TimeType type=TimeType.Beijing)
        {
            DateTime dtStart ;
            switch(type)
            {
                case TimeType.Beijing:dtStart=DateTime.Parse("1970/01/01 08:00:00");break;
                default:dtStart=DateTime.Parse("1970/01/01 00:00:00");break;
            }
            return dtStart.AddSeconds(timestamp);
        }

        public static string CheckTime(this string dateTimeStr,DateTime? defaultTime=null,string format="yyyy/MM/dd HH:mm:ss")
        {
            DateTime dateTime;
            if (!DateTime.TryParse(dateTimeStr, out dateTime))
            {
                dateTime = defaultTime.HasValue ? defaultTime.Value : DateTime.MinValue;
            }
            return dateTime.ToString(format);
        }

        public static DateTime MaxTime()
        {
            return DateTime.Parse("2999/01/01 00:00:00");
        }
        
        public static string ToSqlInWithInt(this IList<int> values)
        {
            if (values == null || values.Count == 0) return string.Empty;
            string strValues = string.Empty;
            foreach (var item in values) strValues += $"{item},";
            return strValues.Remove(strValues.Length - 1);
        }

        public static DateTime GetWeekTargetDay(this DateTime time,DayOfWeek targetDayOfWeek)
        {
            int targetDayOfWeekInt = targetDayOfWeek == DayOfWeek.Sunday ? 7 : (int)targetDayOfWeek ;
            int currentDayOfWeekInt=time.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)time.DayOfWeek;
            return time.AddDays(-(currentDayOfWeekInt - targetDayOfWeekInt));
        }

        public static int ToInt(this string str,int defaultValue=0)
        {
            int value=defaultValue;
            if (!int.TryParse(str, out value)) return defaultValue;
            return value;
        }

        /// <summary>
        /// DateTime convert tp timestamp
        /// </summary>
        /// <param name="dateTimeStr">source time (like this: yyyy/MM/dd HH:mm:ss)</param>
        /// <param name="type">source time type</param>
        /// <returns></returns>
        public static long ToTimestamp(this string dateTimeStr,TimeType type=TimeType.Beijing)
        {
            DateTime dateTime;
            if (!DateTime.TryParse(dateTimeStr, out dateTime)) return 0;
            DateTime dtStart ;
            switch(type)
            {
                case TimeType.Beijing:dtStart= DateTime.Parse("1970/01/01 08:00:00");break;
                default:dtStart= DateTime.Parse("1970/01/01 00:00:00");break;
            }
            return (long)(dateTime-dtStart).TotalSeconds;
        }

        public static string GetExceptionMessage(this Exception exception)
        {
            StringBuilder error = new StringBuilder(exception.Message);
            while(true)
            {
                if (exception.InnerException != null)
                {
                    error.Append($" --> {exception.InnerException.Message}");
                    exception = exception.InnerException;
                }
                else
                {
                    return error.ToString();
                }
            }
        }

        public static string GetCloseTrace(this Exception exception)
        {
            var traces=exception.StackTrace.Split('\n');
            if(traces==null||traces.Length==0)return exception.StackTrace;
            return traces[0];
        }

        public static IList<int> ToList(this string str,char split)
        {
            if (string.IsNullOrWhiteSpace(str)) return new List<int>();
            var ids = new List<int>();
            foreach(var item in str.Split(split))
            {
                ids.Add(item.ToInt(0));
            }
            return ids;
        }

        public static DateTime ToDateTime(this string dateTimeStr,DateTime? defaultTime=null)
        {
            DateTime dateTime;
            if(!DateTime.TryParse(dateTimeStr,out dateTime))
            {
                dateTime = defaultTime.HasValue ? defaultTime.Value:DateTime.MinValue;
            }
            return dateTime;
        }
        
        public static int ToStatus(this ResultCode code)
        {
            switch(code)
            {
                case ResultCode.Success:return 0;
                default:return 4; 
            }
        }

        public static string ToValOrDefault(this string value,string defaultValue)
        {
            if(string.IsNullOrWhiteSpace(value))return defaultValue;
            else return value;
        }

        public static string ToFrontString(this DateTime time)
        {
            return time.ToString("yyyy/MM/dd HH:mm:ss");
        }

        public static string ToStr(this IList<string> values,string split=",")
        {
            if(values==null||values.Count==0)return string.Empty;

            var strValue=new StringBuilder();
            foreach(var str in values)
            {
                strValue.Append(str);
                strValue.Append(",");
            }
            return strValue.ToString().Remove(strValue.Length-1);
        }
    }

    public enum TimeType
    {
        Beijing=1
    }
}
