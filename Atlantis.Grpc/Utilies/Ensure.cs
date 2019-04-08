using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Followme.AspNet.Core.FastCommon.Utilities
{
    public static class Ensure
    {

        public static void NotNull<T>(T obj,string errorMsg,EnsureScope scope=null,Exception innerException=null)
        {
            if (obj == null) throw new EnsureException(errorMsg,scope, innerException);
        }

        public static void NotNull<T>(IList<T> list,string errorMsg,EnsureScope scope=null, Exception innerException=null)
        {
            if (list == null||list.Count()==0) throw new EnsureException(errorMsg,scope, innerException);
        }

        public static void NotNullOrWhiteSpace(string str,string errorMsg,EnsureScope scope=null,Exception innerException=null)
        {
            if (string.IsNullOrWhiteSpace(str)) throw new EnsureException(errorMsg, scope,innerException);
        }

        public static void GrandThan(double maxValue,double minValue,string errorMsg,bool hasAllowEqual=true,EnsureScope scope=null)
        {
            if (hasAllowEqual && maxValue < minValue) throw new EnsureException(errorMsg, scope);
            if (!hasAllowEqual && maxValue <= minValue) throw new EnsureException(errorMsg, scope);
        }

        public static void GrandThan(long maxValue,long minValue,string errorMsg,bool hasAllowEqual=true,EnsureScope scope=null)
        {
            if (hasAllowEqual && maxValue < minValue) throw new EnsureException(errorMsg, scope);
            if (!hasAllowEqual && maxValue <= minValue) throw new EnsureException(errorMsg, scope);
        }

        public static void GrandThan(decimal maxValue,decimal minValue,string errorMsg,bool hasAllowEqual=true,EnsureScope scope=null)
        {
            if (hasAllowEqual && maxValue < minValue) throw new EnsureException(errorMsg, scope);
            if (!hasAllowEqual && maxValue <= minValue) throw new EnsureException(errorMsg, scope);
        }

        public static void GrandThan(DateTime maxValue,DateTime minValue,string errorMsg,bool hasAllowEqual=true,EnsureScope scope=null)
        {
            if (hasAllowEqual && maxValue < minValue) throw new EnsureException(errorMsg, scope);
            if (!hasAllowEqual && maxValue <= minValue) throw new EnsureException(errorMsg, scope);
        }

        public static void MustBeNull<T>(IList<T> list,string errorMsg,EnsureScope scope=null)
        {
            if (list != null && list.Count != 0) throw new EnsureException(errorMsg, scope);
        }

        public static void MustBeNull<T>(T obj,string errorMsg,EnsureScope scope=null)
        {
            if (obj != null ) throw new EnsureException(errorMsg, scope);
        }

        public static void MustBeEqual(int left,int right,string errorMsg,EnsureScope scope=null)
        {
            if (left != right) throw new EnsureException(errorMsg, scope);
        }

        public static void MustBeNoEqual(int left, int right, string errorMsg,EnsureScope scope=null)
        {
            if (left == right) throw new EnsureException(errorMsg, scope);
        }

        public static void MustBeTrue(bool value,string errorMsg,EnsureScope scope=null)
        {
            if (!value) throw new EnsureException(errorMsg, scope);
        }

        public static void MustBeFalse(bool value, string errorMsg,EnsureScope scope=null)
        {
            if (value) throw new EnsureException(errorMsg, scope);
        }

        public static void MustBeNum(string numStr,string errorMsg,EnsureScope scope=null)
        {
            if (!Regex.IsMatch(numStr, @"^[0-9]*$")) throw new EnsureException(errorMsg, scope);
        }

    }

    public class EnsureScope
    {
        public static  EnsureScope Default=new EnsureScope(){ScopeName="System"};

        public string ScopeName{get;set;}
    }

    public class EnsureException:System.Exception
    {
        public EnsureException(string message,EnsureScope scope=null,Exception innerException=null):base(message,innerException)
        { 
            Scope=scope??EnsureScope.Default;
        }

        public EnsureScope Scope{get;set;}

        /// <summary>
        /// if eat success, we can return ensureexception.
        /// if eat failed, we will be return origin exception.
        /// </summary>
        /// <param name="ex">exception or ensureexception</param>
        /// <returns></returns>
        public static bool EatException(ref Exception ex)
        {
            var ensureException = ex;
            while (true)
            {
                if (ensureException == null) break;
                if (ensureException is EnsureException) break;
                ensureException = ensureException.InnerException;
            }
            if (ensureException is EnsureException)
            {
                ex=ensureException;
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
